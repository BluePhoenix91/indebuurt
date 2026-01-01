-- ============================================================================
-- Load Statbel Statistical Sectors
--
-- Prerequisites:
-- 1. Run 20250101_001_initial-schema.sql first
-- 2. Load GeoJSON with ogr2ogr (creates staging_sectors table):
--
-- ogr2ogr -f "PostgreSQL" \
--   "PG:host=localhost dbname=buurtkompas user=postgres password=YOUR_PASSWORD" \
--   database/data/sh_statbel_statistical_sectors_31370_20240101.geojson \
--   -nln staging_sectors \
--   -overwrite \
--   -s_srs EPSG:31370 \
--   -t_srs EPSG:4326
--
-- This script:
-- 1. Loads statistical sectors from staging_sectors into statistical_sectors
-- 2. Aggregates sectors into neighborhoods
-- 3. Links sectors to neighborhoods
-- 4. Cleans up staging table
-- ============================================================================

-- ============================================================================
-- STEP 1: Verify staging table exists
-- ============================================================================
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_name = 'staging_sectors') THEN
        RAISE EXCEPTION 'staging_sectors table not found. Run ogr2ogr first.';
    END IF;
END $$;

-- ============================================================================
-- STEP 2: Create neighborhoods by aggregating staging sectors
-- ============================================================================
INSERT INTO neighborhoods (id, name, city, province, nis_code, sector_count, boundary, centroid, area_km2)
WITH sector_aggregates AS (
    SELECT
        LEFT(cd_sector, 7) AS neighborhood_code,
        tx_munty_descr_nl AS city,
        COALESCE(tx_prov_descr_nl, tx_rgn_descr_nl) AS province,
        -- Use the most common sector name as neighborhood name
        MODE() WITHIN GROUP (ORDER BY COALESCE(tx_sector_descr_nl, cd_sector)) AS neighborhood_name,
        COUNT(*) AS sector_count,
        -- Union all sector boundaries into one neighborhood boundary
        ST_Multi(ST_Union(ST_MakeValid(wkb_geometry))) AS boundary
    FROM staging_sectors
    WHERE tx_prov_descr_nl IN (
        'Provincie Antwerpen',
        'Provincie Limburg',
        'Provincie Oost-Vlaanderen',
        'Provincie West-Vlaanderen',
        'Provincie Vlaams-Brabant'
    )
    OR tx_rgn_descr_nl = 'Brussels Hoofdstedelijk Gewest'
    GROUP BY LEFT(cd_sector, 7), tx_munty_descr_nl, COALESCE(tx_prov_descr_nl, tx_rgn_descr_nl)
),
ranked_neighborhoods AS (
    SELECT
        neighborhood_code,
        neighborhood_name,
        city,
        province,
        sector_count,
        boundary,
        -- Generate base slug
        slugify(city) || '-' || slugify(neighborhood_name) AS base_slug,
        -- Handle duplicate slugs
        ROW_NUMBER() OVER (
            PARTITION BY slugify(city) || '-' || slugify(neighborhood_name)
            ORDER BY neighborhood_code
        ) AS dup_rank,
        COUNT(*) OVER (
            PARTITION BY slugify(city) || '-' || slugify(neighborhood_name)
        ) AS dup_count
    FROM sector_aggregates
)
SELECT
    CASE
        WHEN dup_count > 1 THEN base_slug || '-' || neighborhood_code
        ELSE base_slug
    END AS id,
    neighborhood_name AS name,
    city,
    province,
    neighborhood_code AS nis_code,
    sector_count,
    boundary,
    ST_Centroid(boundary) AS centroid,
    ST_Area(boundary::geography) / 1000000.0 AS area_km2
FROM ranked_neighborhoods
ON CONFLICT (id) DO UPDATE SET
    name = EXCLUDED.name,
    city = EXCLUDED.city,
    province = EXCLUDED.province,
    nis_code = EXCLUDED.nis_code,
    sector_count = EXCLUDED.sector_count,
    boundary = EXCLUDED.boundary,
    centroid = EXCLUDED.centroid,
    area_km2 = EXCLUDED.area_km2,
    updated_at = NOW();

-- ============================================================================
-- STEP 3: Load statistical sectors with neighborhood references
-- ============================================================================
INSERT INTO statistical_sectors (id, name, city, province, nis_code, neighborhood_id, boundary, centroid, area_km2)
WITH ranked_sectors AS (
    SELECT
        -- Generate base slug from municipality + sector name
        slugify(tx_munty_descr_nl) || '-' || slugify(COALESCE(tx_sector_descr_nl, cd_sector)) AS base_slug,
        cd_sector AS sector_code,
        COALESCE(tx_sector_descr_nl, cd_sector) AS name,
        tx_munty_descr_nl AS city,
        COALESCE(tx_prov_descr_nl, tx_rgn_descr_nl) AS province,
        cd_sector AS nis_code,
        ST_Multi(ST_MakeValid(wkb_geometry)) AS boundary,
        -- Rank duplicates so we can append suffix
        ROW_NUMBER() OVER (
            PARTITION BY slugify(tx_munty_descr_nl) || '-' || slugify(COALESCE(tx_sector_descr_nl, cd_sector))
            ORDER BY cd_sector
        ) AS dup_rank,
        COUNT(*) OVER (
            PARTITION BY slugify(tx_munty_descr_nl) || '-' || slugify(COALESCE(tx_sector_descr_nl, cd_sector))
        ) AS dup_count
    FROM staging_sectors
    WHERE tx_prov_descr_nl IN (
        'Provincie Antwerpen',
        'Provincie Limburg',
        'Provincie Oost-Vlaanderen',
        'Provincie West-Vlaanderen',
        'Provincie Vlaams-Brabant'
    )
    OR tx_rgn_descr_nl = 'Brussels Hoofdstedelijk Gewest'
),
sectors_with_ids AS (
    SELECT
        CASE
            WHEN dup_count > 1 THEN base_slug || '-' || lower(sector_code)
            ELSE base_slug
        END AS id,
        name,
        city,
        province,
        nis_code,
        boundary
    FROM ranked_sectors
)
SELECT
    s.id,
    s.name,
    s.city,
    s.province,
    s.nis_code,
    n.id AS neighborhood_id,
    s.boundary,
    ST_Centroid(s.boundary) AS centroid,
    ST_Area(s.boundary::geography) / 1000000.0 AS area_km2
FROM sectors_with_ids s
LEFT JOIN neighborhoods n ON LEFT(s.nis_code, 7) = n.nis_code
ON CONFLICT (id) DO UPDATE SET
    name = EXCLUDED.name,
    city = EXCLUDED.city,
    province = EXCLUDED.province,
    nis_code = EXCLUDED.nis_code,
    neighborhood_id = EXCLUDED.neighborhood_id,
    boundary = EXCLUDED.boundary,
    centroid = EXCLUDED.centroid,
    area_km2 = EXCLUDED.area_km2,
    updated_at = NOW();

-- ============================================================================
-- STEP 4: Cleanup staging table
-- ============================================================================
DROP TABLE IF EXISTS staging_sectors;

-- ============================================================================
-- STEP 5: Verification queries
-- ============================================================================

-- Show counts by province
SELECT
    province,
    COUNT(*) as neighborhood_count
FROM neighborhoods
GROUP BY province
ORDER BY province;

-- Show total counts
SELECT 'neighborhoods' as table_name, COUNT(*) as count FROM neighborhoods
UNION ALL
SELECT 'statistical_sectors', COUNT(*) FROM statistical_sectors;

-- Sample Gent neighborhoods
SELECT id, name, nis_code, sector_count, ROUND(area_km2::numeric, 2) as area_km2
FROM neighborhoods
WHERE city = 'Gent'
ORDER BY nis_code
LIMIT 10;

-- Verify all sectors have neighborhood references
SELECT
    COUNT(*) as total_sectors,
    COUNT(neighborhood_id) as with_neighborhood,
    COUNT(*) - COUNT(neighborhood_id) as missing_neighborhood
FROM statistical_sectors;

-- ============================================================================
-- DONE
-- ============================================================================
