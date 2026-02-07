using Microsoft.Extensions.Logging;

namespace Pipeline.Core.Services.Statbel;

/// <summary>
/// Service for importing statistics from Statbel into the database.
/// Orchestrates download, parsing, and database operations.
/// </summary>
public interface IStatbelImportService
{
    /// <summary>
    /// Available dataset names for --dataset flag.
    /// </summary>
    IReadOnlyList<string> AvailableDatasets { get; }

    /// <summary>
    /// Import Statbel statistics.
    /// </summary>
    /// <param name="year">Target year (auto-detected if null)</param>
    /// <param name="dataset">Specific dataset to import (null = all)</param>
    /// <param name="dryRun">Preview without writing to database</param>
    /// <param name="progress">Progress reporter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<StatbelImportResult> ImportAsync(
        int? year = null,
        string? dataset = null,
        bool dryRun = false,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}

public class StatbelImportService : IStatbelImportService
{
    private readonly IStatbelDownloader _downloader;
    private readonly IPopulationDataParser _populationParser;
    private readonly IHousePriceDataParser _housePriceParser;
    private readonly IStatbelStagingRepository _repository;
    private readonly ILogger<StatbelImportService> _logger;

    public StatbelImportService(
        IStatbelDownloader downloader,
        IPopulationDataParser populationParser,
        IHousePriceDataParser housePriceParser,
        IStatbelStagingRepository repository,
        ILogger<StatbelImportService> logger)
    {
        _downloader = downloader;
        _populationParser = populationParser;
        _housePriceParser = housePriceParser;
        _repository = repository;
        _logger = logger;
    }

    public IReadOnlyList<string> AvailableDatasets { get; } = ["population", "house-prices"];

    public async Task<StatbelImportResult> ImportAsync(
        int? year = null,
        string? dataset = null,
        bool dryRun = false,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // Determine target year
        int targetYear;
        if (year.HasValue)
        {
            targetYear = year.Value;
            progress?.Report($"Using specified year: {targetYear}");
        }
        else
        {
            progress?.Report("Detecting latest population year...");
            targetYear = await _downloader.DetectLatestPopulationYearAsync(cancellationToken);
            progress?.Report($"Detected latest year: {targetYear}");
        }

        _logger.LogInformation("Starting Statbel import for year {Year}, dataset: {Dataset}, dryRun: {DryRun}",
            targetYear, dataset ?? "all", dryRun);

        // Get current counts for comparison
        var (currentPopCount, currentPriceCount) = await _repository.GetCurrentCountsAsync(targetYear, cancellationToken);
        _logger.LogInformation("Current counts for year {Year}: population={PopCount}, prices={PriceCount}",
            targetYear, currentPopCount, currentPriceCount);

        DatasetImportResult? populationResult = null;
        DatasetImportResult? housePriceResult = null;

        // Import population
        if (dataset is null or "population")
        {
            populationResult = await ImportPopulationAsync(targetYear, dryRun, progress, cancellationToken);
        }

        // Import house prices
        if (dataset is null or "house-prices")
        {
            housePriceResult = await ImportHousePricesAsync(targetYear, dryRun, progress, cancellationToken);
        }

        var result = new StatbelImportResult(targetYear, populationResult, housePriceResult);

        if (dryRun)
        {
            progress?.Report("[DRY RUN] No changes written to database.");
        }

        return result;
    }

    private async Task<DatasetImportResult> ImportPopulationAsync(
        int year,
        bool dryRun,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report("Downloading population data...");
        var filePath = await _downloader.DownloadPopulationAsync(year, progress, cancellationToken);

        progress?.Report("Parsing population data...");
        var data = _populationParser.Parse(filePath);

        var totalPopulation = data.Sum(d => d.Population);
        progress?.Report($"Parsed {data.Count:N0} neighborhoods, total population: {totalPopulation:N0}");

        if (dryRun)
        {
            progress?.Report("[DRY RUN] Would import population data");
            return new DatasetImportResult(data.Count, 0, 0, []);
        }

        progress?.Report("Merging population into database...");
        var result = await _repository.MergePopulationAsync(year, data, cancellationToken);

        progress?.Report($"Population: {result.NeighborhoodsUpdated:N0} neighborhoods updated, {result.NeighborhoodsSkipped:N0} skipped");

        return result;
    }

    private async Task<DatasetImportResult> ImportHousePricesAsync(
        int year,
        bool dryRun,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report("Downloading house price data...");
        var filePath = await _downloader.DownloadHousePricesAsync(progress, cancellationToken);

        progress?.Report("Parsing house price data...");
        var (data, detectedYear) = _housePriceParser.Parse(filePath, year);

        if (detectedYear != year)
        {
            progress?.Report($"Note: House price data uses year {detectedYear} (requested {year})");
        }

        if (data.Count > 0)
        {
            var minPrice = data.Min(d => d.MedianHousePrice);
            var maxPrice = data.Max(d => d.MedianHousePrice);
            progress?.Report($"Parsed {data.Count:N0} municipalities, price range: {minPrice:N0} - {maxPrice:N0} EUR");
        }

        if (dryRun)
        {
            progress?.Report("[DRY RUN] Would import house price data");
            return new DatasetImportResult(data.Count, 0, 0, []);
        }

        progress?.Report("Merging house prices into database...");
        var result = await _repository.MergeHousePricesAsync(year, data, cancellationToken);

        progress?.Report($"House prices: {result.NeighborhoodsUpdated:N0} neighborhoods updated");

        return result;
    }
}
