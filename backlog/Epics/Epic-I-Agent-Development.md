# Epic I — Agent Development

**Goal:** Create and test the AI agent prompts that will generate neighborhood content: a researcher to gather data, a writer to create copy, and reviewers for SEO and brand voice.

**Depends on:** Epic H (Infrastructure Foundation) — agents need database access and output schema.

---

## Story I1: Define Agent Output Schema ✅

> As a developer, I want a precisely defined JSON schema that agents must output, so that generated content matches our Content Collections format exactly.

**Context:** This schema bridges agents and Astro. Agents output JSON → validated against schema → saved to Content Collections.

**Acceptance Criteria:**
- [x] JSON Schema document created defining all fields agents must produce
- [x] Schema matches Content Collections Zod schema from Epic H
- [x] Character limits defined: title (50-60), subtitle (80-120), intro (400-800 words)
- [x] Required vs optional fields clearly marked
- [x] Three annotated examples provided showing expected output
- [x] Schema versioned (v1.0) and stored in `/agents/schemas/`
- [x] Validation script created to test JSON against schema

**Implementation Notes (2025-01-02):**

Architecture decisions:
- **Zod as single source of truth** — JSON schemas generated from Zod via `zod-to-json-schema`, not maintained separately
- **Separate intermediate schemas** — ResearcherOutput (factual data only) → WriterOutput (adds narrative + icons) → FinalOutput (matches Astro)
- **Per-agent folder structure** — Each agent has its own folder with schema, examples, and (future) prompts
- **Character limits as soft guidance** — Defined in `shared/character-limits.json`, not enforced in Zod

Key files:
- `/agents/scripts/schemas.ts` — Zod schema definitions (source of truth)
- `/agents/researcher/output-schema.json` — Researcher output (factual, no prose)
- `/agents/writer/output-schema.json` — Writer output (adds narrative + icons)
- `/agents/shared/final-output-schema.json` — Final output (matches Content Collections)
- `/agents/shared/character-limits.json` — Target lengths for content fields
- `/agents/shared/terminology.json` — Brand vocabulary guide
- `/agents/researcher/examples/` — 3 annotated examples (Binnenstad, Dampoort, Wondelgem)

Scripts:
- `npm run schemas:generate` — Regenerate JSON schemas from Zod
- `npm run schemas:validate` — CI check: verify JSON matches Zod
- `npm run validate:json -- <schema> <file>` — Validate any JSON against a schema

Git hooks:
- Lefthook pre-commit hook validates schemas when `agents/scripts/schemas.ts` is modified
- See `/lefthook.yml` for configuration

---

## Story I2: Researcher Agent Prompt ✅
> As a content team member, I want a Researcher agent that queries PostGIS and gathers all data needed for a neighborhood page, so that content is based on real facts, not hallucinated information.

**Context:** Researcher is first in the pipeline. It queries the database and outputs structured data for the Writer.

**Acceptance Criteria:**
- [x] System prompt created defining researcher role and constraints
- [x] Prompt includes example MCP queries for: POIs by category, distances, statistics
- [x] Agent outputs structured JSON with: POI lists, counts, distances, demographic facts
- [x] Output includes data source references (e.g., "OSM 2024", "Statbel Q3 2024")
- [x] Agent tested on 3 Gent neighborhoods with accurate results
- [x] Prompt stored in `/agents/researcher/prompt-v1.md`
- [x] Output validated against intermediate schema (research output, not final page)

**Implementation Notes (2025-01-02):**

Architecture decisions:
- **Hybrid DB access** — Agent uses helper functions (`get_pois_in_neighborhood`, `get_nearest_pois_to_neighborhood`, etc.) for common patterns, raw SQL for edge cases
- **Execution model** — Claude with MCP PostgreSQL tool, iteratively querying the database
- **Missing data handling** — Adaptive radius expansion with category-specific max caps (e.g., vet: 5km, dog_park: 2km)
- **Neighborhood-size adaptive radius** — Base radius calculated from `area_km2`: <1km²=1000m, 1-3km²=1500m, 3-5km²=2000m, >5km²=3000m
- **POI boundary strategy** — Report POIs inside boundary, plus nearest outside if count is low
- **Walking time estimation** — Simple formula `distance_meters / 80` (5 km/h pace), marked as estimated

Schema update:
- Updated `poiCounts` to match actual DB categories: removed `restaurants`/`cafes`, added `pharmacies`/`busStops`/`trainStations`

Key files:
- `/agents/researcher/prompt-v1.md` — Main system prompt with workflow steps
- `/agents/researcher/references/poi-categories.md` — Available POI categories and DB values
- `/agents/researcher/references/query-examples.md` — Tested SQL patterns and helper functions
- `/agents/researcher/references/constraints.md` — Rules, limits, and error handling

Test outputs (validated against schema):
- `/agents/researcher/test-outputs/gent-dampoort-test.json` — Urban, 0.94 km², good amenities
- `/agents/researcher/test-outputs/gent-mendonk-test.json` — Rural, 9.49 km², sparse POIs (tests adaptive radius)
- `/agents/researcher/test-outputs/gent-rabot-test.json` — Dense urban, 0.72 km², 9230 pop/km²

Known limitations:
- Postal codes sparse in database; using "9000" as fallback for Gent
- `pricePerSqm` and `availableHomes` not available in Statbel data; output as null/0
- Example files in `/agents/researcher/examples/` use fictional neighborhood IDs (gent-binnenstad); test outputs use real IDs

---

## Story I3: Writer Agent Prompt ✅
> As a content team member, I want a Writer agent that transforms research data into engaging Dutch copy matching our brand voice, so that generated pages read naturally and include specific local details.

**Context:** Writer receives Researcher output and generates the narrative content: intro, daily life, section intros, benefits list.

**Acceptance Criteria:**
- [x] System prompt created with brand voice guidelines (friendly, informative, dog-owner focused)
- [x] Prompt includes tone examples from existing high-quality neighborhoods
- [x] Agent generates all narrative fields: intro, facilities.intro, dogParks.intro, dailyLife, etc.
- [x] Generated content includes specific data points ("4 dierenartsen binnen 1km")
- [x] Content balances positives with honest trade-offs
- [~] Agent tested on 3 neighborhoods with human-quality-comparable output (2/3 done, pending Rabot)
- [x] Prompt stored in `/agents/writer/prompt-v1.md`

**Implementation Notes (2025-01-03):**

Architecture decisions:
- **File-based input** — Writer reads ResearcherOutput from file path, not inline JSON
- **Read-only context access** — Can read reference files and query DB for verification only
- **Modular prompt structure** — Main prompt + separate reference files (like Researcher)
- **Dynamic examples** — Examples folder prepared but empty; will populate after first validated outputs
- **Nullable statistics** — Schema updated to allow null for pricePerSqm, availableHomes, medianPrice

Key files:
- `/agents/writer/prompt-v1.md` — Main system prompt with 14-step workflow
- `/agents/writer/references/icon-mappings.json` — FontAwesome 6 icon vocabulary
- `/agents/writer/references/content-guidelines.md` — Tone, terminology, sparse-data handling
- `/agents/writer/references/transformation-rules.md` — Distance formatting, zoom calculation
- `/agents/writer/examples/README.md` — Placeholder for future examples
- `/agents/writer/test-outputs/` — Validated test outputs

Schema changes:
- `writerOutputSchema.statistics.medianPrice` — now nullable
- `writerOutputSchema.statistics.availableHomes` — now nullable
- `writerOutputSchema.statistics.pricePerSqm` — now nullable
- Same changes applied to `finalOutputSchema`

Content approach:
- Required elements checklist for intro (character, dog-friendliness, trade-offs)
- Sparse data handling via "acknowledge → pivot → alternative" pattern
- Fixed typeformId ("buurt-feedback") for all neighborhoods
- Icon mappings by category (POIs, features, labels, value cards)
- Section intros stay focused on their topic only (no cross-references)

Test outputs (schema-validated):
- `gent-dampoort-writer-test.json` — Urban (5951/km²), 1 dog park, 1 vet, 1 pet store nearby
- `gent-mendonk-writer-test.json` — Rural (162/km²), sparse POIs, tests honest sparse-data handling

Prompt refinements during testing:
- Added explicit guidance that section intros must stay focused on their topic
- dogParks.intro → only hondenspeelweiden, not general parks
- petStores.intro → only pet stores, not supermarket alternatives
- vets.intro → only veterinary practices

Remaining:
- Test on gent-rabot-test.json (dense urban, different profile)
- Human quality review for final sign-off (Story I6)

---

## Story I4: SEO Reviewer Agent Prompt ✅
> As a content team member, I want an SEO Reviewer agent that validates and improves content for search visibility, so that generated pages rank well without manual SEO optimization.

**Context:** SEO agent reviews Writer output and suggests/makes improvements for search optimization.

**Acceptance Criteria:**
- [x] System prompt defines SEO best practices for local/neighborhood content
- [x] Agent checks: title length, meta description, heading structure, keyword usage
- [x] Agent suggests internal linking opportunities to related neighborhoods/city pages
- [x] Agent flags issues: keyword stuffing, thin content, missing metadata
- [x] Agent outputs: revised content + list of changes made + pass/fail score
- [x] Prompt includes what NOT to change (factual data, statistics)
- [x] Prompt stored in `/agents/seo-reviewer/prompt-v1.md`

**Implementation Notes (2025-01-04):**

Architecture decisions:
- **Direct Edit mode** — Agent modifies JSON directly, outputs revised content (not just suggestions)
- **Extended Schema** — New `seoReviewerOutputSchema` extends WriterOutput with `seoReview` object
- **Audit Trail** — Every change logged in `changesLog[]` with field, before, after, reason
- **70 Pass Threshold** — `passedSEO = true` if `qualityScore >= 70`
- **Non-blocking Pipeline** — Continues with warnings if score < 70
- **Validate-only for Links** — Checks `neighboringNeighborhoods` exist in DB, logs warnings but doesn't remove
- **Static Keyword Strategy** — Uses predefined keywords in reference file (updated periodically via separate research task)

Key files:
- `/agents/seo-reviewer/prompt-v1.md` — Main system prompt with 12-step workflow
- `/agents/seo-reviewer/output-schema.json` — Generated from Zod
- `/agents/seo-reviewer/references/seo-checklist.md` — Detailed SEO rules based on buurtkompas.be audit
- `/agents/seo-reviewer/references/keyword-strategy.md` — Target keywords and density rules
- `/agents/seo-reviewer/references/scoring-algorithm.md` — How 0-100 score calculated
- `/agents/seo-reviewer/references/do-not-modify.md` — Protected fields list

Schema additions to `/agents/scripts/schemas.ts`:
- `seoChangeLogSchema` — Field, before, after, reason (enum of 10 reason types)
- `seoValidationIssueSchema` — Field, issue, severity (error/warning/info)
- `seoScoreBreakdownSchema` — 6 category scores (subtitle, intro, keywords, sections, local, links)
- `seoReviewerOutputSchema` — Extends writerOutputSchema with `seoReview` object

Scoring breakdown (100 points total):
- Subtitle: 15 points (meta description quality)
- Main Intro: 25 points (core SEO content)
- Keywords: 20 points (usage and density)
- Section Intros: 15 points (supporting content SEO)
- Local Relevance: 15 points (local SEO signals)
- Internal Links: 10 points (link validity)

Remaining:
- Test on `gent-dampoort-writer-test.json` (expected score: 75-85)
- Test on `gent-mendonk-writer-test.json` (expected score: 65-75, may not pass)

---

## Story I5: Brand Reviewer Agent Prompt ✅
> As a content team member, I want a Brand Reviewer agent that ensures consistent voice and terminology, so that all generated content feels like it comes from the same source.

**Context:** Brand agent is final quality gate before output. Checks voice consistency and catches generic/cliché content.

**Acceptance Criteria:**
- [x] System prompt defines brand voice: tone, terminology, style guidelines
- [x] Terminology dictionary included: "baasjes" not "eigenaars", "viervoeter" not "huisdier"
- [x] Agent flags: marketing clichés, generic statements, inconsistent tone
- [x] Agent ensures local authenticity (specific details, not generic city descriptions)
- [x] Agent outputs: final polished content + quality score (0-100) + issues found
- [x] Quality threshold defined: score >= 70 passes (aligned with SEO), < 70 flags for review
- [x] Prompt stored in `/agents/brand-reviewer/prompt-v1.md`
- [x] Tested on gent-dampoort with score 94/100 (passed)

**Implementation Notes (2026-01-04):**

Architecture decisions:
- **Pipeline position** — Runs after SEO Reviewer, receives SEOReviewerOutput
- **Trust SEO, focus elsewhere** — SEO owns keywords/clichés, Brand owns terminology/tone
- **Non-blocking with flag** — Output continues but flagged for human review if score < 70
- **Direct edit mode** — Auto-fixes terminology violations, logs all changes
- **Detailed analysis** — Full debugging output like SEO Reviewer
- **Single source of truth** — All reference files point to `/agents/shared/terminology.json` instead of duplicating lists

Key files:
- `/agents/brand-reviewer/prompt-v1.md` — Main system prompt with 12-step workflow
- `/agents/brand-reviewer/output-schema.json` — Generated from Zod
- `/agents/brand-reviewer/references/scoring-algorithm.md` — 5-category scoring, references terminology.json
- `/agents/brand-reviewer/references/brand-checklist.md` — Quick reference, references terminology.json
- `/agents/brand-reviewer/references/tone-examples.md` — Good vs bad Dutch examples
- `/agents/brand-reviewer/references/do-not-modify.md` — Protected fields list

Schema additions to `/agents/scripts/schemas.ts`:
- `brandChangeLogSchema` — 8 change reason types (terminology_violation, tone_formal, etc.)
- `brandScoreBreakdownSchema` — 5 scoring categories
- `brandAnalysisSchema` — Detailed analysis with terminology, tone, authenticity, naturalness, sparse data
- `brandReviewerOutputSchema` — Extends SEOReviewerOutput with `brandReview` object

Scoring breakdown (100 points total, threshold 70):
- Terminology Compliance: 30 points (correct Dutch terms)
- Tone & Voice: 25 points (friendly, second-person, not corporate)
- Local Authenticity: 20 points (specific place names, insider details)
- Narrative Naturalness: 15 points (flows like prose, not fact dump)
- Sparse Data Handling: 10 points (acknowledge → pivot → alternative pattern)

Terminology updates to `/agents/shared/terminology.json`:
- Added `allowedPhrases` for "buurt" exceptions: buurtgevoel, de juiste buurt, buurtbewoners, in de buurt
- Added `alternativeAllowed` for "hondenspeelweide": allows "hondenpark" as SEO synonym

Test outputs:
- `/agents/brand-reviewer/test-outputs/gent-dampoort-brand-test.json` — Score 94/100, passed
  - No terminology violations (all "buurt" uses were in allowed phrases)
  - 10 unique place names, strong local authenticity
  - Perfect sparse data handling for pet stores
  - No changes needed to content

Known limitations:
- Mendonk only has Researcher output; needs Writer + SEO Reviewer before Brand can process

---

## Story I6: Manual Agent Testing Pipeline
> As a developer, I want to manually run the full agent sequence on test neighborhoods, so that we can validate content quality before building automated orchestration.

**Context:** Before automating, test the agents manually to ensure quality. Developer acts as orchestrator.

**Acceptance Criteria:**
- [x] Testing procedure documented: how to run each agent in sequence
- [x] 5 test neighborhoods selected: 2 urban, 2 suburban, 1 rural
- [ ] Each neighborhood run through full pipeline: Researcher → Writer → SEO → Brand
- [ ] Output JSON files saved and validated against schema
- [ ] Human review conducted: content quality rated 1-5 on accuracy, readability, brand voice
- [ ] Issues logged and prompts refined based on findings
- [ ] At least 3/5 neighborhoods achieve quality rating >= 4
- [ ] Learnings documented for orchestration phase

**Implementation Notes (2026-01-05):**

Test neighborhoods selected (verified in database):
- `gent-dampoort` — Urban, 0.94 km², 5951/km² (baseline, has vets/parks)
- `gent-rabot` — Dense urban, 0.72 km², 9230/km² (sparse vets/pet stores)
- `gent-mendonk` — Rural, 9.49 km², 162/km² (very sparse POIs, tests adaptive radius)
- `gent-brugse-poort` — Suburban, 1.90 km², 9467/km² (high density, sparse vets)
- `gent-blaarmeersen` — Suburban, 4.28 km², 1825/km² (green/spacious, 4 dog parks)

Note: `gent-wondelgem` and `gent-binnenstad` from examples don't exist in DB; replaced with brugse-poort and blaarmeersen.

Key files created:
- `/agents/docs/testing-runbook.md` — Step-by-step manual testing procedure
- `/agents/docs/prompt-refinements.md` — Log for tracking prompt changes
- `/agents/scripts/review/review-schema.ts` — Zod schema for human reviews
- `/agents/shared/review-template.json` — Empty template for review JSON
- `/agents/shared/human-review-schema.json` — Generated JSON schema for reviews

Remaining work:
- Run each neighborhood through full pipeline manually
- Fill in human review JSONs
- Refine prompts based on findings
- Document learnings for Epic J

---

## Dependencies

```
I1 (Schema)
  └── I2 (Researcher) ─┐
                       ├── I3 (Writer) ─┐
                       │                ├── I4 (SEO) ──┐
                       │                │              ├── I5 (Brand) ── I6 (Testing)
                       │                └──────────────┘
                       └────────────────────────────────────────────────┘
```

I1 must be done first. I2-I5 can be developed in parallel but tested sequentially. I6 validates the complete chain.
