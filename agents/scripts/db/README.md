# Pipeline Database Setup

This directory contains SQL scripts to create and configure the `buurtkompas_pipeline` database, which tracks content generation jobs for the agent pipeline (Epic J).

## Architecture

The pipeline uses **two separate databases** for security isolation:

| Database | Purpose | Access | MCP Server |
|----------|---------|--------|------------|
| `buurtkompas` | GIS data (neighborhoods, POIs, statistics) | **Read-only** | `gis` |
| `buurtkompas_pipeline` | Pipeline job tracking | **Read-write** | `pipeline` |

This separation ensures the GIS source data cannot be accidentally modified by pipeline operations.

## Prerequisites

- PostgreSQL 14+ running on `localhost:5432`
- Superuser access (typically the `postgres` user)
- The main `buurtkompas` database should already be set up (see `database/README.md`)

## Quick Setup

Run all scripts in order as the PostgreSQL superuser:

```bash
# From the project root directory

# Story J0: Database infrastructure
psql -U postgres -f agents/scripts/db/01-create-pipeline-database.sql
psql -U postgres -f agents/scripts/db/02-create-pipeline-user.sql
psql -U postgres -d buurtkompas_pipeline -f agents/scripts/db/03-grant-pipeline-permissions.sql

# Story J1: Schema
psql -U postgres -d buurtkompas_pipeline -f agents/scripts/db/04-init-pipeline-schema.sql

# Story J3: Add started_at for stale job detection
psql -U postgres -d buurtkompas_pipeline -f agents/scripts/db/05-add-started-at.sql

# Story J5: Add publish tracking columns
psql -U postgres -d buurtkompas_pipeline -f agents/scripts/db/06-add-publish-tracking.sql
```

**Important:** Scripts 03+ require `-d buurtkompas_pipeline` to run in the correct database context.

## Migration Principles

> **Never modify existing migration scripts.** Always create new migration scripts for schema changes.

This ensures:
- Existing databases can upgrade incrementally
- Migration history is preserved
- Idempotent scripts (with `IF NOT EXISTS` checks) can be safely re-run

All migrations use idempotent patterns (checking if columns/indexes exist before creating) so they can be run multiple times without error.

## Scripts

| Script | Purpose | Run As |
|--------|---------|--------|
| `01-create-pipeline-database.sql` | Creates the `buurtkompas_pipeline` database | postgres |
| `02-create-pipeline-user.sql` | Creates the `buurtkompas_pipeline` user | postgres |
| `03-grant-pipeline-permissions.sql` | Grants CRUD permissions | postgres (connected to pipeline DB) |
| `04-init-pipeline-schema.sql` | Creates `pipeline_jobs` table | postgres (connected to pipeline DB) |
| `05-add-started-at.sql` | Adds `started_at` column for stale detection | postgres (connected to pipeline DB) |
| `06-add-publish-tracking.sql` | Adds `published`, `published_at` columns | postgres (connected to pipeline DB) |

## Verification

After running the scripts, verify the setup:

### 1. Test Pipeline Database Connection

```bash
psql -U buurtkompas_pipeline -d buurtkompas_pipeline -c "SELECT current_database(), current_user;"
```

Expected output:
```
 current_database     | current_user
----------------------+---------------------
 buurtkompas_pipeline | buurtkompas_pipeline
```

### 2. Test Write Access

```bash
psql -U buurtkompas_pipeline -d buurtkompas_pipeline -c "
  CREATE TABLE test_write (id SERIAL PRIMARY KEY, name TEXT);
  INSERT INTO test_write (name) VALUES ('test');
  SELECT * FROM test_write;
  DROP TABLE test_write;
"
```

### 3. Verify Isolation (Should Fail)

```bash
psql -U buurtkompas_pipeline -d buurtkompas -c "SELECT 1"
```

Expected error: `FATAL: permission denied for database "buurtkompas"`

## MCP Configuration

After database setup, ensure `.mcp.json` is configured with both servers:

```json
{
  "mcpServers": {
    "gis": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "@modelcontextprotocol/server-postgres",
               "postgresql://buurtkompas_readonly:readonly_local_dev@localhost:5432/buurtkompas"]
    },
    "pipeline": {
      "type": "stdio",
      "command": "npx",
      "args": ["-y", "mcp-postgres-full-access",
               "postgresql://buurtkompas_pipeline:pipeline_local_dev@localhost:5432/buurtkompas_pipeline"]
    }
  }
}
```

**Restart Claude Code** after updating `.mcp.json` for changes to take effect.

## Troubleshooting

### "database already exists"

The database was already created. You can either:
- Skip step 1 and continue with user creation
- Drop and recreate: `psql -U postgres -c "DROP DATABASE buurtkompas_pipeline;"`

### "role already exists"

The user was already created. You can either:
- Skip step 2 and continue with permissions
- Drop and recreate: `psql -U postgres -c "DROP USER buurtkompas_pipeline;"`

### "permission denied for database"

This is expected when testing isolation (step 3 of verification). If you see this when connecting to `buurtkompas_pipeline`, check:
1. User was created correctly (step 2)
2. CONNECT was granted (step 2)
3. You're using the correct password

### MCP Connection Issues

If Claude Code can't connect after setup:
1. Verify PostgreSQL is running: `pg_isready -h localhost -p 5432`
2. Test connection manually: `psql -U buurtkompas_pipeline -d buurtkompas_pipeline`
3. Check `.mcp.json` syntax (valid JSON, correct connection strings)
4. Restart Claude Code

### Fallback: Using psql Directly

If MCP issues persist, you can use psql via Bash as a fallback:

```bash
# Read from pipeline database
psql -U buurtkompas_pipeline -d buurtkompas_pipeline -c "SELECT * FROM pipeline_jobs LIMIT 5;"

# Write to pipeline database
psql -U buurtkompas_pipeline -d buurtkompas_pipeline -c "UPDATE pipeline_jobs SET status = 'completed' WHERE id = 1;"
```

## Connection Details

| Setting | Value |
|---------|-------|
| Host | localhost |
| Port | 5432 |
| Database | buurtkompas_pipeline |
| User | buurtkompas_pipeline |
| Password | pipeline_local_dev |

**Note:** These credentials are for local development only. For production, use strong passwords and secure credential management.

## Schema Overview (Story J1)

### pipeline_jobs Table

Tracks the progress of each neighborhood through the content generation pipeline.

| Column | Type | Description |
|--------|------|-------------|
| `id` | SERIAL | Auto-increment primary key |
| `nis_code` | VARCHAR(7) | Neighborhood NIS code (e.g., '41002A0') - **unique identifier** |
| `municipality_nis` | VARCHAR(5) | Municipality prefix for city filtering (e.g., '41002') |
| `status` | VARCHAR(20) | pending, in_progress, completed, or failed |
| `current_stage` | VARCHAR(20) | researcher, writer, seo_reviewer, or brand_reviewer |
| `seo_score` | DECIMAL(5,2) | SEO reviewer quality score (0-100) |
| `brand_score` | DECIMAL(5,2) | Brand reviewer quality score (0-100) |
| `final_score` | DECIMAL(5,2) | Average of SEO and brand scores |
| `published` | BOOLEAN | Whether content was published to content dir |
| `published_at` | TIMESTAMP | When content was published |
| `retry_count` | INTEGER | Number of retry attempts |
| Stage timestamps | TIMESTAMP | When each stage completed |

### Configuration

Pipeline configuration (paths, thresholds) lives in code rather than in the database. This keeps things simple since you're always working from Claude Code and can edit constants directly.

Configuration will be defined in `agents/config.ts` (created in Story J2/J3).

### Example Queries

```sql
-- Get pipeline status summary
SELECT status, COUNT(*) as count
FROM pipeline_jobs
GROUP BY status;

-- Get all pending jobs for a municipality (e.g., Gent = 44021)
SELECT nis_code, status, current_stage
FROM pipeline_jobs
WHERE municipality_nis = '44021' AND status = 'pending';

-- Get failed jobs eligible for retry
SELECT nis_code, error_message, retry_count
FROM pipeline_jobs
WHERE status = 'failed' AND retry_count < 3;
```

## Known Data Issues

See [backlog/Bugs/](../../../backlog/Bugs/) for tracked data issues affecting the pipeline.

Notable: POI postal codes may be incomplete - see `2026-01-06-poi-address-fields-not-extracted.md`

## Next Steps

After completing this setup (Story J1), proceed to:
- **Story J2:** Create Claude Code subagents for each pipeline stage
- **Story J3:** Create the `/pipeline` slash command
