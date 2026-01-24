using System.Text.Json.Serialization;

namespace Pipeline.Core.Dtos;

/// <summary>
/// Output DTO for a generated value card.
/// Matches the Astro content collection schema.
/// </summary>
public class ValueCardOutput
{
    /// <summary>
    /// Font Awesome icon class (e.g., "fa-solid fa-dog").
    /// </summary>
    [JsonPropertyName("icon")]
    public required string Icon { get; set; }

    /// <summary>
    /// Card title (e.g., "Hondenparken").
    /// </summary>
    [JsonPropertyName("title")]
    public required string Title { get; set; }

    /// <summary>
    /// Distance display (e.g., "10 mins" or "50m").
    /// Empty string if not applicable.
    /// </summary>
    [JsonPropertyName("distance")]
    public required string Distance { get; set; }

    /// <summary>
    /// Distance icon (e.g., "fa-solid fa-person-walking").
    /// Null if distance is not shown.
    /// </summary>
    [JsonPropertyName("distanceIcon")]
    public string? DistanceIcon { get; set; }

    /// <summary>
    /// Main description (e.g., "5 hondenspeelweiden binnen bereik").
    /// </summary>
    [JsonPropertyName("description")]
    public required string Description { get; set; }

    /// <summary>
    /// Detail line (e.g., "Dichtstbijzijnde op 10 min lopen").
    /// </summary>
    [JsonPropertyName("detail")]
    public required string Detail { get; set; }
}
