using System.Text.Json;
using Microsoft.Extensions.Logging;
using Pipeline.Core.Dtos.Overpass;

namespace Pipeline.Core.Services.PoiImport;

/// <summary>
/// Converts Overpass API elements to POI records.
/// </summary>
public class OverpassToPoisConverter(ILogger<OverpassToPoisConverter> logger)
{
    // OSM tag → (category, domain) mapping
    // This matches transform-staging-to-pois.sql exactly
    private static readonly Dictionary<(string tagKey, string tagValue), (string category, string domain)> TagMapping = new()
    {
        [("amenity", "veterinary")] = ("vet", "pets"),
        [("shop", "pet")] = ("pet_store", "pets"),
        [("leisure", "dog_park")] = ("dog_park", "pets"),
        [("shop", "supermarket")] = ("supermarket", "shopping"),
        [("amenity", "pharmacy")] = ("pharmacy", "healthcare"),
        [("amenity", "school")] = ("school", "education"),
        [("highway", "bus_stop")] = ("bus_stop", "transport"),
        [("public_transport", "platform")] = ("bus_stop", "transport"),
        [("railway", "station")] = ("train_station", "transport"),
        [("railway", "halt")] = ("train_station", "transport"),
        [("leisure", "park")] = ("park", "green")
    };

    /// <summary>
    /// Converts Overpass elements grouped by domain to POI records.
    /// Deduplicates by OSM ID across domains.
    /// </summary>
    public List<PoiRecord> Convert(Dictionary<string, OverpassElement[]> domainResults)
    {
        var pois = new List<PoiRecord>();
        var seenOsmIds = new HashSet<long>();

        foreach (var (domain, elements) in domainResults)
        {
            foreach (var element in elements)
            {
                // Skip duplicates (same element might appear in multiple domains)
                if (!seenOsmIds.Add(element.Id))
                    continue;

                // Get coordinates (nodes have lat/lon, ways/relations have center)
                var (lat, lon) = GetCoordinates(element);
                if (lat is null || lon is null)
                {
                    logger.LogDebug("Skipping element {Id} with no coordinates", element.Id);
                    continue;
                }

                // Determine category and domain from tags
                var (category, poiDomain) = GetCategoryAndDomain(element.Tags);
                if (category is null)
                {
                    logger.LogDebug("Skipping element {Id} with no matching category", element.Id);
                    continue;
                }

                pois.Add(new PoiRecord(
                    OsmId: element.Id,
                    Name: element.Tags?.GetValueOrDefault("name"),
                    Category: category,
                    Domain: poiDomain,
                    Lat: lat.Value,
                    Lon: lon.Value,
                    Street: element.Tags?.GetValueOrDefault("addr:street"),
                    HouseNumber: element.Tags?.GetValueOrDefault("addr:housenumber"),
                    PostalCode: element.Tags?.GetValueOrDefault("addr:postcode"),
                    City: element.Tags?.GetValueOrDefault("addr:city"),
                    Phone: element.Tags?.GetValueOrDefault("phone") ?? element.Tags?.GetValueOrDefault("contact:phone"),
                    Website: element.Tags?.GetValueOrDefault("website") ?? element.Tags?.GetValueOrDefault("contact:website"),
                    OpeningHours: element.Tags?.GetValueOrDefault("opening_hours"),
                    OsmTags: element.Tags is not null ? JsonSerializer.Serialize(element.Tags) : null
                ));
            }
        }

        return pois;
    }

    /// <summary>
    /// Extracts coordinates from an Overpass element.
    /// Nodes have direct lat/lon, ways/relations have center from "out center".
    /// </summary>
    public static (double? lat, double? lon) GetCoordinates(OverpassElement element)
    {
        if (element.Lat.HasValue && element.Lon.HasValue)
            return (element.Lat, element.Lon);

        if (element.Center is not null)
            return (element.Center.Lat, element.Center.Lon);

        return (null, null);
    }

    /// <summary>
    /// Maps OSM tags to internal category and domain.
    /// Returns first matching tag from the mapping dictionary.
    /// </summary>
    public static (string? category, string? domain) GetCategoryAndDomain(Dictionary<string, string>? tags)
    {
        if (tags is null)
            return (null, null);

        // Check each tag against our mapping
        foreach (var (key, value) in tags)
        {
            if (TagMapping.TryGetValue((key, value), out var mapping))
                return mapping;
        }

        return (null, null);
    }
}

/// <summary>
/// A POI record ready for database insertion.
/// </summary>
public record PoiRecord(
    long OsmId,
    string? Name,
    string Category,
    string? Domain,
    double Lat,
    double Lon,
    string? Street,
    string? HouseNumber,
    string? PostalCode,
    string? City,
    string? Phone,
    string? Website,
    string? OpeningHours,
    string? OsmTags);
