# Pipeline Architecture

Technical reference for the neighborhood content pipeline. For usage instructions, see [pipeline-quickstart.md](pipeline-quickstart.md).

---

## System Overview

```
                         Claude Code CLI
  ┌──────────────────────────────────────────────────────────────┐
  │                    /pipeline command                          │
  │  • status  • <nis_code>  • municipality  • next  • retry     │
  └──────────────────────────────────────────────────────────────┘
                              │
                    ┌─────────┴─────────┐
                    │    Subagents      │
                    │ (.claude/agents/) │
                    │ 1. researcher     │
                    │ 2. writer         │
                    │ 3. seo-reviewer   │
                    │ 4. brand-reviewer │
                    └─────────┬─────────┘
                              │
              ┌───────────────┴───────────────┐
              │                               │
              ▼                               ▼
┌──────────────────────────┐    ┌──────────────────────────┐
│   buurtkompas (GIS)      │    │  buurtkompas_pipeline    │
│   PostgreSQL + PostGIS   │    │  PostgreSQL              │
│   ─────────────────────  │    │  ─────────────────────   │
│   • neighborhoods        │    │  • pipeline_jobs         │
│   • pois                 │    │    - status              │
│   • statistical_sectors  │    │    - current_stage       │
│   • neighborhood_stats   │    │    - quality scores      │
│   ─────────────────────  │    │    - timestamps          │
│   READ-ONLY access       │    │  READ-WRITE access       │
└──────────────────────────┘    └──────────────────────────┘
```

---

## Two-Database Architecture

The pipeline uses **two separate databases** to protect source data:

| Database | Purpose | Access | MCP Tool |
|----------|---------|--------|----------|
| `buurtkompas` | GIS data (neighborhoods, POIs, statistics) | **Read-only** | `mcp__gis__query` |
| `buurtkompas_pipeline` | Pipeline tracking (jobs, status, scores) | **Read-write** | `mcp__pipeline__execute_*` |

This separation ensures the GIS source data cannot be accidentally modified by the pipeline.

---

## 4-Agent Workflow

Each neighborhood passes through 4 stages sequentially:

```
Researcher → Writer → SEO Reviewer → Brand Reviewer → [Publish]
```

| Stage | Agent | Input | Output | Purpose |
|-------|-------|-------|--------|---------|
| 1 | `neighborhood-researcher` | NIS code | POI data, statistics | Query PostGIS for facts |
| 2 | `neighborhood-writer` | Researcher output | Dutch prose, labels | Transform data to content |
| 3 | `neighborhood-seo-reviewer` | Writer output | SEO-optimized content | Improve search visibility |
| 4 | `neighborhood-brand-reviewer` | SEO output | Final content + scores | Validate brand voice |

### Stage Output Files

Each stage writes to a numbered JSON file:

```
agents/pipeline-outputs/{nis_code}/
├── 1-researcher.json
├── 2-writer.json
├── 3-seo-reviewer.json
└── 4-brand-reviewer.json
```

The `4-brand-reviewer.json` is the final output that gets published.

---

## Configuration

All configuration lives in `agents/config.ts`:

```typescript
PIPELINE_CONFIG = {
  outputBasePath: 'agents/pipeline-outputs',           // Intermediate outputs
  contentOutputPath: 'web/src/content/neighborhoods',  // Final published location
  qualityThreshold: 70,                                // Min score for auto-publish
  maxRetries: 3,                                       // Max retry attempts
}

PIPELINE_STAGES = ['researcher', 'writer', 'seo-reviewer', 'brand-reviewer']
```

### Key Paths

| Path | Purpose |
|------|---------|
| `agents/pipeline-outputs/{nis_code}/` | Intermediate outputs (always preserved) |
| `web/src/content/neighborhoods/{slug}.json` | Published content (Astro content collection) |

---

## Quality Gate and Auto-Publish

After the brand-reviewer completes:

1. **Calculate final score:**
   ```
   final_score = (seo_score + brand_score) / 2
   ```

2. **Check quality threshold:**
   - If `final_score >= 70` AND POI address validation passes → **Auto-publish**
   - If `final_score < 70` → **Not published** (stays in pipeline-outputs)

3. **POI address validation:**
   - All vets and pet stores must have non-null `municipality` and `postalCode`
   - Prevents Astro schema validation errors

Both passing and below-threshold content is marked `status = 'completed'`. The `published` column indicates whether content was published.

### Manual Publish

Below-threshold content can be published after human review:
```
/pipeline publish <nis_code>
```

---

## Resume Behavior

The pipeline can resume from any stage after interruption.

### How Resume Works

1. Pipeline checks for existing output files in order
2. Each file is validated for required fields:

| Stage | File | Validation |
|-------|------|------------|
| researcher | `1-researcher.json` | Has `schemaVersion` field |
| writer | `2-writer.json` | Has `schemaVersion` field |
| seo-reviewer | `3-seo-reviewer.json` | Has `seoReview` object |
| brand-reviewer | `4-brand-reviewer.json` | Has `brandReview` object |

3. If file exists but fails validation → **File is deleted**
4. Processing starts from first incomplete stage

### Stale Job Detection

Jobs stuck in `in_progress` for >30 minutes are treated as stale:
- Listed as warnings in `/pipeline status`
- Picked up by `/pipeline retry-failed`

---

## Pipeline Jobs Table

The `pipeline_jobs` table tracks all processing state:

| Column | Type | Purpose |
|--------|------|---------|
| `nis_code` | VARCHAR(7) | Neighborhood identifier (unique, primary) |
| `municipality_nis` | VARCHAR(5) | City prefix for filtering (derived from nis_code) |
| `status` | VARCHAR(20) | `pending`, `in_progress`, `completed`, `failed` |
| `current_stage` | VARCHAR(20) | Which stage is running |
| `seo_score` | DECIMAL(5,2) | SEO reviewer score (0-100) |
| `brand_score` | DECIMAL(5,2) | Brand reviewer score (0-100) |
| `final_score` | DECIMAL(5,2) | Average of SEO + Brand |
| `published` | BOOLEAN | Whether content was published |
| `retry_count` | INTEGER | Number of failed attempts |
| `started_at` | TIMESTAMP | When current processing began |
| `completed_at` | TIMESTAMP | When processing finished |

### Useful Queries

**Find pending work:**
```sql
SELECT nis_code, status, current_stage FROM pipeline_jobs
WHERE status IN ('pending', 'failed');
```

**Find unpublished content:**
```sql
SELECT nis_code, final_score FROM pipeline_jobs
WHERE status = 'completed' AND published = FALSE;
```

**Find stale jobs:**
```sql
SELECT nis_code, current_stage, started_at FROM pipeline_jobs
WHERE status = 'in_progress'
  AND started_at < NOW() - INTERVAL '30 minutes';
```

---

## Database Setup

Setup scripts are in `agents/scripts/db/`:

| Script | Purpose |
|--------|---------|
| `01-create-pipeline-database.sql` | Create `buurtkompas_pipeline` database |
| `02-create-pipeline-user.sql` | Create database user |
| `03-grant-pipeline-permissions.sql` | Grant CRUD permissions |
| `04-init-pipeline-schema.sql` | Create `pipeline_jobs` table |
| `05-add-started-at.sql` | Add stale job detection column |
| `06-add-publish-tracking.sql` | Add publish tracking columns |

See `agents/scripts/db/README.md` for detailed setup instructions.

---

## Related Documentation

- [Pipeline Quickstart](pipeline-quickstart.md) - Get started in 5 minutes
- [Pipeline Commands](pipeline-commands.md) - Complete command reference
- [Pipeline Troubleshooting](pipeline-troubleshooting.md) - When things go wrong
- [Testing Runbook](testing-runbook.md) - Manual agent testing (without /pipeline)
