using Pipeline.Core.Enums;

namespace Pipeline.Core.Mappers;

/// <summary>
/// Maps enums to Font Awesome icon classes.
/// </summary>
public static class IconMapper
{
    /// <summary>
    /// Gets the Font Awesome icon class for a card type.
    /// </summary>
    public static string GetIcon(CardType cardType) => cardType switch
    {
        CardType.DogParks => "fa-solid fa-dog",
        CardType.Parks => "fa-solid fa-tree",
        CardType.Vets => "fa-solid fa-stethoscope",
        CardType.PetStores => "fa-solid fa-bone",
        CardType.Supermarkets => "fa-solid fa-cart-shopping",
        CardType.Transit => "fa-solid fa-bus",
        _ => "fa-solid fa-circle-question"
    };

    /// <summary>
    /// Gets the distance icon (walking by default).
    /// </summary>
    public static string GetDistanceIcon() => "fa-solid fa-person-walking";
}
