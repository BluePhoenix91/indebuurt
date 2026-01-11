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
- [ ] Lightsail instance provisioned (2GB RAM, $10/month)
- [ ] PostgreSQL 16+ with PostGIS 3.4+ extension installed
- [ ] Three schemas created: `gis`, `pipeline`, `content`
- [ ] Connection secured (firewall rules, not public 5432)
- [ ] Backup strategy: daily automated snapshots

**Technical Notes:**
- Lightsail $10/month (2GB) is comfortable for this workload
- Use `pg_dump -Fc` (custom format) for faster restore
- Set `search_path = gis, pipeline, content` for convenience

---

## Story M2: ASP.NET Core Project Setup

> As a developer, I want an ASP.NET Core project with EF Core connected to the database, so that I have a foundation for building ETL commands and API endpoints.

**Context:** This creates the modular monolith structure that will host both CLI commands (Epic O) and API endpoints (Epic P).

**Project Structure:**
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

**Acceptance Criteria:**
- [ ] ASP.NET Core 8 project created in `pipeline/` directory
- [ ] Entity Framework Core + Npgsql.EntityFrameworkCore.PostgreSQL configured
- [ ] DbContext connects to Lightsail database
- [ ] EF Core entities for existing tables (neighborhoods, pois, pipeline_jobs)
- [ ] Health check endpoint: `GET /health`
- [ ] Swagger documentation enabled for development
- [ ] Connection string via environment variable or user secrets

**Technical Notes:**
- Use `dotnet new webapi` as starting point
- Configure for both API hosting and CLI commands via `System.CommandLine`
- No authentication yet (added in Epic P)

---

## Story M3: Local Development Setup

> As a developer, I want to run the project locally against the remote database, so that I can develop and test without deploying.

**Acceptance Criteria:**
- [ ] SSH tunnel script for secure local access to Lightsail PostgreSQL
- [ ] `appsettings.Development.json` configured for local development
- [ ] `dotnet run` starts API on localhost
- [ ] README with setup instructions for new developers
- [ ] Local MCP configs removed (no longer needed)

**Technical Notes:**
```bash
# SSH tunnel for local development
ssh -L 5432:localhost:5432 user@lightsail-ip

# Then connection string uses localhost:5432
```

---

## Dependencies

```
Epic H (Infrastructure Foundation)
  └── M1 (PostgreSQL on Lightsail)
        └── M2 (ASP.NET Core Project)
              └── M3 (Local Development)

After M3:
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
- Materialized views (Epic L, Story L6)
- Content schema tables (Epic N)
