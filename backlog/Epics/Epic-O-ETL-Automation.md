# Epic O — ETL Automation

**Goal:** Automate data imports (OSM, Statbel, boundaries) via CLI commands, enabling repeatable data refreshes without manual SQL scripts.

**Depends on:** Epic M (Server Infrastructure)

---

## Context

The current ETL process is scattered and manual:
- OSM POI extraction: `osmium` + GDAL commands, then manual SQL import
- Statbel statistics: Python script (`load-statistics.py`) + manual SQL
- Neighborhood boundaries: One-time manual import
- No unified way to "refresh everything"

This epic creates CLI commands that encapsulate these processes:
```bash
dotnet run -- import osm          # Import POIs from Geofabrik
dotnet run -- import statbel      # Import statistics from Statbel
dotnet run -- import boundaries   # Import neighborhood boundaries
dotnet run -- refresh views       # Rebuild materialized views
dotnet run -- import all          # Full refresh
```

---

## Story O1: OSM POI Import Command ✅

> As a pipeline operator, I want to import POIs from OpenStreetMap via a CLI command, so that I can refresh amenity data when OSM updates.

**Current Process:**
1. Download Belgium extract from Geofabrik (.osm.pbf)
2. Filter by tags using `osmium tags-filter`
3. Convert to GeoJSON using GDAL
4. Extract centroids to CSV
5. Import to PostgreSQL via manual SQL

**New Process:**
```bash
dotnet run -- import osm
dotnet run -- import osm --domain pets        # Single domain
dotnet run -- import osm --dry-run            # Preview without writing
dotnet run -- import osm --force              # Skip count validation warnings
```

**Acceptance Criteria:**
- [x] Filters and imports all POI categories: vet, pet_store, dog_park, park, supermarket, pharmacy, school, bus_stop, train_station
- [x] Uses temporary staging table, then atomic swap to `pois`
- [x] Preserves existing data if import fails (transaction rollback)
- [x] Reports counts: imported, delta from previous, by category
- [x] Supports `--domain` flag for single domain refresh (pets, shopping, healthcare, education, transport, green)
- [x] Supports `--dry-run` to show what would be imported
- [ ] Command downloads Belgium extract from Geofabrik if not present or stale — **Not implemented: uses Overpass API directly instead**

**Implementation Notes:**
- Uses **Overpass API** instead of Geofabrik download + osmium/GDAL pipeline — simpler, no local file management
- Rate limiting (15s between requests) to respect Overpass API fair use policy
- Retry with exponential backoff for 429/503 responses and timeouts
- Domains group related POI types (e.g., `pets` = vet + pet_store + dog_park)
- Uses PostgreSQL binary COPY for bulk insert (~63k POIs in seconds)
- NetTopologySuite for geometry handling with NpgsqlDataSource
- Center points used for way/relation geometries (polygon support documented as future improvement)

**Key Files:**
- `Pipeline.Cli/Commands/ImportOsmCommand.cs` — CLI command
- `Pipeline.Core/Services/OverpassClient.cs` — Overpass API client with rate limiting
- `Pipeline.Core/Services/OverpassOptions.cs` — Configuration (bbox, timeout, delay)
- `Pipeline.Core/Services/PoiImportService.cs` — Orchestrates import flow
- `Pipeline.Core/Services/PoiImport/OverpassToPoisConverter.cs` — Element → POI conversion
- `Pipeline.Core/Services/PoiImport/PoiStagingRepository.cs` — Staging table + bulk insert + atomic swap
- `Pipeline.Core/Dtos/Overpass/` — DTOs for Overpass API responses

---

## Story O2: Statbel Statistics Import Command ✅

> As a pipeline operator, I want to import statistics from Statbel via a CLI command, so that I can refresh demographic and price data annually.

**Current Process:**
1. Download CSVs from Statbel open data portal
2. Run Python script `load-statistics.py`
3. Apply NIS code mappings for merged municipalities
4. Execute SQL migration

**New Process:**
```bash
dotnet run -- import-statbel
dotnet run -- import-statbel --dataset population   # Single dataset
dotnet run -- import-statbel --dataset house-prices
dotnet run -- import-statbel --year 2024            # Specific year
dotnet run -- import-statbel --dry-run              # Preview without writing
```

**Acceptance Criteria:**
- [x] Command downloads latest datasets from Statbel if not present
- [x] Imports population data at sector level, aggregates to neighborhoods
- [x] Imports house prices at municipality level, inherits to neighborhoods
- [x] Applies NIS code mappings from `nis_code_mapping_2025.csv` — **via `statbel_municipality_nis` table join**
- [x] Uses staging table, then atomic merge to `gis.neighborhood_statistics`
- [x] Reports: rows imported, neighborhoods updated, missing data warnings
- [x] Supports `--dataset` flag for single dataset refresh
- [x] Supports `--year` flag for specific year import — **Added beyond original spec**
- [x] Supports `--dry-run` to preview without writing — **Added beyond original spec**

**Implementation Notes:**
- **Auto-detect latest year** via HTTP HEAD probes on Statbel URLs (2020→current year), with `--year` override
- **Caching** in system temp directory (`%TEMP%/statbel_cache/`) to avoid re-downloading
- **Belgian decimal format** handled (comma as decimal separator in population files)
- **Population file format:** Pipe-delimited with BOM encoding, area in hectares (converted to km²)
- **House prices file format:** Excel (.xlsx) with year-specific sheets, filtered to municipality level + regular houses
- **Separate transactions** for population and house prices — partial results kept on failure
- **Staging table pattern:** Temp table → COPY binary insert → INSERT...ON CONFLICT DO UPDATE with year-specific merge
- **NIS code mapping:** Uses existing `gis.statbel_municipality_nis` table (populated in Epic L4) for 2025 municipality mergers

**Key Files:**
- `Pipeline.Cli/Commands/ImportStatbelCommand.cs` — CLI command
- `Pipeline.Core/Services/Statbel/StatbelOptions.cs` — Configuration (URLs, timeout)
- `Pipeline.Core/Services/Statbel/StatbelModels.cs` — Data records
- `Pipeline.Core/Services/Statbel/StatbelDownloader.cs` — HTTP client with year detection + caching
- `Pipeline.Core/Services/Statbel/PopulationDataParser.cs` — Parse pipe-delimited population files
- `Pipeline.Core/Services/Statbel/HousePriceDataParser.cs` — Parse Excel house price files
- `Pipeline.Core/Services/Statbel/StatbelStagingRepository.cs` — Staging table + bulk insert + merge
- `Pipeline.Core/Services/Statbel/StatbelImportService.cs` — Orchestrates download → parse → stage → merge

**NuGet Packages Added:**
- `CsvHelper 33.0.1` — CSV parsing (used for pipe-delimited files)
- `ExcelDataReader 3.7.0` + `ExcelDataReader.DataSet 3.7.0` — Excel parsing

**Tests:** 75 tests total (9 new for Statbel components)

---

## Story O3: Neighborhood Boundaries Import Command

> As a pipeline operator, I want to import neighborhood boundaries via a CLI command, so that I can update boundaries if Statbel publishes corrections.

**Current State:**
- Boundaries imported once during Epic H
- No automated way to re-import or update

**New Process:**
```bash
dotnet run -- import boundaries
dotnet run -- import boundaries --source shapefile.zip
```

**Acceptance Criteria:**
- [ ] Command downloads Statistische Sectoren from geo.be if not present
- [ ] Parses shapefile or GeoJSON format
- [ ] Imports to `gis.neighborhoods` and `gis.statistical_sectors`
- [ ] Calculates centroids for all sectors
- [ ] Preserves `statbel_municipality_nis` mappings from L4
- [ ] Reports: sectors imported, municipalities, provinces
- [ ] Supports `--source` flag for local file import

**Technical Notes:**
- Use NetTopologySuite for geometry handling
- Coordinate system: WGS84 (SRID 4326)
- Large import (~10k sectors); use bulk insert

---

## Story O4: Materialized Views Refresh Command

> As a pipeline operator, I want to refresh materialized views via a CLI command, so that pre-computed aggregates reflect the latest data.

**Context:** Epic L6 creates materialized views for POI counts and nearest POIs. These need refreshing after any data import.

**New Process:**
```bash
dotnet run -- refresh views
dotnet run -- refresh views --concurrent   # Non-blocking refresh
```

**Acceptance Criteria:**
- [ ] Command refreshes `mv_neighborhood_poi_counts`
- [ ] Command refreshes `mv_neighborhood_nearest_pois`
- [ ] Supports `--concurrent` flag for `REFRESH MATERIALIZED VIEW CONCURRENTLY`
- [ ] Reports: refresh duration, row counts
- [ ] Fails gracefully if views don't exist yet (Epic L6 not done)

**Technical Notes:**
- `CONCURRENTLY` requires unique index on materialized view
- Standard refresh locks table; concurrent doesn't but is slower
- Consider running after each import automatically

---

## Story O5: Full Refresh Command

> As a pipeline operator, I want to run a full data refresh with a single command, so that I can update everything before a batch processing run.

**New Process:**
```bash
dotnet run -- import all
dotnet run -- import all --skip boundaries   # Skip unchanged data
```

**Acceptance Criteria:**
- [ ] Command runs O1 (OSM), O2 (Statbel), O3 (Boundaries) in sequence
- [ ] Refreshes materialized views (O4) after all imports
- [ ] Supports `--skip` flag to exclude specific imports
- [ ] Reports: total duration, summary of all imports
- [ ] Continues on non-fatal errors, reports at end
- [ ] Exit code reflects overall success/failure

**Orchestration:**
```
import all
  ├── import boundaries (if not --skip)
  ├── import osm
  ├── import statbel
  └── refresh views
```

---

## Story O6: Import Scheduling

> As a pipeline operator, I want imports to run on a schedule, so that data stays fresh without manual intervention.

**Schedule:**
| Import | Frequency | Rationale |
|--------|-----------|-----------|
| OSM POIs | Weekly | OSM updates daily, weekly is sufficient |
| Statbel | Quarterly | Published annually, check quarterly |
| Boundaries | Never | Only on explicit request |
| Views | After each import | Keep aggregates fresh |

**Acceptance Criteria:**
- [ ] Hangfire recurring jobs configured for OSM (weekly) and Statbel (quarterly)
- [ ] Jobs visible in Hangfire dashboard
- [ ] Jobs can be triggered manually from dashboard
- [ ] Jobs log to structured logging (Serilog)
- [ ] Failed jobs retry with exponential backoff
- [ ] Email/Slack notification on failure (optional)

**Technical Notes:**
- Hangfire already set up in Epic M (or P)
- Use cron expressions for scheduling
- Consider running during off-peak hours

---

## Story O7: Content Seed from Astro JSON

> As a pipeline operator, I want to import published content from Astro JSON files into the database, so that I can recover content if the database is lost or repopulate after a fresh install.

**Context:** The Astro content files (`web/src/content/neighborhoods/*.json`) contain AI-generated prose that represents significant processing cost (~$0.15/neighborhood). This command allows recovering that investment.

**CLI Command:**
```bash
dotnet run -- import content
dotnet run -- import content --source ./backup/neighborhoods/
dotnet run -- import content --dry-run
```

**Acceptance Criteria:**
- [ ] Command reads all JSON files from `web/src/content/neighborhoods/`
- [ ] Extracts prose fields: `intro`, `subtitle`, `name`, `city`
- [ ] Looks up `nis_code` from `gis.neighborhoods` using slug
- [ ] Inserts/updates `content.neighborhood_prose` table
- [ ] Skips files where slug doesn't match any neighborhood (logs warning)
- [ ] Reports: imported, skipped, updated counts
- [ ] Supports `--source` flag for alternate directory
- [ ] Supports `--dry-run` to preview without writing
- [ ] Idempotent: can run multiple times safely (upsert)

**Technical Notes:**
- Slug → NIS code lookup via: `SELECT nis_code FROM gis.neighborhoods WHERE slug = $1`
- Some fields not extractable (quality scores, generated_at) — use defaults
- Consider storing `imported_from = 'astro'` to distinguish from API-generated

---

## Dependencies

```
Epic M (Server Infrastructure)
  └── M2 (ASP.NET Core Project)
        └── O1 (OSM Import)
        └── O2 (Statbel Import)
        └── O3 (Boundaries Import)
              └── O4 (Materialized Views) — depends on L6 for view creation
                    └── O5 (Full Refresh)
                          └── O6 (Scheduling) — depends on Hangfire from Epic P
        └── O7 (Content Seed) — depends on O3 for slug→nis_code lookup

Epic L
  └── L6 (Materialized Views) — creates the views that O4 refreshes
```

---

## Current ETL Scripts (Reference)

| File | Purpose | Migrates to |
|------|---------|-------------|
| `database/scripts/statbel/load-statistics.py` | Load population + prices | O2 |
| `database/data/statbel/nis_code_mapping_2025.csv` | Municipality mergers | O2 |
| Various osmium/GDAL commands | POI extraction | O1 |
| `database/migrations/*.sql` | Schema + data | EF Core migrations |

---

## Out of Scope

- Content processing (Epic P)
- Materialized view creation (Epic L, Story L6)
- API endpoints (Epic P)
