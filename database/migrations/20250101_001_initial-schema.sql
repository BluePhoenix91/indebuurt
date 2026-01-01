-- ============================================================================
-- Initial Schema Migration
--
-- Creates the core database structure for Buurtkompas:
-- - PostGIS extension
-- - Geographic tables (neighborhoods, statistical_sectors)
-- - POI and statistics tables
-- - Helper functions
-- - Read-only user permissions
--
-- Run this on a fresh buurtkompas database.
-- ============================================================================

-- Ensure PostGIS is enabled
CREATE EXTENSION IF NOT EXISTS postgis;

-- ============================================================================
-- HELPER FUNCTIONS
-- ============================================================================

-- Function to generate URL-safe slugs
CREATE OR REPLACE FUNCTION slugify(text) RETURNS text AS $$
BEGIN
    RETURN lower(
        regexp_replace(
            regexp_replace(
                regexp_replace(
                    -- Replace Belgian/French special chars
                    translate($1,
                        'àáâãäåæçèéêëìíîïñòóôõöøùúûüýÿÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÑÒÓÔÕÖØÙÚÛÜÝ',
                        'aaaaaaaceeeeiiiinooooooouuuuyyAAAAAAACEEEEIIIINOOOOOOUUUUY'
                    ),
                    '[^a-zA-Z0-9\s-]', '', 'g'  -- Remove special chars
                ),
                '\s+', '-', 'g'  -- Replace spaces with hyphens
            ),
            '-+', '-', 'g'  -- Replace multiple hyphens with single
        )
    );
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- Function to get neighborhood code (first 7 characters of sector code)
CREATE OR REPLACE FUNCTION get_neighborhood_code(sector_code VARCHAR) RETURNS VARCHAR AS $$
BEGIN
    RETURN LEFT(sector_code, 7);
END;
$$ LANGUAGE plpgsql IMMUTABLE;

-- ============================================================================
-- NEIGHBORHOODS TABLE (Level 6 - User-facing)
-- Aggregated from statistical sectors, ~2,800 for Flanders + Brussels
-- ============================================================================
CREATE TABLE IF NOT EXISTS neighborhoods (
    id VARCHAR(100) PRIMARY KEY,              -- e.g., 'gent-binnenstad'
    name VARCHAR(255) NOT NULL,               -- e.g., 'Binnenstad'
    city VARCHAR(100) NOT NULL,               -- e.g., 'Gent'
    province VARCHAR(100),                    -- e.g., 'Provincie Oost-Vlaanderen'
    nis_code VARCHAR(7) NOT NULL,             -- First 7 chars of sector code (e.g., '44021A0')
    sector_count INTEGER,                     -- Number of statistical sectors in this neighborhood
    area_km2 DECIMAL(10, 4),                  -- Area in square kilometers
    centroid GEOMETRY(Point, 4326),           -- Center point (WGS84)
    boundary GEOMETRY(MultiPolygon, 4326),    -- Full boundary polygon (WGS84)
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Spatial indexes for neighborhoods
CREATE INDEX IF NOT EXISTS idx_neighborhoods_centroid ON neighborhoods USING GIST (centroid);
CREATE INDEX IF NOT EXISTS idx_neighborhoods_boundary ON neighborhoods USING GIST (boundary);
CREATE INDEX IF NOT EXISTS idx_neighborhoods_city ON neighborhoods (city);
CREATE INDEX IF NOT EXISTS idx_neighborhoods_nis_code ON neighborhoods (nis_code);

-- ============================================================================
-- STATISTICAL SECTORS TABLE (Level 7 - Fine-grained)
-- Raw sectors from Statbel, ~9,900 for Flanders + Brussels
-- ============================================================================
CREATE TABLE IF NOT EXISTS statistical_sectors (
    id VARCHAR(100) PRIMARY KEY,              -- e.g., 'gent-binnenstad-44021a001'
    name VARCHAR(255) NOT NULL,               -- e.g., 'Binnenstad'
    city VARCHAR(100) NOT NULL,               -- e.g., 'Gent'
    province VARCHAR(100),                    -- e.g., 'Provincie Oost-Vlaanderen'
    nis_code VARCHAR(20),                     -- Full 9-char sector code (e.g., '44021A001')
    neighborhood_id VARCHAR(100),             -- FK to neighborhoods
    area_km2 DECIMAL(10, 4),                  -- Area in square kilometers
    centroid GEOMETRY(Point, 4326),           -- Center point (WGS84)
    boundary GEOMETRY(MultiPolygon, 4326),    -- Full boundary polygon (WGS84)
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT fk_statistical_sectors_neighborhood
        FOREIGN KEY (neighborhood_id) REFERENCES neighborhoods(id)
);

-- Spatial indexes for statistical sectors
CREATE INDEX IF NOT EXISTS idx_statistical_sectors_centroid ON statistical_sectors USING GIST (centroid);
CREATE INDEX IF NOT EXISTS idx_statistical_sectors_boundary ON statistical_sectors USING GIST (boundary);
CREATE INDEX IF NOT EXISTS idx_statistical_sectors_city ON statistical_sectors (city);
CREATE INDEX IF NOT EXISTS idx_statistical_sectors_neighborhood ON statistical_sectors (neighborhood_id);

-- ============================================================================
-- POINTS OF INTEREST (POIs) TABLE
-- Amenities extracted from OpenStreetMap
-- ============================================================================
CREATE TABLE IF NOT EXISTS pois (
    id SERIAL PRIMARY KEY,
    osm_id BIGINT,                            -- OpenStreetMap ID
    name VARCHAR(255),                        -- POI name
    category VARCHAR(50) NOT NULL,            -- e.g., 'vet', 'pet_store', 'dog_park', 'supermarket'
    subcategory VARCHAR(50),                  -- More specific type
    location GEOMETRY(Point, 4326) NOT NULL,  -- Location (WGS84)
    street VARCHAR(255),
    house_number VARCHAR(20),
    postal_code VARCHAR(10),
    city VARCHAR(100),
    phone VARCHAR(50),
    website VARCHAR(500),
    opening_hours VARCHAR(255),
    osm_tags JSONB,                           -- Raw OSM tags for reference
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Indexes for POIs
CREATE INDEX IF NOT EXISTS idx_pois_location ON pois USING GIST (location);
CREATE INDEX IF NOT EXISTS idx_pois_category ON pois (category);
CREATE INDEX IF NOT EXISTS idx_pois_city ON pois (city);

-- ============================================================================
-- NEIGHBORHOOD STATISTICS TABLE
-- Socioeconomic data from Statbel
-- ============================================================================
CREATE TABLE IF NOT EXISTS neighborhood_statistics (
    id SERIAL PRIMARY KEY,
    neighborhood_id VARCHAR(100) REFERENCES neighborhoods(id),
    year INTEGER NOT NULL,                    -- Data year (e.g., 2023)

    -- Housing statistics
    median_house_price INTEGER,               -- Median house price in euros
    price_per_sqm INTEGER,                    -- Price per square meter
    available_homes INTEGER,                  -- Number of homes for sale

    -- Demographics
    population INTEGER,
    population_density DECIMAL(10, 2),        -- People per km2
    avg_age DECIMAL(4, 1),

    -- Income
    median_income INTEGER,                    -- Median household income

    -- Other metrics
    green_space_pct DECIMAL(5, 2),            -- Percentage green space

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    UNIQUE(neighborhood_id, year)
);

CREATE INDEX IF NOT EXISTS idx_stats_neighborhood ON neighborhood_statistics (neighborhood_id);
CREATE INDEX IF NOT EXISTS idx_stats_year ON neighborhood_statistics (year);

-- ============================================================================
-- HELPER FUNCTIONS FOR QUERIES
-- ============================================================================

-- Function to find nearest POIs of a category to a point
CREATE OR REPLACE FUNCTION find_nearest_pois(
    lat DOUBLE PRECISION,
    lon DOUBLE PRECISION,
    poi_category VARCHAR,
    limit_count INTEGER DEFAULT 5
)
RETURNS TABLE (
    poi_id INTEGER,
    poi_name VARCHAR,
    distance_meters DOUBLE PRECISION
) AS $$
BEGIN
    RETURN QUERY
    SELECT
        p.id,
        p.name,
        ST_Distance(
            p.location::geography,
            ST_SetSRID(ST_MakePoint(lon, lat), 4326)::geography
        ) as distance_meters
    FROM pois p
    WHERE p.category = poi_category
    ORDER BY p.location <-> ST_SetSRID(ST_MakePoint(lon, lat), 4326)
    LIMIT limit_count;
END;
$$ LANGUAGE plpgsql;

-- ============================================================================
-- PERMISSIONS
-- Grant read-only access to MCP user
-- ============================================================================
GRANT USAGE ON SCHEMA public TO buurtkompas_readonly;
GRANT SELECT ON ALL TABLES IN SCHEMA public TO buurtkompas_readonly;
GRANT EXECUTE ON FUNCTION find_nearest_pois TO buurtkompas_readonly;
GRANT EXECUTE ON FUNCTION slugify TO buurtkompas_readonly;
GRANT EXECUTE ON FUNCTION get_neighborhood_code TO buurtkompas_readonly;

-- Ensure future tables are also accessible
ALTER DEFAULT PRIVILEGES IN SCHEMA public GRANT SELECT ON TABLES TO buurtkompas_readonly;

-- ============================================================================
-- DONE
-- ============================================================================
