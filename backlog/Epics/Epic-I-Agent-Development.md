# Epic I — Agent Development

**Goal:** Create and test the AI agent prompts that will generate neighborhood content: a researcher to gather data, a writer to create copy, and reviewers for SEO and brand voice.

**Depends on:** Epic H (Infrastructure Foundation) — agents need database access and output schema.

---

## Story I1: Define Agent Output Schema
> As a developer, I want a precisely defined JSON schema that agents must output, so that generated content matches our Content Collections format exactly.

**Context:** This schema bridges agents and Astro. Agents output JSON → validated against schema → saved to Content Collections.

**Acceptance Criteria:**
- [ ] JSON Schema document created defining all fields agents must produce
- [ ] Schema matches Content Collections Zod schema from Epic H
- [ ] Character limits defined: title (50-60), subtitle (80-120), intro (400-800 words)
- [ ] Required vs optional fields clearly marked
- [ ] Three annotated examples provided showing expected output
- [ ] Schema versioned (v1.0) and stored in `/agents/schemas/`
- [ ] Validation script created to test JSON against schema

---

## Story I2: Researcher Agent Prompt
> As a content team member, I want a Researcher agent that queries PostGIS and gathers all data needed for a neighborhood page, so that content is based on real facts, not hallucinated information.

**Context:** Researcher is first in the pipeline. It queries the database and outputs structured data for the Writer.

**Acceptance Criteria:**
- [ ] System prompt created defining researcher role and constraints
- [ ] Prompt includes example MCP queries for: POIs by category, distances, statistics
- [ ] Agent outputs structured JSON with: POI lists, counts, distances, demographic facts
- [ ] Output includes data source references (e.g., "OSM 2024", "Statbel Q3 2024")
- [ ] Agent tested on 3 Gent neighborhoods with accurate results
- [ ] Prompt stored in `/agents/prompts/researcher-v1.md`
- [ ] Output validated against intermediate schema (research output, not final page)

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
- [ ] Prompt stored in `/agents/prompts/writer-v1.md`

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
- [ ] Prompt stored in `/agents/prompts/seo-reviewer-v1.md`

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
- [ ] Prompt stored in `/agents/prompts/brand-reviewer-v1.md`

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
