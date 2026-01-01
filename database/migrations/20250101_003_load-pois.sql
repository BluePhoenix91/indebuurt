-- Load POI Data from Overpass API
-- Prerequisites: Run fetch-pois.sh, then load GeoJSON with ogr2ogr into staging_poi

-- Add domain column to pois
ALTER TABLE pois ADD COLUMN IF NOT EXISTS domain VARCHAR(50);
CREATE INDEX IF NOT EXISTS idx_pois_domain ON pois (domain);

-- Transform staging_poi to pois
INSERT INTO pois (osm_id, name, category, domain, location, osm_tags)
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
    tags::jsonb as osm_tags
FROM staging_poi
WHERE wkb_geometry IS NOT NULL
ON CONFLICT DO NOTHING;

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

-- Verify
SELECT category, domain, COUNT(*) as count FROM pois GROUP BY category, domain ORDER BY domain, count DESC;
SELECT COUNT(*) as total_pois FROM pois;
