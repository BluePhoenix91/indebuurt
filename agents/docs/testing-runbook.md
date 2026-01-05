# Agent Testing Runbook

Manual testing procedure for the content generation pipeline. This runbook guides you through running each agent and evaluating output quality.

## Test Neighborhoods

| ID | Type | Profile | Notes |
|----|------|---------|-------|
| `gent-dampoort` | Urban | 0.94 km², 5951/km² | Good baseline, has vets/parks |
| `gent-rabot` | Dense urban | 0.72 km², 9230/km² | Sparse vets/pet stores |
| `gent-mendonk` | Rural | 9.49 km², 162/km² | Very sparse POIs, tests adaptive radius |
| `gent-brugse-poort` | Suburban | 1.90 km², 9467/km² | High density suburban, sparse vets |
| `gent-blaarmeersen` | Suburban | 4.28 km², 1825/km² | Green/spacious, 4 dog parks |

## Prerequisites

- Claude Code with PostgreSQL MCP configured (for Researcher)
- Access to Claude.ai or Anthropic Workbench (for all agents)
- Node.js 18+ (for validation scripts)

## Pipeline Overview

```
Researcher → Writer → SEO Reviewer → Brand Reviewer
```

Each agent's output feeds into the next. Run them in sequence.

---

## Agent 1: Researcher

**Purpose**: Query PostGIS database for factual neighborhood data.

**Model recommendation**: Claude Haiku (data-heavy, less prose needed)

### Running in Claude Code (Recommended)

1. Start a new Claude Code session with postgres MCP enabled
2. Set the system context by reading the prompt:
   ```
   Read agents/researcher/prompt-v1.md and use it as your guide
   ```
3. Request output:
   ```
   Generate ResearcherOutput JSON for neighborhood: gent-dampoort
   ```
4. The agent will query the database via MCP and produce JSON output
5. Save the output:
   ```bash
   # Copy JSON to clipboard, then:
   # Save to: agents/researcher/test-outputs/gent-dampoort-test.json
   ```

### Running in Claude.ai (Alternative)

1. Open claude.ai
2. Paste the contents of `agents/researcher/prompt-v1.md` as your first message
3. Then send: "Generate ResearcherOutput for neighborhood: gent-dampoort"
4. When Claude requests SQL queries, run them manually:
   ```bash
   # In a terminal with psql access:
   psql $POSTGRES_URL -c "SELECT ... FROM neighborhoods WHERE id = 'gent-dampoort'"
   ```
5. Paste query results back to Claude
6. Continue until JSON output is complete

### Validation

```bash
cd agents
npm run validate:json -- researcher researcher/test-outputs/gent-dampoort-test.json
```

### Output Location

`agents/researcher/test-outputs/{neighborhood-id}-test.json`

---

## Agent 2: Writer

**Purpose**: Transform research data into engaging Dutch narrative content.

**Model recommendation**: Claude Sonnet (prose quality matters)

### Running

1. Open Claude.ai or Anthropic Workbench
2. Set system prompt from `agents/writer/prompt-v1.md`
3. Include reference files inline:
   - `agents/writer/references/icon-mappings.json`
   - `agents/writer/references/content-guidelines.md`
   - `agents/writer/references/transformation-rules.md`
   - `agents/shared/terminology.json`
4. User message:
   ```
   Transform this ResearcherOutput into WriterOutput:

   [paste contents of researcher test output JSON]
   ```
5. Save output to `agents/writer/test-outputs/{neighborhood-id}-writer-test.json`

### Validation

```bash
npm run validate:json -- writer writer/test-outputs/gent-dampoort-writer-test.json
```

---

## Agent 3: SEO Reviewer

**Purpose**: Optimize content for search visibility.

**Model recommendation**: Claude Sonnet

### Running

1. Set system prompt from `agents/seo-reviewer/prompt-v1.md`
2. Include reference files:
   - `agents/seo-reviewer/references/seo-checklist.md`
   - `agents/seo-reviewer/references/keyword-strategy.md`
   - `agents/seo-reviewer/references/scoring-algorithm.md`
   - `agents/seo-reviewer/references/do-not-modify.md`
3. User message:
   ```
   Review and optimize this WriterOutput for SEO:

   [paste writer output JSON]
   ```
4. Save to `agents/seo-reviewer/test-outputs/{neighborhood-id}-seo-test.json`

### Validation

```bash
npm run validate:json -- seo-reviewer seo-reviewer/test-outputs/gent-dampoort-seo-test.json
```

---

## Agent 4: Brand Reviewer

**Purpose**: Ensure consistent brand voice and terminology.

**Model recommendation**: Claude Sonnet

### Running

1. Set system prompt from `agents/brand-reviewer/prompt-v1.md`
2. Include reference files:
   - `agents/brand-reviewer/references/brand-checklist.md`
   - `agents/brand-reviewer/references/tone-examples.md`
   - `agents/brand-reviewer/references/scoring-algorithm.md`
   - `agents/brand-reviewer/references/do-not-modify.md`
   - `agents/shared/terminology.json`
3. User message:
   ```
   Review this SEOReviewerOutput for brand compliance:

   [paste SEO reviewer output JSON]
   ```
4. Save to `agents/brand-reviewer/test-outputs/{neighborhood-id}-brand-test.json`

### Validation

```bash
npm run validate:json -- brand-reviewer brand-reviewer/test-outputs/gent-dampoort-brand-test.json
```

---

## Human Review Process

After completing the pipeline for a neighborhood:

1. Create a review JSON file:
   ```
   agents/reviews/{neighborhood-id}-review.json
   ```

2. Copy the template from `agents/shared/review-template.json`

3. Fill in ratings (1-5 scale):
   - **1**: Poor, needs significant rework
   - **2**: Below average, multiple issues
   - **3**: Acceptable, minor issues
   - **4**: Good, production-ready with minor tweaks
   - **5**: Excellent, publish as-is

4. Document any issues found in the `issuesFound` array

5. If issues require prompt changes, log them in `agents/docs/prompt-refinements.md`

---

## Troubleshooting

### Validation Fails

1. Check the error message for the specific field
2. Common issues:
   - Missing required fields
   - Wrong data types (string vs number)
   - Array length constraints (labels: 2-5, valueCards: 4-8)
3. Re-run the agent with the validation error in context

### Agent Doesn't Follow Schema

1. Ensure the full prompt is in the system context
2. Explicitly request "output valid JSON matching the schema"
3. If persistent, add the JSON schema itself to the context

### MCP Connection Issues

1. Verify postgres MCP is configured in Claude Code settings
2. Test with a simple query: `SELECT 1`
3. Check connection string in MCP config

### Output Too Long / Truncated

1. Request output in parts: "First output the intro and labels, then I'll ask for the rest"
2. Use Anthropic Workbench with higher max_tokens

---

## Quality Targets

**Success criteria for Story I6:**
- At least 3 of 5 neighborhoods achieve overall rating >= 4
- All outputs pass schema validation
- No blocker-severity issues unresolved

**Per-stage quality signals:**

| Stage | Good Sign | Warning Sign |
|-------|-----------|--------------|
| Researcher | All POIs have distances, sources cited | Missing categories, null coordinates |
| Writer | Natural Dutch, specific local details | Generic text, marketing clichés |
| SEO | Score >= 70, keywords natural | Score < 70, keyword stuffing feel |
| Brand | Score >= 70, correct terminology | Wrong terms (eigenaars, huisdier) |
