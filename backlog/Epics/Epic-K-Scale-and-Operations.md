# Epic K — Scale & Operations

**Goal:** Scale content generation to hundreds of neighborhoods with batch processing, monitoring, and operational tooling.

**Depends on:** Epic J (Agent Pipeline) — need working pipeline before scaling.

---

## Story K1: Batch Processing Script
> As a site owner, I want to process multiple neighborhoods in a single batch run, so that I can generate content at scale without triggering each neighborhood manually.

**Context:** Process a list of neighborhoods, track progress, handle failures gracefully.

**Acceptance Criteria:**
- [ ] Batch script accepts: CSV file with neighborhood IDs, or database query
- [ ] Configurable concurrency: process N neighborhoods in parallel (default: 3)
- [ ] Progress displayed in real-time: "Processing 15/120... gent-dampoort"
- [ ] Summary report on completion: successful, failed, flagged for review, total time
- [ ] Failed neighborhoods logged with errors for investigation
- [ ] Resume capability: can restart batch from where it left off after interruption
- [ ] Dry-run mode: validate inputs without actually generating content

---

## Story K2: Scheduled Daily Processing
> As a site owner, I want content generation to run automatically on a schedule, so that new neighborhoods are processed daily without manual triggering.

**Context:** Set up automated job that processes queued neighborhoods each night.

**Acceptance Criteria:**
- [ ] Scheduler configured (cron, Task Scheduler, or similar)
- [ ] Daily run at configurable time (default: 2am local)
- [ ] Batch pulls "pending" neighborhoods from queue (database or file)
- [ ] Rate limiting prevents API quota exhaustion
- [ ] Notification sent on completion: success count, failure count, review queue size
- [ ] Notification channels: email and/or Slack webhook
- [ ] Schedule easily pausable for maintenance

---

## Story K3: Processing Metrics and Logging
> As a site owner, I want detailed metrics logged for each generation run, so that I can identify bottlenecks and track quality trends over time.

**Context:** Operational visibility into pipeline performance.

**Acceptance Criteria:**
- [ ] Each run logs: timestamp, neighborhood, duration per agent, total duration, quality score, success/failure
- [ ] Logs stored in structured format (JSON lines or database)
- [ ] Retention policy: keep detailed logs 30 days, aggregates indefinitely
- [ ] Log viewer: can filter by date, neighborhood, quality score, status
- [ ] Export capability: download logs as CSV for analysis
- [ ] Error logs include full context for debugging failed runs

---

## Story K4: Operations Dashboard
> As a site owner, I want a dashboard showing pipeline health and content coverage, so that I can monitor progress toward full Flanders coverage.

**Context:** At-a-glance view of how content generation is progressing.

**Acceptance Criteria:**
- [ ] Dashboard shows: total neighborhoods in Flanders, content generated, content pending
- [ ] Progress visualization: map or chart showing coverage by region/province
- [ ] Recent activity: last 10 runs with status and quality scores
- [ ] Alerts displayed: failures, review queue backlog, quality score trends
- [ ] Key metrics: average quality score (7-day), success rate, avg generation time
- [ ] Dashboard updates automatically (polling or real-time)
- [ ] Accessible at `/admin/dashboard` with basic authentication

---

## Story K5: Content Regeneration Workflow
> As a content team member, I want to regenerate content for existing neighborhoods when data or prompts improve, so that all pages stay current as our system evolves.

**Context:** As Statbel releases new data or prompts improve, existing content should be refreshable.

**Acceptance Criteria:**
- [ ] Regeneration mode: `--mode=regenerate` flag in orchestrator
- [ ] Detects existing content and creates new version, doesn't overwrite immediately
- [ ] Diff view shows: what changed between current and new version
- [ ] Manual edit detection: warns if content was modified since last generation
- [ ] Review interface shows side-by-side comparison
- [ ] Approve action replaces old content (preserving git history)
- [ ] Bulk regeneration: regenerate all content for a city or province
- [ ] Regeneration reason logged for audit trail

---

## Story K6: Prompt Version Management
> As a developer, I want to test new prompt versions against a baseline, so that I can safely improve prompts without breaking content quality.

**Context:** Prompt engineering is iterative. Need A/B testing capability.

**Acceptance Criteria:**
- [ ] Prompts stored with semantic versions: `writer-v1.0.md`, `writer-v1.1.md`
- [ ] Orchestrator accepts version parameters: `--writer-version=1.1`
- [ ] Test mode: generate same neighborhood with different prompt versions
- [ ] Comparison output: side-by-side content + quality scores for each version
- [ ] A/B test results logged: version, neighborhood, scores, human preference (if rated)
- [ ] Promotion workflow: mark new version as default when proven better
- [ ] Rollback: easily revert to previous prompt version if quality drops

---

## Dependencies

```
K1 (Batch Processing)
  ├── K2 (Scheduled Runs)
  └── K3 (Metrics) ── K4 (Dashboard)

K5 (Regeneration) - independent, can be added anytime after J1

K6 (Prompt Versions) - independent, can be added anytime after I2-I5
```

K1 is foundation for scale. K2-K4 add operational maturity. K5-K6 support iteration and maintenance.
