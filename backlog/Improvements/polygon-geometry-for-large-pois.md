# Improvement: Store Polygon Geometry for Large POIs

## Summary

Currently, all POIs are stored as center points regardless of their actual shape. Large features like parks, schools, and dog parks that span multiple neighborhoods are only associated with the neighborhood containing their centroid. This improvement would store full polygon geometry for ways/relations, enabling more accurate spatial queries.

## Current Behavior

- Overpass queries use `out center tags` returning only centroids
- Database stores `location GEOMETRY(Point, 4326)`
- A park straddling two neighborhoods appears in only one
- Distance calculations measure to center, not nearest edge

## Proposed Behavior

- Overpass queries use `out geom` for polygon data
- Database adds `boundary GEOMETRY(Polygon, 4326)` column (nullable)
- Queries use `ST_Intersects()` to find all neighborhoods a POI touches
- Distance uses `ST_Distance()` to polygon edge for large features

## Impact Analysis

### Changes Required

| Component | Change |
|-----------|--------|
| `OverpassClient.cs` | Change query suffix from `out center tags` to `out geom tags` |
| `OverpassElement` DTO | Add `Nodes[]` array for way geometry |
| `PoiImportService.cs` | Build polygons from node arrays |
| Database schema | Add nullable `boundary` column |
| Spatial queries | Update to use `ST_Intersects()` when boundary exists |

### Trade-offs

| Consideration | Impact |
|---------------|--------|
| Storage | ~10x larger for polygon-heavy domains (parks, schools) |
| API response size | More data per Overpass request |
| Import time | Slightly longer due to larger payloads |
| Query complexity | Need to handle both point and polygon cases |

## Why Not Now

- Current center-point approach is accurate for most POIs (shops, pharmacies, bus stops)
- Only large features (parks, schools) benefit from polygon precision
- Adding later requires only one re-import (~2 minutes)
- MVP can validate without this precision

## When to Consider

- When neighborhood boundary accuracy becomes a user-facing feature
- When "POIs near me" queries need edge-distance precision
- When POI-to-neighborhood relationships are used for scoring

## Related

- Story O1: OSM POI Import Command (current implementation)
- `pois` table schema
- Materialized views using POI data
