namespace Pipeline.Core.Enums;

/// <summary>
/// Categories for grouping neighborhood labels.
/// Used to determine display order in the frontend.
/// </summary>
public enum LabelCategory
{
    Character,      // "Historisch centrum", "Stedelijk", "Landelijk"
    Amenities,      // "Veel groen", "Veel winkels"
    Transport,      // "Goed bereikbaar", "Dicht bij station"
    Demographics    // "Jonge wijk", "Gezinswijk"
}
