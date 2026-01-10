-- ============================================================================
-- Add Statbel Municipality NIS Column for 2025 Municipality Mergers
--
-- Story L4: Fix Statbel NIS Code Mapping for Merged Municipalities
--
-- Problem: Belgium merged 28 municipalities on January 1, 2025. Statbel's
-- house price data uses the new merged NIS codes, but our neighborhoods
-- table uses the old pre-merger codes. This causes ~246 neighborhoods
-- to have NULL house prices.
--
-- Solution: Add a `statbel_municipality_nis` column that stores the
-- Statbel-compatible municipality NIS code for joining with Statbel data.
-- ============================================================================

-- ============================================================================
-- STEP 1: Add the new column
-- ============================================================================
ALTER TABLE neighborhoods
ADD COLUMN IF NOT EXISTS statbel_municipality_nis VARCHAR(5);

COMMENT ON COLUMN neighborhoods.statbel_municipality_nis IS
'Municipality NIS code as used by Statbel (post-2025 mergers). Use for joining with Statbel statistics.';

-- ============================================================================
-- STEP 2: Populate with default values (unchanged municipalities)
-- ============================================================================
UPDATE neighborhoods
SET statbel_municipality_nis = LEFT(nis_code, 5)
WHERE statbel_municipality_nis IS NULL;

-- ============================================================================
-- STEP 3: Apply 2025 municipality merger mappings
-- Source: Statbel REFNIS-NUTS 2025
-- ============================================================================

-- East Flanders
UPDATE neighborhoods SET statbel_municipality_nis = '44088'
WHERE LEFT(nis_code, 5) IN ('44040', '44043');  -- Melle, Merelbeke → Merelbeke-Melle

UPDATE neighborhoods SET statbel_municipality_nis = '44087'
WHERE LEFT(nis_code, 5) IN ('44034', '44073');  -- Lochristi, Wachtebeke → Lochristi

UPDATE neighborhoods SET statbel_municipality_nis = '44086'
WHERE LEFT(nis_code, 5) IN ('44012', '44048');  -- De Pinte, Nazareth → Nazareth-De Pinte

UPDATE neighborhoods SET statbel_municipality_nis = '46030'
WHERE LEFT(nis_code, 5) IN ('46003', '46013', '11056');  -- Beveren, Kruibeke, Zwijndrecht → Beveren-Kruibeke-Zwijndrecht

UPDATE neighborhoods SET statbel_municipality_nis = '46029'
WHERE LEFT(nis_code, 5) IN ('46014', '44045');  -- Lokeren, Moerbeke → Lokeren

-- Limburg
UPDATE neighborhoods SET statbel_municipality_nis = '71072'
WHERE LEFT(nis_code, 5) IN ('71022', '73040');  -- Hasselt, Kortessem → Hasselt

UPDATE neighborhoods SET statbel_municipality_nis = '73110'
WHERE LEFT(nis_code, 5) IN ('73006', '73032');  -- Bilzen, Hoeselt → Bilzen-Hoeselt

UPDATE neighborhoods SET statbel_municipality_nis = '73111'
WHERE LEFT(nis_code, 5) IN ('73083', '73009');  -- Tongeren, Borgloon → Tongeren-Borgloon

UPDATE neighborhoods SET statbel_municipality_nis = '71071'
WHERE LEFT(nis_code, 5) IN ('71057', '71069');  -- Tessenderlo, Ham → Tessenderlo-Ham

-- Flemish Brabant
UPDATE neighborhoods SET statbel_municipality_nis = '23106'
WHERE LEFT(nis_code, 5) IN ('23023', '23024', '23032');  -- Galmaarden, Gooik, Herne → Pajottegem

-- Antwerp
UPDATE neighborhoods SET statbel_municipality_nis = '11002'
WHERE LEFT(nis_code, 5) = '11007';  -- Borsbeek → Antwerpen

-- West Flanders
UPDATE neighborhoods SET statbel_municipality_nis = '37022'
WHERE LEFT(nis_code, 5) IN ('37015', '37007');  -- Tielt, Meulebeke → Tielt

UPDATE neighborhoods SET statbel_municipality_nis = '37021'
WHERE LEFT(nis_code, 5) IN ('37018', '37012');  -- Wingene, Ruiselede → Wingene

-- ============================================================================
-- STEP 4: Add index for efficient joins
-- ============================================================================
CREATE INDEX IF NOT EXISTS idx_neighborhoods_statbel_municipality
ON neighborhoods(statbel_municipality_nis);

-- ============================================================================
-- STEP 5: Verification queries
-- ============================================================================

-- Count of neighborhoods per statbel_municipality_nis
SELECT statbel_municipality_nis, COUNT(*) as count
FROM neighborhoods
GROUP BY statbel_municipality_nis
ORDER BY count DESC
LIMIT 10;

-- Verify merged municipalities have new codes
SELECT
    city,
    LEFT(nis_code, 5) as old_nis,
    statbel_municipality_nis as new_nis,
    COUNT(*) as neighborhoods
FROM neighborhoods
WHERE LEFT(nis_code, 5) != statbel_municipality_nis
GROUP BY city, LEFT(nis_code, 5), statbel_municipality_nis
ORDER BY city;
-- Expected: 27 rows showing the merged municipalities

-- ============================================================================
-- DONE
-- ============================================================================
