# Buurtkompas Database

This directory contains the PostgreSQL/PostGIS database schema for Buurtkompas.

## Prerequisites

- PostgreSQL 14+ with PostGIS extension installed
- A database named `buurtkompas`
- GDAL/ogr2ogr installed (for loading GeoJSON data)

## Quick Start

### 1. Create the Database

Connect to PostgreSQL as superuser and run:

```sql
CREATE DATABASE buurtkompas;
\c buurtkompas
CREATE EXTENSION IF NOT EXISTS postgis;

-- Create read-only user for MCP
CREATE USER buurtkompas_readonly WITH PASSWORD 'readonly_local_dev';
GRANT CONNECT ON DATABASE buurtkompas TO buurtkompas_readonly;
```

### 2. Run Migrations & Load Data

**Option A: Use the master setup script (recommended)**

```bash
# From WSL - runs all scripts in order and loads data
chmod +x database/scripts/setup-all.sh
./database/scripts/setup-all.sh
```

Then run the migrations in TablePlus/psql.

**Option B: Manual step-by-step**

Migrations are in `migrations/` and should be run in timestamp order:

```sql
\i database/migrations/20250101_001_initial-schema.sql
-- Then load Statbel GeoJSON with ogr2ogr (see Data Loading section)
\i database/migrations/20250101_002_load-statbel-sectors.sql
-- Then load POI data with setup-all.sh or individual scripts (see Data Loading section)
\i database/migrations/20250101_003_load-pois.sql
```

## Migration Principles

This project follows Entity Framework-style migrations:

1. **Never modify existing migrations** - Once a migration is created and committed, treat it as immutable
2. **Always add new migrations** - Schema changes go in new timestamped files
3. **Migrations are additive** - Each migration builds on the previous database state
4. **Include schema changes with data** - If a data migration needs a new column, add the `ALTER TABLE` in that same migration

### Naming Convention

Migrations use timestamp prefixes: `YYYYMMDD_NNN_description.sql`

### Running Migrations

Run migrations in order by timestamp. Each migration should be idempotent where possible (use `IF NOT EXISTS`, `ON CONFLICT DO UPDATE`, etc.).

## Schema Overview

### Geographic Hierarchy (Belgian NIS Structure)

The database follows the official Belgian NIS (Nationaal Instituut voor de Statistiek) hierarchy:

| Level | Table | Dutch Term | Description | Example for Gent |
|-------|-------|------------|-------------|------------------|
| 5 | (future) | Deelgemeente | Sub-municipality - first 6 chars of NIS code | ~12 (Gentbrugge, Sint-Amandsberg, etc.) |
| 6 | `neighborhoods` | Wijk | Neighborhood - first 7 chars of NIS code | ~54 neighborhoods |
| 7 | `statistical_sectors` | Statistische sector | Statistical sector - full 9-char NIS code | ~201 sectors |

**NIS Code Structure:** `44021A011`
- `44021` = Municipality code (Gent)
- `A` = Sub-municipality letter (Gent-centrum)
- `0` = Neighborhood digit within sub-municipality
- `11` = Statistical sector within neighborhood

### All Tables

| Table | Purpose |
|-------|---------|
| `neighborhoods` | Neighborhood boundaries (Level 6) - user-facing geographic unit |
| `statistical_sectors` | Statistical sector boundaries (Level 7) - finest granularity |
| `pois` | Points of interest from OpenStreetMap (vets, dog parks, etc.) |
| `neighborhood_statistics` | Socioeconomic data from Statbel (linked to neighborhoods) |

## MCP Integration

Claude Code can query this database via MCP (Model Context Protocol). The configuration is in `.mcp.json` at the project root.

**Security:** MCP uses the `buurtkompas_readonly` user which only has SELECT permissions. Claude cannot modify data.

### Testing MCP Connection

After restarting Claude Code, you should be able to ask Claude to run queries like:

- "Run `SELECT PostGIS_Version();`"
- "Run `SELECT * FROM neighborhoods WHERE city = 'Gent' LIMIT 5;`"
- "Run `SELECT * FROM statistical_sectors WHERE city = 'Gent' LIMIT 5;`"
- "Run `SELECT * FROM find_nearest_pois(51.0535, 3.7250, 'vet', 3);`"

## Helper Functions

### `find_nearest_pois(lat, lon, category, limit)`

Find the nearest POIs of a given category to a point.

```sql
SELECT * FROM find_nearest_pois(51.0535, 3.7250, 'vet', 5);
```

### `slugify(text)`

Generate URL-safe slugs from text (handles Belgian/French characters).

```sql
SELECT slugify('Sint-Amandsberg'); -- Returns 'sint-amandsberg'
```

## Data Loading

### Load Statbel Statistical Sectors

#### Step 1: Download the Data

1. Go to https://statbel.fgov.be/en/open-data/statistical-sectors-2024
2. Download "GeoJSON (ZIP)" for Belgian Lambert 1972 (EPSG: 31370)
3. Extract to `database/data/sh_statbel_statistical_sectors_31370_20240101.geojson`

#### Step 2: Run Initial Schema Migration

```sql
\i database/migrations/20250101_001_initial-schema.sql
```

#### Step 3: Load GeoJSON with ogr2ogr

From WSL or a system with GDAL installed:

```bash
ogr2ogr -f "PostgreSQL" \
  "PG:host=localhost dbname=buurtkompas user=postgres password=YOUR_PASSWORD" \
  database/data/sh_statbel_statistical_sectors_31370_20240101.geojson \
  -nln staging_sectors \
  -overwrite \
  -s_srs EPSG:31370 \
  -t_srs EPSG:4326
```

This loads the data into a temporary `staging_sectors` table and transforms coordinates to WGS84.

#### Step 4: Run Data Loading Migration

```sql
\i database/migrations/20250101_002_load-statbel-sectors.sql
```

This script:
- Creates neighborhoods by aggregating sectors (Level 6)
- Loads statistical sectors (Level 7)
- Links sectors to their parent neighborhoods
- Drops the staging table

#### Step 5: Verify

```sql
-- Total counts
SELECT 'neighborhoods' as table_name, COUNT(*) as count FROM neighborhoods
UNION ALL
SELECT 'statistical_sectors', COUNT(*) FROM statistical_sectors;

-- Neighborhoods by province
SELECT province, COUNT(*) FROM neighborhoods GROUP BY province ORDER BY province;

-- Sample Gent neighborhoods (should be ~54)
SELECT id, name, nis_code, sector_count FROM neighborhoods WHERE city = 'Gent' LIMIT 10;

-- Sample Gent sectors (should be ~201)
SELECT id, name, nis_code FROM statistical_sectors WHERE city = 'Gent' LIMIT 10;
```

### Load POI Data from OpenStreetMap

POI data is extracted from OpenStreetMap via the Overpass API.

**Quick method:** Use the master script (recommended):

```bash
chmod +x database/scripts/setup-all.sh
./database/scripts/setup-all.sh
```

**Manual method:** Run individual scripts:

#### Step 1: Fetch POI Data

```bash
chmod +x database/scripts/pois/fetch.sh
./database/scripts/pois/fetch.sh
```

This downloads JSON files (Overpass format) to `database/data/pois/`.

#### Step 2: Convert to GeoJSON

Overpass API returns OSM JSON which ogr2ogr can't read directly. Convert to GeoJSON:

```bash
chmod +x database/scripts/pois/convert-to-geojson.sh
./database/scripts/pois/convert-to-geojson.sh
```

This creates `.geojson` files alongside the `.json` files.

#### Step 3: Load GeoJSON with ogr2ogr

From WSL:

```bash
for f in database/data/pois/*.geojson; do
  ogr2ogr -f "PostgreSQL" \
    "PG:host=localhost dbname=buurtkompas user=postgres password=YOUR_PASSWORD" \
    "$f" -nln staging_poi -append
done
```

#### Step 4: Run POI Migration

```sql
\i database/migrations/20250101_003_load-pois.sql
```

This script:
- Adds domain column to pois table
- Transforms staging data to pois with category/domain parsing
- Creates helper functions for neighborhood queries

#### Step 5: Verify

```sql
-- Count POIs by category
SELECT category, domain, COUNT(*) FROM pois GROUP BY category, domain ORDER BY domain;

-- Test neighborhood query
SELECT * FROM get_pois_in_neighborhood('gent-rabot', 'vet');

-- Test nearest POIs (fallback for empty neighborhoods)
SELECT * FROM get_nearest_pois_to_neighborhood('gent-rabot', 'dog_park', 3);
```

### Future Data Loading

- **H6: Statistics** - Statbel socioeconomic data (not yet implemented)

## Folder Structure

```
database/
├── migrations/           # Timestamped SQL migrations (run in order)
│   ├── 20250101_001_initial-schema.sql
│   ├── 20250101_002_load-statbel-sectors.sql
│   └── 20250101_003_load-pois.sql
├── queries/              # Overpass API queries for POI extraction
│   ├── pets.overpassql
│   ├── shopping.overpassql
│   ├── healthcare.overpassql
│   ├── education.overpassql
│   ├── transport.overpassql
│   └── green.overpassql
├── scripts/              # Automation scripts
│   ├── setup-all.sh      # Master script - runs all steps in order
│   └── pois/             # POI-specific scripts
│       ├── fetch.sh              # Download from Overpass API
│       └── convert-to-geojson.sh # Convert Overpass JSON to GeoJSON
├── data/                 # GeoJSON and other data files (gitignored)
│   ├── .gitkeep
│   └── pois/             # POI GeoJSON files from Overpass
└── README.md
```

## Resetting the Database

To completely reset:

```sql
DROP DATABASE buurtkompas;
CREATE DATABASE buurtkompas;
\c buurtkompas
CREATE EXTENSION IF NOT EXISTS postgis;
```

Then re-run the migrations.

## Connection Details

| Setting | Value |
|---------|-------|
| Host | localhost |
| Port | 5432 |
| Database | buurtkompas |
| Read-only user | buurtkompas_readonly |
| Read-only password | readonly_local_dev |
