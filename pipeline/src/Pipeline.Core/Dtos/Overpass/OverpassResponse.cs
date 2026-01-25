using System.Text.Json.Serialization;

namespace Pipeline.Core.Dtos.Overpass;

/// <summary>
/// Root response from Overpass API.
/// </summary>
public class OverpassResponse
{
    [JsonPropertyName("elements")]
    public OverpassElement[] Elements { get; set; } = [];
}

/// <summary>
/// An OSM element (node, way, or relation) from Overpass API.
/// </summary>
public class OverpassElement
{
    /// <summary>
    /// Element type: "node", "way", or "relation".
    /// </summary>
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// OpenStreetMap ID (unique within type).
    /// </summary>
    [JsonPropertyName("id")]
    public long Id { get; set; }

    /// <summary>
    /// Latitude (for nodes).
    /// </summary>
    [JsonPropertyName("lat")]
    public double? Lat { get; set; }

    /// <summary>
    /// Longitude (for nodes).
    /// </summary>
    [JsonPropertyName("lon")]
    public double? Lon { get; set; }

    /// <summary>
    /// Center point (for ways/relations when using "out center").
    /// </summary>
    [JsonPropertyName("center")]
    public OverpassCenter? Center { get; set; }

    /// <summary>
    /// OSM tags (key-value pairs).
    /// </summary>
    [JsonPropertyName("tags")]
    public Dictionary<string, string>? Tags { get; set; }
}

/// <summary>
/// Center point for ways/relations.
/// </summary>
public class OverpassCenter
{
    [JsonPropertyName("lat")]
    public double Lat { get; set; }

    [JsonPropertyName("lon")]
    public double Lon { get; set; }
}
