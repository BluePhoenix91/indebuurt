---
name: pipeline
description: Run the neighborhood content pipeline
---

# Pipeline Command

Orchestrates the 4-stage neighborhood content pipeline:
**Researcher** -> **Writer** -> **SEO Reviewer** -> **Brand Reviewer**

## Usage

```
/pipeline                      Show this help
/pipeline status               Show progress dashboard
/pipeline <nis_code>           Process single neighborhood (e.g., 41002A0)
/pipeline municipality <nis5>  Process all pending in municipality (e.g., 44021)
/pipeline next [N]             Process next N pending (default: 5, max: 50)
/pipeline retry-failed         Re-process failed jobs (retry_count < 3)
```

---

## Argument Parsing

Parse `$ARGUMENTS` to determine the subcommand:

1. **Empty or "help"** -> Show the usage section above
2. **"status"** -> Execute status dashboard (see below)
3. **"municipality" + 5-digit code** -> Process municipality batch
4. **"next" + optional number** -> Process next N pending
5. **"retry-failed"** -> Re-process failed jobs
6. **7-character NIS code** (format: `DDDDDLD` where D=digit, L=letter) -> Single neighborhood
7. **Otherwise** -> Show error with valid options

---

## Configuration Reference

From `agents/config.ts`:
- **Output path:** `agents/pipeline-outputs/{nis_code}/`
- **Quality threshold:** 70 (for flagging, not blocking)
- **Max retries:** 3
- **Stale timeout:** 30 minutes (jobs in_progress longer are treated as pending)

---

## Database Tools

- **Read queries (pipeline):** `mcp__pipeline__execute_query`
- **Write queries (pipeline):** `mcp__pipeline__execute_dml_ddl_dcl_tcl` (requires commit)
- **GIS queries:** `mcp__gis__query`

---

## Subagents

Invoke using the **Task tool** with these agent types:
- `neighborhood-researcher` - Gathers POI data from PostGIS
- `neighborhood-writer` - Transforms data to Dutch prose
- `neighborhood-seo-reviewer` - Optimizes for search visibility
- `neighborhood-brand-reviewer` - Validates brand voice

---

## /pipeline status

Display a progress dashboard.

### Steps:

1. **Query overall counts:**
```sql
SELECT status, COUNT(*) as count
FROM pipeline_jobs
GROUP BY status
ORDER BY status;
```

2. **Check for stale jobs** (in_progress > 30 minutes):
```sql
SELECT nis_code, current_stage, started_at
FROM pipeline_jobs
WHERE status = 'in_progress'
  AND started_at < NOW() - INTERVAL '30 minutes';
```
If found, display warning with list.

3. **Query municipality breakdown** (top 10 with pending work):
```sql
SELECT
  pj.municipality_nis,
  (SELECT DISTINCT city FROM neighborhoods WHERE LEFT(nis_code, 5) = pj.municipality_nis LIMIT 1) as city,
  COUNT(*) FILTER (WHERE pj.status = 'pending') as pending,
  COUNT(*) FILTER (WHERE pj.status = 'in_progress') as in_progress,
  COUNT(*) FILTER (WHERE pj.status = 'completed') as completed,
  COUNT(*) FILTER (WHERE pj.status = 'failed') as failed
FROM pipeline_jobs pj
GROUP BY pj.municipality_nis
HAVING COUNT(*) FILTER (WHERE pj.status IN ('pending', 'failed')) > 0
ORDER BY pending DESC
LIMIT 10;
```

4. **Query recent activity** (last 10 completed/failed):
```sql
SELECT nis_code, status, seo_score, brand_score, final_score, updated_at
FROM pipeline_jobs
WHERE status IN ('completed', 'failed')
ORDER BY updated_at DESC
LIMIT 10;
```

5. **Quality summary:**
```sql
SELECT
  COUNT(*) as total_completed,
  ROUND(AVG(seo_score), 1) as avg_seo,
  ROUND(AVG(brand_score), 1) as avg_brand,
  ROUND(AVG(final_score), 1) as avg_final,
  COUNT(*) FILTER (WHERE final_score < 70) as needs_review
FROM pipeline_jobs
WHERE status = 'completed';
```

### Output Format:

```
## Pipeline Status

### Overall
- Pending: X
- In Progress: X
- Completed: X
- Failed: X

### Stale Jobs Warning (if any)
These jobs have been in_progress for >30 minutes and may need attention:
- {nis_code}: stuck at {stage} since {started_at}

### By Municipality
| City | Pending | In Progress | Completed | Failed |
|------|---------|-------------|-----------|--------|
| ...  | ...     | ...         | ...       | ...    |

### Recent Activity
| NIS Code | Status | SEO | Brand | Final | Updated |
|----------|--------|-----|-------|-------|---------|
| ...      | ...    | ... | ...   | ...   | ...     |

### Quality Summary
- Completed: X neighborhoods
- Avg SEO Score: X
- Avg Brand Score: X
- Avg Final Score: X
- Needs Review: X (score < 70)
```

---

## /pipeline <nis_code>

Process a single neighborhood through all 4 agents.

### Steps:

1. **Validate NIS code format:**
   - Must be 7 characters
   - Pattern: 5 digits + 1 letter + 1 digit (e.g., `41002A0`, `44021B1`)
   - If invalid, report error and stop

2. **Verify neighborhood exists in GIS database:**
```sql
SELECT nis_code, id, name, city FROM neighborhoods WHERE nis_code = '{nis_code}';
```
If no rows returned, report: "Neighborhood not found: {nis_code}" and stop.

3. **Create or get job record:**
```sql
INSERT INTO pipeline_jobs (nis_code, municipality_nis, status, created_at, updated_at)
VALUES ('{nis_code}', LEFT('{nis_code}', 5), 'pending', NOW(), NOW())
ON CONFLICT (nis_code) DO UPDATE SET updated_at = NOW()
RETURNING id, status, current_stage, retry_count;
```

4. **Check for existing outputs (resume logic):**
   For each stage in order, check if output file exists and is valid:

   | Stage | File | Validation |
   |-------|------|------------|
   | researcher | `agents/pipeline-outputs/{nis_code}/1-researcher.json` | Has `schemaVersion` field |
   | writer | `agents/pipeline-outputs/{nis_code}/2-writer.json` | Has `schemaVersion` field |
   | seo-reviewer | `agents/pipeline-outputs/{nis_code}/3-seo-reviewer.json` | Has `seoReview` object |
   | brand-reviewer | `agents/pipeline-outputs/{nis_code}/4-brand-reviewer.json` | Has `brandReview` object |

   - Read each file, parse as JSON
   - If parse fails or required field missing, treat as incomplete
   - If file exists but validation fails, DELETE the file before re-running stage
   - Start from first incomplete stage

5. **Update job status to in_progress:**
```sql
UPDATE pipeline_jobs
SET status = 'in_progress',
    current_stage = '{first_incomplete_stage}',
    started_at = NOW(),
    error_message = NULL
WHERE nis_code = '{nis_code}';
```
Then commit the transaction.

6. **Run each remaining stage sequentially:**

   For each stage that needs to run:

   a. Report: "Running {stage} for {nis_code}..."

   b. Use the **Task tool** to invoke the subagent:
      ```
      Task: neighborhood-{stage}
      Prompt: Process neighborhood {nis_code}
      ```

   c. After agent completes, verify output file exists and is valid JSON

   d. Update database with stage completion:
   ```sql
   UPDATE pipeline_jobs
   SET current_stage = '{next_stage}',
       {stage}_completed_at = NOW()
   WHERE nis_code = '{nis_code}';
   ```
   Then commit.

   e. If agent fails or output invalid:
   ```sql
   UPDATE pipeline_jobs
   SET status = 'failed',
       error_message = '{stage}: {error_description}',
       last_error_at = NOW(),
       retry_count = retry_count + 1
   WHERE nis_code = '{nis_code}';
   ```
   Then commit and stop processing this neighborhood.

7. **After brand-reviewer completes:**

   a. Read the final output file: `agents/pipeline-outputs/{nis_code}/4-brand-reviewer.json`

   b. Extract scores:
      - `seo_score` = `output.seoReview.qualityScore`
      - `brand_score` = `output.brandReview.qualityScore`
      - `final_score` = `(seo_score + brand_score) / 2`

   c. Update database with completion:
   ```sql
   UPDATE pipeline_jobs
   SET status = 'completed',
       current_stage = NULL,
       brand_reviewer_completed_at = NOW(),
       seo_score = {seo_score},
       brand_score = {brand_score},
       final_score = {final_score},
       completed_at = NOW()
   WHERE nis_code = '{nis_code}';
   ```
   Then commit.

8. **Report completion:**
   ```
   Completed {nis_code} ({name})
   - SEO Score: {seo_score}
   - Brand Score: {brand_score}
   - Final Score: {final_score}
   - Status: {if final_score >= 70: "Ready" else: "Needs Review"}
   ```

---

## /pipeline municipality <nis5>

Process all pending neighborhoods for a municipality.

### Steps:

1. **Validate municipality code:**
   - Must be exactly 5 digits
   - If invalid, report error and stop

2. **Query pending neighborhoods:**
```sql
SELECT n.nis_code, n.name, n.city
FROM neighborhoods n
LEFT JOIN pipeline_jobs pj ON n.nis_code = pj.nis_code
WHERE LEFT(n.nis_code, 5) = '{nis5}'
  AND (
    pj.status IS NULL
    OR pj.status = 'pending'
    OR (pj.status = 'in_progress' AND pj.started_at < NOW() - INTERVAL '30 minutes')
  )
ORDER BY n.name;
```

3. **If no pending neighborhoods:**
   Report: "No pending neighborhoods found for municipality {nis5}"

4. **Report start:**
   ```
   Processing {count} neighborhoods in {city}
   ```

5. **Process each neighborhood:**
   For each `nis_code` in the list:
   - Run the single-neighborhood workflow (see `/pipeline <nis_code>`)
   - Report progress: `[{n}/{total}] Completed {name} (SEO: {seo}, Brand: {brand})`
   - If a neighborhood fails, log it and continue to the next

6. **Report final summary:**
   ```
   ## Municipality {city} Complete
   - Processed: {total}
   - Succeeded: {success_count}
   - Failed: {fail_count}
   - Average Score: {avg_final}
   ```

---

## /pipeline next [N]

Process the next N pending neighborhoods.

### Steps:

1. **Parse N:**
   - Default: 5
   - Maximum: 50 (cap to prevent runaway sessions)

2. **Query next N pending:**
```sql
SELECT n.nis_code, n.name, n.city
FROM neighborhoods n
LEFT JOIN pipeline_jobs pj ON n.nis_code = pj.nis_code
WHERE pj.status IS NULL OR pj.status = 'pending'
  OR (pj.status = 'in_progress' AND pj.started_at < NOW() - INTERVAL '30 minutes')
ORDER BY n.city, n.name
LIMIT {N};
```

3. **If no pending neighborhoods:**
   Report: "No pending neighborhoods found"

4. **Process each:**
   Same as municipality batch - run single-neighborhood workflow, report progress.

5. **Report final summary.**

---

## /pipeline retry-failed

Re-process failed jobs that haven't exceeded max retries.

### Steps:

1. **Query eligible failed jobs:**
```sql
SELECT nis_code, error_message, retry_count, current_stage, last_error_at
FROM pipeline_jobs
WHERE status = 'failed'
  AND retry_count < 3
ORDER BY last_error_at ASC;
```

2. **If no eligible jobs:**
   Report: "No failed jobs eligible for retry (retry_count < 3)"

3. **Report:**
   ```
   Found {count} failed jobs eligible for retry:
   - {nis_code}: {error_message} (attempt {retry_count + 1}/3)
   ```

4. **For each job:**
   - Reset status to allow reprocessing:
   ```sql
   UPDATE pipeline_jobs
   SET status = 'pending',
       error_message = NULL,
       current_stage = NULL
   WHERE nis_code = '{nis_code}';
   ```
   - Run single-neighborhood workflow
   - Report result

5. **Report final summary.**

---

## Error Handling

### Agent Failure
If a subagent fails or produces invalid output:
1. Record error in database with descriptive message
2. Increment retry_count
3. Set status to 'failed'
4. For batch operations: log and continue to next neighborhood

### Database Errors
If MCP database tools fail:
1. Report the error clearly
2. Stop processing (cannot track state without database)

### Invalid JSON Output
If agent writes a file that doesn't parse as JSON:
1. Treat as agent failure
2. Record error: "{stage}: Invalid JSON output"

### Missing Required Fields
If output JSON lacks required fields:
1. Treat as agent failure
2. Record error: "{stage}: Missing required field {field}"

### Cleanup of Invalid Output Files
If an output file exists but fails validation (doesn't parse as JSON, or missing required fields):
1. DELETE the invalid/corrupted file
2. Re-run the stage from scratch
3. Never attempt to repair or merge partial outputs

This ensures a clean state and avoids debugging confusion from half-written files.

---

## Quality Gate

The quality threshold is **70** (from `agents/config.ts`).

- Content with `final_score >= 70`: Marked as ready
- Content with `final_score < 70`: Marked as "needs review" in the database

**Note:** This command does NOT copy files to `src/content/neighborhoods/`. That functionality is part of Story J5 (Quality Gate and Auto-Publish).

To find content needing review:
```sql
SELECT nis_code, final_score, seo_score, brand_score
FROM pipeline_jobs
WHERE status = 'completed' AND final_score < 70;
```
