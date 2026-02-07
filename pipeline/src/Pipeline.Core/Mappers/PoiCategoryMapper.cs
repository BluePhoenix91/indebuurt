using Pipeline.Core.Enums;

namespace Pipeline.Core.Mappers;

/// <summary>
/// Maps CardType to POI category strings used in GIS queries.
/// </summary>
public static class PoiCategoryMapper
{
    /// <summary>
    /// Gets the POI category to query from GIS database for this card type.
    /// Returns null for card types that don't map to a single POI category (e.g., Transit).
    /// </summary>
    public static string? GetPoiCategory(CardType cardType) => cardType switch
    {
        CardType.DogParks => "dog_park",
        CardType.Parks => "park",
        CardType.Vets => "vet",
        CardType.PetStores => "pet_store",
        CardType.Supermarkets => "supermarket",
        CardType.Transit => null, // Handled specially via GetTransitCategories
        _ => null
    };

    /// <summary>
    /// Gets the POI categories that count as transit stops.
    /// Used for aggregating transit card counts.
    /// </summary>
    public static string[] GetTransitCategories() =>
        ["bus_stop", "tram_stop", "train_station"];
}
