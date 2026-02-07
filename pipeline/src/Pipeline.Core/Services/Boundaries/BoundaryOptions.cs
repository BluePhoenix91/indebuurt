namespace Pipeline.Core.Services.Boundaries;

/// <summary>
/// Configuration options for boundary data imports.
/// </summary>
public class BoundaryOptions
{
    public const string SectionName = "Boundaries";

    /// <summary>
    /// URL for the Statbel statistical sectors GeoJSON ZIP download.
    /// </summary>
    public string DownloadUrl { get; set; } =
        "https://statbel.fgov.be/sites/default/files/files/opendata/Statistische%20sectoren/sh_statbel_statistical_sectors_31370_20240101.geojson.zip";

    /// <summary>
    /// HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 300;
}
