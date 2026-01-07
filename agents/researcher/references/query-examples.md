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

## Dog Park Feature Inference

Dog parks in OSM have sparse tagging (~27% have fence info, ~11% have surface info).
Use this query to extract features with tag-based AND name-based inference:

```sql
-- Extract dog park features with tag + name inference
SELECT
  p.osm_id,
  COALESCE(p.name, 'Unnamed') as name,
  ST_Y(p.location) AS lat,
  ST_X(p.location) AS lon,
  ROUND(ST_Distance(p.location::geography, n.centroid::geography)) AS distance_meters,

  -- Fenced: tags first, then name inference
  CASE
    WHEN p.osm_tags->>'barrier' IN ('fence', 'hedge') THEN true
    WHEN p.osm_tags->>'fenced' = 'yes' THEN true
    WHEN p.osm_tags->>'fence' = 'yes' THEN true
    WHEN p.osm_tags->>'fence_type' IS NOT NULL THEN true
    WHEN LOWER(p.name) LIKE '%losloop%' THEN true
    WHEN LOWER(p.name) LIKE '%hondenweide%' THEN true
    WHEN LOWER(p.name) LIKE '%hondenspeelweide%' THEN true
    WHEN LOWER(p.name) LIKE '%hondenspeelzone%' THEN true
    WHEN LOWER(p.name) LIKE '%hondenpark%' THEN true
    WHEN LOWER(p.name) LIKE '%hondenzone%' THEN true
    WHEN LOWER(p.name) LIKE '%vrijheidszone%' THEN true
    ELSE false
  END AS is_fenced,

  -- Surface: tags first, then name inference
  COALESCE(
    p.osm_tags->>'surface',
    CASE p.osm_tags->>'landuse'
      WHEN 'grass' THEN 'grass'
      WHEN 'meadow' THEN 'grass'
      WHEN 'forest' THEN 'mixed'
    END,
    CASE p.osm_tags->>'landcover' WHEN 'grass' THEN 'grass' END,
    CASE p.osm_tags->>'natural' WHEN 'sand' THEN 'sand' END,
    CASE
      WHEN LOWER(p.name) LIKE '%weide%' THEN 'grass'
      WHEN LOWER(p.name) LIKE '%bos%' THEN 'mixed'
    END
  ) AS surface,

  -- Water: tags first, then name/description
  CASE
    WHEN p.osm_tags->>'swimming:dog' = 'yes' THEN true
    WHEN LOWER(COALESCE(p.name, '')) LIKE '%water%' THEN true
    WHEN LOWER(COALESCE(p.name, '')) LIKE '%zwem%' THEN true
    WHEN LOWER(COALESCE(p.osm_tags->>'description', '')) LIKE '%water%' THEN true
    WHEN LOWER(COALESCE(p.osm_tags->>'description', '')) LIKE '%zwem%' THEN true
    ELSE false
  END AS has_water,

  -- Additional optional features (sparse coverage, include when available)
  p.osm_tags->>'wheelchair' AS is_accessible,  -- 19% coverage: 'yes', 'no', 'limited'
  CASE p.osm_tags->>'lit'
    WHEN 'yes' THEN true
    WHEN 'no' THEN false
  END AS is_lit,  -- 2% coverage
  p.osm_tags->>'opening_hours' AS opening_hours,  -- 3% coverage
  CASE WHEN p.osm_tags->>'small_dog' IS NOT NULL THEN true END AS has_small_dog_area  -- 2% coverage

FROM pois p
CROSS JOIN (SELECT centroid FROM neighborhoods WHERE id = '{neighborhood_id}') n
WHERE p.category = 'dog_park'
AND ST_DWithin(p.location::geography, n.centroid::geography, 3000)
ORDER BY distance_meters
LIMIT 10;
```

### Name Pattern Inference Rules

Belgian dog park names follow predictable patterns:

| Name Pattern | Implies | Example |
|--------------|---------|---------|
| `*losloop*` | Fenced (official off-leash zone) | Hondenlosloopzone |
| `*hondenweide*` | Fenced, grass surface | Hondenweide Muizen |
| `*hondenspeelweide*` | Fenced, grass surface | Hondenspeelweide |
| `*hondenpark*` | Fenced | Hondenpark |
| `*hondenzone*` | Fenced | Hondenzone |
| `*vrijheidszone*` | Fenced | Zone de liberté |
| `*weide*` | Grass surface | Any *weide |
| `*bos*` | Mixed/forest surface | Hondenlosloopbos |
| `*water*`, `*zwem*` | Has water | Hondenzwemzone |

---

## Walking Time Estimation

Convert distance to walking time using:
```
walking_minutes = ROUND(distance_meters / 80)
```

This assumes 5 km/h walking speed (80 meters per minute).
Mark times as "estimated" in output.
