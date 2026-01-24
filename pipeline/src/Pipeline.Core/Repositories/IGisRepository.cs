namespace Pipeline.Core.Repositories;

/// <summary>
/// Repository for querying GIS data for value card generation.
/// </summary>
public interface IGisRepository
{
    /// <summary>
    /// Gets the POI count for a neighborhood and category from the materialized view.
    /// </summary>
    Task<PoiCount?> GetPoiCountAsync(string nisCode, string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the nearest POI for a neighborhood and category from the materialized view.
    /// </summary>
    Task<NearestPoi?> GetNearestPoiAsync(string nisCode, string category, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the total transit stop count (bus + tram + train) for a neighborhood.
    /// </summary>
    Task<int> GetTransitCountAsync(string nisCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the name of the neighborhood that contains a POI.
    /// Used to determine "in naburig [naam]" context.
    /// </summary>
    Task<string?> GetContainingNeighborhoodNameAsync(int poiId, CancellationToken cancellationToken = default);
}

/// <summary>
/// POI count data from the materialized view.
/// </summary>
/// <param name="Count">Number of POIs in the neighborhood for this category.</param>
/// <param name="NearestDistanceMeters">Distance to the nearest POI in meters (may be outside neighborhood).</param>
public record PoiCount(int Count, double? NearestDistanceMeters);

/// <summary>
/// Nearest POI data from the materialized view.
/// </summary>
/// <param name="PoiId">Database ID of the POI.</param>
/// <param name="Name">Name of the POI.</param>
/// <param name="DistanceMeters">Distance from neighborhood centroid in meters.</param>
/// <param name="IsInside">True if the POI is within the neighborhood boundary.</param>
public record NearestPoi(int PoiId, string Name, double DistanceMeters, bool IsInside);
