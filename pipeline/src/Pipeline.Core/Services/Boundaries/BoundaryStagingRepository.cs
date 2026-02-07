using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Npgsql;
using NpgsqlTypes;

namespace Pipeline.Core.Services.Boundaries;

/// <summary>
/// Repository for boundary import staging operations.
/// Handles staging table creation, bulk insert, neighborhood aggregation, and finalization.
/// </summary>
public interface IBoundaryStagingRepository
{
    /// <summary>
    /// Get current counts of neighborhoods and statistical sectors.
    /// Returns (0, 0) if tables don't exist.
    /// </summary>
    Task<(int NeighborhoodCount, int SectorCount)> GetCurrentCountsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Check if neighborhood_statistics table has data (to warn about re-import consequences).
    /// </summary>
    Task<int> GetStatisticsCountAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Create the sectors staging table with SRID 31370 geometry.
    /// </summary>
    Task CreateStagingTableAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Bulk insert sector features into the staging table using PostgreSQL COPY.
    /// </summary>
    Task BulkInsertSectorsAsync(List<SectorFeature> sectors, CancellationToken cancellationToken);

    /// <summary>
    /// Query the staging table for aggregated neighborhood metadata.
    /// Used by the service to compute slugs in C#.
    /// </summary>
    Task<List<NeighborhoodMetadata>> GetNeighborhoodMetadataAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Query the staging table for all sector records (for slug generation).
    /// </summary>
    Task<List<(string CdSector, string City, string Name)>> GetSectorMetadataAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Create the neighborhoods table and populate it by aggregating from staging.
    /// Uses PostGIS ST_Transform + ST_Union for geometry aggregation.
    /// </summary>
    Task<int> CreateAndPopulateNeighborhoodsAsync(
        Dictionary<string, string> slugMap,
        CancellationToken cancellationToken);

    /// <summary>
    /// Apply the 2025 municipality merger mapping to statbel_municipality_nis.
    /// </summary>
    Task ApplyMunicipalityMappingAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Create the statistical_sectors table and populate from staging with ST_Transform.
    /// Links to neighborhoods via NIS code prefix.
    /// </summary>
    Task<int> CreateAndPopulateStatisticalSectorsAsync(
        Dictionary<string, string> slugMap,
        CancellationToken cancellationToken);

    /// <summary>
    /// Create the neighborhood_statistics table if it doesn't exist (empty, for O2).
    /// </summary>
    Task CreateNeighborhoodStatisticsTableAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Clean up the staging table.
    /// </summary>
    Task DropStagingTableAsync(CancellationToken cancellationToken);
}

public class BoundaryStagingRepository : IBoundaryStagingRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<BoundaryStagingRepository> _logger;

    public BoundaryStagingRepository(string connectionString, ILogger<BoundaryStagingRepository> logger)
    {
        _logger = logger;

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseNetTopologySuite();
        _dataSource = dataSourceBuilder.Build();
    }

    public async Task<(int NeighborhoodCount, int SectorCount)> GetCurrentCountsAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        var neighborhoodCount = await GetTableCountAsync(conn, "neighborhoods", cancellationToken);
        var sectorCount = await GetTableCountAsync(conn, "statistical_sectors", cancellationToken);

        return (neighborhoodCount, sectorCount);
    }

    public async Task<int> GetStatisticsCountAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        return await GetTableCountAsync(conn, "neighborhood_statistics", cancellationToken);
    }

    public async Task CreateStagingTableAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var dropCmd = new NpgsqlCommand(
            "DROP TABLE IF EXISTS sectors_staging CASCADE", conn);
        await dropCmd.ExecuteNonQueryAsync(cancellationToken);

        await using var createCmd = new NpgsqlCommand("""
            CREATE TABLE sectors_staging (
                cd_sector VARCHAR(20) PRIMARY KEY,
                name TEXT NOT NULL,
                city TEXT NOT NULL,
                province TEXT,
                region TEXT,
                boundary GEOMETRY(MultiPolygon, 31370) NOT NULL
            )
            """, conn);
        await createCmd.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("Created sectors_staging table");
    }

    public async Task BulkInsertSectorsAsync(List<SectorFeature> sectors, CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        var wkbReader = new WKBReader();

        {
            await using var writer = await conn.BeginBinaryImportAsync(
                "COPY sectors_staging (cd_sector, name, city, province, region, boundary) FROM STDIN (FORMAT BINARY)",
                cancellationToken);

            foreach (var sector in sectors)
            {
                await writer.StartRowAsync(cancellationToken);
                await writer.WriteAsync(sector.CdSector, NpgsqlDbType.Varchar, cancellationToken);
                await writer.WriteAsync(sector.SectorNameNl, NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync(sector.CityNameNl, NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync((object?)sector.ProvinceNl ?? DBNull.Value, NpgsqlDbType.Text, cancellationToken);
                await writer.WriteAsync((object?)sector.RegionNl ?? DBNull.Value, NpgsqlDbType.Text, cancellationToken);

                // Read WKB back into NTS Geometry for proper binary protocol serialization
                var geometry = wkbReader.Read(sector.GeometryWkb);
                geometry.SRID = 31370;
                await writer.WriteAsync(geometry, NpgsqlDbType.Geometry, cancellationToken);
            }

            await writer.CompleteAsync(cancellationToken);
        }

        _logger.LogInformation("Bulk inserted {Count} sectors into staging", sectors.Count);
    }

    public async Task<List<NeighborhoodMetadata>> GetNeighborhoodMetadataAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand("""
            SELECT
                LEFT(cd_sector, 7) AS nis_code,
                MODE() WITHIN GROUP (ORDER BY name) AS neighborhood_name,
                city,
                COALESCE(province, region) AS province,
                COUNT(*) AS sector_count
            FROM sectors_staging
            GROUP BY LEFT(cd_sector, 7), city, COALESCE(province, region)
            ORDER BY nis_code
            """, conn);

        var result = new List<NeighborhoodMetadata>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new NeighborhoodMetadata(
                NisCode: reader.GetString(0),
                Name: reader.GetString(1),
                City: reader.GetString(2),
                Province: reader.IsDBNull(3) ? null : reader.GetString(3),
                SectorCount: reader.GetInt32(4)));
        }

        _logger.LogInformation("Found {Count} neighborhoods from staging aggregation", result.Count);
        return result;
    }

    public async Task<List<(string CdSector, string City, string Name)>> GetSectorMetadataAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(
            "SELECT cd_sector, city, name FROM sectors_staging ORDER BY cd_sector", conn);

        var result = new List<(string, string, string)>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add((reader.GetString(0), reader.GetString(1), reader.GetString(2)));
        }

        return result;
    }

    public async Task<int> CreateAndPopulateNeighborhoodsAsync(
        Dictionary<string, string> slugMap,
        CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);

        try
        {
            // Drop existing tables that depend on neighborhoods (cascade)
            await using var dropSectorsCmd = new NpgsqlCommand(
                "DROP TABLE IF EXISTS gis.statistical_sectors CASCADE", conn, transaction);
            await dropSectorsCmd.ExecuteNonQueryAsync(cancellationToken);

            await using var dropNeighborhoodsCmd = new NpgsqlCommand(
                "DROP TABLE IF EXISTS gis.neighborhoods CASCADE", conn, transaction);
            await dropNeighborhoodsCmd.ExecuteNonQueryAsync(cancellationToken);

            // Create neighborhoods table
            await using var createCmd = new NpgsqlCommand("""
                CREATE TABLE gis.neighborhoods (
                    id VARCHAR(100) PRIMARY KEY,
                    name VARCHAR(255) NOT NULL,
                    city VARCHAR(100) NOT NULL,
                    province VARCHAR(100),
                    nis_code VARCHAR(7) NOT NULL,
                    statbel_municipality_nis VARCHAR(5),
                    sector_count INTEGER,
                    area_km2 DECIMAL(10, 4),
                    centroid GEOMETRY(Point, 4326),
                    boundary GEOMETRY(MultiPolygon, 4326),
                    created_at TIMESTAMP DEFAULT NOW(),
                    updated_at TIMESTAMP DEFAULT NOW()
                )
                """, conn, transaction);
            await createCmd.ExecuteNonQueryAsync(cancellationToken);

            // Upload slug mapping to a temp table for JOIN
            await using var createSlugCmd = new NpgsqlCommand("""
                CREATE TEMP TABLE slug_mapping (
                    nis_code VARCHAR(7) PRIMARY KEY,
                    slug VARCHAR(100) NOT NULL
                ) ON COMMIT DROP
                """, conn, transaction);
            await createSlugCmd.ExecuteNonQueryAsync(cancellationToken);

            {
                await using var slugWriter = await conn.BeginBinaryImportAsync(
                    "COPY slug_mapping (nis_code, slug) FROM STDIN (FORMAT BINARY)",
                    cancellationToken);

                foreach (var (nisCode, slug) in slugMap)
                {
                    await slugWriter.StartRowAsync(cancellationToken);
                    await slugWriter.WriteAsync(nisCode, NpgsqlDbType.Varchar, cancellationToken);
                    await slugWriter.WriteAsync(slug, NpgsqlDbType.Varchar, cancellationToken);
                }
                await slugWriter.CompleteAsync(cancellationToken);
            }

            // Aggregate sectors into neighborhoods using PostGIS
            await using var insertCmd = new NpgsqlCommand("""
                INSERT INTO gis.neighborhoods (id, name, city, province, nis_code, sector_count, boundary, centroid, area_km2)
                SELECT
                    sm.slug AS id,
                    agg.neighborhood_name AS name,
                    agg.city,
                    agg.province,
                    agg.nis_code,
                    agg.sector_count,
                    ST_Multi(ST_Union(ST_MakeValid(ST_Transform(s.boundary, 4326)))) AS boundary,
                    ST_Centroid(ST_Multi(ST_Union(ST_MakeValid(ST_Transform(s.boundary, 4326))))) AS centroid,
                    ST_Area(ST_Union(ST_MakeValid(ST_Transform(s.boundary, 4326)))::geography) / 1000000.0 AS area_km2
                FROM sectors_staging s
                JOIN (
                    SELECT
                        LEFT(cd_sector, 7) AS nis_code,
                        MODE() WITHIN GROUP (ORDER BY name) AS neighborhood_name,
                        city,
                        COALESCE(province, region) AS province,
                        COUNT(*) AS sector_count
                    FROM sectors_staging
                    GROUP BY LEFT(cd_sector, 7), city, COALESCE(province, region)
                ) agg ON LEFT(s.cd_sector, 7) = agg.nis_code
                JOIN slug_mapping sm ON sm.nis_code = agg.nis_code
                GROUP BY sm.slug, agg.neighborhood_name, agg.city, agg.province, agg.nis_code, agg.sector_count
                """, conn, transaction);

            // This can take 30-60 seconds for ~10k polygons
            insertCmd.CommandTimeout = 300;
            var inserted = await insertCmd.ExecuteNonQueryAsync(cancellationToken);

            // Create indexes
            await using var indexCmd = new NpgsqlCommand("""
                CREATE INDEX idx_neighborhoods_centroid ON gis.neighborhoods USING GIST (centroid);
                CREATE INDEX idx_neighborhoods_boundary ON gis.neighborhoods USING GIST (boundary);
                CREATE INDEX idx_neighborhoods_city ON gis.neighborhoods (city);
                CREATE INDEX idx_neighborhoods_nis_code ON gis.neighborhoods (nis_code);
                """, conn, transaction);
            await indexCmd.ExecuteNonQueryAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Created {Count} neighborhoods", inserted);
            return inserted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Neighborhood creation failed, rolling back");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task ApplyMunicipalityMappingAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);

        try
        {
            // Set default: municipality NIS = first 5 chars of neighborhood NIS
            await using var defaultCmd = new NpgsqlCommand("""
                UPDATE gis.neighborhoods
                SET statbel_municipality_nis = LEFT(nis_code, 5)
                WHERE statbel_municipality_nis IS NULL
                """, conn, transaction);
            var defaultCount = await defaultCmd.ExecuteNonQueryAsync(cancellationToken);

            // Apply 2025 merger overrides
            var mergerGroups = MunicipalityMergerMapping.GetMergerGroups();
            var mergerCount = 0;

            foreach (var (newNis, oldNisCodes) in mergerGroups)
            {
                var placeholders = string.Join(", ", oldNisCodes.Select((_, i) => $"@old{i}"));
                var sql = $"UPDATE gis.neighborhoods SET statbel_municipality_nis = @newNis WHERE LEFT(nis_code, 5) IN ({placeholders})";

                await using var cmd = new NpgsqlCommand(sql, conn, transaction);
                cmd.Parameters.AddWithValue("newNis", newNis);
                for (var i = 0; i < oldNisCodes.Count; i++)
                {
                    cmd.Parameters.AddWithValue($"old{i}", oldNisCodes[i]);
                }

                var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                mergerCount += affected;
            }

            // Create index on the mapping column
            await using var indexCmd = new NpgsqlCommand(
                "CREATE INDEX IF NOT EXISTS idx_neighborhoods_statbel_municipality ON gis.neighborhoods(statbel_municipality_nis)",
                conn, transaction);
            await indexCmd.ExecuteNonQueryAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation(
                "Applied municipality mapping: {Default} defaults, {Merged} merger overrides",
                defaultCount, mergerCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Municipality mapping failed, rolling back");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<int> CreateAndPopulateStatisticalSectorsAsync(
        Dictionary<string, string> slugMap,
        CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create statistical_sectors table
            await using var createCmd = new NpgsqlCommand("""
                CREATE TABLE IF NOT EXISTS gis.statistical_sectors (
                    id VARCHAR(100) PRIMARY KEY,
                    name VARCHAR(255) NOT NULL,
                    city VARCHAR(100) NOT NULL,
                    province VARCHAR(100),
                    nis_code VARCHAR(20),
                    neighborhood_id VARCHAR(100),
                    area_km2 DECIMAL(10, 4),
                    centroid GEOMETRY(Point, 4326),
                    boundary GEOMETRY(MultiPolygon, 4326),
                    created_at TIMESTAMP DEFAULT NOW(),
                    updated_at TIMESTAMP DEFAULT NOW(),
                    CONSTRAINT fk_statistical_sectors_neighborhood
                        FOREIGN KEY (neighborhood_id) REFERENCES gis.neighborhoods(id)
                )
                """, conn, transaction);
            await createCmd.ExecuteNonQueryAsync(cancellationToken);

            // Upload sector slug mapping to temp table
            await using var createSlugCmd = new NpgsqlCommand("""
                CREATE TEMP TABLE sector_slug_mapping (
                    cd_sector VARCHAR(20) PRIMARY KEY,
                    slug VARCHAR(100) NOT NULL
                ) ON COMMIT DROP
                """, conn, transaction);
            await createSlugCmd.ExecuteNonQueryAsync(cancellationToken);

            {
                await using var slugWriter = await conn.BeginBinaryImportAsync(
                    "COPY sector_slug_mapping (cd_sector, slug) FROM STDIN (FORMAT BINARY)",
                    cancellationToken);

                foreach (var (cdSector, slug) in slugMap)
                {
                    await slugWriter.StartRowAsync(cancellationToken);
                    await slugWriter.WriteAsync(cdSector, NpgsqlDbType.Varchar, cancellationToken);
                    await slugWriter.WriteAsync(slug, NpgsqlDbType.Varchar, cancellationToken);
                }
                await slugWriter.CompleteAsync(cancellationToken);
            }

            // Insert sectors with transformed geometry and neighborhood FK
            await using var insertCmd = new NpgsqlCommand("""
                INSERT INTO gis.statistical_sectors (id, name, city, province, nis_code, neighborhood_id, boundary, centroid, area_km2)
                SELECT
                    ssm.slug AS id,
                    s.name,
                    s.city,
                    COALESCE(s.province, s.region) AS province,
                    s.cd_sector AS nis_code,
                    n.id AS neighborhood_id,
                    ST_Multi(ST_MakeValid(ST_Transform(s.boundary, 4326))) AS boundary,
                    ST_Centroid(ST_MakeValid(ST_Transform(s.boundary, 4326))) AS centroid,
                    ST_Area(ST_MakeValid(ST_Transform(s.boundary, 4326))::geography) / 1000000.0 AS area_km2
                FROM sectors_staging s
                JOIN sector_slug_mapping ssm ON ssm.cd_sector = s.cd_sector
                LEFT JOIN gis.neighborhoods n ON LEFT(s.cd_sector, 7) = n.nis_code
                """, conn, transaction);
            insertCmd.CommandTimeout = 300;
            var inserted = await insertCmd.ExecuteNonQueryAsync(cancellationToken);

            // Create indexes
            await using var indexCmd = new NpgsqlCommand("""
                CREATE INDEX idx_statistical_sectors_centroid ON gis.statistical_sectors USING GIST (centroid);
                CREATE INDEX idx_statistical_sectors_boundary ON gis.statistical_sectors USING GIST (boundary);
                CREATE INDEX idx_statistical_sectors_city ON gis.statistical_sectors (city);
                CREATE INDEX idx_statistical_sectors_neighborhood ON gis.statistical_sectors (neighborhood_id);
                """, conn, transaction);
            await indexCmd.ExecuteNonQueryAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Created {Count} statistical sectors", inserted);
            return inserted;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Statistical sectors creation failed, rolling back");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task CreateNeighborhoodStatisticsTableAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand("""
            CREATE TABLE IF NOT EXISTS gis.neighborhood_statistics (
                id SERIAL PRIMARY KEY,
                neighborhood_id VARCHAR(100) REFERENCES gis.neighborhoods(id),
                year INTEGER NOT NULL,
                median_house_price INTEGER,
                price_per_sqm INTEGER,
                available_homes INTEGER,
                population INTEGER,
                population_density DECIMAL(10, 2),
                avg_age DECIMAL(4, 1),
                median_income INTEGER,
                green_space_pct DECIMAL(5, 2),
                created_at TIMESTAMP DEFAULT NOW(),
                updated_at TIMESTAMP DEFAULT NOW(),
                UNIQUE(neighborhood_id, year)
            )
            """, conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        await using var indexCmd = new NpgsqlCommand("""
            CREATE INDEX IF NOT EXISTS idx_stats_neighborhood ON gis.neighborhood_statistics (neighborhood_id);
            CREATE INDEX IF NOT EXISTS idx_stats_year ON gis.neighborhood_statistics (year);
            """, conn);
        await indexCmd.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("Ensured neighborhood_statistics table exists");
    }

    public async Task DropStagingTableAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        await using var cmd = new NpgsqlCommand(
            "DROP TABLE IF EXISTS sectors_staging CASCADE", conn);
        await cmd.ExecuteNonQueryAsync(cancellationToken);

        _logger.LogInformation("Dropped sectors_staging table");
    }

    private static readonly HashSet<string> AllowedTableNames =
    [
        "neighborhoods",
        "statistical_sectors",
        "neighborhood_statistics"
    ];

    private static async Task<int> GetTableCountAsync(NpgsqlConnection conn, string tableName, CancellationToken cancellationToken)
    {
        if (!AllowedTableNames.Contains(tableName))
            throw new ArgumentException($"Table name '{tableName}' is not in the allowed list", nameof(tableName));

        await using var checkCmd = new NpgsqlCommand(
            "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'gis' AND table_name = @tableName)",
            conn);
        checkCmd.Parameters.AddWithValue("tableName", tableName);
        var exists = (bool)(await checkCmd.ExecuteScalarAsync(cancellationToken))!;

        if (!exists)
            return 0;

        await using var countCmd = new NpgsqlCommand(
            $"SELECT COUNT(*)::int FROM gis.{tableName}", conn);
        return (int)(await countCmd.ExecuteScalarAsync(cancellationToken))!;
    }
}
