namespace Pipeline.Core.Services;

/// <summary>
/// Configuration options for the Overpass API client.
/// </summary>
public class OverpassOptions
{
    public const string SectionName = "Overpass";

    /// <summary>
    /// Overpass API endpoint URL.
    /// </summary>
    public string BaseUrl { get; set; } = "https://overpass-api.de/api/interpreter";

    /// <summary>
    /// Bounding box for queries: "south,west,north,east" (Flanders + Brussels).
    /// </summary>
    public string Bbox { get; set; } = "50.68,2.54,51.51,5.92";

    /// <summary>
    /// HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// Delay between requests in milliseconds (rate limiting).
    /// </summary>
    public int DelayBetweenRequestsMs { get; set; } = 15000;
}
