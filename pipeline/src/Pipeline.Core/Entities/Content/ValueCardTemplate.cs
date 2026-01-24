using Pipeline.Core.Enums;

namespace Pipeline.Core.Entities.Content;

/// <summary>
/// Configuration for value cards. Text generation is handled by ValueCardBuilder.
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
    /// Display order (lower = shown first).
    /// </summary>
    public int SortOrder { get; set; }
}
