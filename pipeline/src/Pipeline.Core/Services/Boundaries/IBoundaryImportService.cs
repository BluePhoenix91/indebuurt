namespace Pipeline.Core.Services.Boundaries;

/// <summary>
/// Service for importing neighborhood boundaries from Statbel statistical sectors.
/// </summary>
public interface IBoundaryImportService
{
    /// <summary>
    /// Import boundaries from a GeoJSON file.
    /// </summary>
    /// <param name="filePath">Explicit file path, or null to download/use cache</param>
    /// <param name="dryRun">If true, don't write to database</param>
    /// <param name="progress">Progress reporter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<BoundaryImportResult> ImportAsync(
        string? filePath = null,
        bool dryRun = false,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}
