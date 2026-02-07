using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Pipeline.Core.Services.Boundaries;

/// <summary>
/// Downloads Statbel statistical sectors GeoJSON from the web, with caching.
/// </summary>
public interface IBoundaryDownloader
{
    /// <summary>
    /// Resolve the GeoJSON file path: use the provided path, check cache, or download from Statbel.
    /// </summary>
    Task<string> ResolveGeoJsonPathAsync(
        string? explicitFilePath = null,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}

public class BoundaryDownloader : IBoundaryDownloader
{
    private readonly HttpClient _httpClient;
    private readonly BoundaryOptions _options;
    private readonly ILogger<BoundaryDownloader> _logger;
    private readonly string _cacheDir;

    public BoundaryDownloader(
        HttpClient httpClient,
        IOptions<BoundaryOptions> options,
        ILogger<BoundaryDownloader> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
        _cacheDir = Path.Combine(Path.GetTempPath(), "statbel_cache");
        Directory.CreateDirectory(_cacheDir);
    }

    public async Task<string> ResolveGeoJsonPathAsync(
        string? explicitFilePath,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        // If explicit path provided, use it directly
        if (!string.IsNullOrEmpty(explicitFilePath))
        {
            if (!File.Exists(explicitFilePath))
            {
                throw new FileNotFoundException(
                    $"GeoJSON file not found: {explicitFilePath}",
                    explicitFilePath);
            }

            progress?.Report($"Using file: {explicitFilePath}");
            return explicitFilePath;
        }

        // Check cache
        var cachedPath = FindCachedGeoJson();
        if (cachedPath != null && IsCacheFresh(cachedPath))
        {
            var fileName = Path.GetFileName(cachedPath);
            _logger.LogInformation("Using cached GeoJSON: {Path}", cachedPath);
            progress?.Report($"Using cached file: {fileName}");
            return cachedPath;
        }

        // Download from Statbel
        return await DownloadAndExtractAsync(progress, cancellationToken);
    }

    private async Task<string> DownloadAndExtractAsync(
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var url = _options.DownloadUrl;
        var zipFileName = Path.GetFileName(new Uri(url).LocalPath);
        var zipPath = Path.Combine(_cacheDir, zipFileName);

        _logger.LogInformation("Downloading boundary data from {Url}", url);
        progress?.Report($"Downloading from Statbel ({url})...");

        // Download ZIP — scope streams so the file is closed before extraction
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? -1;
            var totalMb = totalBytes > 0 ? totalBytes / 1024.0 / 1024.0 : 0;

            await using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            long bytesRead = 0;
            int read;
            var lastProgress = DateTime.MinValue;

            while ((read = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                bytesRead += read;

                if (DateTime.Now - lastProgress > TimeSpan.FromMilliseconds(500) && totalBytes > 0)
                {
                    var percent = (int)(bytesRead * 100 / totalBytes);
                    var mbRead = bytesRead / 1024.0 / 1024.0;
                    progress?.Report($"Downloaded {mbRead:F1} / {totalMb:F1} MB ({percent}%)");
                    lastProgress = DateTime.Now;
                }
            }
        }

        var fileSizeMb = new FileInfo(zipPath).Length / 1024.0 / 1024.0;
        progress?.Report($"Downloaded {fileSizeMb:F1} MB");

        // Extract GeoJSON from ZIP
        progress?.Report("Extracting GeoJSON from ZIP...");
        var geoJsonPath = ExtractGeoJson(zipPath);

        // Clean up ZIP
        File.Delete(zipPath);

        var extractedSizeMb = new FileInfo(geoJsonPath).Length / 1024.0 / 1024.0;
        _logger.LogInformation("GeoJSON extracted: {Path} ({Size:F1} MB)", geoJsonPath, extractedSizeMb);
        progress?.Report($"Extracted {extractedSizeMb:F0} MB GeoJSON");

        return geoJsonPath;
    }

    private string ExtractGeoJson(string zipPath)
    {
        using var archive = ZipFile.OpenRead(zipPath);

        var geoJsonEntry = archive.Entries.FirstOrDefault(e =>
            e.Name.EndsWith(".geojson", StringComparison.OrdinalIgnoreCase));

        if (geoJsonEntry == null)
        {
            throw new InvalidOperationException(
                $"No .geojson file found in ZIP archive. Contents: {string.Join(", ", archive.Entries.Select(e => e.Name))}");
        }

        var destPath = Path.Combine(_cacheDir, geoJsonEntry.Name);
        _logger.LogDebug("Extracting {EntryName} from ZIP", geoJsonEntry.Name);
        geoJsonEntry.ExtractToFile(destPath, overwrite: true);

        return destPath;
    }

    private string? FindCachedGeoJson()
    {
        if (!Directory.Exists(_cacheDir))
            return null;

        return Directory.GetFiles(_cacheDir, "*.geojson")
            .OrderByDescending(File.GetLastWriteTime)
            .FirstOrDefault();
    }

    private bool IsCacheFresh(string filePath)
    {
        var fileInfo = new FileInfo(filePath);
        var age = DateTime.Now - fileInfo.LastWriteTime;

        // Boundary data changes very rarely (annually at most).
        // Consider cache fresh for 30 days.
        var isFresh = age < TimeSpan.FromDays(30);

        if (!isFresh)
        {
            _logger.LogDebug("Cache file {Path} is {Age} old, will re-download", filePath, age);
        }

        return isFresh;
    }
}
