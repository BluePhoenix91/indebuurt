-- POI Schema and Helper Functions
--
-- This migration sets up the POI schema extensions and helper functions.
-- The actual POI data import is handled by: database/scripts/pois/transform-staging-to-pois.sql
--
-- Run order:
-- 1. Run this migration (schema + functions)
-- 2. Run setup-all.sh to fetch and load POI data into staging_poi
-- 3. Run transform-staging-to-pois.sql to populate the pois table

-- Add domain column to pois
ALTER TABLE pois ADD COLUMN IF NOT EXISTS domain VARCHAR(50);
CREATE INDEX IF NOT EXISTS idx_pois_domain ON pois (domain);

-- Helper functions

CREATE OR REPLACE FUNCTION get_pois_in_neighborhood(p_neighborhood_id VARCHAR, p_category VARCHAR DEFAULT NULL)
RETURNS TABLE (poi_id INTEGER, poi_name VARCHAR, poi_category VARCHAR, poi_domain VARCHAR, lat DOUBLE PRECISION, lon DOUBLE PRECISION, distance_m DOUBLE PRECISION, osm_tags JSONB) AS $$
BEGIN
    RETURN QUERY
    SELECT p.id, p.name, p.category, p.domain, ST_Y(p.location), ST_X(p.location),
           ST_Distance(p.location::geography, n.centroid::geography), p.osm_tags
    FROM pois p JOIN neighborhoods n ON ST_Within(p.location, n.boundary)
    WHERE n.id = p_neighborhood_id AND (p_category IS NULL OR p.category = p_category)
    ORDER BY p.location <-> n.centroid;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_pois_in_sector(p_sector_id VARCHAR, p_category VARCHAR DEFAULT NULL)
RETURNS TABLE (poi_id INTEGER, poi_name VARCHAR, poi_category VARCHAR, poi_domain VARCHAR, lat DOUBLE PRECISION, lon DOUBLE PRECISION, distance_m DOUBLE PRECISION, osm_tags JSONB) AS $$
BEGIN
    RETURN QUERY
    SELECT p.id, p.name, p.category, p.domain, ST_Y(p.location), ST_X(p.location),
           ST_Distance(p.location::geography, s.centroid::geography), p.osm_tags
    FROM pois p JOIN statistical_sectors s ON ST_Within(p.location, s.boundary)
    WHERE s.id = p_sector_id AND (p_category IS NULL OR p.category = p_category)
    ORDER BY p.location <-> s.centroid;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION get_nearest_pois_to_neighborhood(p_neighborhood_id VARCHAR, p_category VARCHAR, p_limit INTEGER DEFAULT 5)
RETURNS TABLE (poi_id INTEGER, poi_name VARCHAR, poi_category VARCHAR, poi_domain VARCHAR, lat DOUBLE PRECISION, lon DOUBLE PRECISION, distance_m DOUBLE PRECISION, osm_tags JSONB) AS $$
BEGIN
    RETURN QUERY
    SELECT p.id, p.name, p.category, p.domain, ST_Y(p.location), ST_X(p.location),
           ST_Distance(p.location::geography, n.centroid::geography), p.osm_tags
    FROM pois p, neighborhoods n
    WHERE n.id = p_neighborhood_id AND p.category = p_category
    ORDER BY p.location <-> n.centroid LIMIT p_limit;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION count_pois_in_radius(p_lat DOUBLE PRECISION, p_lon DOUBLE PRECISION, p_category VARCHAR, p_radius_m INTEGER DEFAULT 500)
RETURNS INTEGER AS $$
DECLARE v_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO v_count FROM pois p
    WHERE p.category = p_category
      AND ST_DWithin(p.location::geography, ST_SetSRID(ST_MakePoint(p_lon, p_lat), 4326)::geography, p_radius_m);
    RETURN v_count;
END;
$$ LANGUAGE plpgsql;

-- Get POIs within or near a neighborhood (uses buffer for "nearby" POIs)
CREATE OR REPLACE FUNCTION get_pois_near_neighborhood(p_neighborhood_id VARCHAR, p_category VARCHAR DEFAULT NULL, p_buffer_m INTEGER DEFAULT 500)
RETURNS TABLE (poi_id INTEGER, poi_name VARCHAR, poi_category VARCHAR, poi_domain VARCHAR, lat DOUBLE PRECISION, lon DOUBLE PRECISION, distance_m DOUBLE PRECISION, is_inside BOOLEAN, osm_tags JSONB) AS $$
BEGIN
    RETURN QUERY
    SELECT p.id, p.name, p.category, p.domain, ST_Y(p.location), ST_X(p.location),
           ST_Distance(p.location::geography, n.centroid::geography),
           ST_Within(p.location, n.boundary) as is_inside,
           p.osm_tags
    FROM pois p, neighborhoods n
    WHERE n.id = p_neighborhood_id
      AND (p_category IS NULL OR p.category = p_category)
      AND ST_DWithin(p.location::geography, n.boundary::geography, p_buffer_m)
    ORDER BY ST_Within(p.location, n.boundary) DESC, p.location <-> n.centroid;
END;
$$ LANGUAGE plpgsql;

DROP FUNCTION IF EXISTS get_neighborhood_poi_summary(VARCHAR);
CREATE OR REPLACE FUNCTION get_neighborhood_poi_summary(p_neighborhood_id VARCHAR)
RETURNS TABLE (out_category VARCHAR, out_domain VARCHAR, count_in_neighborhood BIGINT, nearest_distance_m DOUBLE PRECISION) AS $$
BEGIN
    RETURN QUERY
    WITH neighborhood AS (SELECT n.id, n.centroid, n.boundary FROM neighborhoods n WHERE n.id = p_neighborhood_id),
    categories AS (SELECT DISTINCT p.category AS cat, p.domain AS dom FROM pois p),
    counts AS (SELECT p.category AS cat, p.domain AS dom, COUNT(*) as cnt FROM pois p, neighborhood n WHERE ST_Within(p.location, n.boundary) GROUP BY p.category, p.domain),
    nearest AS (SELECT DISTINCT ON (p.category) p.category AS cat, ST_Distance(p.location::geography, n.centroid::geography) as dist FROM pois p, neighborhood n ORDER BY p.category, p.location <-> n.centroid)
    SELECT c.cat, c.dom, COALESCE(cnt.cnt, 0)::BIGINT, nr.dist
    FROM categories c LEFT JOIN counts cnt ON c.cat = cnt.cat LEFT JOIN nearest nr ON c.cat = nr.cat
    ORDER BY c.dom, c.cat;
END;
$$ LANGUAGE plpgsql;

-- Permissions
GRANT SELECT ON pois TO buurtkompas_readonly;
GRANT EXECUTE ON FUNCTION get_pois_in_neighborhood TO buurtkompas_readonly;
GRANT EXECUTE ON FUNCTION get_pois_in_sector TO buurtkompas_readonly;
GRANT EXECUTE ON FUNCTION get_nearest_pois_to_neighborhood TO buurtkompas_readonly;
GRANT EXECUTE ON FUNCTION count_pois_in_radius TO buurtkompas_readonly;
GRANT EXECUTE ON FUNCTION get_neighborhood_poi_summary TO buurtkompas_readonly;
GRANT EXECUTE ON FUNCTION get_pois_near_neighborhood TO buurtkompas_readonly;
