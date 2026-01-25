using Microsoft.Extensions.Logging;
using Npgsql;

namespace Pipeline.Core.Services.PoiImport;

/// <summary>
/// Repository for POI staging table operations.
/// Handles staging table creation, bulk insert, and atomic swap.
/// </summary>
public interface IPoiStagingRepository
{
    /// <summary>
    /// Gets current POI counts by category from the production table.
    /// Returns empty dictionary if table doesn't exist.
    /// </summary>
    Task<Dictionary<string, int>> GetCurrentCountsAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates the staging table with same structure as pois.
    /// Drops existing staging table if present.
    /// </summary>
    Task CreateStagingTableAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Bulk inserts POI records into staging table using PostgreSQL COPY.
    /// </summary>
    Task BulkInsertAsync(List<PoiRecord> pois, CancellationToken cancellationToken);

    /// <summary>
    /// Atomically swaps staging table with production table.
    /// Uses transaction to ensure consistency.
    /// </summary>
    Task SwapTablesAsync(CancellationToken cancellationToken);
}

public class PoiStagingRepository : IPoiStagingRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<PoiStagingRepository> _logger;

    public PoiStagingRepository(string connectionString, ILogger<PoiStagingRepository> logger)
    {
        _logger = logger;

        // Configure data source with NetTopologySuite for geometry support
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        dataSourceBuilder.UseNetTopologySuite();
        _dataSource = dataSourceBuilder.Build();
    }
    /// <summary>
    /// Gets current POI counts by category from the production table.
    /// Returns empty dictionary if table doesn't exist.
    /// </summary>
    public async Task<Dictionary<string, int>> GetCurrentCountsAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
                // Check if table exists first (gracefully handle fresh database)
        await using var checkCmd = new NpgsqlCommand(
            "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'pois')",
            conn);
        var tableExists = (bool)(await checkCmd.ExecuteScalarAsync(cancellationToken))!;

        if (!tableExists)
        {
            _logger.LogInformation("pois table does not exist yet - this is a fresh import");
            return new Dictionary<string, int>();
        }

        await using var cmd = new NpgsqlCommand(
            "SELECT category, COUNT(*)::int FROM pois GROUP BY category",
            conn);

        var counts = new Dictionary<string, int>();
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            counts[reader.GetString(0)] = reader.GetInt32(1);
        }

        return counts;
    }

    /// <summary>
    /// Creates the staging table with same structure as pois.
    /// Drops existing staging table if present.
    /// </summary>
    public async Task CreateStagingTableAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
                // Drop existing staging table if it exists
        await using var dropCmd = new NpgsqlCommand("DROP TABLE IF EXISTS pois_staging CASCADE", conn);
        await dropCmd.ExecuteNonQueryAsync(cancellationToken);

        // Create staging table with same structure as pois
        await using var createCmd = new NpgsqlCommand("""
            CREATE TABLE pois_staging (
                id SERIAL PRIMARY KEY,
                osm_id BIGINT,
                name TEXT,
                category VARCHAR(50) NOT NULL,
                subcategory VARCHAR(50),
                domain VARCHAR(50),
                location GEOMETRY(Point, 4326) NOT NULL,
                street TEXT,
                house_number VARCHAR(50),
                postal_code VARCHAR(20),
                city TEXT,
                phone TEXT,
                website TEXT,
                opening_hours TEXT,
                osm_tags JSONB,
                created_at TIMESTAMP DEFAULT NOW(),
                updated_at TIMESTAMP DEFAULT NOW()
            )
            """, conn);
        await createCmd.ExecuteNonQueryAsync(cancellationToken);

        // Create indexes on staging table
        await using var indexCmd = new NpgsqlCommand("""
            CREATE INDEX idx_pois_staging_location ON pois_staging USING GIST (location);
            CREATE INDEX idx_pois_staging_category ON pois_staging (category);
            CREATE INDEX idx_pois_staging_domain ON pois_staging (domain);
            CREATE INDEX idx_pois_staging_city ON pois_staging (city);
            """, conn);
        await indexCmd.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <summary>
    /// Bulk inserts POI records into staging table using PostgreSQL COPY.
    /// </summary>
    public async Task BulkInsertAsync(List<PoiRecord> pois, CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
                // Use COPY for bulk insert (much faster than individual INSERTs)
        await using var writer = await conn.BeginBinaryImportAsync(
            """
            COPY pois_staging (osm_id, name, category, domain, location, street, house_number, postal_code, city, phone, website, opening_hours, osm_tags)
            FROM STDIN (FORMAT BINARY)
            """,
            cancellationToken);

        foreach (var poi in pois)
        {
            await writer.StartRowAsync(cancellationToken);
            await writer.WriteAsync(poi.OsmId, NpgsqlTypes.NpgsqlDbType.Bigint, cancellationToken);
            await writer.WriteAsync(poi.Name, NpgsqlTypes.NpgsqlDbType.Varchar, cancellationToken);
            await writer.WriteAsync(poi.Category, NpgsqlTypes.NpgsqlDbType.Varchar, cancellationToken);
            await writer.WriteAsync(poi.Domain, NpgsqlTypes.NpgsqlDbType.Varchar, cancellationToken);

            // Write geometry as WKT and let PostGIS convert it
            var point = new NetTopologySuite.Geometries.Point(poi.Lon, poi.Lat) { SRID = 4326 };
            await writer.WriteAsync(point, NpgsqlTypes.NpgsqlDbType.Geometry, cancellationToken);

            await writer.WriteAsync(poi.Street, NpgsqlTypes.NpgsqlDbType.Varchar, cancellationToken);
            await writer.WriteAsync(poi.HouseNumber, NpgsqlTypes.NpgsqlDbType.Varchar, cancellationToken);
            await writer.WriteAsync(poi.PostalCode, NpgsqlTypes.NpgsqlDbType.Varchar, cancellationToken);
            await writer.WriteAsync(poi.City, NpgsqlTypes.NpgsqlDbType.Varchar, cancellationToken);
            await writer.WriteAsync(poi.Phone, NpgsqlTypes.NpgsqlDbType.Varchar, cancellationToken);
            await writer.WriteAsync(poi.Website, NpgsqlTypes.NpgsqlDbType.Varchar, cancellationToken);
            await writer.WriteAsync(poi.OpeningHours, NpgsqlTypes.NpgsqlDbType.Varchar, cancellationToken);
            await writer.WriteAsync(poi.OsmTags, NpgsqlTypes.NpgsqlDbType.Jsonb, cancellationToken);
        }

        await writer.CompleteAsync(cancellationToken);
    }

    /// <summary>
    /// Atomically swaps staging table with production table.
    /// Uses transaction to ensure consistency.
    /// </summary>
    public async Task SwapTablesAsync(CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
                // Check if original pois table exists
        await using var checkCmd = new NpgsqlCommand(
            "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'pois')",
            conn);
        var tableExists = (bool)(await checkCmd.ExecuteScalarAsync(cancellationToken))!;

        // Atomic table swap within a transaction
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);

        try
        {
            if (tableExists)
            {
                // Rename current table to old
                await using var renameOldCmd = new NpgsqlCommand(
                    "ALTER TABLE pois RENAME TO pois_old", conn, transaction);
                await renameOldCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            // Rename staging to production
            await using var renameNewCmd = new NpgsqlCommand(
                "ALTER TABLE pois_staging RENAME TO pois", conn, transaction);
            await renameNewCmd.ExecuteNonQueryAsync(cancellationToken);

            // Rename indexes to match expected names
            await using var renameIndexesCmd = new NpgsqlCommand("""
                ALTER INDEX idx_pois_staging_location RENAME TO idx_pois_location;
                ALTER INDEX idx_pois_staging_category RENAME TO idx_pois_category;
                ALTER INDEX idx_pois_staging_domain RENAME TO idx_pois_domain;
                ALTER INDEX idx_pois_staging_city RENAME TO idx_pois_city;
                """, conn, transaction);
            await renameIndexesCmd.ExecuteNonQueryAsync(cancellationToken);

            if (tableExists)
            {
                // Drop old table
                await using var dropOldCmd = new NpgsqlCommand(
                    "DROP TABLE pois_old CASCADE", conn, transaction);
                await dropOldCmd.ExecuteNonQueryAsync(cancellationToken);
            }

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Table swap completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Table swap failed, rolling back");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
