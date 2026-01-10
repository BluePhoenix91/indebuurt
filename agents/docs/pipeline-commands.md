# Pipeline Commands

Complete reference for the `/pipeline` slash command.

---

## Command Overview

| Command | Description |
|---------|-------------|
| `/pipeline` | Show help |
| `/pipeline status` | Display progress dashboard |
| `/pipeline <nis_code>` | Process single neighborhood |
| `/pipeline municipality <nis5>` | Process all pending in a city |
| `/pipeline next [N]` | Process next N pending |
| `/pipeline retry-failed` | Re-process failed jobs |
| `/pipeline publish <nis_code>` | Manually publish completed content |

### Feature Flag: --separate-reviewers

The default pipeline uses the merged 3-stage flow (researcher → writer → quality-reviewer). Add `--separate-reviewers` to use the legacy 4-stage pipeline with separate SEO and Brand reviewers:

| Command | Description |
|---------|-------------|
| `/pipeline <nis_code> --separate-reviewers` | Process with 4-stage pipeline |
| `/pipeline municipality <nis5> --separate-reviewers` | Municipality with 4-stage |
| `/pipeline next N --separate-reviewers` | Next N with 4-stage |

**Default (3-stage) benefits:**
- ~45% fewer tokens (review stage)
- One fewer agent round-trip
- Single pass for both SEO and Brand checks

**Output files:**
- Default: `3-quality-reviewer.json`
- With `--separate-reviewers`: `3-seo-reviewer.json` + `4-brand-reviewer.json`

---

## NIS Code Format

NIS codes identify Belgian statistical sectors (neighborhoods).

**Format:** 7 characters = 5 digits + 1 letter + 1 digit

**Examples:**
- `44021A1` - A neighborhood in Gent (44021)
- `41002A0` - A neighborhood in Aalst (41002)
- `11002B3` - A neighborhood in Antwerpen (11002)

**Municipality prefix:** First 5 characters identify the city.

**Lookup neighborhoods:**
```sql
SELECT nis_code, name, city FROM neighborhoods WHERE city = 'Gent' ORDER BY name;
```

**Lookup municipalities:**
```sql
SELECT DISTINCT LEFT(nis_code, 5) as nis5, city FROM neighborhoods ORDER BY city;
```

---

## `/pipeline status`

Display a progress dashboard showing overall counts, municipality breakdown, and recent activity.

**Usage:**
```
/pipeline status
```

**Output:**

```
## Pipeline Status

### Overall
- Pending: 2847
- In Progress: 0
- Completed: 12
- Failed: 1

### Stale Jobs Warning
These jobs have been in_progress for >30 minutes and may need attention:
- 44021A2: stuck at writer since 2025-01-06 14:23:00

### By Municipality
| City | Pending | In Progress | Completed | Failed |
|------|---------|-------------|-----------|--------|
| Gent | 28 | 0 | 12 | 1 |
| Aalst | 42 | 0 | 0 | 0 |
| ... | ... | ... | ... | ... |

### Recent Activity
| NIS Code | Status | SEO | Brand | Final | Updated |
|----------|--------|-----|-------|-------|---------|
| 44021A1 | completed | 79 | 85 | 82 | 2025-01-06 |
| ... | ... | ... | ... | ... | ... |

### Quality Summary
- Completed: 12 neighborhoods
- Published: 10 | Unpublished: 2
- Avg Final Score: 78.5
- Below threshold (<70): 2
```

---

## `/pipeline <nis_code>`

Process a single neighborhood through the agent pipeline.

**Usage:**
```
/pipeline 44021A1                       # Default 3-stage pipeline
/pipeline 44021A1 --separate-reviewers  # Legacy 4-stage pipeline
```

**What happens:**
1. Validates NIS code format (7 chars, pattern `DDDDDLD`)
2. Verifies neighborhood exists in GIS database
3. Creates or retrieves job record
4. Checks for existing outputs (resume support)
5. Runs remaining agents in sequence
6. Calculates final score (average of SEO + Brand)
7. Auto-publishes if score >= 70 and POI data valid

**Output (default 3-stage):**

```
Running researcher for 44021A1...
Running writer for 44021A1...
Running quality-reviewer for 44021A1...

Completed 44021A1 (Gent - Dampoort)
- SEO Score: 82
- Brand Score: 88
- Quality Score: 85
- Published: Yes -> web/src/content/neighborhoods/gent-dampoort.json
```

**Output (with --separate-reviewers):**

```
Running researcher for 44021A1...
Running writer for 44021A1...
Running seo-reviewer for 44021A1...
Running brand-reviewer for 44021A1...

Completed 44021A1 (Gent - Dampoort)
- SEO Score: 82
- Brand Score: 88
- Final Score: 85
- Published: Yes -> web/src/content/neighborhoods/gent-dampoort.json
```

**Resume behavior:** If interrupted, re-running the same command continues from the last incomplete stage. Existing valid outputs are preserved.

---

## `/pipeline municipality <nis5>`

Process all pending neighborhoods for a municipality (city).

**Usage:**
```
/pipeline municipality 44021
```

**What happens:**
1. Validates municipality code (5 digits)
2. Queries all pending/stale neighborhoods for that city
3. Processes each sequentially
4. Continues to next neighborhood if one fails

**Output:**

```
Processing 28 neighborhoods in Gent

[1/28] Completed Dampoort (SEO: 82, Brand: 88)
[2/28] Completed Rabot (SEO: 71, Brand: 76)
[3/28] FAILED Mendonk: writer stage error
[4/28] Completed Brugse Poort (SEO: 79, Brand: 84)
...

## Municipality Gent Complete
- Processed: 28
- Succeeded: 26
- Failed: 2
- Average Score: 78.3
```

---

## `/pipeline next [N]`

Process the next N pending neighborhoods across all municipalities.

**Usage:**
```
/pipeline next        # Default: 5
/pipeline next 10     # Process 10
/pipeline next 50     # Maximum: 50
```

**What happens:**
1. Queries N pending/stale neighborhoods (ordered by city, then name)
2. Processes each sequentially
3. Reports progress and summary

**Output:**

```
Processing 5 pending neighborhoods

[1/5] Completed Gent - Dampoort (SEO: 82, Brand: 88)
[2/5] Completed Gent - Rabot (SEO: 71, Brand: 76)
[3/5] Completed Aalst - Station (SEO: 79, Brand: 91)
...

## Batch Complete
- Processed: 5
- Succeeded: 5
- Failed: 0
- Average Score: 81.2
```

**Note:** Maximum is capped at 50 to prevent runaway sessions.

---

## `/pipeline retry-failed`

Re-process failed jobs that haven't exceeded the maximum retry count (3).

**Usage:**
```
/pipeline retry-failed
```

**What happens:**
1. Finds failed jobs with retry_count < 3
2. Resets each to pending status
3. Processes through the pipeline
4. Reports results

**Output:**

```
Found 3 failed jobs eligible for retry:
- 44021B2: writer: Invalid JSON output (attempt 2/3)
- 44021C1: seo-reviewer: Missing required field seoReview (attempt 1/3)
- 41002A0: researcher: Database query timeout (attempt 3/3)

[1/3] Completed 44021B2 (SEO: 75, Brand: 80)
[2/3] FAILED 44021C1: seo-reviewer stage error
[3/3] Completed 41002A0 (SEO: 79, Brand: 85)

## Retry Complete
- Retried: 3
- Succeeded: 2
- Failed: 1
```

**Note:** Jobs that fail 3 times remain in `failed` status and require manual investigation.

---

## `/pipeline publish <nis_code>`

Manually publish a completed neighborhood, bypassing the quality threshold.

Use this after human review of below-threshold content.

**Usage:**
```
/pipeline publish 44021B2
```

**Requirements:**
- Job must exist and have `status = 'completed'`
- Output file must exist at:
  - `agents/pipeline-outputs/{nis_code}/3-quality-reviewer.json` (default 3-stage), OR
  - `agents/pipeline-outputs/{nis_code}/4-brand-reviewer.json` (legacy 4-stage)
- POI address data must be complete (all vets/pet stores have municipality/postalCode)

**Output:**

```
Published 44021B2
- Score: 65
- File: web/src/content/neighborhoods/gent-rabot.json
Note: Published with below-threshold score after manual review.
```

**If already published:**

```
Published 44021B2
- Score: 82
- File: web/src/content/neighborhoods/gent-rabot.json
Note: Overwrote previously published version.
```

---

## Error Messages

| Error | Cause | Solution |
|-------|-------|----------|
| "Invalid NIS code format" | Not 7 chars or wrong pattern | Use format `DDDDDLD` (e.g., `44021A1`) |
| "Neighborhood not found" | NIS code doesn't exist in GIS | Check `neighborhoods` table |
| "Job not found" | No job record for manual publish | Run `/pipeline <nis_code>` first |
| "Cannot publish: status is X" | Job not completed | Wait for completion or fix failure |
| "POI address data incomplete" | vets/petStores missing addresses | See troubleshooting guide |

---

## Related Documentation

- [Pipeline Quickstart](pipeline-quickstart.md) - Get started fast
- [Pipeline Architecture](pipeline-architecture.md) - Technical reference
- [Pipeline Troubleshooting](pipeline-troubleshooting.md) - When things go wrong
