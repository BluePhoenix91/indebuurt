namespace Pipeline.Core.Entities.Content;

/// <summary>
/// AI-generated prose content for a neighborhood.
/// One row per neighborhood, keyed by NIS code.
/// </summary>
public class NeighborhoodProse
{
    /// <summary>
    /// Belgian NIS code (7 characters) identifying the neighborhood.
    /// Primary key.
    /// </summary>
    public required string NisCode { get; set; }

    /// <summary>
    /// URL-friendly slug (e.g., "gent-binnenstad").
    /// Must be unique.
    /// </summary>
    public required string Slug { get; set; }

    /// <summary>
    /// City name (e.g., "Gent").
    /// </summary>
    public required string City { get; set; }

    /// <summary>
    /// Neighborhood name (e.g., "Binnenstad").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Long-form AI-generated introduction text.
    /// </summary>
    public required string Intro { get; set; }

    /// <summary>
    /// Short subtitle/tagline for the neighborhood.
    /// </summary>
    public required string Subtitle { get; set; }

    /// <summary>
    /// Brand quality score from AI review (0-100).
    /// Nullable until reviewed.
    /// </summary>
    public decimal? QualityScore { get; set; }

    /// <summary>
    /// SEO quality score from AI review (0-100).
    /// Nullable until reviewed.
    /// </summary>
    public decimal? SeoQualityScore { get; set; }

    /// <summary>
    /// Version identifier of the prompt used for generation.
    /// </summary>
    public string? PromptVersion { get; set; }

    /// <summary>
    /// Timestamp when the prose was generated.
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Timestamp when manually modified.
    /// Null if never edited after generation.
    /// </summary>
    public DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Identifier of who made manual modifications.
    /// </summary>
    public string? ModifiedBy { get; set; }
}
