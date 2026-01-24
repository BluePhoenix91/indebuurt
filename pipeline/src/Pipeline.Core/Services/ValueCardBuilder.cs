using Pipeline.Core.Dtos;
using Pipeline.Core.Entities.Content;
using Pipeline.Core.Enums;
using Pipeline.Core.Mappers;
using Pipeline.Core.Repositories;

namespace Pipeline.Core.Services;

/// <summary>
/// Builds value cards from templates and GIS data.
/// All text generation is situational based on location and distance.
/// </summary>
public class ValueCardBuilder(IGisRepository gisRepository)
{
    // Walking speed: 80 m/min = 4.8 km/h
    private const double WalkingMetersPerMinute = 80.0;

    // Biking speed: 250 m/min = 15 km/h
    private const double BikingMetersPerMinute = 250.0;

    // Travel mode thresholds (in walking minutes)
    private const int WalkingMaxMinutes = 15;   // < 15 min = walking
    private const int BikingMaxMinutes = 30;    // 15-30 min = biking, 30+ = car

    // Distance below which we show meters instead of minutes
    private const double ShortDistanceThresholdMeters = 100.0;

    /// <summary>
    /// Builds a value card for a neighborhood using the given template.
    /// </summary>
    public async Task<ValueCardOutput> BuildAsync(
        string nisCode,
        ValueCardTemplate template,
        CancellationToken cancellationToken = default)
    {
        if (template.CardType == CardType.Transit)
        {
            return await BuildTransitCardAsync(nisCode, template, cancellationToken);
        }

        return await BuildPoiCardAsync(nisCode, template, cancellationToken);
    }

    /// <summary>
    /// Builds all value cards for a neighborhood.
    /// </summary>
    public async Task<IReadOnlyList<ValueCardOutput>> BuildAllAsync(
        string nisCode,
        IEnumerable<ValueCardTemplate> templates,
        CancellationToken cancellationToken = default)
    {
        var results = new List<ValueCardOutput>();

        foreach (var template in templates.OrderBy(t => t.SortOrder))
        {
            var card = await BuildAsync(nisCode, template, cancellationToken);
            results.Add(card);
        }

        return results;
    }

    private async Task<ValueCardOutput> BuildPoiCardAsync(
        string nisCode,
        ValueCardTemplate template,
        CancellationToken cancellationToken)
    {
        var category = PoiCategoryMapper.GetPoiCategory(template.CardType)!;

        var poiCount = await gisRepository.GetPoiCountAsync(nisCode, category, cancellationToken);
        var count = poiCount?.Count ?? 0;
        var nearest = await gisRepository.GetNearestPoiAsync(nisCode, category, cancellationToken);

        // Determine location context for POIs outside neighborhood
        string? neighborhoodName = null;
        if (nearest is not null && !nearest.IsInside)
        {
            neighborhoodName = await gisRepository.GetContainingNeighborhoodNameAsync(nearest.PoiId, cancellationToken);
        }

        return BuildPoiOutput(template, count, nearest, neighborhoodName);
    }

    private async Task<ValueCardOutput> BuildTransitCardAsync(
        string nisCode,
        ValueCardTemplate template,
        CancellationToken cancellationToken)
    {
        var count = await gisRepository.GetTransitCountAsync(nisCode, cancellationToken);

        var (description, detail) = GetTransitText(count);

        return new ValueCardOutput
        {
            Icon = IconMapper.GetIcon(template.CardType),
            Title = template.Title,
            Distance = "",
            DistanceIcon = null,
            Description = description,
            Detail = detail
        };
    }

    private ValueCardOutput BuildPoiOutput(
        ValueCardTemplate template,
        int count,
        NearestPoi? nearest,
        string? neighborhoodName)
    {
        var distanceMeters = nearest?.DistanceMeters ?? 0;
        var walkingMinutes = distanceMeters > 0
            ? (int)Math.Ceiling(distanceMeters / WalkingMetersPerMinute)
            : 0;

        var travelMode = GetTravelMode(walkingMinutes);
        var (description, detail) = GetPoiText(template.CardType, count, nearest, neighborhoodName, walkingMinutes, travelMode);
        var (distance, distanceIcon) = FormatDistance(distanceMeters, travelMode);

        return new ValueCardOutput
        {
            Icon = IconMapper.GetIcon(template.CardType),
            Title = template.Title,
            Distance = distance,
            DistanceIcon = distanceIcon,
            Description = description,
            Detail = detail
        };
    }

    private static TravelMode GetTravelMode(int walkingMinutes)
    {
        if (walkingMinutes <= 0) return TravelMode.Walking;
        if (walkingMinutes < WalkingMaxMinutes) return TravelMode.Walking;
        if (walkingMinutes < BikingMaxMinutes) return TravelMode.Biking;
        return TravelMode.Car;
    }

    private static (string description, string detail) GetPoiText(
        CardType cardType,
        int count,
        NearestPoi? nearest,
        string? neighborhoodName,
        int walkingMinutes,
        TravelMode travelMode)
    {
        var isInside = nearest?.IsInside ?? false;

        // Case 1: No POIs found at all
        if (count == 0 && nearest is null)
        {
            return GetZeroPoiText(cardType);
        }

        // Case 2: POIs exist but none in neighborhood (nearest is outside)
        if (count == 0 && nearest is not null)
        {
            return GetNearbyOnlyText(cardType, nearest, neighborhoodName, walkingMinutes, travelMode);
        }

        // Case 3: POIs in neighborhood
        return GetInNeighborhoodText(cardType, count, nearest, walkingMinutes, travelMode);
    }

    private static (string description, string detail) GetZeroPoiText(CardType cardType)
    {
        return cardType switch
        {
            CardType.DogParks => ("Geen hondenspeelweide gevonden", "Niet binnen 30 minuten bereikbaar"),
            CardType.Parks => ("Geen parken gevonden", "Niet binnen 30 minuten bereikbaar"),
            CardType.Vets => ("Geen dierenarts gevonden", "Niet binnen 30 minuten bereikbaar"),
            CardType.PetStores => ("Geen dierenwinkel gevonden", "Niet binnen 30 minuten bereikbaar"),
            CardType.Supermarkets => ("Geen supermarkt gevonden", "Niet binnen 30 minuten bereikbaar"),
            _ => ("Niet beschikbaar", "")
        };
    }

    private static (string description, string detail) GetNearbyOnlyText(
        CardType cardType,
        NearestPoi nearest,
        string? neighborhoodName,
        int walkingMinutes,
        TravelMode travelMode)
    {
        var locationPhrase = neighborhoodName is not null
            ? $"in naburig {neighborhoodName}"
            : "in de buurt";

        var travelPhrase = FormatTravelPhrase(walkingMinutes, travelMode);

        var description = cardType switch
        {
            CardType.DogParks => $"Hondenspeelweide {locationPhrase}",
            CardType.Parks => $"Park {locationPhrase}",
            CardType.Vets => $"Dierenarts {locationPhrase}",
            CardType.PetStores => $"Dierenwinkel {locationPhrase}",
            CardType.Supermarkets => $"Supermarkt {locationPhrase}",
            _ => $"{locationPhrase}"
        };

        // For named POIs (vets, pet stores, supermarkets), include the name
        var detail = ShouldShowPoiName(cardType) && !string.IsNullOrEmpty(nearest.Name)
            ? $"{nearest.Name} op {travelPhrase}"
            : $"Dichtstbijzijnde op {travelPhrase}";

        return (description, detail);
    }

    private static (string description, string detail) GetInNeighborhoodText(
        CardType cardType,
        int count,
        NearestPoi? nearest,
        int walkingMinutes,
        TravelMode travelMode)
    {
        var countWord = GetCountWord(cardType, count);
        var description = $"{count} {countWord} in de wijk";

        string detail;
        if (nearest is not null && walkingMinutes > 0)
        {
            var travelPhrase = FormatTravelPhrase(walkingMinutes, travelMode);

            // For named POIs, include the name if available
            detail = ShouldShowPoiName(cardType) && !string.IsNullOrEmpty(nearest.Name)
                ? $"{nearest.Name} op {travelPhrase}"
                : $"Dichtstbijzijnde op {travelPhrase}";
        }
        else
        {
            detail = GetDefaultDetail(cardType);
        }

        return (description, detail);
    }

    private static string GetCountWord(CardType cardType, int count)
    {
        var plural = count != 1;
        return cardType switch
        {
            CardType.DogParks => plural ? "hondenspeelweiden" : "hondenspeelweide",
            CardType.Parks => plural ? "parken" : "park",
            CardType.Vets => plural ? "dierenartsen" : "dierenarts",
            CardType.PetStores => plural ? "dierenwinkels" : "dierenwinkel",
            CardType.Supermarkets => plural ? "supermarkten" : "supermarkt",
            _ => ""
        };
    }

    private static bool ShouldShowPoiName(CardType cardType)
    {
        // Parks and dog parks often don't have meaningful names
        return cardType is CardType.Vets or CardType.PetStores or CardType.Supermarkets;
    }

    private static string GetDefaultDetail(CardType cardType)
    {
        return cardType switch
        {
            CardType.DogParks => "Hondenspeelweide dichtbij",
            CardType.Parks => "Groen op loopafstand",
            CardType.Vets => "Dierenarts dichtbij",
            CardType.PetStores => "Dierenwinkel dichtbij",
            CardType.Supermarkets => "Dagelijkse boodschappen dichtbij",
            _ => ""
        };
    }

    private static string FormatTravelPhrase(int walkingMinutes, TravelMode travelMode)
    {
        return travelMode switch
        {
            TravelMode.Walking => $"{walkingMinutes} min lopen",
            TravelMode.Biking => $"{ConvertToBikingMinutes(walkingMinutes)} min fietsen",
            TravelMode.Car => $"{ConvertToCarMinutes(walkingMinutes)} min rijden",
            _ => $"{walkingMinutes} min"
        };
    }

    private static int ConvertToBikingMinutes(int walkingMinutes)
    {
        // Walking is 80 m/min, biking is 250 m/min (3.125x faster)
        return Math.Max(1, (int)Math.Ceiling(walkingMinutes * WalkingMetersPerMinute / BikingMetersPerMinute));
    }

    private static int ConvertToCarMinutes(int walkingMinutes)
    {
        // Approximate: car is about 6x faster than walking in urban areas (30 km/h vs 5 km/h)
        return Math.Max(1, (int)Math.Ceiling(walkingMinutes / 6.0));
    }

    private static (string description, string detail) GetTransitText(int count)
    {
        var description = count switch
        {
            0 => "Geen haltes in de wijk",
            1 => "1 halte in de wijk",
            _ => $"{count} haltes in de wijk"
        };

        var detail = count switch
        {
            0 => "Beperkt openbaar vervoer",
            1 => "1 halte beschikbaar",
            < 10 => "Redelijke bereikbaarheid",
            < 20 => "Goede bereikbaarheid",
            _ => "Uitstekende verbindingen"
        };

        return (description, detail);
    }

    private static (string distance, string? icon) FormatDistance(double meters, TravelMode travelMode)
    {
        if (meters <= 0)
            return ("", null);

        if (meters < ShortDistanceThresholdMeters)
            return ($"{(int)meters}m", IconMapper.GetDistanceIcon(TravelMode.Walking));

        var walkingMinutes = (int)Math.Ceiling(meters / WalkingMetersPerMinute);

        return travelMode switch
        {
            TravelMode.Walking => ($"{walkingMinutes} mins", IconMapper.GetDistanceIcon(TravelMode.Walking)),
            TravelMode.Biking => ($"{ConvertToBikingMinutes(walkingMinutes)} mins", IconMapper.GetDistanceIcon(TravelMode.Biking)),
            TravelMode.Car => ($"{ConvertToCarMinutes(walkingMinutes)} mins", IconMapper.GetDistanceIcon(TravelMode.Car)),
            _ => ($"{walkingMinutes} mins", IconMapper.GetDistanceIcon(TravelMode.Walking))
        };
    }
}

public enum TravelMode
{
    Walking,
    Biking,
    Car
}
