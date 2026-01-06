-- ============================================================================
-- Transform staging_poi to pois (with address extraction)
--
-- Run this script after loading POI data into staging_poi via ogr2ogr.
-- This is the standalone version for re-imports (separate from migration 003).
--
-- Usage:
--   \i database/scripts/pois/transform-staging-to-pois.sql
--
-- Prerequisites:
--   1. staging_poi table populated via ogr2ogr (setup-all.sh does this)
--   2. pois table exists (from migration 001)
-- ============================================================================

-- ============================================================================
-- STEP 1: Clear existing POI data
-- ============================================================================
-- Comment out this line if you want to append instead of replace
TRUNCATE pois RESTART IDENTITY;

-- ============================================================================
-- STEP 2: Transform and insert POI data
-- ============================================================================
INSERT INTO pois (osm_id, name, category, domain, location, osm_tags,
                  street, house_number, postal_code, city, phone, website, opening_hours)
SELECT
    CAST(id AS BIGINT) as osm_id,
    CASE WHEN tags IS NOT NULL AND tags::jsonb ? 'name' THEN tags::jsonb->>'name' ELSE NULL END as name,
    CASE
        WHEN tags::jsonb->>'amenity' = 'veterinary' THEN 'vet'
        WHEN tags::jsonb->>'shop' = 'pet' THEN 'pet_store'
        WHEN tags::jsonb->>'leisure' = 'dog_park' THEN 'dog_park'
        WHEN tags::jsonb->>'shop' = 'supermarket' THEN 'supermarket'
        WHEN tags::jsonb->>'amenity' = 'pharmacy' THEN 'pharmacy'
        WHEN tags::jsonb->>'amenity' = 'school' THEN 'school'
        WHEN tags::jsonb->>'highway' = 'bus_stop' THEN 'bus_stop'
        WHEN tags::jsonb->>'public_transport' = 'platform' THEN 'bus_stop'
        WHEN tags::jsonb->>'railway' = 'station' THEN 'train_station'
        WHEN tags::jsonb->>'railway' = 'halt' THEN 'train_station'
        WHEN tags::jsonb->>'leisure' = 'park' THEN 'park'
        ELSE 'other'
    END as category,
    CASE
        WHEN tags::jsonb->>'amenity' = 'veterinary' THEN 'pets'
        WHEN tags::jsonb->>'shop' = 'pet' THEN 'pets'
        WHEN tags::jsonb->>'leisure' = 'dog_park' THEN 'pets'
        WHEN tags::jsonb->>'shop' = 'supermarket' THEN 'shopping'
        WHEN tags::jsonb->>'amenity' = 'pharmacy' THEN 'healthcare'
        WHEN tags::jsonb->>'amenity' = 'school' THEN 'education'
        WHEN tags::jsonb->>'highway' = 'bus_stop' THEN 'transport'
        WHEN tags::jsonb->>'public_transport' = 'platform' THEN 'transport'
        WHEN tags::jsonb->>'railway' IN ('station', 'halt') THEN 'transport'
        WHEN tags::jsonb->>'leisure' = 'park' THEN 'green'
        ELSE 'other'
    END as domain,
    wkb_geometry as location,
    tags::jsonb as osm_tags,
    -- Address fields extracted from OSM tags
    tags::jsonb->>'addr:street' as street,
    tags::jsonb->>'addr:housenumber' as house_number,
    tags::jsonb->>'addr:postcode' as postal_code,
    tags::jsonb->>'addr:city' as city,
    COALESCE(tags::jsonb->>'phone', tags::jsonb->>'contact:phone') as phone,
    COALESCE(tags::jsonb->>'website', tags::jsonb->>'contact:website') as website,
    tags::jsonb->>'opening_hours' as opening_hours
FROM staging_poi
WHERE wkb_geometry IS NOT NULL
ON CONFLICT DO NOTHING;

-- ============================================================================
-- STEP 3: Verification
-- ============================================================================

-- Summary by category and domain
SELECT category, domain, COUNT(*) as count
FROM pois
GROUP BY category, domain
ORDER BY domain, count DESC;

-- Total count
SELECT COUNT(*) as total_pois FROM pois;

-- Address field coverage
SELECT
  'postal_code' AS field,
  COUNT(CASE WHEN postal_code IS NOT NULL THEN 1 END) AS extracted
FROM pois
UNION ALL
SELECT 'city', COUNT(city) FROM pois
UNION ALL
SELECT 'street', COUNT(street) FROM pois
UNION ALL
SELECT 'phone', COUNT(phone) FROM pois
UNION ALL
SELECT 'website', COUNT(website) FROM pois;
