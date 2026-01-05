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
psql -U postgres -f agents/scripts/db/01-create-pipeline-database.sql
psql -U postgres -f agents/scripts/db/02-create-pipeline-user.sql
psql -U postgres -d buurtkompas_pipeline -f agents/scripts/db/03-grant-pipeline-permissions.sql
```

**Important:** The third script requires `-d buurtkompas_pipeline` to set default privileges correctly.

## Scripts

| Script | Purpose | Run As |
|--------|---------|--------|
| `01-create-pipeline-database.sql` | Creates the `buurtkompas_pipeline` database | postgres |
| `02-create-pipeline-user.sql` | Creates the `buurtkompas_pipeline` user | postgres |
| `03-grant-pipeline-permissions.sql` | Grants CRUD permissions | postgres (connected to pipeline DB) |

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

## Next Steps

After completing this setup (Story J0), proceed to:
- **Story J1:** Create the `pipeline_jobs` table schema
- **Story J2:** Create Claude Code subagents for each pipeline stage
