using Microsoft.Extensions.Logging;
using Pipeline.Core.Services.PoiImport;

namespace Pipeline.Core.Services;

/// <summary>
/// Result of a POI import operation.
/// </summary>
public record PoiImportResult(
    int TotalImported,
    int PreviousCount,
    Dictionary<string, int> CountsByCategory,
    Dictionary<string, int> PreviousCountsByCategory,
    bool HasWarnings,
    List<string> Warnings);

/// <summary>
/// Service for importing POIs from Overpass API into the database.
/// Uses staging table and atomic swap for zero-downtime updates.
/// </summary>
public interface IPoiImportService
{
    /// <summary>
    /// Gets the list of available domain names.
    /// </summary>
    IReadOnlyList<string> AvailableDomains { get; }

    /// <summary>
    /// Imports POIs from Overpass API.
    /// </summary>
    /// <param name="domains">Domains to import, or null for all.</param>
    /// <param name="dryRun">If true, don't write to database.</param>
    /// <param name="force">If true, skip count validation warnings.</param>
    /// <param name="progress">Progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task<PoiImportResult> ImportAsync(
        IEnumerable<string>? domains = null,
        bool dryRun = false,
        bool force = false,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}

public class PoiImportService(
    IOverpassClient overpassClient,
    OverpassToPoisConverter converter,
    IPoiStagingRepository repository,
    ILogger<PoiImportService> logger) : IPoiImportService
{
    public IReadOnlyList<string> AvailableDomains => overpassClient.AvailableDomains;

    public async Task<PoiImportResult> ImportAsync(
        IEnumerable<string>? domains = null,
        bool dryRun = false,
        bool force = false,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        progress?.Report("Getting current POI counts...");
        var previousCounts = await repository.GetCurrentCountsAsync(cancellationToken);
        var previousTotal = previousCounts.Values.Sum();

        progress?.Report("Fetching POIs from Overpass API...");
        var domainResults = await overpassClient.FetchDomainsAsync(
            domains,
            new Progress<(string domain, int count)>(p =>
                progress?.Report($"  {p.domain}: {p.count:N0} elements")),
            cancellationToken);

        // Convert Overpass elements to POI records
        progress?.Report("Processing elements...");
        var pois = converter.Convert(domainResults);
        var newCounts = pois.GroupBy(p => p.Category).ToDictionary(g => g.Key, g => g.Count());
        var newTotal = pois.Count;

        // Validate counts
        var warnings = ValidateCounts(previousCounts, newCounts, force, progress);

        if (dryRun)
        {
            progress?.Report("Dry run - no changes written to database.");
            return new PoiImportResult(newTotal, previousTotal, newCounts, previousCounts, warnings.Count > 0, warnings);
        }

        // Write to database using staging table + atomic swap
        progress?.Report("Creating staging table...");
        await repository.CreateStagingTableAsync(cancellationToken);

        progress?.Report($"Inserting {newTotal:N0} POIs into staging table...");
        await repository.BulkInsertAsync(pois, cancellationToken);

        progress?.Report("Swapping tables...");
        await repository.SwapTablesAsync(cancellationToken);

        progress?.Report("Import complete.");
        return new PoiImportResult(newTotal, previousTotal, newCounts, previousCounts, warnings.Count > 0, warnings);
    }

    private List<string> ValidateCounts(
        Dictionary<string, int> previousCounts,
        Dictionary<string, int> newCounts,
        bool force,
        IProgress<string>? progress)
    {
        var warnings = new List<string>();

        foreach (var (category, newCount) in newCounts)
        {
            if (previousCounts.TryGetValue(category, out var oldCount) && oldCount > 0)
            {
                var dropPercent = (double)(oldCount - newCount) / oldCount * 100;
                if (dropPercent > 20)
                {
                    warnings.Add($"{category}: count dropped {dropPercent:F1}% ({oldCount:N0} → {newCount:N0})");
                }
            }
        }

        if (warnings.Count > 0 && !force)
        {
            foreach (var warning in warnings)
            {
                logger.LogWarning("Count validation warning: {Warning}", warning);
                progress?.Report($"  WARNING: {warning}");
            }
        }

        return warnings;
    }
}
