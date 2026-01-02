-- ============================================================================
-- Schema Cleanup: Remove Unused Statistics Columns
--
-- Story H6: Load Statbel Statistics
-- These columns cannot be populated from Statbel open data:
-- - price_per_sqm: Statbel only has total median prices, not per sqm
-- - available_homes: Would need Immoweb or similar real estate source
-- - green_space_pct: Would need land use data, not in Statbel
--
-- Run this BEFORE loading Statbel statistics.
-- ============================================================================

-- Remove columns we can't populate from Statbel
ALTER TABLE neighborhood_statistics DROP COLUMN IF EXISTS price_per_sqm;
ALTER TABLE neighborhood_statistics DROP COLUMN IF EXISTS available_homes;
ALTER TABLE neighborhood_statistics DROP COLUMN IF EXISTS green_space_pct;

-- Verify the updated schema
SELECT column_name, data_type, is_nullable
FROM information_schema.columns
WHERE table_name = 'neighborhood_statistics'
ORDER BY ordinal_position;

-- ============================================================================
-- DONE
-- ============================================================================
