# Epic M — API Migration & Server Infrastructure

**Goal:** Move from interactive Claude Code sessions to an automated, server-based pipeline that can process neighborhoods unattended using the Anthropic Batch API.

**Depends on:** Epic J (Agent Pipeline), Epic L (Agent Fine-Tuning)

---

## Context

The current pipeline runs via Claude Code interactive sessions, requiring manual attention and limiting throughput to ~2 parallel terminals. Processing all 2,800 Flanders neighborhoods would take 150-230 hours of active session time.

Moving to a server-based architecture with the Anthropic Batch API enables:
- **Unattended processing**: Submit batch, get results next day
- **True parallelism**: 50+ concurrent API requests vs 2 terminals
- **Cost efficiency**: Batch API offers 50% discount on tokens
- **Scalability**: Process all neighborhoods in 6-12 hours vs 150+ hours

**Estimated one-time cost for full run:** $450-620 (Batch API pricing)

---

## Story M1: PostGIS Database on Lightsail

> As a pipeline operator, I want the GIS database hosted on a server, so that it's always available and doesn't require my local machine to be running.

**Current State:**
- PostgreSQL + PostGIS runs locally
- Pipeline requires local DB connection
- Can't process when laptop is off/sleeping

**Acceptance Criteria:**
- [ ] Lightsail instance provisioned (2GB RAM recommended for PostGIS)
- [ ] PostgreSQL 14+ with PostGIS extension installed
- [ ] GIS data migrated (neighborhoods, pois, neighborhood_statistics)
- [ ] Connection secured (SSH tunnel or VPC, not public)
- [ ] Backup strategy documented
- [ ] Local MCP config updated to connect to remote DB

**Technical Notes:**
- Lightsail $10/month (2GB) is comfortable for PostGIS with spatial queries
- Consider pg_dump/pg_restore for migration
- Keep pipeline_jobs table local or migrate to same instance

---

## Story M2: ASP.NET Core Data API

> As a pipeline operator, I want an API that serves pre-fetched research data, so that batch processing doesn't require real-time database tool calls.

**Context:** The Anthropic API doesn't have MCP tool access. Instead of having Claude query the database, we pre-fetch all data and pass it as context.

**Proposed Endpoints:**

| Endpoint | Purpose |
|----------|---------|
| `GET /api/research/{nis_code}` | Return all POI data for neighborhood |
| `GET /api/neighborhoods/pending` | List NIS codes not yet processed |
| `GET /api/neighborhoods/municipality/{nis5}` | List neighborhoods in municipality |
| `POST /api/jobs/{nis_code}/complete` | Mark job completed with scores |

**Acceptance Criteria:**
- [ ] ASP.NET Core Web API project created
- [ ] Entity Framework Core + Npgsql.EntityFrameworkCore.PostgreSQL configured
- [ ] `/api/research/{nis_code}` returns data matching Researcher output schema
- [ ] API deployed to Lightsail alongside PostGIS
- [ ] Basic auth or API key protection
- [ ] Swagger documentation generated

**Technical Notes:**
- Single endpoint replaces 7-8 Researcher database queries
- Response should match `ResearcherOutput` schema for compatibility
- Consider caching frequently accessed data

---

## Story M3: Hangfire Job Orchestration

> As a pipeline operator, I want background job processing with a dashboard, so that I can monitor progress and retry failed jobs.

**Why Hangfire:**
- Visual dashboard for job monitoring
- Automatic retries with exponential backoff
- Job chaining (`ContinueWith`) for pipeline stages
- Persistence survives server restarts
- Rate limiting to stay within API limits

**Job Structure:**

```
ProcessNeighborhoodJob(nisCode)
  ├── FetchResearchData (local, no API)
  ├── RunWriter (Anthropic API)
  ├── RunQualityReviewer (Anthropic API)
  └── PublishIfQualified (local)
```

**Acceptance Criteria:**
- [ ] Hangfire NuGet packages installed
- [ ] PostgreSQL storage configured (same instance as GIS)
- [ ] Dashboard accessible with authentication
- [ ] `ProcessNeighborhoodJob` implemented with stage chaining
- [ ] `ProcessMunicipalityBatch` for bulk operations
- [ ] Retry policy: 3 attempts with exponential backoff
- [ ] Concurrency limit configurable (default: 10 parallel jobs)

**Technical Notes:**
- Hangfire uses same PostgreSQL instance (separate schema)
- Dashboard at `/hangfire` with basic auth
- Consider separate queue for batch vs interactive jobs

---

## Story M4: Anthropic API Integration

> As a pipeline operator, I want to call Claude via the Anthropic API, so that processing can happen without interactive sessions.

**Current State:**
- Agents invoked via Claude Code Task tool
- Prompts in markdown files with `@file` references
- Output written to `agents/pipeline-outputs/`

**Migration Approach:**

```csharp
public class AnthropicService
{
    public async Task<WriterOutput> RunWriter(ResearchData data)
    {
        var prompt = LoadPrompt("writer");  // Load from agents/writer/prompt-v1.md
        var references = LoadReferences("writer");  // Load from agents/writer/references/

        var response = await _client.Messages.CreateAsync(new()
        {
            Model = "claude-sonnet-4-20250514",
            System = prompt + references,
            Messages = [new() { Role = "user", Content = JsonSerializer.Serialize(data) }],
            MaxTokens = 8192
        });

        return ParseOutput<WriterOutput>(response);
    }
}
```

**Acceptance Criteria:**
- [ ] Anthropic API key securely stored (environment variable or secrets manager)
- [ ] HTTP client configured with retry/timeout policies
- [ ] Prompt loader reads existing markdown files
- [ ] Reference files concatenated into system prompt
- [ ] JSON output parsing with Zod-equivalent validation
- [ ] Error handling for rate limits, timeouts, invalid responses
- [ ] Token usage logging for cost tracking

**Technical Notes:**
- Use [Claudia](https://github.com/Cysharp/Claudia) C# SDK or raw HttpClient
- Prompts remain in `agents/*/prompt-v1.md` (single source of truth)
- Consider prompt caching for repeated system prompts

---

## Story M5: Batch API Integration

> As a pipeline operator, I want to submit large batches to Anthropic's Batch API, so that I can process overnight at 50% discount.

**Batch API Flow:**

```
1. Generate JSONL file with all requests (local, 5 min)
2. Upload and submit batch (API call)
3. Poll for completion or wait for webhook
4. Download results JSONL
5. Parse and save outputs
```

**Acceptance Criteria:**
- [ ] Batch request generator creates valid JSONL format
- [ ] Batch submission via Anthropic API
- [ ] Status polling with configurable interval
- [ ] Results download and parsing
- [ ] Output files written to `agents/pipeline-outputs/`
- [ ] Failed items identified and logged for retry
- [ ] Cost tracking (tokens used, estimated cost)

**Technical Notes:**
- Batch API processes within 24 hours (usually faster)
- 50% discount on all tokens
- Max 10,000 requests per batch
- Consider splitting by municipality for manageable batches

---

## Story M6: CLI Trigger for Batch Processing

> As a pipeline operator, I want simple commands to trigger batch processing, so that I can start a run before going to bed.

**Proposed Commands:**

```bash
# Generate and submit batch for municipality
dotnet run -- batch submit --municipality 44021

# Generate and submit batch for all pending
dotnet run -- batch submit --all-pending --limit 500

# Check batch status
dotnet run -- batch status <batch-id>

# Download and process results
dotnet run -- batch download <batch-id>

# Full pipeline: submit, wait, download, publish
dotnet run -- batch run --municipality 44021
```

**Acceptance Criteria:**
- [ ] CLI commands implemented using System.CommandLine
- [ ] `batch submit` generates JSONL and submits to Anthropic
- [ ] `batch status` shows progress and estimated completion
- [ ] `batch download` retrieves results and saves outputs
- [ ] `batch run` combines all steps with polling
- [ ] Progress output suitable for running in tmux/screen
- [ ] Exit codes for scripting (0 = success, 1 = partial failure, 2 = error)

---

## Dependencies

```
Epic J (Agent Pipeline) - existing prompts and schemas
Epic L (Agent Fine-Tuning) - prompt optimizations reduce costs
  │
  └── M1 (PostGIS on Lightsail)
        └── M2 (Data API)
              └── M3 (Hangfire)
                    └── M4 (Anthropic API)
                          └── M5 (Batch API)
                                └── M6 (CLI Trigger)
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

**Infrastructure:**
- Lightsail 2GB: $10/month
- Domain/SSL: ~$12/year (optional)

---

## Migration Path

**Phase 1: Infrastructure (M1-M2)**
- Set up Lightsail with PostGIS
- Build data API
- Keep using Claude Code for processing

**Phase 2: Job System (M3-M4)**
- Add Hangfire
- Implement standard API integration
- Test with small batches

**Phase 3: Batch Processing (M5-M6)**
- Implement Batch API integration
- Run full municipality batches overnight
- Retire Claude Code pipeline (keep for debugging)

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| API costs exceed estimate | Medium | Start with one municipality, verify costs before full run |
| Batch API latency unpredictable | Low | Use standard API for urgent single items |
| Prompt behavior differs from Claude Code | Medium | Test extensively on sample before full run |
| Server costs add up | Low | $10/month is minimal; can scale down when not processing |
| Anthropic rate limits | Medium | Implement backoff; Batch API has no rate limits |
