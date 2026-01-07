# Epic K — Scale & Operations

**Goal:** Scale content generation to all 2,800 Flanders neighborhoods through parallel processing and support content refresh when data improves.

**Depends on:** Epic J (Agent Pipeline) — requires working `/pipeline` commands.

**Architecture Note:** The pipeline runs via Claude Code CLI sessions, not traditional batch scripts. This means scheduled automation is not possible, but parallel human-operated terminals can scale throughput significantly.

---

## Story K1: Parallel Multi-Terminal Processing ✅

> As a site owner, I want to run multiple Claude Code sessions in parallel to process neighborhoods faster, so that I can generate content for all of Flanders in weeks instead of months.

**Context:** Sequential processing of 2,800 neighborhoods would take ~60 days. With 3-4 parallel terminals, this drops to ~15-20 days.

**Implementation:** Municipality-level claiming with heartbeat tracking prevents conflicts between sessions.

**Acceptance Criteria:**
- [x] `pipeline_claims` table tracks which session claimed which municipality
- [x] `claimed_by` and `heartbeat_at` columns added to `pipeline_jobs`
- [x] `/pipeline claim [auto|<nis5>] [session-name]` claims a municipality
- [x] `/pipeline sessions` shows all active sessions with progress
- [x] `/pipeline release [<nis5>|all]` releases claims manually
- [x] `/pipeline municipality` checks for existing claims before processing
- [x] `/pipeline status` shows active sessions summary
- [x] Stale claims (>30 min no heartbeat) auto-released
- [x] Session names auto-generated or user-provided
- [x] Heartbeat updated during stage transitions
- [x] After completing municipality, auto-claims next available

**Commands:**
```
/pipeline claim auto                  # Claim next available municipality
/pipeline claim 44021 my-laptop       # Claim specific municipality with custom session name
/pipeline sessions                    # View all active sessions
/pipeline release 44021               # Release specific claim
/pipeline release all                 # Release all claims
```

**Parallel Workflow:**
```
# Terminal 1:
/pipeline claim auto
→ Claimed Gent (44021) - processing 28 neighborhoods...

# Terminal 2:
/pipeline claim auto
→ Claimed Antwerpen (11002) - processing 45 neighborhoods...

# Terminal 3:
/pipeline claim auto
→ Claimed Brugge (31005) - processing 22 neighborhoods...
```

---

## Story K2: Content Regeneration ✅

> As a site owner, I want to regenerate content for existing neighborhoods when Statbel releases new data or prompts improve, so that all pages stay current.

**Context:** Annual Statbel updates, prompt improvements, and bug fixes require re-running the pipeline on existing content.

**Implementation:** Backup existing content, reset job, re-run pipeline, compare results.

**Acceptance Criteria:**
- [x] `/pipeline regenerate <nis_code>` regenerates single neighborhood
- [x] `/pipeline regenerate municipality <nis5>` regenerates all completed in municipality
- [x] Old content backed up before regeneration
- [x] Comparison shows score changes (previous vs new)
- [x] Auto-publish if new score meets threshold
- [x] Backup preserved for rollback if needed

**Commands:**
```
/pipeline regenerate 44021A01                    # Regenerate single neighborhood
/pipeline regenerate municipality 44021          # Regenerate all in Gent
```

**Output:**
```
## Regeneration Complete: 44021A01

| Metric | Previous | New | Change |
|--------|----------|-----|--------|
| SEO Score | 72 | 78 | +6 |
| Brand Score | 68 | 75 | +7 |
| Final Score | 70 | 76.5 | +6.5 |

Backup saved to: agents/pipeline-outputs/44021A01/backup-2026-01-06T14:30:00/
```

---

## Removed Stories

The following stories from the original Epic K have been removed as they are either impossible with the Claude Code architecture or already covered by existing functionality:

### ~~K2: Scheduled Daily Processing~~ — REMOVED

**Reason:** Claude Code CLI requires human interaction. Automated scheduling is not possible without a headless orchestration layer, which would be significant additional infrastructure. The parallel claiming approach achieves similar throughput goals.

### ~~K3: Processing Metrics and Logging~~ — REMOVED

**Reason:** The `pipeline_jobs` table already tracks:
- Timestamps (started_at, completed_at, stage completion times)
- Scores (seo_score, brand_score, final_score)
- Status (pending, in_progress, completed, failed)
- Errors (error_message, retry_count)

The `/pipeline status` command provides sufficient operational visibility.

### ~~K4: Operations Dashboard~~ — REMOVED

**Reason:** `/pipeline status` and `/pipeline sessions` provide the necessary visibility. A web dashboard would add infrastructure complexity without proportional value at current scale.

### ~~K6: Prompt Version Management~~ — REMOVED (deferred)

**Reason:** The current workflow (edit prompt → test on single neighborhood → commit if good) is sufficient. Formal A/B testing infrastructure would be over-engineering at this stage. Can be revisited after initial content generation is complete.

---

## Database Schema

### Table: pipeline_claims
```sql
CREATE TABLE pipeline_claims (
    municipality_nis VARCHAR(5) PRIMARY KEY,
    claimed_by VARCHAR(50) NOT NULL,
    claimed_at TIMESTAMP DEFAULT NOW(),
    heartbeat_at TIMESTAMP DEFAULT NOW(),
    neighborhoods_total INTEGER NOT NULL,
    neighborhoods_completed INTEGER DEFAULT 0
);
```

### Added to pipeline_jobs
```sql
ALTER TABLE pipeline_jobs ADD COLUMN claimed_by VARCHAR(50);
ALTER TABLE pipeline_jobs ADD COLUMN heartbeat_at TIMESTAMP;
```

---

## Migration

Run: `agents/scripts/db/07-add-claiming-mechanism.sql`

---

## Summary

| Original Story | New Status |
|----------------|------------|
| K1: Batch Processing | → **K1: Parallel Multi-Terminal** (implemented differently) |
| K2: Scheduled Daily | REMOVED (not possible with Claude Code) |
| K3: Metrics & Logging | REMOVED (covered by existing DB) |
| K4: Dashboard | REMOVED (covered by /pipeline status) |
| K5: Regeneration | → **K2: Content Regeneration** (implemented) |
| K6: Prompt Versions | REMOVED (deferred) |

Epic K is now complete with 2 stories focused on practical scaling and maintenance.
