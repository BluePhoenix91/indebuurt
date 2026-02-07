using Microsoft.Extensions.Logging;
using Npgsql;

namespace Pipeline.Core.Services.Statbel;

/// <summary>
/// Repository for Statbel statistics staging and merge operations.
/// Uses temporary tables for year-specific data, then merges into main table.
/// </summary>
public interface IStatbelStagingRepository
{
    /// <summary>
    /// Get current statistics counts for comparison.
    /// </summary>
    Task<(int PopulationCount, int HousePriceCount)> GetCurrentCountsAsync(int year, CancellationToken cancellationToken);

    /// <summary>
    /// Merge population data for a specific year.
    /// Uses INSERT ... ON CONFLICT DO UPDATE.
    /// </summary>
    Task<DatasetImportResult> MergePopulationAsync(
        int year,
        List<NeighborhoodPopulation> data,
        CancellationToken cancellationToken);

    /// <summary>
    /// Merge house price data for a specific year.
    /// Updates existing rows that were created by population merge.
    /// </summary>
    Task<DatasetImportResult> MergeHousePricesAsync(
        int year,
        List<MunicipalityHousePrice> data,
        CancellationToken cancellationToken);
}

public class StatbelStagingRepository : IStatbelStagingRepository
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly ILogger<StatbelStagingRepository> _logger;

    public StatbelStagingRepository(string connectionString, ILogger<StatbelStagingRepository> logger)
    {
        _logger = logger;

        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        _dataSource = dataSourceBuilder.Build();
    }

    public async Task<(int PopulationCount, int HousePriceCount)> GetCurrentCountsAsync(int year, CancellationToken cancellationToken)
    {
        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);

        // Check if table exists
        await using var checkCmd = new NpgsqlCommand(
            "SELECT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'gis' AND table_name = 'neighborhood_statistics')",
            conn);
        var tableExists = (bool)(await checkCmd.ExecuteScalarAsync(cancellationToken))!;

        if (!tableExists)
        {
            _logger.LogInformation("neighborhood_statistics table does not exist yet");
            return (0, 0);
        }

        await using var countCmd = new NpgsqlCommand("""
            SELECT
                COUNT(*) FILTER (WHERE population IS NOT NULL) as pop_count,
                COUNT(*) FILTER (WHERE median_house_price IS NOT NULL) as price_count
            FROM gis.neighborhood_statistics
            WHERE year = @year
            """, conn);
        countCmd.Parameters.AddWithValue("year", year);

        await using var reader = await countCmd.ExecuteReaderAsync(cancellationToken);
        if (await reader.ReadAsync(cancellationToken))
        {
            return (reader.GetInt32(0), reader.GetInt32(1));
        }

        return (0, 0);
    }

    public async Task<DatasetImportResult> MergePopulationAsync(
        int year,
        List<NeighborhoodPopulation> data,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Merging {Count} population records for year {Year}", data.Count, year);

        var warnings = new List<string>();
        int updated = 0;
        int skipped = 0;

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create temp staging table
            await using var createCmd = new NpgsqlCommand("""
                CREATE TEMP TABLE pop_staging (
                    nis_code VARCHAR(7) PRIMARY KEY,
                    population INTEGER,
                    population_density DECIMAL(10,2)
                ) ON COMMIT DROP
                """, conn, transaction);
            await createCmd.ExecuteNonQueryAsync(cancellationToken);

            // Bulk insert to staging
            await using var writer = await conn.BeginBinaryImportAsync(
                "COPY pop_staging (nis_code, population, population_density) FROM STDIN (FORMAT BINARY)",
                cancellationToken);

            foreach (var row in data)
            {
                await writer.StartRowAsync(cancellationToken);
                await writer.WriteAsync(row.NisCode, NpgsqlTypes.NpgsqlDbType.Varchar, cancellationToken);
                await writer.WriteAsync(row.Population, NpgsqlTypes.NpgsqlDbType.Integer, cancellationToken);
                await writer.WriteAsync(row.PopulationDensity, NpgsqlTypes.NpgsqlDbType.Numeric, cancellationToken);
            }
            await writer.CompleteAsync(cancellationToken);

            // Merge into main table
            await using var mergeCmd = new NpgsqlCommand("""
                INSERT INTO gis.neighborhood_statistics (neighborhood_id, year, population, population_density, created_at, updated_at)
                SELECT n.id, @year, s.population, s.population_density, NOW(), NOW()
                FROM pop_staging s
                JOIN gis.neighborhoods n ON n.nis_code = s.nis_code
                ON CONFLICT (neighborhood_id, year) DO UPDATE SET
                    population = EXCLUDED.population,
                    population_density = EXCLUDED.population_density,
                    updated_at = NOW()
                """, conn, transaction);
            mergeCmd.Parameters.AddWithValue("year", year);
            updated = await mergeCmd.ExecuteNonQueryAsync(cancellationToken);

            // Find skipped records (NIS codes not in neighborhoods table)
            await using var skippedCmd = new NpgsqlCommand("""
                SELECT s.nis_code
                FROM pop_staging s
                LEFT JOIN gis.neighborhoods n ON n.nis_code = s.nis_code
                WHERE n.id IS NULL
                """, conn, transaction);
            await using var reader = await skippedCmd.ExecuteReaderAsync(cancellationToken);

            var skippedCodes = new List<string>();
            while (await reader.ReadAsync(cancellationToken))
            {
                skippedCodes.Add(reader.GetString(0));
                skipped++;
            }

            if (skippedCodes.Count > 0)
            {
                var sample = string.Join(", ", skippedCodes.Take(5));
                var message = skippedCodes.Count <= 5
                    ? $"Skipped {skippedCodes.Count} neighborhoods not in database: {sample}"
                    : $"Skipped {skippedCodes.Count} neighborhoods not in database (e.g., {sample})";
                warnings.Add(message);
                _logger.LogWarning("{Message}", message);
            }

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Population merge complete: {Updated} updated, {Skipped} skipped", updated, skipped);

            return new DatasetImportResult(data.Count, updated, skipped, warnings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Population merge failed, rolling back");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<DatasetImportResult> MergeHousePricesAsync(
        int year,
        List<MunicipalityHousePrice> data,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Merging {Count} house price records for year {Year}", data.Count, year);

        var warnings = new List<string>();
        int updated = 0;
        int skipped = 0;

        await using var conn = await _dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await conn.BeginTransactionAsync(cancellationToken);

        try
        {
            // Create temp staging table
            await using var createCmd = new NpgsqlCommand("""
                CREATE TEMP TABLE price_staging (
                    municipality_nis VARCHAR(5) PRIMARY KEY,
                    median_house_price INTEGER
                ) ON COMMIT DROP
                """, conn, transaction);
            await createCmd.ExecuteNonQueryAsync(cancellationToken);

            // Bulk insert to staging
            await using var writer = await conn.BeginBinaryImportAsync(
                "COPY price_staging (municipality_nis, median_house_price) FROM STDIN (FORMAT BINARY)",
                cancellationToken);

            foreach (var row in data)
            {
                await writer.StartRowAsync(cancellationToken);
                await writer.WriteAsync(row.MunicipalityNis, NpgsqlTypes.NpgsqlDbType.Varchar, cancellationToken);
                await writer.WriteAsync(row.MedianHousePrice, NpgsqlTypes.NpgsqlDbType.Integer, cancellationToken);
            }
            await writer.CompleteAsync(cancellationToken);

            // Update existing neighborhood_statistics rows
            // Join via neighborhoods.statbel_municipality_nis to handle 2025 mergers
            await using var updateCmd = new NpgsqlCommand("""
                UPDATE gis.neighborhood_statistics ns
                SET median_house_price = ps.median_house_price,
                    updated_at = NOW()
                FROM price_staging ps
                JOIN gis.neighborhoods n ON n.statbel_municipality_nis = ps.municipality_nis
                WHERE ns.neighborhood_id = n.id AND ns.year = @year
                """, conn, transaction);
            updateCmd.Parameters.AddWithValue("year", year);
            updated = await updateCmd.ExecuteNonQueryAsync(cancellationToken);

            // Find municipalities without any matching neighborhoods
            await using var unmatchedCmd = new NpgsqlCommand("""
                SELECT ps.municipality_nis
                FROM price_staging ps
                LEFT JOIN gis.neighborhoods n ON n.statbel_municipality_nis = ps.municipality_nis
                WHERE n.id IS NULL
                GROUP BY ps.municipality_nis
                """, conn, transaction);
            await using var reader = await unmatchedCmd.ExecuteReaderAsync(cancellationToken);

            var unmatchedMunicipalities = new List<string>();
            while (await reader.ReadAsync(cancellationToken))
            {
                unmatchedMunicipalities.Add(reader.GetString(0));
            }

            if (unmatchedMunicipalities.Count > 0)
            {
                skipped = unmatchedMunicipalities.Count;
                var sample = string.Join(", ", unmatchedMunicipalities.Take(5));
                var message = $"Skipped {unmatchedMunicipalities.Count} municipalities not matching any neighborhoods: {sample}";
                warnings.Add(message);
                _logger.LogWarning("{Message}", message);
            }

            // Find neighborhoods without house prices (expected for some small municipalities)
            await using var missingCmd = new NpgsqlCommand("""
                SELECT COUNT(*)
                FROM gis.neighborhood_statistics ns
                WHERE ns.year = @year AND ns.median_house_price IS NULL
                """, conn, transaction);
            missingCmd.Parameters.AddWithValue("year", year);
            var missingCount = (long)(await missingCmd.ExecuteScalarAsync(cancellationToken))!;

            if (missingCount > 0)
            {
                warnings.Add($"{missingCount} neighborhoods have no house price data (expected for small municipalities)");
            }

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("House price merge complete: {Updated} neighborhoods updated", updated);

            return new DatasetImportResult(data.Count, updated, skipped, warnings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "House price merge failed, rolling back");
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
