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

## Story I3: Writer Agent Prompt
> As a content team member, I want a Writer agent that transforms research data into engaging Dutch copy matching our brand voice, so that generated pages read naturally and include specific local details.

**Context:** Writer receives Researcher output and generates the narrative content: intro, daily life, section intros, benefits list.

**Acceptance Criteria:**
- [ ] System prompt created with brand voice guidelines (friendly, informative, dog-owner focused)
- [ ] Prompt includes tone examples from existing high-quality neighborhoods
- [ ] Agent generates all narrative fields: intro, facilities.intro, dogParks.intro, dailyLife, etc.
- [ ] Generated content includes specific data points ("4 dierenartsen binnen 1km")
- [ ] Content balances positives with honest trade-offs
- [ ] Agent tested on 3 neighborhoods with human-quality-comparable output
- [ ] Prompt stored in `/agents/writer/prompt-v1.md`

---

## Story I4: SEO Reviewer Agent Prompt
> As a content team member, I want an SEO Reviewer agent that validates and improves content for search visibility, so that generated pages rank well without manual SEO optimization.

**Context:** SEO agent reviews Writer output and suggests/makes improvements for search optimization.

**Acceptance Criteria:**
- [ ] System prompt defines SEO best practices for local/neighborhood content
- [ ] Agent checks: title length, meta description, heading structure, keyword usage
- [ ] Agent suggests internal linking opportunities to related neighborhoods/city pages
- [ ] Agent flags issues: keyword stuffing, thin content, missing metadata
- [ ] Agent outputs: revised content + list of changes made + pass/fail score
- [ ] Prompt includes what NOT to change (factual data, statistics)
- [ ] Prompt stored in `/agents/seo-reviewer/prompt-v1.md`

---

## Story I5: Brand Reviewer Agent Prompt
> As a content team member, I want a Brand Reviewer agent that ensures consistent voice and terminology, so that all generated content feels like it comes from the same source.

**Context:** Brand agent is final quality gate before output. Checks voice consistency and catches generic/cliché content.

**Acceptance Criteria:**
- [ ] System prompt defines brand voice: tone, terminology, style guidelines
- [ ] Terminology dictionary included: "baasjes" not "eigenaars", "viervoeter" not "huisdier"
- [ ] Agent flags: marketing clichés, generic statements, inconsistent tone
- [ ] Agent ensures local authenticity (specific details, not generic city descriptions)
- [ ] Agent outputs: final polished content + quality score (0-100) + issues found
- [ ] Quality threshold defined: score >= 80 passes, < 80 flags for review
- [ ] Prompt stored in `/agents/brand-reviewer/prompt-v1.md`

---

## Story I6: Manual Agent Testing Pipeline
> As a developer, I want to manually run the full agent sequence on test neighborhoods, so that we can validate content quality before building automated orchestration.

**Context:** Before automating, test the agents manually to ensure quality. Developer acts as orchestrator.

**Acceptance Criteria:**
- [ ] Testing procedure documented: how to run each agent in sequence
- [ ] 5 test neighborhoods selected: 2 urban, 2 suburban, 1 rural
- [ ] Each neighborhood run through full pipeline: Researcher → Writer → SEO → Brand
- [ ] Output JSON files saved and validated against schema
- [ ] Human review conducted: content quality rated 1-5 on accuracy, readability, brand voice
- [ ] Issues logged and prompts refined based on findings
- [ ] At least 3/5 neighborhoods achieve quality rating >= 4
- [ ] Learnings documented for orchestration phase

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
