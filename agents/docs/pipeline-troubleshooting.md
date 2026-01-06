# Pipeline Troubleshooting

Solutions for common pipeline issues.

---

## Session Interrupted

**Symptom:** You closed Claude Code or lost connection mid-pipeline.

**Solution:** Re-run the same command:
```
/pipeline <nis_code>
```

The pipeline checks for existing output files and resumes from the last incomplete stage. No manual cleanup needed.

---

## Stale Jobs (In Progress > 30 Minutes)

**Symptom:** `/pipeline status` shows warnings about stale jobs:

```
### Stale Jobs Warning
These jobs have been in_progress for >30 minutes and may need attention:
- 44021A2: stuck at writer since 2025-01-06 14:23:00
```

**Cause:** Session was interrupted while processing, leaving job in `in_progress` state.

**Solution:** The stale job will be picked up by:
- `/pipeline retry-failed` - Processes all stale/failed jobs
- `/pipeline <nis_code>` - Re-running the specific neighborhood
- `/pipeline municipality <nis5>` - Batch processing includes stale jobs

Or manually reset:
```sql
UPDATE pipeline_jobs
SET status = 'pending', current_stage = NULL, error_message = NULL
WHERE nis_code = '44021A2';
-- Then commit
```

---

## Failed Jobs

**Symptom:** Job shows `status = 'failed'` in dashboard.

**Check the error:**
```sql
SELECT nis_code, current_stage, error_message, retry_count
FROM pipeline_jobs
WHERE status = 'failed';
```

**If retry_count < 3:**
```
/pipeline retry-failed
```

**If retry_count >= 3:**
The job has failed 3 times. You need to investigate manually:
1. Check the error message
2. Look at partial outputs in `agents/pipeline-outputs/{nis_code}/`
3. Fix the underlying issue
4. Reset and retry:
```sql
UPDATE pipeline_jobs
SET status = 'pending', retry_count = 0, error_message = NULL
WHERE nis_code = '{nis_code}';
-- Then commit
```

---

## Invalid JSON Output

**Symptom:** Error message mentions "Invalid JSON output" or "Missing required field".

**Cause:** Agent produced malformed or incomplete output.

**What happens automatically:**
1. Invalid output file is deleted
2. Job is marked as failed
3. `retry_count` is incremented

**Solution:** Run `/pipeline retry-failed` or re-run the specific neighborhood.

**If it keeps failing:** The agent may be hitting a prompt issue. Check:
- `agents/{stage}/prompt-v1.md` - Agent instructions
- `agents/{stage}/output-schema.json` - Expected schema

---

## POI Address Validation Failure

**Symptom:** Job completes but doesn't publish:

```
Cannot publish: POI address data incomplete
- 3 items have null municipality/postalCode
```

**Cause:** Some vets or pet stores in the neighborhood don't have complete address data extracted from OSM. This is a known data quality issue.

**Current status:** Tracked in `backlog/Bugs/2026-01-06-poi-address-fields-not-extracted.md`

**Workaround options:**

1. **Wait for data fix** - The bug is being addressed at the POI extraction level.

2. **Manual data enrichment** - Edit the brand-reviewer output to add missing fields:
   ```
   agents/pipeline-outputs/{nis_code}/4-brand-reviewer.json
   ```
   Then run `/pipeline publish <nis_code>`.

3. **Skip affected neighborhoods** - Process other neighborhoods first.

**Check which neighborhoods are affected:**
```sql
SELECT nis_code, final_score FROM pipeline_jobs
WHERE status = 'completed' AND published = FALSE;
```

---

## MCP Connection Issues

**Symptom:** Database queries fail with connection errors.

**Check MCP configuration:**

1. Verify `.mcp.json` has both servers configured:
   - `gis` - for `buurtkompas` (read-only)
   - `pipeline` - for `buurtkompas_pipeline` (read-write)

2. Test connections:
   ```
   -- Test GIS (via mcp__gis__query)
   SELECT 1;

   -- Test Pipeline (via mcp__pipeline__execute_query)
   SELECT 1;
   ```

3. Check PostgreSQL is running and accessible.

4. Verify database credentials in `.mcp.json` match your local setup.

---

## Score Below Threshold

**Symptom:** Job completes but content isn't published (score < 70).

**This is expected behavior.** Content below the quality threshold requires human review.

**Options:**

1. **Review and publish anyway:**
   ```
   /pipeline publish <nis_code>
   ```

2. **Investigate why score is low:**
   - Read `agents/pipeline-outputs/{nis_code}/3-seo-reviewer.json` for SEO feedback
   - Read `agents/pipeline-outputs/{nis_code}/4-brand-reviewer.json` for brand feedback
   - Check the `*Review.reasoning` fields for specific issues

3. **Re-run with updated prompts** (if you've improved agent prompts).

**Find all below-threshold content:**
```sql
SELECT nis_code, final_score, seo_score, brand_score
FROM pipeline_jobs
WHERE status = 'completed' AND final_score < 70
ORDER BY final_score DESC;
```

---

## Neighborhood Not Found

**Symptom:** Error "Neighborhood not found: {nis_code}"

**Cause:** The NIS code doesn't exist in the GIS database.

**Verify the NIS code:**
```sql
SELECT nis_code, name, city FROM neighborhoods WHERE nis_code = '{nis_code}';
```

**Find valid NIS codes:**
```sql
SELECT nis_code, name, city FROM neighborhoods
WHERE city = 'Gent'
ORDER BY name;
```

---

## Transaction Not Committed

**Symptom:** Database changes don't persist.

**Cause:** Pipeline write operations use transactions that must be explicitly committed.

**Solution:** After any `execute_dml_ddl_dcl_tcl` call, ensure commit was called. If you're debugging manually, always commit:
```
-- After any INSERT/UPDATE/DELETE
COMMIT;
```

---

## Debugging Tips

### Check Job State Directly

```sql
SELECT * FROM pipeline_jobs WHERE nis_code = '{nis_code}';
```

### View Recent Errors

```sql
SELECT nis_code, current_stage, error_message, last_error_at
FROM pipeline_jobs
WHERE status = 'failed'
ORDER BY last_error_at DESC
LIMIT 10;
```

### Clear All Jobs for a Neighborhood (Start Fresh)

```sql
DELETE FROM pipeline_jobs WHERE nis_code = '{nis_code}';
-- Then commit
```
Also delete output files:
```
rm -rf agents/pipeline-outputs/{nis_code}/
```

### Reset Pipeline for a Municipality

```sql
UPDATE pipeline_jobs
SET status = 'pending', current_stage = NULL, error_message = NULL, retry_count = 0
WHERE municipality_nis = '{nis5}';
-- Then commit
```

---

## Related Documentation

- [Pipeline Quickstart](pipeline-quickstart.md) - Get started fast
- [Pipeline Commands](pipeline-commands.md) - Complete command reference
- [Pipeline Architecture](pipeline-architecture.md) - Technical reference
