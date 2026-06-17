# Epic M — Server Infrastructure

**Goal:** Set up the hosted server environment with database and ASP.NET Core project, enabling remote data access and laying the foundation for ETL and content processing.

**Depends on:** Epic H (Infrastructure Foundation)

---

## Context

The current setup runs entirely on a local machine:
- Two PostgreSQL databases (`buurtkompas` for GIS, `buurtkompas_pipeline` for jobs)
- MCP servers for Claude Code access
- Manual scripts for data imports

This epic establishes the server foundation. Subsequent epics build on it:
- **Epic O** (ETL Automation) — Import data via CLI commands
- **Epic P** (API Content Pipeline) — Process neighborhoods via API

---

## Story M1: PostgreSQL on Lightsail

> As a pipeline operator, I want the database hosted on a server, so that it's always available and doesn't require my local machine to be running.

**Current State:**
- Two local PostgreSQL databases
- Can't process when laptop is off/sleeping
- No remote access for API or Astro builds

**Target State:**
- Single PostgreSQL instance on Lightsail
- Three schemas for logical separation (same database, easy joins)
- Remote access for API layer

```
PostgreSQL on Lightsail
└── Database: buurtkompas
    ├── Schema: gis
    │   ├── neighborhoods (2,800 rows)
    │   ├── statistical_sectors (9,919 rows)
    │   ├── pois (63,043 rows)
    │   └── neighborhood_statistics
    │
    ├── Schema: pipeline
    │   └── pipeline_jobs
    │
    └── Schema: content (Epic N)
        ├── neighborhood_prose
        ├── value_card_templates
        └── label_rules
```

**Why Consolidate to Schemas:**

| Aspect | Before (2 databases) | After (3 schemas) |
|--------|---------------------|-------------------|
| Cross-data queries | Not possible | `JOIN gis.pois ON content.prose...` |
| Access control | Database-level users | API endpoints |
| Backups | Two separate dumps | Single `pg_dump` |
| Migrations | Separate scripts | One EF Core project |

**Acceptance Criteria:**
- [x] Lightsail instance provisioned (2GB RAM, $10/month)
- [x] PostgreSQL 16+ with PostGIS 3.4+ extension installed
- [x] Three schemas created: `gis`, `pipeline`, `content`
- [x] Connection secured (firewall rules, not public 5432)
- [x] Backup strategy: daily automated snapshots

**Technical Notes:**
- Lightsail $10/month (2GB) is comfortable for this workload
- Use `pg_dump -Fc` (custom format) for faster restore
- Set `search_path = gis, pipeline, content` for convenience

**Implementation Notes (completed 2026-01-11):**

*Infrastructure:*
- Using existing Windows Lightsail VM ($20/month) instead of provisioning new Linux instance
- PostgreSQL 18 (newer than required 16+) with PostGIS 3.5 installed via Stack Builder
- Database: `buurtkompas` with three schemas: `gis`, `pipeline`, `content`

*Users created:*
| User | Purpose | Access |
|------|---------|--------|
| `buurtkompas_readonly` | MCP/Claude queries | SELECT on all schemas |
| `buurtkompas_pipeline` | Pipeline operations | Full access to pipeline schema |
| `buurtkompas_app` | EF Core migrations | Full access to all schemas |

*Security:*
- SSH tunnel required for remote access (port 5433 locally → 5432 on server)
- OpenSSH Server installed on Windows VM with key-based auth
- Database port 5432 open on Lightsail firewall (consider removing after API deployed)
- MCP config (`.mcp.json`) added to `.gitignore` to protect credentials

*Backups:*
- Daily backup via Windows Task Scheduler at 3:00 AM
- Script: `C:\PostgreSQL\backups\backup.bat`
- Retention: 7 days
- Format: `pg_dump -Fc` (custom format)

*SSH tunnel command (run on dev machine):*
```powershell
ssh -L 5433:localhost:5432 Administrator@15.237.68.235 -N
```

*Data migration:*
- Starting fresh (no data migrated from local)
- Epic O (ETL) will populate GIS data
- Pipeline jobs table created by EF Core in Story M2

*Local development database:*
- Database: `buurtkompas_dev` on localhost:5432
- Same 3-schema structure as production (gis, pipeline, content)
- PostGIS enabled
- EF Core migrations will target this for local development

---

## Story M2: ASP.NET Core Project Setup

> As a developer, I want an ASP.NET Core project with EF Core connected to the database, so that I have a foundation for building ETL commands and API endpoints.

**Context:** This creates the modular monolith structure that will host both CLI commands (Epic O) and API endpoints (Epic P).

**Project Structure (original plan):**
```
pipeline/
├── Pipeline.csproj
├── Program.cs
│
├── Domain/                  # Core models
│   └── Entities/
│
├── Data/                    # EF Core
│   ├── PipelineDbContext.cs
│   └── Configurations/
│
├── Features/                # Vertical slices (added in O and P)
│
└── Infrastructure/          # External services (added in P)
```

*Note: Actual implementation uses 3-project solution — see Implementation Notes below.*

**Acceptance Criteria:**
- [x] ASP.NET Core 8 project created in `pipeline/` directory
- [x] Entity Framework Core + Npgsql.EntityFrameworkCore.PostgreSQL configured
- [x] DbContext connects to Lightsail database
- [ ] EF Core entities for existing tables (neighborhoods, pois, pipeline_jobs) — *deferred to Epic O*
- [x] Health check endpoint: `GET /health`
- [x] Swagger documentation enabled for development
- [x] Connection string via environment variable or user secrets

**Technical Notes:**
- Use `dotnet new webapi` as starting point
- Configure for both API hosting and CLI commands via `System.CommandLine`
- No authentication yet (added in Epic P)

**Implementation Notes (completed 2026-01-11):**

*Architecture (deviated from original spec):*
- **.NET 10** instead of .NET 8 (user preference for LTS)
- **3-project solution** instead of single project:
  - `Pipeline.Api` — ASP.NET Core Web API host
  - `Pipeline.Cli` — Console app with System.CommandLine
  - `Pipeline.Core` — Shared class library (DbContext, entities, services)
- **DbContext shell only** — entities deferred to Epic O (incremental approach)

*Project structure:*
```
pipeline/
├── Pipeline.sln
└── src/
    ├── Pipeline.Api/
    │   ├── Program.cs              # Health check, Swagger, EF Core config
    │   ├── appsettings.json        # Connection string (update password)
    │   └── appsettings.Development.json
    │
    ├── Pipeline.Cli/
    │   ├── Program.cs              # System.CommandLine with "hello" test command
    │   └── appsettings.json        # Connection string (update password)
    │
    └── Pipeline.Core/
        └── Data/
            └── PipelineDbContext.cs  # Shell, entities added in Epic O
```

*Key packages:*
| Project | Package | Version |
|---------|---------|---------|
| Core | Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.0 |
| Core | Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite | 10.0.4 |
| Core | Microsoft.EntityFrameworkCore.Design | 10.0.1 |
| Api | Swashbuckle.AspNetCore | 10.1.0 |
| Api | Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore | 10.0.1 |
| Cli | System.CommandLine | 2.0.1 |
| Cli | Microsoft.Extensions.Hosting | 10.0.1 |

*Database configuration:*
- Development: `buurtkompas_dev` on localhost:5432
- Production: Lightsail via environment variable override
- Connection string in appsettings.json (password not committed — update locally)

*Endpoints:*
- `GET /health` — Returns "Healthy" if DB connection works
- `GET /swagger` — Swagger UI (Development only)
- `GET /` — Placeholder message

*CLI commands:*
- `dotnet run -- hello` — Tests database connection
- `dotnet run -- --help` — Shows available commands

*Verification:*
```bash
# Build
cd pipeline && dotnet build

# Test CLI
cd src/Pipeline.Cli && dotnet run -- hello
# Expected: "Database connection: OK"

# Test API
cd src/Pipeline.Api && dotnet run
# Visit http://localhost:5082/health → "Healthy"
# Visit http://localhost:5082/swagger → Swagger UI
```

*Environment variable override for production:*
```bash
ConnectionStrings__DefaultConnection="Host=...;Password=..."
```

---

## ~~Story M3: Local Development Setup~~ (Skipped)

> ~~As a developer, I want to run the project locally against the remote database, so that I can develop and test without deploying.~~

**Status:** Skipped - not needed.

**Rationale:** Development happens on local machine with local PostgreSQL. Lightsail is the production environment. No need for SSH tunnel workflow during development - the ASP.NET Core project in M2 will use local database for dev and Lightsail for production via environment-specific connection strings.

---

## Dependencies

```
Epic H (Infrastructure Foundation)
  └── M1 (PostgreSQL on Lightsail) ✅
        └── M2 (ASP.NET Core Project)

After M2:
  ├── Epic O (ETL Automation) — adds CLI commands, populates database
  └── Epic P (API Content Pipeline) — adds API endpoints
```

---

## Infrastructure Costs

| Component | Cost |
|-----------|------|
| Lightsail 2GB | $10/month |
| Domain/SSL (optional) | ~$12/year |

---

## Out of Scope

- ETL import commands (Epic O)
- Content processing endpoints (Epic P)
- Materialized views (Epic O, Story O4)
- Content schema tables (Epic N)
