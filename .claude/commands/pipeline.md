---
name: pipeline
description: Run the neighborhood content pipeline
---

# Pipeline Command

Orchestrates the 3-stage neighborhood content pipeline:
**Researcher** -> **Writer** -> **Quality Reviewer**

## Usage

```
/pipeline                      Show this help
/pipeline status               Show progress dashboard
/pipeline sessions             Show active parallel sessions
/pipeline <nis_code>           Process single neighborhood (e.g., 41002A0)
/pipeline municipality <nis5>  Process all pending in municipality (e.g., 44021)
/pipeline next [N]             Process next N pending (default: 5, max: 50)
/pipeline retry-failed         Re-process failed jobs (retry_count < 3)
/pipeline publish <nis_code>   Manually publish completed neighborhood
/pipeline claim [auto|<nis5>] [session-name]  Claim municipality for parallel processing
/pipeline release [<nis5>|all]               Release claimed municipalities
/pipeline regenerate <nis_code>              Re-run pipeline on existing content
/pipeline regenerate municipality <nis5>     Regenerate all in municipality
```

### Feature Flag: --separate-reviewers

Add `--separate-reviewers` to use the legacy 4-stage pipeline with separate SEO and Brand reviewers:

```
/pipeline <nis_code> --separate-reviewers
/pipeline municipality <nis5> --separate-reviewers
/pipeline next N --separate-reviewers
/pipeline regenerate <nis_code> --separate-reviewers
```

When `--separate-reviewers` is present:
- Use 4-stage pipeline: researcher → writer → seo-reviewer → brand-reviewer
- Output files: `3-seo-reviewer.json` + `4-brand-reviewer.json`
- Final scores extracted from `seoReview` and `brandReview` objects

**Default (no flag):**
- Use 3-stage pipeline: researcher → writer → quality-reviewer
- Output file: `3-quality-reviewer.json`
- Final scores extracted from `qualityReview` object

---

## Argument Parsing

Parse `$ARGUMENTS` to determine the subcommand:

**First, check for `--separate-reviewers` flag:**
- If present, set `use_separate_reviewers = true` and remove flag from arguments
- Otherwise, set `use_separate_reviewers = false` (default: use quality-reviewer)

**Then parse remaining arguments:**

1. **Empty or "help"** -> Show the usage section above
2. **"status"** -> Execute status dashboard (see below)
3. **"sessions"** -> Show active parallel sessions (see below)
4. **"municipality" + 5-digit code** -> Process municipality batch
5. **"next" + optional number** -> Process next N pending
6. **"retry-failed"** -> Re-process failed jobs
7. **"publish" + 7-character NIS code** -> Manual publish subcommand
8. **"claim" + [auto|5-digit code] + [session-name]** -> Claim municipality for parallel processing
9. **"release" + [5-digit code|all]** -> Release claimed municipalities
10. **"regenerate" + 7-char NIS code** -> Regenerate single neighborhood
11. **"regenerate" + "municipality" + 5-digit code** -> Regenerate municipality
12. **7-character NIS code** (format: `DDDDDLD` where D=digit, L=letter) -> Single neighborhood
13. **Otherwise** -> Show error with valid options

---

## Configuration Reference

From `agents/config.ts`:
- **Output path:** `agents/pipeline-outputs/{nis_code}/`
- **Content path:** `web/src/content/neighborhoods/{slug}.json` (slug from brand-reviewer output `id` field)
- **Quality threshold:** 70 (auto-publish if score >= 70)
- **Max retries:** 3
- **Stale timeout:** 30 minutes (jobs/claims inactive longer are released)
- **Claim stale timeout:** 30 minutes (claims without heartbeat are auto-released)

---

## Database Tools

**CRITICAL: Two separate databases - use the correct tool!**

The databases are separated by purpose:
- **GIS database** (read-only): Contains geographic/source data - neighborhood boundaries, POIs, statistics. Shared across all systems.
- **Pipeline database** (read-write): Contains pipeline state - job status, claims. Only used by the content pipeline.

You cannot JOIN across these databases - they are separate PostgreSQL instances.

| Tool | Database | Tables | Use For |
|------|----------|--------|---------|
| `mcp__gis__query` | GIS (PostGIS) | `neighborhoods`, `pois`, `neighborhood_statistics` | Verifying NIS codes exist, getting neighborhood names/cities |
| `mcp__pipeline__execute_query` | Pipeline | `pipeline_jobs`, `pipeline_claims` | **SELECT only** - reading job status, claims |
| `mcp__pipeline__execute_dml_ddl_dcl_tcl` | Pipeline | `pipeline_jobs`, `pipeline_claims` | **INSERT/UPDATE/DELETE** - all writes (requires commit) |

**Common mistakes:**
- Using `mcp__pipeline__execute_query` to look up a NIS code in `neighborhoods` table will fail because that table only exists in the GIS database.
- Using `mcp__pipeline__execute_query` for DELETE/UPDATE/INSERT will fail - use `mcp__pipeline__execute_dml_ddl_dcl_tcl` instead.

**Tool selection rule:** If the SQL starts with SELECT → use `execute_query`. If it starts with INSERT/UPDATE/DELETE/CREATE/ALTER/DROP → use `execute_dml_ddl_dcl_tcl` and then immediately commit.

### Transaction Error Recovery

The `mcp-postgres-full-access` MCP server can get stuck in an aborted transaction state after certain errors. If you see:
```
current transaction is aborted, commands ignored until end of transaction block
```

**Why this happens:** PostgreSQL marks a transaction as "aborted" when any query fails, and refuses to execute further commands until you explicitly ROLLBACK. The MCP server keeps a persistent connection, so the aborted state persists across queries.

**Recovery steps:**
1. **Immediately call** `mcp__pipeline__execute_rollback` with any transaction ID (e.g., `"recovery"`)
2. If that fails, the MCP server connection needs to be restarted (user must restart Claude Code)
3. As a workaround, use `mcp__gis__query` for read operations on GIS data (neighborhoods, POIs) - this uses a separate connection that won't be affected

**Prevention:** When a `mcp__pipeline__execute_query` call fails, immediately attempt a rollback before continuing.

**IMPORTANT:** If you find yourself in this stuck state, DO NOT keep retrying queries. Call `mcp__pipeline__execute_rollback` first to clear the aborted transaction.

---

## Subagents

Invoke using the **Task tool** with these agent types:
- `neighborhood-researcher` - Gathers POI data from PostGIS
- `neighborhood-writer` - Transforms data to Dutch prose
- `neighborhood-quality-reviewer` - Combined SEO + Brand review (default 3-stage pipeline)
- `neighborhood-seo-reviewer` - Optimizes for search visibility (4-stage pipeline, use with `--separate-reviewers` flag)
- `neighborhood-brand-reviewer` - Validates brand voice (4-stage pipeline, use with `--separate-reviewers` flag)

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
   Use `mcp__pipeline__execute_query`:
```sql
SELECT
  pj.municipality_nis,
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

   Then use `mcp__gis__query` to get city names for the municipality codes returned above:
```sql
SELECT DISTINCT city, LEFT(nis_code, 5) as municipality_nis
FROM neighborhoods
WHERE LEFT(nis_code, 5) IN ('{nis5_1}', '{nis5_2}', ...);
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
  COUNT(*) FILTER (WHERE published = TRUE) as published,
  COUNT(*) FILTER (WHERE published = FALSE) as unpublished,
  ROUND(AVG(final_score), 1) as avg_final,
  COUNT(*) FILTER (WHERE final_score < 70) as below_threshold
FROM pipeline_jobs
WHERE status = 'completed';
```

6. **Query active sessions** (brief summary):
   Use `mcp__pipeline__execute_query`:
```sql
SELECT
  pc.claimed_by as session,
  pc.municipality_nis,
  pc.neighborhoods_completed || '/' || pc.neighborhoods_total as progress,
  EXTRACT(EPOCH FROM (NOW() - pc.heartbeat_at)) / 60 as minutes_ago
FROM pipeline_claims pc
WHERE pc.heartbeat_at > NOW() - INTERVAL '30 minutes'
ORDER BY pc.heartbeat_at DESC;
```
   Then use `mcp__gis__query` to get city names (same pattern as step 3).

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
- Published: X | Unpublished: X
- Avg Final Score: X
- Below threshold (<70): X

### Active Sessions (if any)
| Session | Municipality | City | Progress |
|---------|--------------|------|----------|
| terminal-1 | 44021 | Gent | 12/28 |

Use `/pipeline sessions` for detailed session info.
```

---

## /pipeline <nis_code>

Process a single neighborhood through the agent pipeline.

**Pipeline modes:**
- **3-stage (default):** researcher → writer → quality-reviewer
- **4-stage (with `--separate-reviewers`):** researcher → writer → seo-reviewer → brand-reviewer

### Steps:

1. **Validate NIS code format:**
   - Must be 7 characters
   - Pattern: 5 digits + 1 letter + 1 digit (e.g., `41002A0`, `44021B1`)
   - If invalid, report error and stop

2. **Verify neighborhood exists in GIS database:**
   **IMPORTANT:** Use `mcp__gis__query` for this lookup (NOT `mcp__pipeline__execute_query`):
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

   **Default 3-stage pipeline:**

   | Stage | File | Validation |
   |-------|------|------------|
   | researcher | `agents/pipeline-outputs/{nis_code}/1-researcher.json` | Has `schemaVersion` field |
   | writer | `agents/pipeline-outputs/{nis_code}/2-writer.json` | Has `schemaVersion` field |
   | quality-reviewer | `agents/pipeline-outputs/{nis_code}/3-quality-reviewer.json` | Has `qualityReview` object |

   **If using `--separate-reviewers` (4-stage pipeline):**

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
    error_message = NULL,
    claimed_by = '{session_name}',  -- NULL if not in a claim session
    heartbeat_at = NOW()
WHERE nis_code = '{nis_code}';
```
Then commit the transaction.

   **Note:** If this is being run as part of a `/pipeline claim` session, include the `claimed_by` and `heartbeat_at` values. If run standalone, these can be NULL.

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
       {stage}_completed_at = NOW(),
       heartbeat_at = NOW()
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

7. **After final review stage completes:**

   **Default 3-stage pipeline:**

   a. Read the final output file: `agents/pipeline-outputs/{nis_code}/3-quality-reviewer.json`

   b. Extract scores from `qualityReview` object:
      - `seo_score` = `output.qualityReview.seoScore`
      - `brand_score` = `output.qualityReview.brandScore`
      - `final_score` = `output.qualityReview.qualityScore`

   c. Update database with completion:
   ```sql
   UPDATE pipeline_jobs
   SET status = 'completed',
       current_stage = NULL,
       seo_reviewer_completed_at = NOW(),
       brand_reviewer_completed_at = NOW(),
       seo_score = {seo_score},
       brand_score = {brand_score},
       final_score = {final_score},
       completed_at = NOW()
   WHERE nis_code = '{nis_code}';
   ```
   Then commit.

   **If using `--separate-reviewers` (4-stage pipeline):**

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

8. **Quality gate and auto-publish:**

   a. Check if `final_score >= 70` (quality threshold from config)

   b. Extract the slug from the final output:
      - **Default:** Read `agents/pipeline-outputs/{nis_code}/3-quality-reviewer.json`
      - **If `--separate-reviewers`:** Read `agents/pipeline-outputs/{nis_code}/4-brand-reviewer.json`
      - Extract the `id` field value (e.g., `"aalst-aalst-station"`)
      - This slug becomes the published filename

   c. **Validate POI address fields** (prevents Astro schema errors):
      - Check all items in `vets.practices[]` and `petStores.stores[]`
      - Each must have non-null `municipality` and `postalCode`
      - If any are null, block publishing and report:
        ```
        Cannot publish: POI address data incomplete
        - {count} items have null municipality/postalCode
        See bug: backlog/Bugs/2026-01-06-poi-address-fields-not-extracted.md
        ```
      - Set `published = FALSE` and continue (job is still completed)

   d. **If final_score >= 70 AND address validation passes:**
      - Create directory if needed: `mkdir -p web/src/content/neighborhoods/`
      - Copy the final output to content directory using the slug as filename:
        - **Default:** `agents/pipeline-outputs/{nis_code}/3-quality-reviewer.json` -> `web/src/content/neighborhoods/{slug}.json`
        - **If `--separate-reviewers`:** `agents/pipeline-outputs/{nis_code}/4-brand-reviewer.json` -> `web/src/content/neighborhoods/{slug}.json`
      - Update database with publish status:
        ```sql
        UPDATE pipeline_jobs
        SET published = TRUE, published_at = NOW()
        WHERE nis_code = '{nis_code}';
        ```
        Then commit.

   e. **If final_score < 70:**
      - Do NOT publish (content stays in pipeline-outputs only)
      - `published` remains FALSE in database

9. **Report completion:**
   ```
   Completed {nis_code} ({name})
   - SEO Score: {seo_score}
   - Brand Score: {brand_score}
   - Final Score: {final_score}
   - Published: {if published: "Yes -> web/src/content/neighborhoods/{slug}.json" else: "No (score below 70)"}
   ```

   If not published, add hint:
   ```
   Use `/pipeline publish {nis_code}` to manually publish after review.
   ```

---

## /pipeline municipality <nis5>

Process all pending neighborhoods for a municipality.

### Steps:

1. **Validate municipality code:**
   - Must be exactly 5 digits
   - If invalid, report error and stop

2. **Check for existing claim:**
```sql
SELECT claimed_by, heartbeat_at, neighborhoods_completed, neighborhoods_total
FROM pipeline_claims
WHERE municipality_nis = '{nis5}'
  AND heartbeat_at > NOW() - INTERVAL '30 minutes';
```

   - If claimed by another session (heartbeat recent):
     ```
     Municipality {nis5} ({city}) is currently being processed by session "{claimed_by}"
     Progress: {neighborhoods_completed}/{neighborhoods_total}
     Last heartbeat: {X} minutes ago

     Use `/pipeline claim auto` to process a different municipality,
     or wait for this session to complete.
     ```
     Stop processing.

   - If claim exists but is stale (>30 min), release it:
     ```sql
     DELETE FROM pipeline_claims WHERE municipality_nis = '{nis5}';
     ```
     Then commit and continue.

3. **Query pending neighborhoods:**
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
1. **First, attempt recovery:** Call `mcp__pipeline__execute_rollback` with transaction ID `"recovery"`
2. If recovery succeeds, retry the failed query
3. If recovery fails with "transaction not found", the connection may be clean - retry the query
4. If you see "current transaction is aborted", the MCP server needs restart - inform user
5. For GIS reads (neighborhoods, POIs), fall back to `mcp__gis__query` which uses a separate connection

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

## Quality Gate and Auto-Publish

The quality threshold is **70** (from `agents/config.ts`).

- Content with `final_score >= 70`: **Auto-published** to `web/src/content/neighborhoods/{nis_code}.json`
- Content with `final_score < 70`: **Not published** (stays in `pipeline-outputs/` for review)

Both are marked as `status = 'completed'`. The `published` column tracks whether content was published.

**Find unpublished content:**
```sql
SELECT nis_code, final_score, seo_score, brand_score
FROM pipeline_jobs
WHERE status = 'completed' AND published = FALSE;
```

**Find below-threshold content:**
```sql
SELECT nis_code, final_score, seo_score, brand_score
FROM pipeline_jobs
WHERE status = 'completed' AND final_score < 70;
```

---

## /pipeline publish <nis_code>

Manually publish a completed neighborhood regardless of score.
Use after human review of below-threshold content.

### Steps:

1. **Validate NIS code format:**
   - Must be 7 characters
   - Pattern: 5 digits + 1 letter + 1 digit (e.g., `41002A0`)
   - If invalid, report error and stop

2. **Verify job exists and is completed:**
```sql
SELECT nis_code, status, final_score, published, published_at
FROM pipeline_jobs
WHERE nis_code = '{nis_code}';
```
   - If not found: Report "Job not found: {nis_code}" and stop
   - If status != 'completed': Report "Cannot publish: job status is '{status}', must be 'completed'" and stop

3. **Verify output file exists and extract slug:**
   - Check: `agents/pipeline-outputs/{nis_code}/4-brand-reviewer.json`
   - If missing: Report "Output file not found. Run `/pipeline {nis_code}` first." and stop
   - Read the file and extract the `id` field value (this is the slug for the filename)

4. **Validate POI address fields** (prevents Astro schema errors):
   - Check all items in `vets.practices[]` and `petStores.stores[]`
   - Each must have non-null `municipality` and `postalCode`
   - If any are null, report error and stop:
     ```
     Cannot publish: POI address data incomplete
     - {count} items have null municipality/postalCode
     Fix the data source first. See: backlog/Bugs/2026-01-06-poi-address-fields-not-extracted.md
     ```

5. **Publish:**
   - Create directory if needed: `mkdir -p web/src/content/neighborhoods/`
   - Copy file using the slug as filename: `agents/pipeline-outputs/{nis_code}/4-brand-reviewer.json` -> `web/src/content/neighborhoods/{slug}.json`
   - Update database:
   ```sql
   UPDATE pipeline_jobs
   SET published = TRUE, published_at = NOW()
   WHERE nis_code = '{nis_code}';
   ```
   Then commit.

6. **Report:**
   ```
   Published {nis_code}
   - Score: {final_score}
   - File: web/src/content/neighborhoods/{slug}.json
   ```

   If `final_score < 70`:
   ```
   Note: Published with below-threshold score after manual review.
   ```

   If was already published (overwriting):
   ```
   Note: Overwrote previously published version.
   ```

---

## /pipeline sessions

Show active parallel processing sessions.

### Steps:

1. **Release stale claims first** (auto-cleanup):
   Use `mcp__pipeline__execute_dml_ddl_dcl_tcl` (NOT execute_query):
```sql
DELETE FROM pipeline_claims
WHERE heartbeat_at < NOW() - INTERVAL '30 minutes';
```
Then immediately call `mcp__pipeline__execute_commit` with the returned transaction_id.

2. **Query active sessions:**
   Use `mcp__pipeline__execute_query` for the claims data:
```sql
SELECT
  pc.municipality_nis,
  pc.claimed_by,
  pc.claimed_at,
  pc.heartbeat_at,
  pc.neighborhoods_total,
  pc.neighborhoods_completed,
  EXTRACT(EPOCH FROM (NOW() - pc.heartbeat_at)) / 60 as minutes_since_heartbeat
FROM pipeline_claims pc
ORDER BY pc.heartbeat_at DESC;
```

   Then use `mcp__gis__query` to get city names for each municipality_nis:
```sql
SELECT DISTINCT city, LEFT(nis_code, 5) as municipality_nis
FROM neighborhoods
WHERE LEFT(nis_code, 5) IN ('{nis5_1}', '{nis5_2}', ...);
```

3. **Query in-progress jobs per session:**
```sql
SELECT
  claimed_by,
  nis_code,
  current_stage,
  started_at
FROM pipeline_jobs
WHERE status = 'in_progress'
  AND claimed_by IS NOT NULL
ORDER BY claimed_by, started_at;
```

### Output Format:

```
## Active Pipeline Sessions

| Session | Municipality | City | Progress | Last Heartbeat |
|---------|--------------|------|----------|----------------|
| terminal-1 | 44021 | Gent | 12/28 (43%) | 2 min ago |
| alice-laptop | 11002 | Antwerpen | 5/45 (11%) | 1 min ago |

### Currently Processing
| Session | Neighborhood | Stage | Started |
|---------|--------------|-------|---------|
| terminal-1 | 44021A01 | writer | 3 min ago |
| alice-laptop | 11002B02 | researcher | 1 min ago |

No stale sessions (all heartbeats within 30 minutes).
```

If no active sessions:
```
## Active Pipeline Sessions

No active sessions. Use `/pipeline claim auto` to start processing.
```

---

## /pipeline claim [auto|<nis5>] [session-name]

Claim a municipality for parallel processing.

### Arguments:
- **auto**: Automatically pick the municipality with the most pending neighborhoods
- **<nis5>**: Specific 5-digit municipality NIS code to claim
- **[session-name]**: Optional name for this session (default: auto-generated)

### Session Naming:
- If not provided, generate: `session-{random-4-alphanumeric}` (e.g., `session-a3f2`)
- User can override: `/pipeline claim auto my-laptop`

### Steps:

1. **Release stale claims first** (auto-cleanup):
   Use `mcp__pipeline__execute_dml_ddl_dcl_tcl` (NOT execute_query):
```sql
DELETE FROM pipeline_claims
WHERE heartbeat_at < NOW() - INTERVAL '30 minutes';
```
Then immediately call `mcp__pipeline__execute_commit` with the returned transaction_id.

2. **Reset stale in_progress jobs to pending:**
   Use `mcp__pipeline__execute_dml_ddl_dcl_tcl`:
```sql
UPDATE pipeline_jobs
SET status = 'pending',
    claimed_by = NULL,
    heartbeat_at = NULL,
    current_stage = NULL
WHERE status = 'in_progress'
  AND heartbeat_at < NOW() - INTERVAL '30 minutes';
```
Then immediately call `mcp__pipeline__execute_commit` with the returned transaction_id.

3. **Determine municipality to claim:**

   **If "auto":**
   This requires coordinating between both databases:

   a. First, get all municipality NIS codes from GIS database using `mcp__gis__query`:
   ```sql
   SELECT LEFT(nis_code, 5) as municipality_nis, city, COUNT(*) as total_neighborhoods
   FROM neighborhoods
   GROUP BY LEFT(nis_code, 5), city
   ORDER BY total_neighborhoods DESC;
   ```

   b. Then, get claimed municipalities and job statuses from pipeline database using `mcp__pipeline__execute_query`:
   ```sql
   SELECT municipality_nis FROM pipeline_claims;
   ```

   ```sql
   SELECT municipality_nis, COUNT(*) as processed_count
   FROM pipeline_jobs
   WHERE status IN ('completed', 'in_progress')
   GROUP BY municipality_nis;
   ```

   c. In code, find the municipality with the most pending (total_neighborhoods - processed_count) that is not claimed.

   **If specific municipality:**
   - Use the provided 5-digit code
   - Verify it exists using `mcp__gis__query`:
   ```sql
   SELECT DISTINCT city FROM neighborhoods WHERE LEFT(nis_code, 5) = '{nis5}' LIMIT 1;
   ```

4. **Check if municipality is already claimed:**
   Use `mcp__pipeline__execute_query`:
```sql
SELECT claimed_by, heartbeat_at
FROM pipeline_claims
WHERE municipality_nis = '{nis5}';
```

   - If claimed and heartbeat recent (<30 min):
     ```
     Municipality {nis5} ({city}) is currently claimed by {claimed_by}
     Last heartbeat: {X} minutes ago

     Use `/pipeline claim auto` to claim a different municipality.
     ```
     Stop processing.

5. **Count neighborhoods in municipality:**
   Use `mcp__gis__query` (neighborhoods table is in GIS database):
```sql
SELECT COUNT(*) as total
FROM neighborhoods
WHERE LEFT(nis_code, 5) = '{nis5}';
```

6. **Create claim:**
   Use `mcp__pipeline__execute_dml_ddl_dcl_tcl`:
```sql
INSERT INTO pipeline_claims (municipality_nis, claimed_by, neighborhoods_total)
VALUES ('{nis5}', '{session_name}', {total})
ON CONFLICT (municipality_nis) DO UPDATE
SET claimed_by = '{session_name}',
    claimed_at = NOW(),
    heartbeat_at = NOW(),
    neighborhoods_completed = 0;
```
Then immediately call `mcp__pipeline__execute_commit` with the returned transaction_id.

7. **Report claim:**
```
## Claimed Municipality

Session: {session_name}
Municipality: {nis5} ({city})
Neighborhoods: {total} total, {pending_count} pending

Starting processing...
```

8. **IMPORTANT: Remember Session Name Throughout Processing**
   - Store the session name and use it for ALL subsequent operations
   - The session name MUST be passed to the single-neighborhood workflow for heartbeat updates
   - Display reminder: `Session "{session_name}" active - use this terminal exclusively for this claim`

9. **Process all pending neighborhoods:**
   - Query pending neighborhoods in this municipality
   - Process each using the single-neighborhood workflow (see `/pipeline <nis_code>`)
   - Pass the session_name to update heartbeat during processing
   - Update claim progress after each completion (use `mcp__pipeline__execute_dml_ddl_dcl_tcl`):
     ```sql
     UPDATE pipeline_claims
     SET heartbeat_at = NOW(),
         neighborhoods_completed = neighborhoods_completed + 1
     WHERE municipality_nis = '{nis5}' AND claimed_by = '{session_name}';
     ```
     Then immediately commit.

10. **After completing municipality:**
    - Release the claim (use `mcp__pipeline__execute_dml_ddl_dcl_tcl`):
      ```sql
      DELETE FROM pipeline_claims
      WHERE municipality_nis = '{nis5}' AND claimed_by = '{session_name}';
      ```
      Then immediately commit.
    - Report completion
    - Auto-claim next available municipality (recursive `/pipeline claim auto {session_name}`)

### If No Municipalities Available:

```
## No Municipalities Available

All municipalities are either:
- Already claimed by another session
- Already completed

Use `/pipeline sessions` to see active sessions.
Use `/pipeline status` to see overall progress.
```

---

## /pipeline release [<nis5>|all]

Release claimed municipalities so other sessions can pick them up.

### Arguments:
- **<nis5>**: Release specific municipality
- **all**: Release all claims for this session (requires knowing session name)

### Steps:

1. **If specific municipality:**
   Use `mcp__pipeline__execute_dml_ddl_dcl_tcl`:
```sql
DELETE FROM pipeline_claims
WHERE municipality_nis = '{nis5}';
```
Then immediately commit.

Report:
```
Released claim on {nis5} ({city})
```

2. **If "all":**
   First query current claims (use `mcp__pipeline__execute_query`):
```sql
SELECT municipality_nis, claimed_by
FROM pipeline_claims;
```

Report current claims, then delete all (use `mcp__pipeline__execute_dml_ddl_dcl_tcl`):
```sql
DELETE FROM pipeline_claims;
```
Then immediately commit.

Report:
```
Released all claims:
- {nis5} ({city}) - was claimed by {session}
- ...
```

3. **Also reset any in-progress jobs for released municipalities:**
   Use `mcp__pipeline__execute_dml_ddl_dcl_tcl`:
```sql
UPDATE pipeline_jobs
SET status = 'pending',
    claimed_by = NULL,
    heartbeat_at = NULL,
    current_stage = NULL
WHERE status = 'in_progress'
  AND municipality_nis = '{nis5}';
```
Then immediately commit.

---

## Heartbeat Updates During Processing

When processing a neighborhood (in the single-neighborhood workflow), update heartbeat on:

1. **Job start:**
```sql
UPDATE pipeline_jobs
SET status = 'in_progress',
    current_stage = '{stage}',
    started_at = NOW(),
    claimed_by = '{session_name}',
    heartbeat_at = NOW()
WHERE nis_code = '{nis_code}';
```

2. **Stage transitions:**
```sql
UPDATE pipeline_jobs
SET current_stage = '{next_stage}',
    heartbeat_at = NOW(),
    {stage}_completed_at = NOW()
WHERE nis_code = '{nis_code}';
```

3. **Municipality claim heartbeat** (after each neighborhood):
```sql
UPDATE pipeline_claims
SET heartbeat_at = NOW(),
    neighborhoods_completed = neighborhoods_completed + 1
WHERE municipality_nis = '{nis5}' AND claimed_by = '{session_name}';
```

This ensures other sessions can detect stale claims and pick up abandoned work.

---

## /pipeline regenerate <nis_code|municipality nis5>

Re-run the pipeline on existing content when data or prompts improve.

### Arguments:
- **<nis_code>**: Regenerate single neighborhood (7-char NIS code)
- **municipality <nis5>**: Regenerate all completed neighborhoods in a municipality

### Steps (single neighborhood):

1. **Validate NIS code format:**
   - Must be 7 characters
   - If invalid, report error and stop

2. **Check for existing content:**
```sql
SELECT nis_code, status, published, final_score, completed_at
FROM pipeline_jobs
WHERE nis_code = '{nis_code}';
```
   - If not found or status != 'completed': Report "No existing content found for {nis_code}. Use `/pipeline {nis_code}` to generate first." and stop.

3. **Backup existing output:**
   - Create backup directory: `agents/pipeline-outputs/{nis_code}/backup-{timestamp}/`
   - Copy all 4 output files to backup directory:
     - `1-researcher.json`
     - `2-writer.json`
     - `3-seo-reviewer.json`
     - `4-brand-reviewer.json`

4. **Reset job for regeneration:**
```sql
UPDATE pipeline_jobs
SET status = 'pending',
    current_stage = NULL,
    researcher_completed_at = NULL,
    writer_completed_at = NULL,
    seo_reviewer_completed_at = NULL,
    brand_reviewer_completed_at = NULL,
    seo_score = NULL,
    brand_score = NULL,
    final_score = NULL,
    started_at = NULL,
    completed_at = NULL,
    error_message = NULL
WHERE nis_code = '{nis_code}';
```
Then commit.

5. **Delete existing output files:**
   - Remove: `agents/pipeline-outputs/{nis_code}/1-researcher.json`
   - Remove: `agents/pipeline-outputs/{nis_code}/2-writer.json`
   - Remove: `agents/pipeline-outputs/{nis_code}/3-seo-reviewer.json`
   - Remove: `agents/pipeline-outputs/{nis_code}/4-brand-reviewer.json`
   - Keep the backup directory intact

6. **Run the standard pipeline:**
   - Process using the single-neighborhood workflow (see `/pipeline <nis_code>`)

7. **After completion, show comparison:**
   - Read old final score from backup `4-brand-reviewer.json`
   - Compare with new final score
   - Report:
     ```
     ## Regeneration Complete: {nis_code}

     | Metric | Previous | New | Change |
     |--------|----------|-----|--------|
     | SEO Score | 72 | 78 | +6 |
     | Brand Score | 68 | 75 | +7 |
     | Final Score | 70 | 76.5 | +6.5 |

     Backup saved to: agents/pipeline-outputs/{nis_code}/backup-{timestamp}/
     ```

8. **Auto-publish if score improved or meets threshold:**
   - If new `final_score >= 70` and was previously published: Update published content
   - If new `final_score >= 70` and was not published: Publish now
   - If new `final_score < 70`: Do not publish, prompt for manual review

### Steps (municipality batch):

1. **Query completed neighborhoods in municipality:**
```sql
SELECT nis_code, final_score
FROM pipeline_jobs
WHERE municipality_nis = '{nis5}'
  AND status = 'completed'
ORDER BY nis_code;
```

2. **Report scope:**
   ```
   Found {count} completed neighborhoods in {city} ({nis5})
   Regenerating all...
   ```

3. **For each neighborhood:**
   - Run single-neighborhood regeneration workflow
   - Report progress: `[{n}/{total}] Regenerated {nis_code}: {old_score} -> {new_score}`

4. **Report final summary:**
   ```
   ## Municipality {city} Regeneration Complete

   - Total: {count}
   - Improved: {improved_count} (avg +{avg_improvement})
   - Declined: {declined_count}
   - New Average: {new_avg} (was {old_avg})
   ```

### Use Cases:
- After updating Statbel data (new year's statistics)
- After improving agent prompts
- After fixing bugs in researcher queries
- Periodic refresh for quality consistency
