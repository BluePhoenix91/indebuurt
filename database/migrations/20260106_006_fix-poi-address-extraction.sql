-- ============================================================================
-- Fix POI Address Field Extraction
--
-- Bug: backlog/Bugs/2026-01-06-poi-address-fields-not-extracted.md
--
-- This migration updates the schema and helper functions to support address fields.
-- After running this migration, re-import POI data using:
--   \i database/scripts/pois/transform-staging-to-pois.sql
--
-- Prerequisites: 20250101_003_load-pois.sql must have run
-- ============================================================================

-- ============================================================================
-- STEP 1: Expand columns that are too small for OSM data
-- ============================================================================

-- postal_code: some OSM data has multiple postcodes like "8570;8572;8573" (max 14 chars)
ALTER TABLE pois ALTER COLUMN postal_code TYPE VARCHAR(20);

-- house_number: some OSM data has ranges like "123-125 bus 1-5" (max 43 chars)
ALTER TABLE pois ALTER COLUMN house_number TYPE VARCHAR(50);

-- ============================================================================
-- STEP 2: Update helper functions to return address columns
-- ============================================================================

-- get_pois_in_neighborhood: Add address columns at end
DROP FUNCTION IF EXISTS get_pois_in_neighborhood(VARCHAR, VARCHAR);
CREATE OR REPLACE FUNCTION get_pois_in_neighborhood(p_neighborhood_id VARCHAR, p_category VARCHAR DEFAULT NULL)
RETURNS TABLE (
  poi_id INTEGER,
  poi_name VARCHAR,
  poi_category VARCHAR,
  poi_domain VARCHAR,
  lat DOUBLE PRECISION,
  lon DOUBLE PRECISION,
  distance_m DOUBLE PRECISION,
  osm_tags JSONB,
  street VARCHAR,
  house_number VARCHAR,
  postal_code VARCHAR,
  city VARCHAR,
  phone VARCHAR,
  website VARCHAR,
  opening_hours VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT p.id, p.name, p.category, p.domain, ST_Y(p.location), ST_X(p.location),
           ST_Distance(p.location::geography, n.centroid::geography), p.osm_tags,
           p.street, p.house_number, p.postal_code, p.city, p.phone, p.website, p.opening_hours
    FROM pois p JOIN neighborhoods n ON ST_Within(p.location, n.boundary)
    WHERE n.id = p_neighborhood_id AND (p_category IS NULL OR p.category = p_category)
    ORDER BY p.location <-> n.centroid;
END;
$$ LANGUAGE plpgsql;

-- get_pois_in_sector: Add address columns at end
DROP FUNCTION IF EXISTS get_pois_in_sector(VARCHAR, VARCHAR);
CREATE OR REPLACE FUNCTION get_pois_in_sector(p_sector_id VARCHAR, p_category VARCHAR DEFAULT NULL)
RETURNS TABLE (
  poi_id INTEGER,
  poi_name VARCHAR,
  poi_category VARCHAR,
  poi_domain VARCHAR,
  lat DOUBLE PRECISION,
  lon DOUBLE PRECISION,
  distance_m DOUBLE PRECISION,
  osm_tags JSONB,
  street VARCHAR,
  house_number VARCHAR,
  postal_code VARCHAR,
  city VARCHAR,
  phone VARCHAR,
  website VARCHAR,
  opening_hours VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT p.id, p.name, p.category, p.domain, ST_Y(p.location), ST_X(p.location),
           ST_Distance(p.location::geography, s.centroid::geography), p.osm_tags,
           p.street, p.house_number, p.postal_code, p.city, p.phone, p.website, p.opening_hours
    FROM pois p JOIN statistical_sectors s ON ST_Within(p.location, s.boundary)
    WHERE s.id = p_sector_id AND (p_category IS NULL OR p.category = p_category)
    ORDER BY p.location <-> s.centroid;
END;
$$ LANGUAGE plpgsql;

-- get_nearest_pois_to_neighborhood: Add address columns at end
DROP FUNCTION IF EXISTS get_nearest_pois_to_neighborhood(VARCHAR, VARCHAR, INTEGER);
CREATE OR REPLACE FUNCTION get_nearest_pois_to_neighborhood(p_neighborhood_id VARCHAR, p_category VARCHAR, p_limit INTEGER DEFAULT 5)
RETURNS TABLE (
  poi_id INTEGER,
  poi_name VARCHAR,
  poi_category VARCHAR,
  poi_domain VARCHAR,
  lat DOUBLE PRECISION,
  lon DOUBLE PRECISION,
  distance_m DOUBLE PRECISION,
  osm_tags JSONB,
  street VARCHAR,
  house_number VARCHAR,
  postal_code VARCHAR,
  city VARCHAR,
  phone VARCHAR,
  website VARCHAR,
  opening_hours VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT p.id, p.name, p.category, p.domain, ST_Y(p.location), ST_X(p.location),
           ST_Distance(p.location::geography, n.centroid::geography), p.osm_tags,
           p.street, p.house_number, p.postal_code, p.city, p.phone, p.website, p.opening_hours
    FROM pois p, neighborhoods n
    WHERE n.id = p_neighborhood_id AND p.category = p_category
    ORDER BY p.location <-> n.centroid LIMIT p_limit;
END;
$$ LANGUAGE plpgsql;

-- get_pois_near_neighborhood: Add address columns at end
DROP FUNCTION IF EXISTS get_pois_near_neighborhood(VARCHAR, VARCHAR, INTEGER);
CREATE OR REPLACE FUNCTION get_pois_near_neighborhood(p_neighborhood_id VARCHAR, p_category VARCHAR DEFAULT NULL, p_buffer_m INTEGER DEFAULT 500)
RETURNS TABLE (
  poi_id INTEGER,
  poi_name VARCHAR,
  poi_category VARCHAR,
  poi_domain VARCHAR,
  lat DOUBLE PRECISION,
  lon DOUBLE PRECISION,
  distance_m DOUBLE PRECISION,
  is_inside BOOLEAN,
  osm_tags JSONB,
  street VARCHAR,
  house_number VARCHAR,
  postal_code VARCHAR,
  city VARCHAR,
  phone VARCHAR,
  website VARCHAR,
  opening_hours VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT p.id, p.name, p.category, p.domain, ST_Y(p.location), ST_X(p.location),
           ST_Distance(p.location::geography, n.centroid::geography),
           ST_Within(p.location, n.boundary) as is_inside,
           p.osm_tags,
           p.street, p.house_number, p.postal_code, p.city, p.phone, p.website, p.opening_hours
    FROM pois p, neighborhoods n
    WHERE n.id = p_neighborhood_id
      AND (p_category IS NULL OR p.category = p_category)
      AND ST_DWithin(p.location::geography, n.boundary::geography, p_buffer_m)
    ORDER BY ST_Within(p.location, n.boundary) DESC, p.location <-> n.centroid;
END;
$$ LANGUAGE plpgsql;

-- find_nearest_pois: Add address columns (simpler version, from initial-schema.sql)
DROP FUNCTION IF EXISTS find_nearest_pois(DOUBLE PRECISION, DOUBLE PRECISION, VARCHAR, INTEGER);
CREATE OR REPLACE FUNCTION find_nearest_pois(
    lat DOUBLE PRECISION,
    lon DOUBLE PRECISION,
    poi_category VARCHAR,
    limit_count INTEGER DEFAULT 5
)
RETURNS TABLE (
    poi_id INTEGER,
    poi_name VARCHAR,
    distance_meters DOUBLE PRECISION,
    street VARCHAR,
    house_number VARCHAR,
    postal_code VARCHAR,
    city VARCHAR
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        p.id,
        p.name,
        ST_Distance(
            p.location::geography,
            ST_SetSRID(ST_MakePoint(lon, lat), 4326)::geography
        ) as distance_meters,
        p.street,
        p.house_number,
        p.postal_code,
        p.city
    FROM pois p
    WHERE p.category = poi_category
    ORDER BY p.location <-> ST_SetSRID(ST_MakePoint(lon, lat), 4326)
    LIMIT limit_count;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- STEP 3: Re-grant permissions
-- ============================================================================

GRANT EXECUTE ON FUNCTION get_pois_in_neighborhood TO buurtkompas_readonly;
GRANT EXECUTE ON FUNCTION get_pois_in_sector TO buurtkompas_readonly;
GRANT EXECUTE ON FUNCTION get_nearest_pois_to_neighborhood TO buurtkompas_readonly;
GRANT EXECUTE ON FUNCTION get_pois_near_neighborhood TO buurtkompas_readonly;
GRANT EXECUTE ON FUNCTION find_nearest_pois TO buurtkompas_readonly;

-- ============================================================================
-- DONE - Now re-import POI data:
--   \i database/scripts/pois/transform-staging-to-pois.sql
-- ============================================================================
