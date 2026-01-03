# Query Examples for Researcher Agent

This document contains tested SQL query patterns for extracting neighborhood data.
All queries use the MCP PostgreSQL tool.

## Helper Functions

The database provides these helper functions for common operations:

### get_pois_in_neighborhood
Returns POIs within a neighborhood boundary.
```sql
SELECT * FROM get_pois_in_neighborhood(
  'gent-binnenstad',  -- neighborhood_id
  'vet'               -- category (optional, NULL for all)
);
-- Returns: poi_id, poi_name, poi_category, poi_domain, lat, lon, distance_m, osm_tags
```

### get_nearest_pois_to_neighborhood
Returns nearest POIs to neighborhood centroid (useful when nothing inside boundary).
```sql
SELECT * FROM get_nearest_pois_to_neighborhood(
  'gent-binnenstad',  -- neighborhood_id
  'vet',              -- category
  5                   -- limit (default 5)
);
-- Returns: poi_id, poi_name, poi_category, poi_domain, lat, lon, distance_m, osm_tags
```

### get_neighborhood_poi_summary
Returns POI counts and nearest distances per category.
```sql
SELECT * FROM get_neighborhood_poi_summary('gent-binnenstad');
-- Returns: out_category, out_domain, count_in_neighborhood, nearest_distance_m
```

### find_nearest_pois
Returns nearest POIs to any point (by lat/lon).
```sql
SELECT * FROM find_nearest_pois(
  51.0535,  -- latitude
  3.7282,   -- longitude
  'vet',    -- category
  5         -- limit
);
-- Returns: poi_id, poi_name, distance_meters
```

### count_pois_in_radius
Count POIs within radius of a point.
```sql
SELECT count_pois_in_radius(
  51.0535,  -- latitude
  3.7282,   -- longitude
  'vet',    -- category
  2000      -- radius_m (default 500)
);
-- Returns: integer count
```

---

## Common Query Patterns

### 1. Get Neighborhood Basic Info
```sql
SELECT
  id,
  name,
  city,
  province,
  area_km2,
  ST_Y(centroid) AS center_lat,
  ST_X(centroid) AS center_lon,
  ST_YMax(boundary) AS bbox_north,
  ST_YMin(boundary) AS bbox_south,
  ST_XMax(boundary) AS bbox_east,
  ST_XMin(boundary) AS bbox_west
FROM neighborhoods
WHERE id = 'gent-binnenstad';
```

### 2. Get POIs with Address Details
```sql
SELECT
  p.id,
  p.name,
  p.category,
  p.street,
  p.house_number,
  p.postal_code,
  p.city AS municipality,
  ST_Y(p.location) AS lat,
  ST_X(p.location) AS lon,
  ST_Distance(
    p.location::geography,
    n.centroid::geography
  ) AS distance_meters,
  p.osm_tags
FROM pois p
CROSS JOIN (SELECT centroid FROM neighborhoods WHERE id = 'gent-binnenstad') n
WHERE p.category = 'vet'
AND ST_DWithin(
  p.location::geography,
  n.centroid::geography,
  3000  -- radius in meters
)
ORDER BY distance_meters
LIMIT 10;
```

### 3. Get POI Counts for All Categories
```sql
SELECT
  category,
  COUNT(*) AS count
FROM pois
WHERE ST_Contains(
  (SELECT boundary FROM neighborhoods WHERE id = 'gent-binnenstad'),
  location
)
GROUP BY category;
```

### 4. Get Neighboring Neighborhoods
```sql
SELECT
  n2.id,
  n2.name,
  ST_Distance(n1.centroid::geography, n2.centroid::geography) AS distance_meters
FROM neighborhoods n1
CROSS JOIN neighborhoods n2
WHERE n1.id = 'gent-binnenstad'
AND n2.id != n1.id
AND n1.city = n2.city
ORDER BY distance_meters
LIMIT 5;
```

### 5. Get Statistics for Neighborhood
```sql
SELECT
  ns.population AS inhabitants,
  ns.median_house_price,
  ns.population_density
FROM neighborhood_statistics ns
WHERE ns.wijk_id = 'gent-binnenstad'
AND ns.year = (SELECT MAX(year) FROM neighborhood_statistics WHERE wijk_id = 'gent-binnenstad');
```

### 6. Count Neighborhoods in City
```sql
SELECT COUNT(*) AS total_neighborhoods
FROM neighborhoods
WHERE city = 'Gent';
```

### 7. Get Postal Code from Statistical Sectors
```sql
SELECT DISTINCT postal_code
FROM statistical_sectors
WHERE neighborhood_id = 'gent-binnenstad'
LIMIT 1;
```

---

## Adaptive Radius Strategy

When querying for POIs, use neighborhood size to determine search radius:

```sql
-- Calculate base radius from neighborhood area
SELECT
  CASE
    WHEN area_km2 < 1 THEN 1000    -- Small urban: 1km
    WHEN area_km2 < 3 THEN 1500    -- Medium: 1.5km
    WHEN area_km2 < 5 THEN 2000    -- Large: 2km
    ELSE 3000                       -- Very large/rural: 3km
  END AS base_radius_m
FROM neighborhoods
WHERE id = 'gent-binnenstad';
```

### Category-Specific Radius Multipliers
- `vet`: 1.5x base (vets are less common, willing to travel further)
- `pet_store`: 1.0x base
- `dog_park`: 1.2x base (important, worth slightly longer walk)
- `park`: 0.8x base (usually many nearby)
- `supermarket`: 1.0x base
- `pharmacy`: 1.0x base
- `school`: 1.0x base
- `bus_stop`: 0.5x base (should be very close)
- `train_station`: 2.0x base (regional connectivity, expect further)

---

## Walking Time Estimation

Convert distance to walking time using:
```
walking_minutes = ROUND(distance_meters / 80)
```

This assumes 5 km/h walking speed (80 meters per minute).
Mark times as "estimated" in output.
