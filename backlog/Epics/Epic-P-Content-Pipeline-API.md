# Epic P — Content Pipeline API

**Goal:** Process neighborhood content via API calls to Anthropic, enabling automated batch processing without interactive Claude Code sessions.

**Depends on:** Epic M (Server Infrastructure), Epic O (ETL Automation), Epic L (Agent Fine-Tuning)

---

## Context

The current pipeline runs via Claude Code interactive sessions:
- Requires manual attention
- Limited to ~2 parallel terminals
- Processing 2,800 neighborhoods would take 150-230 hours of active session time

This epic moves processing to API calls:
- **Unattended processing**: Submit batch, get results next day
- **True parallelism**: 50+ concurrent API requests
- **Cost efficiency**: Batch API offers 50% discount on tokens
- **Scalability**: Process all neighborhoods in 6-12 hours

**Estimated one-time cost for full run:** ~$420 (Batch API pricing, assumes L8 merged reviewers)

---

## Story P1: Research Data Endpoint

> As an API consumer, I want to fetch all research data for a neighborhood in one call, so that AI processing doesn't need database access.

**Context:** The Anthropic API doesn't have MCP tool access. Instead of having Claude query the database, we pre-fetch all data and pass it as context.

**Endpoint:**
```
GET /api/research/{nis_code}
```

**Response:** Matches `ResearcherOutput` schema from Epic I.

**Acceptance Criteria:**
- [ ] Endpoint returns POI counts, nearest POIs, statistics for neighborhood
- [ ] Uses materialized views (L6) for efficient queries
- [ ] Response matches `ResearcherOutput` JSON schema exactly
- [ ] Returns 404 for unknown NIS codes
- [ ] Caches response (POI data changes weekly, not per-request)
- [ ] Swagger documentation with example response

**Technical Notes:**
- Single endpoint replaces 7-8 Researcher database queries
- Consider Redis or in-memory cache with 1-hour TTL
- Include `dataVersion` timestamp for cache invalidation

---

## Story P2: Single Neighborhood Processing

> As a pipeline operator, I want to process one neighborhood via API, so that I can test the pipeline before running batches.

**Endpoint:**
```
POST /api/process/{nis_code}
```

**Process:**
```
1. Fetch research data (P1)
2. Call Writer agent (Anthropic API)
3. Call Quality Reviewer agent (Anthropic API)
4. Save prose to content.neighborhood_prose
5. Update pipeline.pipeline_jobs status
6. Return result with scores
```

**Acceptance Criteria:**
- [ ] Endpoint processes neighborhood through Writer + Quality Reviewer
- [ ] Uses existing prompts from `agents/writer/prompt-v1.md` etc.
- [ ] Saves output to `content.neighborhood_prose` table
- [ ] Updates `pipeline.pipeline_jobs` with status and scores
- [ ] Returns quality scores and processing time
- [ ] Handles Anthropic API errors gracefully (timeout, rate limit)
- [ ] Logs token usage for cost tracking

**Technical Notes:**
- Use Claudia C# SDK or raw HttpClient for Anthropic API
- Prompts loaded from existing markdown files (single source of truth)
- Consider prompt caching for repeated system prompts

---

## Story P3: Anthropic API Integration

> As a developer, I want a robust Anthropic API client, so that all agent calls are reliable and observable.

**Acceptance Criteria:**
- [ ] Anthropic API key stored securely (environment variable or secrets manager)
- [ ] HTTP client configured with retry policy (exponential backoff)
- [ ] Timeout handling (default 120s, configurable)
- [ ] Prompt loader reads markdown files from `agents/*/prompt-v1.md`
- [ ] Reference files concatenated into system prompt
- [ ] JSON output parsing with schema validation
- [ ] Token usage logged per request
- [ ] Rate limit handling (429 → backoff and retry)

**Service Interface:**
```csharp
public interface IAnthropicService
{
    Task<WriterOutput> RunWriter(ResearchData data, CancellationToken ct);
    Task<QualityReviewerOutput> RunQualityReviewer(WriterOutput data, CancellationToken ct);
}
```

**Technical Notes:**
- Consider Claudia NuGet package for typed SDK
- Log to structured logging (Serilog) for cost analysis
- Implement circuit breaker for repeated failures

---

## Story P4: Batch Submission

> As a pipeline operator, I want to submit a batch of neighborhoods for processing, so that I can process overnight at 50% discount.

**Anthropic Batch API Flow:**
```
1. Generate JSONL file with all requests
2. Upload and submit batch to Anthropic
3. Return batch ID for status tracking
```

**CLI Command:**
```bash
dotnet run -- batch submit --municipality 44021
dotnet run -- batch submit --all-pending --limit 500
```

**Acceptance Criteria:**
- [ ] Command generates valid JSONL for Anthropic Batch API
- [ ] Each line contains: custom_id (nis_code), model, messages
- [ ] Submits batch via Anthropic API
- [ ] Stores batch ID and metadata in `pipeline.batch_jobs` table
- [ ] Reports: neighborhoods submitted, estimated cost
- [ ] Supports `--municipality` and `--all-pending` filters
- [ ] Supports `--limit` to cap batch size
- [ ] Supports `--dry-run` to show what would be submitted

**Technical Notes:**
- Batch API processes within 24 hours (usually faster)
- Max 10,000 requests per batch
- 50% discount on all tokens

---

## Story P5: Batch Status and Results

> As a pipeline operator, I want to check batch status and download results, so that I can see progress and retrieve completed content.

**CLI Commands:**
```bash
dotnet run -- batch status <batch-id>
dotnet run -- batch download <batch-id>
dotnet run -- batch list                    # Show all batches
```

**Acceptance Criteria:**
- [ ] `batch status` shows: progress, estimated completion, errors
- [ ] `batch download` retrieves results JSONL from Anthropic
- [ ] Parses results and saves to `content.neighborhood_prose`
- [ ] Updates `pipeline.pipeline_jobs` with status and scores
- [ ] Identifies failed items and logs for retry
- [ ] Reports: successful, failed, total cost
- [ ] `batch list` shows recent batches with status summary

**Technical Notes:**
- Poll Anthropic API for status (configurable interval)
- Consider webhook for completion notification (optional)
- Store raw results for debugging

---

## Story P6: Batch Run Command

> As a pipeline operator, I want a single command that submits, waits, and downloads, so that I can start a run before going to bed.

**CLI Command:**
```bash
dotnet run -- batch run --municipality 44021
dotnet run -- batch run --all-pending --limit 100
```

**Acceptance Criteria:**
- [ ] Command combines: submit → poll → download
- [ ] Polls status every 5 minutes (configurable)
- [ ] Outputs progress suitable for tmux/screen
- [ ] Handles interruption gracefully (can resume with `batch download`)
- [ ] Reports final summary: successful, failed, cost, duration
- [ ] Exit codes: 0 = success, 1 = partial failure, 2 = error

---

## Story P7: Pipeline Jobs Management

> As a pipeline operator, I want to view and manage pipeline jobs, so that I can track progress and retry failures.

**CLI Commands:**
```bash
dotnet run -- jobs status                   # Dashboard view
dotnet run -- jobs list --status failed     # Filter by status
dotnet run -- jobs retry --municipality 44021
dotnet run -- jobs reset <nis_code>         # Reset to pending
```

**API Endpoints:**
```
GET /api/jobs                               # List with filters
GET /api/jobs/{nis_code}                    # Single job details
POST /api/jobs/{nis_code}/retry             # Retry failed job
```

**Acceptance Criteria:**
- [ ] Dashboard shows: pending, in_progress, completed, failed counts
- [ ] Breakdown by municipality
- [ ] Recent activity (last 10 completed/failed)
- [ ] Retry command reprocesses failed jobs
- [ ] Reset command clears job for fresh processing
- [ ] API endpoints for programmatic access

---

## Story P8: Hangfire Job Dashboard

> As a pipeline operator, I want a visual dashboard for monitoring jobs, so that I can see what's running and troubleshoot failures.

**Acceptance Criteria:**
- [ ] Hangfire NuGet packages installed
- [ ] PostgreSQL storage in `pipeline` schema
- [ ] Dashboard accessible at `/hangfire`
- [ ] Basic authentication for dashboard access
- [ ] Background jobs visible: queued, processing, succeeded, failed
- [ ] Retry and delete actions available in UI
- [ ] Job details show: duration, exception, retry count

**Technical Notes:**
- Hangfire.PostgreSql for storage
- Configure in `Program.cs` with dashboard options
- Consider separate queue for batch vs interactive jobs

---

## Story P9: Scheduled Processing

> As a pipeline operator, I want processing to run on a schedule, so that new neighborhoods are processed automatically.

**Acceptance Criteria:**
- [ ] Recurring job: process pending neighborhoods daily
- [ ] Configurable batch size per run (default: 50)
- [ ] Runs during off-peak hours (e.g., 2 AM)
- [ ] Skips if no pending neighborhoods
- [ ] Notifications on failure (optional: email/Slack)
- [ ] Can be disabled via configuration

**Technical Notes:**
- Uses Hangfire recurring jobs
- Coordinates with ETL schedule (O6) — run after data refresh

---

## Story P10: Cost Tracking and Reporting

> As a pipeline operator, I want to see how much processing costs, so that I can budget and optimize.

**Acceptance Criteria:**
- [ ] Token usage logged per request (input, output, cached)
- [ ] Cost calculated using current Anthropic pricing
- [ ] Daily/weekly/monthly cost reports
- [ ] Cost breakdown by municipality
- [ ] Dashboard widget showing recent spend
- [ ] Alert when approaching budget threshold (optional)

**Report Endpoint:**
```
GET /api/costs?period=month
```

---

## Dependencies

```
Epic M (Server Infrastructure)
  └── M3 (ASP.NET Core Project)

Epic O (ETL Automation)
  └── O4 (Materialized Views) — P1 uses these for efficient queries

Epic L (Agent Fine-Tuning)
  └── L6 (Materialized Views) — creates views used by P1
  └── L7 (Qualitative Language) — improves prose quality
  └── L8 (Merged Reviewers) — reduces token costs

Epic P (Content Pipeline API)
  └── P1 (Research Endpoint)
        └── P2 (Single Processing)
              └── P3 (Anthropic Integration)
                    └── P4 (Batch Submission)
                          └── P5 (Batch Results)
                                └── P6 (Batch Run)
  └── P7 (Jobs Management) — parallel to P4-P6
  └── P8 (Hangfire Dashboard) — parallel, enables P9
        └── P9 (Scheduled Processing)
  └── P10 (Cost Tracking) — parallel, builds over time
```

---

## Cost Analysis

**One-time full run (2,800 neighborhoods):**

| Component | Tokens | Batch API Cost |
|-----------|--------|----------------|
| Writer | ~14K in, ~4K out | ~$0.05/neighborhood |
| Quality Reviewer | ~18K in, ~10K out | ~$0.10/neighborhood |
| **Total per neighborhood** | ~32K in, ~14K out | **~$0.15** |
| **Full run (2,800)** | | **~$420** |

*Assumes L8 (merged reviewers) and L7 (qualitative language) are implemented first.*

---

## Migration Path

**Phase 1: Single Processing (P1-P3)**
- Build research endpoint
- Implement Anthropic integration
- Test with single neighborhoods

**Phase 2: Batch Processing (P4-P6)**
- Add batch submission and retrieval
- Run first municipality batch

**Phase 3: Automation (P7-P9)**
- Add Hangfire dashboard
- Enable scheduled processing
- Retire Claude Code pipeline (keep for debugging)

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| API costs exceed estimate | Medium | Start with one municipality, verify costs before full run |
| Batch API latency unpredictable | Low | Use standard API for urgent single items |
| Prompt behavior differs from Claude Code | Medium | Test extensively on sample before full run |
| Anthropic rate limits | Medium | Implement backoff; Batch API has no rate limits |

---

## Out of Scope

- ETL import commands (Epic O)
- Content architecture changes (Epic N)
- Materialized view creation (Epic L)
