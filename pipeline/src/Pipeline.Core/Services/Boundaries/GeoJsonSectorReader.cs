using System.Text.Json;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Pipeline.Core.Services.Boundaries;

/// <summary>
/// Reads and filters statistical sector features from a Statbel GeoJSON file.
/// </summary>
public interface IGeoJsonSectorReader
{
    /// <summary>
    /// Read all sector features from a GeoJSON file, filtered to Flanders + Brussels.
    /// </summary>
    /// <param name="filePath">Path to the .geojson file (EPSG:31370)</param>
    /// <param name="progress">Progress reporter</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Total features read and filtered sector features</returns>
    Task<(int TotalFeatures, List<SectorFeature> Sectors)> ReadAsync(
        string filePath,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default);
}

public class GeoJsonSectorReader : IGeoJsonSectorReader
{
    private readonly ILogger<GeoJsonSectorReader> _logger;

    private static readonly HashSet<string> FlemishProvinces =
    [
        "Provincie Antwerpen",
        "Provincie Limburg",
        "Provincie Oost-Vlaanderen",
        "Provincie West-Vlaanderen",
        "Provincie Vlaams-Brabant"
    ];

    private const string BrusselsRegion = "Brussels Hoofdstedelijk Gewest";

    public GeoJsonSectorReader(ILogger<GeoJsonSectorReader> logger)
    {
        _logger = logger;
    }

    public async Task<(int TotalFeatures, List<SectorFeature> Sectors)> ReadAsync(
        string filePath,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reading GeoJSON from {Path}", filePath);
        progress?.Report($"Loading GeoJSON file ({new FileInfo(filePath).Length / 1024 / 1024} MB)...");

        // Load entire file into JsonDocument (acceptable for batch CLI; ~200 MB peak memory)
        await using var stream = File.OpenRead(filePath);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);

        var root = doc.RootElement;

        if (!root.TryGetProperty("features", out var featuresElement))
        {
            throw new InvalidOperationException("GeoJSON file does not contain a 'features' array");
        }

        var totalFeatures = featuresElement.GetArrayLength();
        progress?.Report($"Found {totalFeatures:N0} features in GeoJSON");

        var geoJsonReader = new GeoJsonReader();
        var sectors = new List<SectorFeature>();
        var skippedNoGeometry = 0;
        var skippedNoSectorCode = 0;

        foreach (var feature in featuresElement.EnumerateArray())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!feature.TryGetProperty("properties", out var props))
                continue;

            // Extract properties
            var cdSector = GetStringProperty(props, "cd_sector");
            if (string.IsNullOrEmpty(cdSector))
            {
                skippedNoSectorCode++;
                continue;
            }

            var province = GetStringProperty(props, "tx_prov_descr_nl");
            var region = GetStringProperty(props, "tx_rgn_descr_nl");

            // Filter: only Flanders + Brussels
            if (!IsIncluded(province, region))
                continue;

            var sectorName = GetStringProperty(props, "tx_sector_descr_nl") ?? cdSector;
            var cityName = GetStringProperty(props, "tx_munty_descr_nl") ?? "";

            // Parse geometry
            if (!feature.TryGetProperty("geometry", out var geometryElement))
            {
                skippedNoGeometry++;
                _logger.LogWarning("Feature {CdSector} has no geometry, skipping", cdSector);
                continue;
            }

            try
            {
                var geometry = ParseGeometry(geoJsonReader, geometryElement);
                if (geometry == null)
                {
                    skippedNoGeometry++;
                    continue;
                }

                // Ensure MultiPolygon (some sectors might be single Polygon)
                geometry = EnsureMultiPolygon(geometry);

                // Set SRID to source CRS
                geometry.SRID = 31370;

                // Convert to WKB for storage (SRID is set on Geometry object,
                // Npgsql handles SRID separately in binary protocol)
                var wkbWriter = new NetTopologySuite.IO.WKBWriter();
                var wkb = wkbWriter.Write(geometry);

                sectors.Add(new SectorFeature(
                    CdSector: cdSector,
                    SectorNameNl: sectorName,
                    CityNameNl: cityName,
                    ProvinceNl: province,
                    RegionNl: region,
                    GeometryWkb: wkb));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse geometry for sector {CdSector}, skipping", cdSector);
                skippedNoGeometry++;
            }
        }

        if (skippedNoSectorCode > 0)
            _logger.LogWarning("Skipped {Count} features without sector code", skippedNoSectorCode);
        if (skippedNoGeometry > 0)
            _logger.LogWarning("Skipped {Count} features without valid geometry", skippedNoGeometry);

        _logger.LogInformation(
            "Read {Total} features, filtered to {Count} sectors (Flanders + Brussels)",
            totalFeatures, sectors.Count);

        progress?.Report($"Filtered to {sectors.Count:N0} sectors (Flanders + Brussels)");

        return (totalFeatures, sectors);
    }

    private static bool IsIncluded(string? province, string? region)
    {
        if (province != null && FlemishProvinces.Contains(province))
            return true;
        if (region != null && region == BrusselsRegion)
            return true;
        return false;
    }

    private static string? GetStringProperty(JsonElement props, string name)
    {
        if (props.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
            return value.GetString();
        return null;
    }

    private static Geometry? ParseGeometry(GeoJsonReader reader, JsonElement geometryElement)
    {
        var geoJson = geometryElement.GetRawText();
        return reader.Read<Geometry>(geoJson);
    }

    private static Geometry EnsureMultiPolygon(Geometry geometry)
    {
        if (geometry is Polygon polygon)
        {
            return new MultiPolygon([polygon]) { SRID = geometry.SRID };
        }
        return geometry;
    }
}
