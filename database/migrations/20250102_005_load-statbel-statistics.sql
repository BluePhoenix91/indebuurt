-- ============================================================================
-- Load Statbel Statistics into Neighborhood Statistics Table
--
-- Story H6: Load Statbel Statistics
--
-- Prerequisites:
-- 1. Run 20250102_004_cleanup-statistics-schema.sql first
-- 2. Run the Python ETL script to generate the staging CSV:
--    python database/scripts/statbel/load-statistics.py
--
-- This script:
-- 1. Creates a staging table for the CSV data
-- 2. Loads data from the staging CSV
-- 3. Inserts/updates neighborhood_statistics with proper FK joins
-- 4. Cleans up staging table
-- ============================================================================

-- ============================================================================
-- STEP 1: Create staging table for CSV import
-- ============================================================================
DROP TABLE IF EXISTS staging_statistics;

CREATE TABLE staging_statistics (
    nis_code VARCHAR(10) NOT NULL,
    year INTEGER NOT NULL,
    population INTEGER,
    population_density DECIMAL(12, 2),
    median_house_price INTEGER
);

-- ============================================================================
-- STEP 2: Load data from CSV
-- ============================================================================
-- NOTE: Run this from psql with the correct path, or use TablePlus Import
--
-- Option A: From psql (adjust path as needed):
--   \copy staging_statistics FROM 'database/data/statbel/neighborhood_statistics_staging.csv' WITH CSV HEADER;
--
-- Option B: Using absolute path in WSL:
--   \copy staging_statistics FROM '/mnt/d/Repos/indebuurt/indebuurt/database/data/statbel/neighborhood_statistics_staging.csv' WITH CSV HEADER;
--
-- Option C: Use TablePlus "Import from CSV" feature

-- Verify staging data loaded
SELECT COUNT(*) as staging_count FROM staging_statistics;

-- ============================================================================
-- STEP 3: Fix wijk_id column size if needed
-- ============================================================================
-- neighborhoods.id is VARCHAR(100), but wijk_id may be VARCHAR(50)
-- Some neighborhood IDs are up to 71 characters long
ALTER TABLE neighborhood_statistics
ALTER COLUMN wijk_id TYPE VARCHAR(100);

-- ============================================================================
-- STEP 4: Insert/Update neighborhood_statistics
-- ============================================================================
-- Only insert for neighborhoods that exist in our database
-- (Flanders + Brussels, ~2800 neighborhoods)

INSERT INTO neighborhood_statistics (
    wijk_id,
    year,
    population,
    population_density,
    median_house_price,
    median_income,
    avg_age,
    created_at,
    updated_at
)
SELECT
    n.id AS wijk_id,
    s.year,
    s.population,
    s.population_density,
    s.median_house_price,
    NULL AS median_income,  -- Will be loaded in future update
    NULL AS avg_age,        -- Will be loaded in future update
    NOW() AS created_at,
    NOW() AS updated_at
FROM staging_statistics s
INNER JOIN neighborhoods n ON n.nis_code = s.nis_code
ON CONFLICT (wijk_id, year) DO UPDATE SET
    population = EXCLUDED.population,
    population_density = EXCLUDED.population_density,
    median_house_price = EXCLUDED.median_house_price,
    updated_at = NOW();

-- ============================================================================
-- STEP 5: Verification queries
-- ============================================================================

-- Count records loaded
SELECT
    'Total statistics rows' AS metric,
    COUNT(*) AS value
FROM neighborhood_statistics
UNION ALL
SELECT
    'With population > 0',
    COUNT(*)
FROM neighborhood_statistics
WHERE population > 0
UNION ALL
SELECT
    'With house prices',
    COUNT(*)
FROM neighborhood_statistics
WHERE median_house_price IS NOT NULL;

-- Sample data for Gent
SELECT
    n.name,
    n.city,
    ns.population,
    ns.population_density,
    ns.median_house_price
FROM neighborhood_statistics ns
JOIN neighborhoods n ON n.id = ns.wijk_id
WHERE n.city = 'Gent'
ORDER BY ns.population DESC
LIMIT 10;

-- Province-level aggregates
SELECT
    n.province,
    COUNT(*) AS neighborhoods,
    SUM(ns.population) AS total_population,
    ROUND(AVG(ns.median_house_price)) AS avg_house_price
FROM neighborhood_statistics ns
JOIN neighborhoods n ON n.id = ns.wijk_id
GROUP BY n.province
ORDER BY total_population DESC;

-- ============================================================================
-- STEP 6: Test acceptance criteria queries
-- ============================================================================

-- AC: "Get median house price for sector X"
SELECT
    n.id,
    n.name,
    ns.median_house_price
FROM neighborhoods n
JOIN neighborhood_statistics ns ON ns.wijk_id = n.id
WHERE n.id = 'gent-rabot'
LIMIT 1;

-- AC: "Get population for municipality Y"
SELECT
    n.city,
    SUM(ns.population) AS total_population
FROM neighborhoods n
JOIN neighborhood_statistics ns ON ns.wijk_id = n.id
WHERE n.city = 'Gent'
GROUP BY n.city;

-- ============================================================================
-- STEP 7: Cleanup
-- ============================================================================
DROP TABLE IF EXISTS staging_statistics;

-- ============================================================================
-- DONE
-- ============================================================================
