-- ============================================================================
-- Enrich POIs with City and Postal Code via Spatial Join
--
-- This script enriches the pois table with city and postal_code by:
--   1. Loading NIS9 -> PostCode mapping into statistical_sectors (if needed)
--   2. Filling gaps in statistical_sectors postal_code from neighbors
--   3. Spatial joining POIs to sectors to get city/postal_code
--
-- This is the FINAL step after POI import. It fills in address data for POIs
-- that don't have addr:city or addr:postcode in their OSM tags.
--
-- Source for postal codes: https://github.com/mathiasleroy/Belgium-Geographic-Data
--
-- Prerequisites:
--   1. statistical_sectors table populated (migration 002)
--   2. pois table populated (transform-staging-to-pois.sql)
--   3. postal-inserts.sql generated (run generate-postal-inserts.py once)
--
-- Usage:
--   \i database/scripts/geo/enrich-pois-with-address.sql
--
-- Pipeline order:
--   1. setup-all.sh (fetch POIs -> staging_poi)
--   2. transform-staging-to-pois.sql (staging_poi -> pois with OSM addr tags)
--   3. THIS SCRIPT (enrich pois with city/postal_code from sectors)
-- ============================================================================

-- ============================================================================
-- STEP 1: Load postal codes into statistical_sectors (if not already done)
-- ============================================================================

-- Check current coverage
DO $$
DECLARE
    v_coverage NUMERIC;
BEGIN
    SELECT ROUND(100.0 * COUNT(postal_code) / COUNT(*), 1) INTO v_coverage
    FROM statistical_sectors
    WHERE postal_code IS NOT NULL AND postal_code != '';

    RAISE NOTICE 'Current statistical_sectors postal_code coverage: %', v_coverage || '%';

    IF v_coverage < 90 THEN
        RAISE NOTICE 'Coverage below 90%% - loading postal codes from staging table...';
    ELSE
        RAISE NOTICE 'Coverage OK - skipping postal code load.';
    END IF;
END $$;

-- Create staging table for postal code lookup
DROP TABLE IF EXISTS staging_postal_codes;
CREATE TABLE staging_postal_codes (
    nis9 TEXT NOT NULL,
    postcode TEXT
);

-- Load the INSERT statements (generated from be-dictionary.csv)
-- This file must exist - run: python database/scripts/geo/generate-postal-inserts.py > database/data/postal-inserts.sql
\i database/data/postal-inserts.sql

-- Update statistical_sectors from staging
UPDATE statistical_sectors ss
SET postal_code = spc.postcode
FROM staging_postal_codes spc
WHERE ss.nis_code = spc.nis9
  AND (ss.postal_code IS NULL OR ss.postal_code = '');

-- Report staging results
SELECT
    'After staging load' as step,
    COUNT(*) as total_sectors,
    COUNT(CASE WHEN postal_code IS NOT NULL AND postal_code != '' THEN 1 END) as with_postal_code
FROM statistical_sectors;

-- ============================================================================
-- STEP 2: Fill remaining gaps from neighboring sectors
-- ============================================================================

-- Some sectors use newer NIS9 codes not in the dictionary
-- Fill these from spatially adjacent sectors
UPDATE statistical_sectors ss
SET postal_code = (
    SELECT s2.postal_code
    FROM statistical_sectors s2
    WHERE s2.postal_code IS NOT NULL
      AND s2.postal_code != ''
      AND ST_DWithin(ss.boundary, s2.boundary, 0.01)
    ORDER BY ST_Distance(ss.boundary, s2.boundary)
    LIMIT 1
)
WHERE ss.postal_code IS NULL OR ss.postal_code = '';

-- Report after neighbor fill
SELECT
    'After neighbor fill' as step,
    COUNT(*) as total_sectors,
    COUNT(CASE WHEN postal_code IS NOT NULL AND postal_code != '' THEN 1 END) as with_postal_code
FROM statistical_sectors;

-- ============================================================================
-- STEP 3: Enrich POIs with city and postal_code via spatial join
-- ============================================================================

-- Only update POIs that:
--   1. Fall within a statistical sector boundary
--   2. Don't already have the value from OSM tags (COALESCE preserves OSM data)
UPDATE pois p
SET
    city = COALESCE(NULLIF(p.city, ''), s.city),
    postal_code = COALESCE(NULLIF(p.postal_code, ''), s.postal_code)
FROM statistical_sectors s
WHERE ST_Within(p.location, s.boundary)
  AND (p.city IS NULL OR p.city = '' OR p.postal_code IS NULL OR p.postal_code = '');

-- ============================================================================
-- STEP 4: Verification
-- ============================================================================

-- POI coverage summary
SELECT
    COUNT(*) as total_pois,
    COUNT(CASE WHEN city IS NOT NULL AND city != '' THEN 1 END) as with_city,
    COUNT(CASE WHEN postal_code IS NOT NULL AND postal_code != '' THEN 1 END) as with_postal_code,
    ROUND(100.0 * COUNT(CASE WHEN city IS NOT NULL AND city != '' THEN 1 END) / COUNT(*), 1) as pct_city,
    ROUND(100.0 * COUNT(CASE WHEN postal_code IS NOT NULL AND postal_code != '' THEN 1 END) / COUNT(*), 1) as pct_postal_code
FROM pois;

-- POIs outside Flanders/Brussels (expected ~19% - these are in Wallonia or abroad)
SELECT
    COUNT(*) as pois_outside_coverage,
    ROUND(100.0 * COUNT(*) / (SELECT COUNT(*) FROM pois), 1) as pct_outside
FROM pois p
WHERE (p.city IS NULL OR p.city = '')
  AND NOT EXISTS (
    SELECT 1 FROM statistical_sectors s
    WHERE ST_Within(p.location, s.boundary)
  );

-- Sample enriched POIs
SELECT name, category, city, postal_code, street
FROM pois
WHERE city IS NOT NULL AND city != '' AND name IS NOT NULL
ORDER BY random()
LIMIT 5;

-- ============================================================================
-- STEP 5: Cleanup
-- ============================================================================
DROP TABLE IF EXISTS staging_postal_codes;

-- ============================================================================
-- DONE
--
-- Expected results:
--   - statistical_sectors: ~100% postal_code coverage
--   - pois: ~80% city/postal_code coverage (limited by Flanders/Brussels boundaries)
--   - ~19% of POIs are outside our coverage area (Wallonia, Netherlands, etc.)
-- ============================================================================
