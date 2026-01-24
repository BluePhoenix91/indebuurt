using Pipeline.Core.Enums;

namespace Pipeline.Core.Mappers;

/// <summary>
/// Maps CardType to POI category strings used in GIS queries.
/// </summary>
public static class PoiCategoryMapper
{
    /// <summary>
    /// Gets the POI category to query from GIS database for this card type.
    /// Returns null for card types that don't map to POIs (e.g., Transit).
    /// </summary>
    public static string? GetPoiCategory(CardType cardType) => cardType switch
    {
        CardType.DogParks => "dog_park",
        CardType.Parks => "park",
        CardType.Vets => "veterinary",
        CardType.PetStores => "pet_shop",
        CardType.Supermarkets => "supermarket",
        CardType.Transit => null,
        _ => null
    };
}
