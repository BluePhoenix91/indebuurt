# Epic H — Infrastructure Foundation

**Goal:** Set up the technical foundation required for AI agents to generate neighborhood content: database connectivity, content storage format, and Flanders-wide data.

---

## Story H1: Migrate to Astro Content Collections ✅
> As a developer, I want neighborhood content stored as JSON in Astro Content Collections, so that AI agents can generate content files that are automatically validated at build time.

**Context:** Current .ts files work but are harder for agents to generate. Content Collections provide schema validation, auto-generated types, and a standardized format.

**Acceptance Criteria:**
- [x] Content Collections configured in `src/content/config.ts` with Zod schema
- [x] Schema covers all existing Neighborhood interface fields
- [x] One neighborhood migrated to JSON as proof of concept (`gent-binnenstad.json`)
- [x] Page renders correctly using `getCollection()` and `getEntry()` APIs
- [x] TypeScript types auto-generated and working in components
- [x] Build fails with clear error when JSON doesn't match schema

**Implementation Notes:**
- Created `src/content/config.ts` with full Zod schema matching the `Neighborhood` interface
- Created `src/content/neighborhoods/gent-binnenstad.json` as first migrated neighborhood
- Created `src/lib/neighborhoods.ts` utility that tries Content Collections first, falls back to legacy .ts files
- Updated `src/pages/buurt/[slug]/index.astro` to use the new utility
- Build succeeds and page renders correctly with data from JSON
- Backward compatible: other neighborhoods still work from legacy .ts files during migration

---

## Story H2: Complete Content Collections Migration ✅
> As a developer, I want all 12 existing neighborhoods migrated to Content Collections, so that the entire site runs on the new content format before we add new neighborhoods.

**Context:** Depends on H1. Once the pattern is proven, migrate all neighborhoods.

**Acceptance Criteria:**
- [x] All 12 Gent neighborhoods converted to JSON format
- [x] Old .ts data files removed or archived
- [x] All neighborhood pages render correctly
- [x] All subpages (huizen, demografie) work with new data format
- [x] Index page and city page display neighborhoods correctly
- [x] Sitemap and SEO metadata still generated properly
- [x] Build time not significantly increased

**Implementation Notes:**
- Converted all 12 neighborhoods from TypeScript to JSON using a vm-based Node.js script
- Removed legacy `src/data/neighborhoods/*.ts` files and `src/data/neighborhoods.ts` type definitions
- Simplified `src/lib/neighborhoods.ts` to only use Content Collections (removed fallback logic)
- Updated `src/scripts/generate-og-images.ts` to read directly from JSON files
- All pages updated to use the new `lib/neighborhoods.ts` utility:
  - `src/pages/index.astro`
  - `src/pages/gent/index.astro`
  - `src/pages/buurt/[slug]/index.astro`
  - `src/pages/buurt/[slug]/huizen/index.astro`
  - `src/pages/buurt/[slug]/demografie/index.astro`
- Build time ~1.5s (26 pages), 14 OG images generated

---

## Story H3: PostGIS MCP Server Setup ✅
> As a developer, I want Claude to query my PostGIS database directly via MCP, so that AI agents can look up real neighborhood data instead of hallucinating facts.

**Context:** MCP (Model Context Protocol) allows Claude to execute SQL queries against PostGIS. This is how agents will get POI counts, distances, and statistics.

**Acceptance Criteria:**
- [x] `@modelcontextprotocol/server-postgres` installed and configured
- [x] MCP server connects to local PostGIS database
- [x] Claude can execute read-only queries via MCP
- [x] Test query works: "Find all vets within 1km of Gent Binnenstad center"
- [x] Test query works: "Get inhabitant count for neighborhood X"
- [x] Connection documented in project README or setup guide
- [x] Security: read-only access, no destructive queries possible

**Implementation Notes:**
- Created `buurtkompas` database with PostGIS extension on native PostgreSQL
- Created `buurtkompas_readonly` user with SELECT-only permissions for MCP
- Schema in `database/schema/01-setup.sql` with tables: `neighborhoods`, `pois`, `neighborhood_statistics`
- Helper function `find_nearest_pois(lat, lon, category, limit)` for proximity queries
- MCP configured via `.mcp.json` at project root using `@modelcontextprotocol/server-postgres`
- Documentation in `database/README.md`
- Verified: spatial queries work, INSERT blocked (read-only enforced)

---

## Story H4: Load Flanders Neighborhood Boundaries ✅
> As a developer, I want official Flanders neighborhood boundaries loaded into PostGIS, so that we have authoritative geographic definitions for all neighborhoods we'll generate content for.

**Context:** Statistische Sectoren from Statbel provide official Belgian neighborhood boundaries in GeoJSON/Shapefile format.

**Acceptance Criteria:**
- [x] Statistische Sectoren downloaded from Statbel or geo.be
- [x] Data loaded into PostGIS with proper SRID (Belgian Lambert or WGS84)
- [x] Each sector has: unique ID, name, geometry, municipality, province
- [x] Query works: "Get boundary polygon for sector X"
- [x] Query works: "Find all sectors in municipality Gent"
- [x] Total sector count matches official Flanders count
- [x] Center point (centroid) calculated for each sector

**Implementation Notes:**
- Loaded 9,919 statistical sectors covering all Flemish provinces + Brussels
- Data stored in `statistical_sectors` table with columns: `id`, `name`, `city`, `province`, `nis_code`, `boundary`, `centroid`
- All sectors have official NIS codes for linking to Statbel data
- WGS84 (SRID 4326) used for coordinates
- Centroids pre-calculated for all sectors
- Province breakdown: Oost-Vlaanderen (2,215), Antwerpen (1,997), West-Vlaanderen (1,862), Vlaams-Brabant (1,725), Limburg (1,396), Brussels (724)

---

## Story H5: Load Flanders POI Data ✅
> As a developer, I want POI data for all of Flanders loaded into PostGIS, so that agents can query amenities (vets, parks, shops) for any neighborhood.

**Context:** Existing POC scripts extract POIs from OSM. Need to run for all Flanders, not just Gent.

**Acceptance Criteria:**
- [x] Belgium OSM extract downloaded from Geofabrik
- [x] POI extraction scripts run for all Flanders
- [x] Categories loaded: vets, pet stores, dog parks, supermarkets, pharmacies, schools, public transport stops
- [x] Each POI has: name, category, coordinates, OSM tags
- [x] Spatial index created for fast proximity queries
- [x] Query works: "Find 5 nearest vets to point X,Y"
- [x] Query works: "Count parks within 500m of sector centroid"

**Implementation Notes:**
- Loaded 63,043 POIs covering all of Flanders + Brussels
- Categories: bus_stop (43,850), school (5,869), park (5,396), supermarket (2,927), pharmacy (2,803), dog_park (764), vet (612), train_station (494), pet_store (328)
- OSM tags stored as JSONB in `osm_tags` column
- Spatial index `idx_pois_location` (GiST) created for fast proximity queries
- POIs distributed across all provinces: Oost-Vlaanderen (10,086), Antwerpen (9,554), Vlaams-Brabant (9,191), West-Vlaanderen (7,471), Limburg (7,133), Brussels (5,789)

---

## Story H6: Load Statbel Statistics ✅
> As a developer, I want socioeconomic statistics from Statbel loaded into PostGIS, so that agents can include real data about prices, income, and demographics.

**Context:** Statbel publishes open data on house prices, income distribution, population by age, etc. at various geographic levels.

**Acceptance Criteria:**
- [x] Relevant Statbel datasets identified and downloaded
- [x] Data loaded and linked to statistical sectors
- [x] Available metrics include: median house price, inhabitants, population density
- [x] Query works: "Get median house price for sector X"
- [x] Query works: "Get population for municipality Y"
- [x] Data vintage documented (which year's data)
- [x] Update process documented for when new Statbel data releases

**Implementation Notes:**
- Data sources:
  - Population: Statbel OPENDATA_SECTOREN_2024.txt (sector-level, aggregated to neighborhoods)
  - House prices: Statbel vastgoed_2010_9999.xlsx (municipality-level, inherited to neighborhoods)
- Schema cleaned up: removed `price_per_sqm`, `available_homes`, `green_space_pct` columns not available from Statbel
- Python ETL script: `database/scripts/statbel/load-statistics.py` (uses pandas)
- Migrations: `20250102_004_cleanup-statistics-schema.sql`, `20250102_005_load-statbel-statistics.sql`
- Data vintage: Population 2024, House prices 2024
- Coverage: ~2,800 Flanders+Brussels neighborhoods with population and price data
- Note: House prices are at municipality level (all neighborhoods in a city share same median price)

---

## Dependencies

```
H1 (Content Collections POC)
  └── H2 (Full Migration)

H3 (MCP Server)
  ├── H4 (Boundaries) ─┐
  ├── H5 (POIs) ───────┼── All needed before agents can work
  └── H6 (Statistics) ─┘
```

H1-H2 and H3-H6 can be worked in parallel.
