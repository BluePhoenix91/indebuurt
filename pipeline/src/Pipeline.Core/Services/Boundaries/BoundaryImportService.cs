using Microsoft.Extensions.Logging;

namespace Pipeline.Core.Services.Boundaries;

/// <summary>
/// Orchestrates the boundary import: download → parse → stage → aggregate → finalize.
/// </summary>
public class BoundaryImportService(
    IBoundaryDownloader downloader,
    IGeoJsonSectorReader reader,
    ISlugGenerator slugGenerator,
    IBoundaryStagingRepository repository,
    ILogger<BoundaryImportService> logger) : IBoundaryImportService
{
    public async Task<BoundaryImportResult> ImportAsync(
        string? filePath = null,
        bool dryRun = false,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var warnings = new List<string>();

        // 1. Get current counts
        progress?.Report("Checking current data...");
        var (previousNeighborhoodCount, previousSectorCount) = await repository.GetCurrentCountsAsync(cancellationToken);

        if (previousNeighborhoodCount > 0)
        {
            progress?.Report($"Found {previousNeighborhoodCount:N0} existing neighborhoods and {previousSectorCount:N0} sectors");

            // Check if statistics data exists
            var statsCount = await repository.GetStatisticsCountAsync(cancellationToken);
            if (statsCount > 0)
            {
                var warning = $"Re-importing will replace {previousNeighborhoodCount:N0} neighborhoods. " +
                              $"The {statsCount:N0} rows in neighborhood_statistics will be dropped. " +
                              "Run 'import-statbel' afterwards to refresh statistics.";
                warnings.Add(warning);
                progress?.Report($"! {warning}");
            }
        }
        else
        {
            progress?.Report("No existing boundary data found (fresh import)");
        }

        // 2. Resolve source file
        progress?.Report("Resolving GeoJSON source...");
        var resolvedPath = await downloader.ResolveGeoJsonPathAsync(filePath, progress, cancellationToken);

        // 3. Parse GeoJSON
        var (totalFeatures, sectors) = await reader.ReadAsync(resolvedPath, progress, cancellationToken);

        if (sectors.Count == 0)
        {
            throw new InvalidOperationException(
                "No sectors found after filtering. The GeoJSON file may be empty, " +
                "have an unexpected format, or contain no Flemish/Brussels data.");
        }

        // 4. Dry run → return preview
        if (dryRun)
        {
            // Compute expected neighborhood count from sector NIS code prefixes
            var expectedNeighborhoods = sectors
                .Select(s => s.CdSector[..7])
                .Distinct()
                .Count();

            progress?.Report("Dry run complete.");

            return new BoundaryImportResult(
                TotalFeaturesInFile: totalFeatures,
                SectorsAfterFilter: sectors.Count,
                SectorsImported: 0,
                NeighborhoodsCreated: expectedNeighborhoods,
                PreviousNeighborhoodCount: previousNeighborhoodCount,
                PreviousSectorCount: previousSectorCount,
                Warnings: warnings);
        }

        // 5. Create staging table
        progress?.Report("Creating staging table...");
        await repository.CreateStagingTableAsync(cancellationToken);

        // 6. Bulk insert sectors
        progress?.Report($"Inserting {sectors.Count:N0} sectors into staging table...");
        await repository.BulkInsertSectorsAsync(sectors, cancellationToken);

        // 7. Compute neighborhood slugs
        progress?.Report("Computing neighborhood slugs...");
        var neighborhoodMetadata = await repository.GetNeighborhoodMetadataAsync(cancellationToken);
        var neighborhoodSlugs = slugGenerator.GenerateNeighborhoodSlugs(neighborhoodMetadata);

        // 8. Aggregate sectors into neighborhoods (PostGIS ST_Union)
        progress?.Report($"Aggregating {sectors.Count:N0} sectors into {neighborhoodMetadata.Count:N0} neighborhoods (this may take a minute)...");
        var neighborhoodsCreated = await repository.CreateAndPopulateNeighborhoodsAsync(neighborhoodSlugs, cancellationToken);

        // 9. Apply municipality merger mapping
        progress?.Report("Applying 2025 municipality merger mapping...");
        await repository.ApplyMunicipalityMappingAsync(cancellationToken);

        // 10. Compute sector slugs and create statistical_sectors table
        progress?.Report("Computing sector slugs...");
        var sectorMetadata = await repository.GetSectorMetadataAsync(cancellationToken);
        var sectorSlugs = slugGenerator.GenerateSectorSlugs(sectorMetadata);

        progress?.Report($"Creating {sectorMetadata.Count:N0} statistical sectors with transformed geometries...");
        var sectorsImported = await repository.CreateAndPopulateStatisticalSectorsAsync(sectorSlugs, cancellationToken);

        // 11. Create neighborhood_statistics table (empty, for O2)
        progress?.Report("Ensuring neighborhood_statistics table exists...");
        await repository.CreateNeighborhoodStatisticsTableAsync(cancellationToken);

        // 12. Clean up staging
        progress?.Report("Cleaning up staging table...");
        await repository.DropStagingTableAsync(cancellationToken);

        progress?.Report("Import complete.");

        logger.LogInformation(
            "Boundary import complete: {Neighborhoods} neighborhoods, {Sectors} sectors from {Total} features",
            neighborhoodsCreated, sectorsImported, totalFeatures);

        return new BoundaryImportResult(
            TotalFeaturesInFile: totalFeatures,
            SectorsAfterFilter: sectors.Count,
            SectorsImported: sectorsImported,
            NeighborhoodsCreated: neighborhoodsCreated,
            PreviousNeighborhoodCount: previousNeighborhoodCount,
            PreviousSectorCount: previousSectorCount,
            Warnings: warnings);
    }
}
