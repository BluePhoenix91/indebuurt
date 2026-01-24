using Pipeline.Core.Enums;

namespace Pipeline.Core.Entities.Content;

/// <summary>
/// Global template for generating value cards from GIS data.
/// One row per card type (~6 total).
/// </summary>
public class ValueCardTemplate
{
    /// <summary>
    /// Type of value card (e.g., DogParks, Vets).
    /// Primary key. Stored as string in database.
    /// </summary>
    public CardType CardType { get; set; }

    /// <summary>
    /// Display title for the card (e.g., "Hondenparken").
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Template for the description with placeholders.
    /// Example: "{count} hondenspeelweiden binnen bereik"
    /// Placeholders: {count}
    /// </summary>
    public required string DescriptionTemplate { get; set; }

    /// <summary>
    /// Template for the detail line with placeholders.
    /// Example: "Dichtstbijzijnde omheind op {nearest_minutes} min"
    /// Placeholders: {nearest_name}, {nearest_minutes}, {nearest_meters}
    /// </summary>
    public required string DetailTemplate { get; set; }

    /// <summary>
    /// Display order (lower = shown first).
    /// </summary>
    public int SortOrder { get; set; }
}
