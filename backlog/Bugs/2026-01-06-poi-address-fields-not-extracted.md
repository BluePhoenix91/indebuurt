# POI Address Fields Not Extracted from OSM Tags

**Date discovered:** 2026-01-06
**Type:** Enhancement / Data quality issue
**Severity:** Low
**Component:** POI import pipeline
**Discovered during:** Story J2 testing (Epic J - Agent Pipeline)

---

## Summary

The `pois` table has dedicated `postal_code` and `city` columns that are frequently NULL, even when this data exists in the `osm_tags` JSONB field. The POI import pipeline extracts some address fields (`street`, `house_number`) but not others (`postal_code`, `city`).

---

## Detailed Description

### Current Behavior

When POIs are imported from OpenStreetMap, the import pipeline:
- **Does extract:** `name`, `street` (from `addr:street`), `house_number` (from `addr:housenumber`)
- **Does NOT extract:** `postal_code` (from `addr:postcode`), `city` (from `addr:city`)

The raw OSM tags are preserved in the `osm_tags` JSONB column, so the data is available but requires JSON extraction at query time.

### Evidence

Query run during Story J2 testing:

```sql
SELECT
  id,
  name,
  postal_code,           -- Column value
  city,                  -- Column value
  osm_tags->>'addr:postcode' as tag_postcode,  -- JSON value
  osm_tags->>'addr:city' as tag_city           -- JSON value
FROM pois
WHERE id IN (251, 559, 1228, 1229, 1319);
```

Results:

| id | name | postal_code | city | tag_postcode | tag_city |
|----|------|-------------|------|--------------|----------|
| 251 | Tom & Co | NULL | NULL | NULL | NULL |
| 559 | UniKOi | NULL | NULL | NULL | NULL |
| 1228 | Iris Van de Sijpe | NULL | NULL | **9320** | **Nieuwerkerken** |
| 1229 | De Mey | NULL | NULL | **9300** | **Aalst** |
| 1319 | Dierenartspraktijk Sonck Wout | NULL | NULL | NULL | NULL |

Note: POIs 1228 and 1229 have the data in `osm_tags` but it wasn't extracted to the dedicated columns.

### OSM Element Types

The data completeness also varies by OSM element type:

| POI | OSM ID | Element Type | Has addr:postcode |
|-----|--------|--------------|-------------------|
| Tom & Co | 3762308760 | node | No |
| UniKOi | 9648850005 | node | No |
| Iris Van de Sijpe | 737780202 | way | Yes |
| De Mey | 737780209 | way | Yes |
| Dierenartspraktijk Sonck | 886994384 | way | No |

**Observation:** Ways (buildings) tend to have more complete address data than nodes (points). This is because buildings often inherit address data from the building polygon, while POI nodes may only have the amenity tag.

---

## Impact

### Affected Components

1. **Researcher agent output** - POI addresses in `1-researcher.json` may be incomplete
2. **Writer agent content** - Cannot generate accurate "located at [address]" descriptions
3. **SEO value** - Missing local signals (postal codes, city names) reduce search relevance
4. **User experience** - Visitors can't easily find/verify POI locations

### Scope

```sql
-- Count POIs with postal_code in osm_tags but not in column
SELECT COUNT(*)
FROM pois
WHERE postal_code IS NULL
AND osm_tags->>'addr:postcode' IS NOT NULL;
```

This query would reveal the extent of the issue.

---

## Workaround

Until fixed, consumers can use COALESCE to fall back to osm_tags:

```sql
SELECT
  name,
  COALESCE(postal_code, osm_tags->>'addr:postcode') as postal_code,
  COALESCE(city, osm_tags->>'addr:city') as city
FROM pois
WHERE id = 123;
```

The helper functions (`get_pois_in_neighborhood`, `get_nearest_pois_to_neighborhood`) could be updated to include this fallback logic.

---

## Proposed Fix

### Option A: Update import pipeline (recommended)

Modify the POI import script to extract address fields:

```python
# Pseudocode for import logic
poi.postal_code = osm_tags.get('addr:postcode')
poi.city = osm_tags.get('addr:city')
```

**Pros:** One-time fix, all downstream consumers benefit
**Cons:** Requires re-import or migration script

### Option B: Backfill existing data

Run a one-time UPDATE to populate columns from existing osm_tags:

```sql
UPDATE pois
SET
  postal_code = osm_tags->>'addr:postcode',
  city = osm_tags->>'addr:city'
WHERE postal_code IS NULL
  AND osm_tags->>'addr:postcode' IS NOT NULL;
```

**Pros:** Quick fix for existing data
**Cons:** Doesn't fix the import pipeline, issue will recur on next import

### Option C: Update helper functions

Modify `get_pois_in_neighborhood()` and related functions to use COALESCE:

```sql
-- In function body
SELECT
  ...,
  COALESCE(p.postal_code, p.osm_tags->>'addr:postcode') as postal_code,
  COALESCE(p.city, p.osm_tags->>'addr:city') as city
FROM pois p
...
```

**Pros:** Transparent to consumers
**Cons:** Adds runtime overhead, doesn't fix root cause

### Recommended Approach

1. **Immediate:** Option B (backfill) + Option C (update helpers)
2. **Long-term:** Option A (fix import pipeline) to prevent recurrence

---

## Files Involved

- POI import script (location TBD - likely in `database/` or `scripts/`)
- `get_pois_in_neighborhood()` function
- `get_nearest_pois_to_neighborhood()` function
- `agents/researcher/prompt-v1.md` (documents expected data structure)

---

## Related

- **Epic J** - Agent Pipeline (discovered during J2 testing)
- **Epic H** - Infrastructure (POI import was part of initial setup)

---

## Notes

This is categorized as an enhancement rather than a bug because:
1. The system works - data is preserved in `osm_tags`
2. The original import may have intentionally skipped these fields
3. No functionality is broken, just suboptimal

However, fixing this would improve data quality across the platform.

---

## Implementation Notes (2026-01-06)

**Status: RESOLVED**

### What Was Implemented

We implemented a hybrid approach combining Options A, B, and a new spatial enrichment strategy:

1. **Fixed the import pipeline** (`transform-staging-to-pois.sql`)
   - Now extracts `addr:postcode` and `addr:city` from OSM tags during import
   - Coverage from OSM tags alone: ~8% (most POIs don't have address tags)

2. **Created spatial enrichment pipeline** (`enrich-pois-with-address.sql`)
   - Loads NIS9 → PostCode mapping from Belgium Geographic Data dictionary
   - Updates `statistical_sectors.postal_code` via NIS code join
   - Fills gaps in sectors using spatial neighbor lookup
   - Enriches POIs with `city` and `postal_code` via spatial join to sectors

3. **Updated helper functions** (migration `20260106_006`)
   - All POI helper functions now return address columns

### Final Coverage

| Table | Field | Coverage |
|-------|-------|----------|
| statistical_sectors | postal_code | 100% |
| pois | city | 80.8% |
| pois | postal_code | 80.8% |

### Why Not 100%?

~19% of POIs fall outside our sector boundaries:
- **11,677** in Wallonia (we only loaded Flanders/Brussels sectors)
- **290** in Netherlands (cross-border POIs)
- **106** edge cases at Flanders border

These POIs are outside our target market (Flanders) so this is acceptable.

### Pipeline Order

```
1. setup-all.sh          → Fetch POIs from Overpass → staging_poi
2. transform-staging-to-pois.sql → Extract OSM addr:* tags → pois
3. enrich-pois-with-address.sql  → Spatial join to sectors → city/postal_code
```

### Files Created/Modified

- `database/scripts/pois/transform-staging-to-pois.sql` - Added addr:postcode, addr:city extraction
- `database/scripts/geo/enrich-pois-with-address.sql` - NEW: Spatial enrichment script
- `database/scripts/geo/generate-postal-inserts.py` - NEW: CSV to SQL converter
- `database/data/postal-inserts.sql` - NEW: 20,318 NIS9→PostCode mappings
- `database/migrations/20260106_006_fix-poi-address-extraction.sql` - Schema + function updates
- `database/scripts/setup-all.sh` - Updated with new pipeline steps

### Technical Details

**NIS9 Version Mismatch:**
The Belgium Geographic Data dictionary uses older NIS9 codes (e.g., `11002A00-`) while Statbel uses newer codes (e.g., `11002K1WN`). We handle this by:
1. First matching on exact NIS9 code
2. Then filling gaps via spatial neighbor lookup (`ST_DWithin`)

**Preserving OSM Data:**
The enrichment uses `COALESCE(NULLIF(p.city, ''), s.city)` to preserve any address data that came from OSM tags, only filling in from sectors when the POI has no OSM address.
