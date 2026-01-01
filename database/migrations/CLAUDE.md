# Database Migrations

Entity Framework-style migrations for the Buurtkompas database.

## Core Principles

1. **Never modify existing migrations** - Once committed, treat as immutable
2. **Always add new migrations** - Schema changes go in new timestamped files
3. **Migrations are additive** - Each builds on previous database state
4. **Include schema changes with data** - If a migration needs a new column, add `ALTER TABLE` in that same migration

## Naming Convention

```
YYYYMMDD_NNN_description.sql
```

Examples:
- `20250101_001_initial-schema.sql`
- `20250101_002_load-statbel-sectors.sql`
- `20250101_003_load-pois.sql`

## Development Workflow

### When developing a new migration:

1. **Create the migration file** with the full SQL that would work on a fresh database
2. **Test incrementally** by running individual statements directly in TablePlus/psql
3. **Fix issues inline** - run corrected SQL directly, don't re-run the whole migration
4. **Update the migration file** to reflect what actually works
5. **Commit** the final, working migration

### Example: Fixing a function during development

```sql
-- Migration file has:
CREATE OR REPLACE FUNCTION my_func() RETURNS INTEGER AS $$ ... $$ LANGUAGE plpgsql;

-- You find a bug. DON'T re-run the whole migration.
-- Instead, run the fixed version directly in TablePlus:
CREATE OR REPLACE FUNCTION my_func() RETURNS INTEGER AS $$ ...fixed... $$ LANGUAGE plpgsql;

-- Then update the migration file with the fix for future use.
```

### Example: Changing a function's return type

```sql
-- This fails because PostgreSQL can't change return types:
CREATE OR REPLACE FUNCTION my_func() RETURNS TABLE(...new columns...) AS $$ ... $$;

-- Fix: Drop first, then create
DROP FUNCTION IF EXISTS my_func(VARCHAR);  -- Include parameter types!
CREATE OR REPLACE FUNCTION my_func() RETURNS TABLE(...) AS $$ ... $$;

-- Update migration file to include the DROP before CREATE
```

## Idempotency

Use these patterns to make migrations re-runnable:

```sql
-- Tables
CREATE TABLE IF NOT EXISTS ...

-- Columns
ALTER TABLE x ADD COLUMN IF NOT EXISTS y ...

-- Indexes
CREATE INDEX IF NOT EXISTS ...

-- Data
INSERT INTO ... ON CONFLICT DO NOTHING;
-- or
INSERT INTO ... ON CONFLICT (id) DO UPDATE SET ...;

-- Functions (usually safe to replace)
CREATE OR REPLACE FUNCTION ...

-- Functions with changed return types (must drop first)
DROP FUNCTION IF EXISTS func_name(parameter_types);
CREATE OR REPLACE FUNCTION ...
```

## Migration Structure

A typical migration includes:

```sql
-- 1. Header comment explaining purpose and prerequisites
-- Load POI Data from Overpass API
-- Prerequisites: Run fetch-pois.sh, then load GeoJSON with ogr2ogr

-- 2. Schema changes (if needed)
ALTER TABLE pois ADD COLUMN IF NOT EXISTS domain VARCHAR(50);
CREATE INDEX IF NOT EXISTS idx_pois_domain ON pois (domain);

-- 3. Data transformations
INSERT INTO pois (...) SELECT ... FROM staging_table ON CONFLICT DO NOTHING;

-- 4. Helper functions
CREATE OR REPLACE FUNCTION get_something(...) RETURNS ... AS $$ ... $$ LANGUAGE plpgsql;

-- 5. Permissions
GRANT SELECT ON table_name TO buurtkompas_readonly;
GRANT EXECUTE ON FUNCTION func_name TO buurtkompas_readonly;

-- 6. Verification queries (optional, for manual checking)
SELECT COUNT(*) FROM pois;
```

## Staging Tables

We use a Bronze/Silver pattern:
- **Staging tables** (`staging_*`): Raw data loaded by ogr2ogr
- **Final tables** (`pois`, `neighborhoods`): Cleaned, typed, indexed data

Staging tables are kept for reproducibility. Don't drop them in migrations.

## Common Gotchas

### Table names from ogr2ogr
ogr2ogr creates tables with the name you specify in `-nln`. Check actual table name:
```sql
SELECT table_name FROM information_schema.tables WHERE table_name LIKE 'staging%';
```

### Column ambiguity in functions
When joining tables in functions, alias columns to avoid "column reference is ambiguous":
```sql
-- Bad
SELECT category, COUNT(*) FROM pois p, neighborhoods n ...

-- Good
SELECT p.category AS cat, COUNT(*) as cnt FROM pois p, neighborhoods n ...
```

### Function parameter types in DROP
Include parameter types when dropping functions:
```sql
DROP FUNCTION IF EXISTS my_func(VARCHAR);  -- Not just my_func()
```

## No Ups/Downs

We don't implement rollback migrations. If you need to undo:
1. Create a new migration that reverses the changes
2. Or reset the database and re-run all migrations

This keeps things simple without building a custom ORM.

## Related Files

- Scripts: `database/scripts/`
- Overpass queries: `database/queries/`
- Main docs: `database/README.md`
