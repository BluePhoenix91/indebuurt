using System.Text.Json.Serialization;

namespace Pipeline.Cli.Dtos;

/// <summary>
/// DTO for deserializing 4-brand-reviewer.json files.
/// Only includes fields needed for NeighborhoodProse migration.
/// </summary>
public class BrandReviewerOutputDto
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("city")]
    public required string City { get; set; }

    [JsonPropertyName("name")]
    public required string Name { get; set; }

    [JsonPropertyName("intro")]
    public required string Intro { get; set; }

    [JsonPropertyName("subtitle")]
    public required string Subtitle { get; set; }

    [JsonPropertyName("generatedAt")]
    public DateTime GeneratedAt { get; set; }

    [JsonPropertyName("schemaVersion")]
    public string? SchemaVersion { get; set; }

    [JsonPropertyName("seoReview")]
    public ReviewDto? SeoReview { get; set; }

    [JsonPropertyName("brandReview")]
    public ReviewDto? BrandReview { get; set; }

    public class ReviewDto
    {
        [JsonPropertyName("qualityScore")]
        public decimal? QualityScore { get; set; }
    }
}
