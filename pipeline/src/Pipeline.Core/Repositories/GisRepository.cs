using Microsoft.EntityFrameworkCore;
using Pipeline.Core.Data;
using Pipeline.Core.Mappers;

namespace Pipeline.Core.Repositories;

/// <summary>
/// Repository for querying GIS data for value card generation.
/// Uses raw SQL to query materialized views and GIS tables.
/// </summary>
public class GisRepository : IGisRepository
{
    private readonly PipelineDbContext _db;

    public GisRepository(PipelineDbContext db)
    {
        _db = db;
    }

    public async Task<PoiCount?> GetPoiCountAsync(string nisCode, string category, CancellationToken cancellationToken = default)
    {
        var result = await _db.Database
            .SqlQuery<PoiCountResult>($"""
                SELECT poi_count AS "Count", nearest_distance_m AS "NearestDistanceMeters"
                FROM mv_neighborhood_poi_counts
                WHERE nis_code = {nisCode} AND category = {category}
                """)
            .FirstOrDefaultAsync(cancellationToken);

        return result is not null
            ? new PoiCount(result.Count, result.NearestDistanceMeters)
            : null;
    }

    public async Task<NearestPoi?> GetNearestPoiAsync(string nisCode, string category, CancellationToken cancellationToken = default)
    {
        var result = await _db.Database
            .SqlQuery<NearestPoiResult>($"""
                SELECT poi_id AS "PoiId", poi_name AS "Name", distance_m AS "DistanceMeters", is_inside AS "IsInside"
                FROM mv_neighborhood_nearest_pois
                WHERE nis_code = {nisCode} AND category = {category}
                """)
            .FirstOrDefaultAsync(cancellationToken);

        return result is not null
            ? new NearestPoi(result.PoiId, result.Name ?? "", result.DistanceMeters, result.IsInside)
            : null;
    }

    public async Task<int> GetTransitCountAsync(string nisCode, CancellationToken cancellationToken = default)
    {
        var transitCategories = PoiCategoryMapper.GetTransitCategories();

        // Sum counts across all transit categories
        var result = await _db.Database
            .SqlQuery<int>($"""
                SELECT COALESCE(SUM(poi_count), 0)::int
                FROM mv_neighborhood_poi_counts
                WHERE nis_code = {nisCode}
                  AND category = ANY({transitCategories})
                """)
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    public async Task<string?> GetContainingNeighborhoodNameAsync(int poiId, CancellationToken cancellationToken = default)
    {
        var result = await _db.Database
            .SqlQuery<string>($"""
                SELECT neighborhood_name
                FROM get_poi_neighborhood({poiId})
                """)
            .FirstOrDefaultAsync(cancellationToken);

        return result;
    }

    // Internal result types for SqlQuery mapping
    private record PoiCountResult(int Count, double? NearestDistanceMeters);
    private record NearestPoiResult(int PoiId, string? Name, double DistanceMeters, bool IsInside);
}
