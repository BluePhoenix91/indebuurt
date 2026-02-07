using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Pipeline.Core.Services.Statbel;

/// <summary>
/// Downloads Statbel data files with caching and year detection.
/// </summary>
public interface IStatbelDownloader
{
    /// <summary>
    /// Detect the latest available population year by probing Statbel URLs.
    /// </summary>
    Task<int> DetectLatestPopulationYearAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Download population data for the specified year.
    /// Returns path to the extracted .txt file.
    /// </summary>
    Task<string> DownloadPopulationAsync(int year, IProgress<string>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Download house prices Excel file.
    /// Returns path to the .xlsx file.
    /// </summary>
    Task<string> DownloadHousePricesAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default);
}

public class StatbelDownloader : IStatbelDownloader
{
    private readonly HttpClient _httpClient;
    private readonly StatbelOptions _options;
    private readonly ILogger<StatbelDownloader> _logger;
    private readonly string _cacheDir;

    public StatbelDownloader(
        HttpClient httpClient,
        IOptions<StatbelOptions> options,
        ILogger<StatbelDownloader> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _cacheDir = Path.Combine(Path.GetTempPath(), "statbel");
        Directory.CreateDirectory(_cacheDir);
    }

    public async Task<int> DetectLatestPopulationYearAsync(CancellationToken cancellationToken = default)
    {
        var currentYear = DateTime.Now.Year;

        // Start from current year if we're past March (data usually published Q1)
        // Otherwise start from previous year
        var startYear = DateTime.Now.Month >= 4 ? currentYear : currentYear - 1;

        _logger.LogInformation("Detecting latest population year, starting from {StartYear}", startYear);

        for (var year = startYear; year >= startYear - 3; year--)
        {
            var url = GetPopulationUrl(year);
            _logger.LogDebug("Probing {Year}: {Url}", year, url);

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, url);
                using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Found population data for year {Year}", year);
                    return year;
                }

                _logger.LogDebug("Year {Year} returned {StatusCode}", year, response.StatusCode);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogDebug("Year {Year} probe failed: {Message}", year, ex.Message);
            }
        }

        throw new InvalidOperationException(
            $"Could not detect latest population year. Tried {startYear} to {startYear - 3}. " +
            $"URL template: {_options.PopulationUrlTemplate}");
    }

    public async Task<string> DownloadPopulationAsync(int year, IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var zipFileName = $"OPENDATA_SECTOREN_{year}.zip";
        var zipPath = Path.Combine(_cacheDir, zipFileName);
        var txtFileName = $"OPENDATA_SECTOREN_{year}.txt";
        var txtPath = Path.Combine(_cacheDir, txtFileName);

        // Check if already extracted
        if (File.Exists(txtPath) && IsCacheFresh(txtPath))
        {
            _logger.LogInformation("Using cached population file: {Path}", txtPath);
            progress?.Report($"Using cached file: {txtFileName}");
            return txtPath;
        }

        // Download ZIP
        var url = GetPopulationUrl(year);
        _logger.LogInformation("Downloading population data from {Url}", url);
        progress?.Report($"Downloading {url}");

        await DownloadFileAsync(url, zipPath, progress, cancellationToken);

        // Extract TXT from ZIP
        progress?.Report("Extracting ZIP file...");
        ExtractPopulationZip(zipPath, txtPath);

        // Clean up ZIP
        File.Delete(zipPath);

        _logger.LogInformation("Population data extracted to {Path}", txtPath);
        return txtPath;
    }

    public async Task<string> DownloadHousePricesAsync(IProgress<string>? progress = null, CancellationToken cancellationToken = default)
    {
        var fileName = "vastgoed_2010_9999.xlsx";
        var filePath = Path.Combine(_cacheDir, fileName);

        // Check cache
        if (File.Exists(filePath) && IsCacheFresh(filePath))
        {
            _logger.LogInformation("Using cached house prices file: {Path}", filePath);
            progress?.Report($"Using cached file: {fileName}");
            return filePath;
        }

        // Download
        _logger.LogInformation("Downloading house prices from {Url}", _options.HousePricesUrl);
        progress?.Report($"Downloading {_options.HousePricesUrl}");

        await DownloadFileAsync(_options.HousePricesUrl, filePath, progress, cancellationToken);

        _logger.LogInformation("House prices downloaded to {Path}", filePath);
        return filePath;
    }

    private string GetPopulationUrl(int year)
    {
        return _options.PopulationUrlTemplate.Replace("{year}", year.ToString());
    }

    private async Task DownloadFileAsync(string url, string destPath, IProgress<string>? progress, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        var totalMb = totalBytes > 0 ? totalBytes / 1024.0 / 1024.0 : 0;

        await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
        await using var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        long bytesRead = 0;
        int read;
        var lastProgress = DateTime.MinValue;

        while ((read = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            bytesRead += read;

            // Report progress every 500ms
            if (DateTime.Now - lastProgress > TimeSpan.FromMilliseconds(500) && totalBytes > 0)
            {
                var percent = (int)(bytesRead * 100 / totalBytes);
                var mbRead = bytesRead / 1024.0 / 1024.0;
                progress?.Report($"Downloaded {mbRead:F1} / {totalMb:F1} MB ({percent}%)");
                lastProgress = DateTime.Now;
            }
        }

        var fileSizeMb = new FileInfo(destPath).Length / 1024.0 / 1024.0;
        progress?.Report($"Downloaded {fileSizeMb:F1} MB");
        _logger.LogInformation("Downloaded {Size:F1} MB to {Path}", fileSizeMb, destPath);
    }

    private void ExtractPopulationZip(string zipPath, string txtPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);

        // Find the .txt file in the archive
        var txtEntry = archive.Entries.FirstOrDefault(e =>
            e.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase));

        if (txtEntry == null)
        {
            throw new InvalidOperationException(
                $"No .txt file found in ZIP archive. Contents: {string.Join(", ", archive.Entries.Select(e => e.Name))}");
        }

        _logger.LogDebug("Extracting {EntryName} from ZIP", txtEntry.Name);
        txtEntry.ExtractToFile(txtPath, overwrite: true);
    }

    private bool IsCacheFresh(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        var age = DateTime.Now - fileInfo.LastWriteTime;

        // Consider cache fresh if less than 24 hours old
        var isFresh = age < TimeSpan.FromHours(24);

        if (!isFresh)
        {
            _logger.LogDebug("Cache file {Path} is {Age} old, will re-download", filePath, age);
        }

        return isFresh;
    }
}
