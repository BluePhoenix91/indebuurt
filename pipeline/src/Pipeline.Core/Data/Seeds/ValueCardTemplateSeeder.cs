using Pipeline.Core.Entities.Content;
using Pipeline.Core.Enums;

namespace Pipeline.Core.Data.Seeds;

/// <summary>
/// Provides seed data for value card templates.
/// Templates provide config (title, sort order); text generation is in ValueCardBuilder.
/// </summary>
public static class ValueCardTemplateSeeder
{
    /// <summary>
    /// Returns the default set of value card templates.
    /// </summary>
    public static IEnumerable<ValueCardTemplate> GetTemplates() =>
    [
        new ValueCardTemplate { CardType = CardType.DogParks, Title = "Hondenparken", SortOrder = 1 },
        new ValueCardTemplate { CardType = CardType.Parks, Title = "Parken", SortOrder = 2 },
        new ValueCardTemplate { CardType = CardType.Vets, Title = "Dierenartsen", SortOrder = 3 },
        new ValueCardTemplate { CardType = CardType.PetStores, Title = "Dierenwinkels", SortOrder = 4 },
        new ValueCardTemplate { CardType = CardType.Supermarkets, Title = "Supermarkten", SortOrder = 5 },
        new ValueCardTemplate { CardType = CardType.Transit, Title = "Openbaar vervoer", SortOrder = 6 }
    ];
}
