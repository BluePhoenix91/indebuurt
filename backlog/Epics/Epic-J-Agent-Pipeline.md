# Epic J — Agent Pipeline

**Goal:** Build interactive orchestration using Claude Code CLI that runs agents in sequence, tracks progress across sessions, and outputs validated content files.

**Depends on:** Epic H (Infrastructure) and Epic I (Agent Development)

---

## Approach & Architecture Decision

### The Challenge

We need to run a 4-agent content generation pipeline at scale (2800+ neighborhoods):

```
Researcher → Writer → SEO Reviewer → Brand Reviewer → Final JSON
```

### Options Evaluated

| Approach | How it Works | Cost | Automation |
|----------|--------------|------|------------|
| **A. Anthropic API + Python** | Python script calls Claude API programmatically | ~$0.05-0.15 per neighborhood ($140-420 total) | Full automation, can run unattended |
| **B. Claude Agent SDK** | SDK for building custom agents | Requires API keys (same cost as A) | Full automation |
| **C. Claude Code CLI (chosen)** | Interactive sessions with subagents | $0 (uses Max subscription) | Manual, requires active sessions |

### Why Claude Code CLI?

**Key insight:** The Claude Agent SDK and Anthropic API require separate API keys and pay-per-token billing. They **cannot** use an existing Claude Max subscription. These are completely separate billing systems:

| Feature | Claude Max Subscription | Anthropic API |
|---------|------------------------|---------------|
| Interactive use (claude.ai, Claude Code) | ✅ Included | ❌ Not available |
| Programmatic SDK/API calls | ❌ Not available | ✅ Pay-per-token |
| Automated batch processing | ❌ Not possible | ✅ With 50% batch discount |

**Decision:** Use Claude Code CLI to leverage the existing Max subscription at $0 additional cost, accepting that this requires interactive sessions.

### Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Claude Code CLI                          │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │                  /pipeline command                        │  │
│  │  • status  • city <name>  • next <N>  • retry-failed     │  │
│  └──────────────────────────────────────────────────────────┘  │
│                              │                                  │
│                    ┌─────────┴─────────┐                       │
│                    │    Subagents      │                       │
│                    │ (.claude/agents/) │                       │
│                    │ 1. researcher     │                       │
│                    │ 2. writer         │                       │
│                    │ 3. seo-reviewer   │                       │
│                    │ 4. brand-reviewer │                       │
│                    └─────────┬─────────┘                       │
└──────────────────────────────┼──────────────────────────────────┘
                               │
              ┌────────────────┴────────────────┐
              │                                 │
              ▼                                 ▼
┌──────────────────────────┐    ┌──────────────────────────┐
│   indebuurt_gis (existing)│    │  indebuurt_pipeline (new) │
│   PostgreSQL + PostGIS    │    │  PostgreSQL               │
│   ─────────────────────── │    │  ─────────────────────── │
│   • neighborhoods         │    │  • pipeline_jobs          │
│   • pois                  │    │    - status               │
│   • statistical_sectors   │    │    - current_stage        │
│   • neighborhood_stats    │    │    - quality scores       │
│   ─────────────────────── │    │    - timestamps           │
│   READ-ONLY access        │    │  READ-WRITE access        │
└──────────────────────────┘    └──────────────────────────┘
```

### Database Separation

The pipeline uses **two separate databases**:

| Database | Purpose | Access |
|----------|---------|--------|
| `indebuurt_gis` | GIS data (neighborhoods, POIs, statistics) | **Read-only** — protects source data |
| `indebuurt_pipeline` | Pipeline tracking (jobs, status, scores) | **Read-write** — allows progress updates |

This separation ensures the GIS source data cannot be accidentally modified by the pipeline.

### Claude Code Features Used

| Feature | Location | Purpose |
|---------|----------|---------|
| **Subagents** | `.claude/agents/*.md` | Autonomous agents with specific expertise |
| **Slash commands** | `.claude/commands/*.md` | User-invokable `/pipeline` command |
| **MCP Servers** | `settings.local.json` | Two PostgreSQL connections (GIS read-only, Pipeline read-write) |

### Trade-offs Accepted

**Benefits:**
- $0 API costs (uses existing Max subscription)
- Can start immediately without API setup
- Progress tracked in database, resumable across sessions
- Subagent prompts reusable if migrating to API later

**Limitations:**
- Requires interactive Claude Code sessions (cannot run unattended)
- Cannot schedule automatic runs (no cron/automation)
- Processing time depends on user availability
- Estimated 5-10 minutes per neighborhood

### Time Investment Reality

For 2800 neighborhoods at ~5-10 minutes each:
- **Optimistic:** 230-460 hours of active sessions
- **Realistic:** Process city-by-city over weeks/months
- **Starting point:** Gent (12 neighborhoods) = 1-2 hours

### Future Migration Path

If the manual process proves too slow, the architecture supports migration to API automation:

1. Subagent prompts transfer directly to API calls
2. Database schema remains unchanged
3. Slash command logic converts to Python orchestrator
4. Estimated API cost: $140-420 for all 2800 neighborhoods (using Batch API 50% discount)

---

## Story J0: Pipeline Database and MCP Setup

> As a developer, I want a separate pipeline database with read-write access, so that Claude Code can track progress without risking the GIS source data.

**Context:** The existing `indebuurt_gis` database is configured as read-only via MCP. The pipeline needs write access to track job status, but we don't want to grant write access to GIS data. Solution: create a separate `indebuurt_pipeline` database.

**Acceptance Criteria:**
- [x] New PostgreSQL database `buurtkompas_pipeline` created
- [x] Database user `buurtkompas_pipeline` created with read-write access
- [x] User has NO access to `buurtkompas` GIS database (separation enforced)
- [x] MCP server configuration added to `.mcp.json`:
  - `gis` server (read-only) for GIS data
  - `pipeline` server (read-write) using `mcp-postgres-full-access` package
- [x] Connection tested: can INSERT/UPDATE/DELETE in pipeline database
- [x] SQL setup scripts stored in `agents/scripts/db/`:
  - `01-create-pipeline-database.sql`
  - `02-create-pipeline-user.sql`
  - `03-grant-pipeline-permissions.sql`
- [x] README with setup instructions at `agents/scripts/db/README.md`

---

## Story J1: Pipeline Jobs Schema ✅

> As a developer, I want a `pipeline_jobs` table to track progress, so that I can resume processing across Claude Code sessions.

**Context:** Since Claude Code is interactive (not automated), we need persistent state to track which neighborhoods are pending, in-progress, completed, or failed.

**Acceptance Criteria:**
- [x] `pipeline_jobs` table created in `buurtkompas_pipeline` database with fields:
  - `nis_code` (unique, 7-char) — *Changed from `neighborhood_id` slug to official NIS code*
  - `municipality_nis` (5-char) — *Changed from `city` to NIS prefix for indexed filtering*
  - `status` (pending, in_progress, completed, failed)
  - `current_stage` (researcher, writer, seo_reviewer, brand_reviewer)
  - Stage completion timestamps for each agent
  - `seo_score`, `brand_score`, `final_score`
  - `error_message`, `retry_count`, `last_error_at`
- [x] Indexes on `status` and `municipality_nis` for efficient queries
- [x] SQL script stored in `agents/scripts/db/04-init-pipeline-schema.sql`
- [x] Configuration moved to code (`agents/config.ts`) instead of database table
- [x] Jobs created on-demand by `/pipeline` command — *Changed from bulk seed to on-demand*

**Implementation Notes:**
- **Primary identifier:** Uses `nis_code` (e.g., `44021A1`) instead of slugs (e.g., `gent-binnenstad`) for stability
- **City filtering:** Uses `municipality_nis` (first 5 chars of NIS) with CHECK constraint enforcing derivation
- **No config table:** Configuration (paths, thresholds) lives in `agents/config.ts` since we're always in Claude Code
- **No bulk seeding:** Jobs created when `/pipeline` targets a neighborhood (validated against GIS first)
- **Auto-updated timestamps:** Database trigger updates `updated_at` on every row change

---

## Story J2: Claude Code Subagents ✅

> As a developer, I want Claude Code subagent definitions for each pipeline stage, so that each agent can be invoked with consistent behavior.

**Context:** Subagents in `.claude/agents/` provide specialized autonomous agents that Claude Code can spawn. Each wraps an existing agent prompt from `agents/`.

**Acceptance Criteria:**
- [x] `neighborhood-researcher.md` subagent created
  - Queries PostGIS database for neighborhood data
  - Outputs ResearcherOutput JSON
  - References `agents/researcher/prompt-v1.md` instructions
- [x] `neighborhood-writer.md` subagent created
  - Transforms ResearcherOutput into Dutch prose
  - Outputs WriterOutput JSON
  - References `agents/writer/prompt-v1.md` instructions
- [x] `neighborhood-seo-reviewer.md` subagent created
  - Optimizes content for search visibility
  - Outputs SEOReviewerOutput JSON with quality score
  - References `agents/seo-reviewer/prompt-v1.md` instructions
- [x] `neighborhood-brand-reviewer.md` subagent created
  - Validates brand voice and terminology
  - Outputs BrandReviewerOutput JSON with quality score
  - References `agents/brand-reviewer/prompt-v1.md` instructions
- [x] All subagents follow format of existing `user-story-architect.md`
- [x] Each subagent specifies `model: sonnet` for cost efficiency

**Implementation Notes:**
- **Referencing approach:** "Reference + Read" pattern — subagents are lean (~50 lines) and instruct the agent to read the full prompt file first
- **ID format:** Subagents receive `nis_code` (e.g., `41002A0`) and look up slug ID for helper functions
- **MCP guidance:** Role-based (e.g., "read-only access to GIS data") rather than explicit server names
- **Descriptions:** Minimal (no examples) since `/pipeline` command handles orchestration
- **Color:** All use `green` for visual grouping as "content pipeline"
- **Config update:** `agents/config.ts` updated to use hyphens in `PIPELINE_STAGES` (`seo-reviewer` not `seo_reviewer`)
- **Test output:** Full pipeline tested with `41002A0` (Aalst - Station), sample outputs in `agents/pipeline-outputs/41002A0/`
- **Known issue:** GIS helper functions use slug IDs, so researcher must first look up slug from nis_code

---

## Story J3: Pipeline Slash Command ✅

> As a developer, I want a `/pipeline` slash command that orchestrates the 4 agents, so that I can process neighborhoods with a single command.

**Context:** Slash commands in `.claude/commands/` provide user-invokable commands. The `/pipeline` command coordinates subagents and database updates.

**Acceptance Criteria:**
- [x] `/pipeline status` shows progress dashboard:
  - Neighborhoods by status (pending/in_progress/completed/failed)
  - Breakdown by municipality
  - Recent activity
- [x] `/pipeline <nis_code>` processes single neighborhood through all 4 agents
- [x] `/pipeline municipality <nis5>` processes all pending neighborhoods for a municipality (e.g., `44021` for Gent)
- [x] `/pipeline next <N>` processes next N pending neighborhoods
- [x] `/pipeline retry-failed` re-processes failed items (retry_count < max_retries)
- [x] Command updates `pipeline_jobs` table at each stage
- [x] Intermediate outputs saved to `agents/pipeline-outputs/{nis_code}/`
- [x] Resume logic: checks for existing valid outputs and skips completed stages
- [x] Stale job detection: jobs in_progress > 30 minutes treated as pending
- [x] Write permission added for `agents/pipeline-outputs/**`

**Note:** Uses `nis_code` (7-char, e.g., `44021A1`) as identifier. Municipality filtering uses 5-char prefix (e.g., `44021` = Gent). Configuration from `agents/config.ts`.

**Implementation Notes:**
- Slash command: `.claude/commands/pipeline.md`
- Added `started_at` column to `pipeline_jobs` for stale detection
- Migration script: `agents/scripts/db/05-add-started-at.sql`
- Publishing to `src/content/neighborhoods/` deferred to J5 (Quality Gate)

---

## Story J4: Intermediate Output Storage ✅

> As a developer, I want intermediate outputs saved at each pipeline stage, so that I can debug issues and resume from failures.

**Context:** If a session ends mid-pipeline or an agent fails, we need the previous stage's output to resume without re-running everything.

**Acceptance Criteria:**
- [x] Output directory structure: `agents/pipeline-outputs/{nis_code}/`
- [x] Files saved after each stage:
  - `1-researcher.json` — ResearcherOutput
  - `2-writer.json` — WriterOutput
  - `3-seo-reviewer.json` — SEOReviewerOutput
  - `4-brand-reviewer.json` — BrandReviewerOutput
- [x] Pipeline checks for existing outputs before re-running agent
- [x] `current_stage` in database tracks where to resume
- [x] Timestamps in database track when each stage completed
- [x] Invalid outputs deleted before retry (changed from "preserve for debugging")

**Implementation Notes:**
- Most functionality implemented as part of J2/J3
- Resume logic is instruction-based (`pipeline.md` prompts Claude to check files) not programmatic code
- Outputs gitignored via `.gitignore:79-80`
- Invalid/corrupted output files are deleted before retry, not preserved
- Lightweight validation (field existence checks) used for resume decisions

**Note:** Path derivation handled by `getOutputPath()` in `agents/config.ts`.

---

## Story J5: Quality Gate and Auto-Publish

> As a content team member, I want content auto-published when quality score >= 70, so that good content flows to the site without manual approval.

**Context:** SEO and Brand reviewers output quality scores. Content meeting threshold goes directly to Content Collections.

**Acceptance Criteria:**
- [ ] Quality threshold configurable in `agents/config.ts` (default: 70)
- [ ] Final score = average of SEO score and Brand score
- [ ] Content with score >= threshold:
  - Copied to `src/content/neighborhoods/{nis_code}.json`
  - Status updated to `completed`
  - Completion timestamp recorded
- [ ] Content with score < threshold:
  - Status updated to `failed`
  - Error message includes score breakdown
  - Remains in `pipeline-outputs/` for review
- [ ] `/pipeline status` shows score distribution of completed content

**Note:** Threshold configured via `PIPELINE_CONFIG.qualityThreshold` in `agents/config.ts`.

---

## Story J6: Pipeline Documentation

> As a developer, I want clear documentation for running the pipeline, so that the workflow is repeatable and understandable.

**Context:** The interactive Claude Code approach has specific workflows that need documentation.

**Acceptance Criteria:**
- [ ] `agents/docs/pipeline-usage.md` created with:
  - Prerequisites (database setup, Claude Code configuration)
  - Command reference (`/pipeline status`, `/pipeline city`, etc.)
  - Example workflow for processing a city
  - Troubleshooting common issues
  - Resuming after session interruption
- [ ] Time estimates documented (5-10 min per neighborhood)
- [ ] Best practices for batch processing sessions

---

## Dependencies

```
J0 (Pipeline Database Setup)
  └── J1 (Pipeline Jobs Schema)
        └── J3 (Slash Command)
              ├── J4 (Intermediate Storage)
              └── J5 (Quality Gate)

J2 (Subagents) ─────┘

J6 (Documentation) — parallel, no blockers
```

J0 must be done first (database infrastructure). J1 and J2 can be done in parallel after J0. J3 depends on both J1 and J2. J4 and J5 extend J3.

---

## Technical Notes

### Why Claude Code CLI Instead of API Automation?

**Cost:** Claude Max subscription covers interactive use. API automation would cost ~$0.05-0.15 per neighborhood ($140-420 for 2800 neighborhoods).

**Trade-off:** Requires interactive sessions (~5-10 min per neighborhood). For 2800 neighborhoods, this means processing city-by-city over weeks/months.

**Future option:** Can migrate to API automation later if manual process proves too slow. The subagent prompts and database schema would transfer directly.

### Session Workflow

```
1. Start Claude Code: `claude`
2. Check progress: `/pipeline status`
3. Process a municipality: `/pipeline municipality 44021`  (44021 = Gent)
4. Process single neighborhood: `/pipeline 44021A1`
5. Review any failures: `/pipeline retry-failed`
6. Commit when ready: `git add . && git commit -m "Generated Gent neighborhoods"`
```

**NIS Code Reference:** Municipality NIS codes can be looked up via GIS database. Example: Gent = `44021`, Antwerpen = `11002`, Aalst = `41002`.

### Limitations

- Cannot run unattended (requires active Claude Code session)
- Cannot schedule automatic runs
- Progress depends on user availability
