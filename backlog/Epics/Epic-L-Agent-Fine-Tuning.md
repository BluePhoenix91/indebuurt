# Epic L — Agent Fine-Tuning

**Goal:** Improve agent output quality through iterative refinements based on real pipeline results.

**Depends on:** Epic J (Agent Pipeline)

---

## Context

After running the initial pipeline on test neighborhoods, several output quality issues were identified that require agent prompt adjustments or additional data processing. This epic tracks incremental improvements to agent behavior.

---

## Story L1: Dog Park Feature Tags and Nearest Street

> As a content reader, I want dog parks to show relevant features (fenced, water, etc.) and a nearby street name, so that I can better identify and navigate to the location.

**Context:** Dog parks in OSM typically don't have street addresses. The current output shows generic names like "Hondenpark west" with no features. The researcher could look up the nearest street for better naming, and the writer could display feature tags when available.

**Current State:**
- Dog parks have coordinates but no address
- Names are generic placeholders or OSM names (often missing)
- Feature tags (isFenced, hasWater, etc.) exist in researcher output but show as empty in final content

**Acceptance Criteria:**
- [ ] Researcher queries nearest street name for each dog park coordinate
- [ ] Dog park objects include `nearestStreet` field in researcher output
- [ ] Writer uses nearest street in generated names when OSM name is unavailable (e.g., "Hondenpark nabij Bergemeesterstraat")
- [ ] Feature tags displayed when available (fenced, water access, lighting, etc.)
- [ ] Researcher and writer output schemas updated to include new fields

**Technical Notes:**
- PostGIS can find nearest street via spatial query on OSM road network
- Consider caching street lookups since multiple dog parks may share nearest street
- Feature tags come from OSM tags like `fenced=yes`, `drinking_water=yes`

---

## Story L2: Normalize Neighborhood Name Casing ✅

> As a content reader, I want neighborhood names in normal title case, so that the content looks professional rather than SHOUTING.

**Status:** Completed (2026-01-10)

**Solution:** Fixed at GIS database level rather than in Writer agent.

**Implementation:**
- Created PostgreSQL function `normalize_dutch_name()` in migration `20260110_007`
- Function uses `initcap()` + regex fixes for Dutch prefixes ('s, 't) and Roman numerals
- Updated 2,791 neighborhoods and ~19,000 statistical sectors
- City names were already correctly cased (no change needed)

**Acceptance Criteria:**
- [x] ~~Writer transforms neighborhood name to title case~~ → Fixed at DB level
- [x] Handles edge cases: "SINT-NIKLAAS" → "Sint-Niklaas", hyphenated words
- [x] City name also normalized if needed → Already correct
- [x] ~~Updated in writer output schema and transformation rules~~ → N/A (DB fix)

**Files:**
- `database/migrations/20260110_007_normalize-neighborhood-names.sql`

---

## Story L3: Ralph Loop for Batch Processing

> As a pipeline operator, I want batch operations to automatically continue until completion, so that Claude doesn't quit early when processing multiple neighborhoods.

**Context:** When processing multiple neighborhoods (e.g., `/pipeline municipality 44021`), Claude sometimes exits after 1-2 items instead of completing all work. This is Claude's default "single-pass" behavior. The Ralph Wiggum technique (official Anthropic plugin) solves this by using a Stop hook to intercept exit attempts and re-inject the prompt until a completion promise is detected.

**Current State:**
- Batch commands exist: `municipality`, `next N`, `retry-failed`, `regenerate municipality`
- Claude may stop mid-batch believing task is "done"
- No automatic continuation mechanism
- State tracking (pipeline_jobs) supports resume but requires manual re-invocation

**Proposed Solution:**
Use the official Ralph Wiggum plugin with smart defaults:

| Command | Ralph Loop | Max Iterations |
|---------|------------|----------------|
| `/pipeline <nis_code>` | No | N/A (single item) |
| `/pipeline municipality <nis5>` | Yes | count × 2 |
| `/pipeline next N` | Yes (if N > 1) | N × 2 |
| `/pipeline retry-failed` | Yes | count × 2 |
| `/pipeline publish <nis_code>` | No | N/A (single item) |
| `/pipeline claim auto` | Yes | count × 2 |
| `/pipeline claim <nis5>` | Yes | count × 2 |
| `/pipeline release` | No | N/A (admin, no processing) |
| `/pipeline regenerate <nis_code>` | No | N/A (single item) |
| `/pipeline regenerate municipality` | Yes | count × 2 |
| `/pipeline status` | No | N/A (read-only) |
| `/pipeline sessions` | No | N/A (read-only) |

**Acceptance Criteria:**
- [ ] Ralph Wiggum plugin installed and configured
- [ ] Pipeline detects batch vs single-item operations
- [ ] Batch operations invoke `/ralph-loop` with appropriate completion promise
- [ ] Max iterations calculated from batch size (count × 2 allows one retry per item)
- [ ] Completion promise matches structured pipeline output (e.g., "All {N} neighborhoods processed")
- [ ] Manual override available: `--no-loop` flag to disable for testing
- [ ] Documentation updated with loop behavior and cost implications

**Premortem - Risks & Mitigations:**

| Risk | Impact | Mitigation |
|------|--------|------------|
| Runaway API costs | High | Always cap iterations at count × 2; document cost implications |
| Loop continues after completion | Medium | Use structured JSON completion signals, not text matching |
| Partial failures block progress | Low | Existing pipeline handles this: failed items logged, processing continues |
| Conflicts with claim mechanism | Low | Claims are per-municipality; Ralph operates within session |

**Technical Notes:**
- Install: `/plugin install ralph-wiggum@claude-plugins-official`
- Usage: `/ralph-loop "Process municipality 44021" --max-iterations 20 --completion-promise "All 10 neighborhoods processed"`
- Cancel: `/cancel-ralph`
- Windows users need `jq` installed (undocumented dependency)

**Out of Scope:**
- Custom hook implementation (using official plugin instead)
- Parallel Ralph loops (one loop per terminal session is sufficient)

---

## Story L4: Fix Statbel NIS Code Mapping for Merged Municipalities

> As a content reader, I want to see house price data for all neighborhoods, so that I can compare affordability across areas.

**Context:** Belgium merged several municipalities on January 1, 2025. Statbel's 2024 house price data already uses the **new merged NIS codes**, but our neighborhoods database still uses the old pre-merger codes. This causes a join failure in the ETL script, resulting in NULL house prices for affected neighborhoods.

**Current State:**
- 246 neighborhoods (~8.8%) have NULL `median_house_price`
- Affected municipalities include Melle, Merelbeke, Lochristi, and others
- Pipeline outputs show `medianHousePrice: 0` for these areas
- The data **exists** in Statbel's source file but isn't matched

**Evidence:**

Statbel Excel file (`vastgoed_2010_9999.xlsx`) shows:

| Our NIS Code | Statbel NIS | Municipality Name | Median Price |
|--------------|-------------|-------------------|--------------|
| `44040` (Melle) | `44088` | MERELBEKE-MELLE | €400,000 |
| `44043` (Merelbeke) | `44088` | MERELBEKE-MELLE | €400,000 |
| `44034` (Lochristi) | `44087` | LOCHRISTI | €380,000 |

The ETL script joins on `municipality_nis` but the codes don't match:
```python
# In load-statistics.py line 270-274
merged = pop_df.merge(
    price_df,
    on="municipality_nis",  # Fails when codes differ
    how="left"
)
```

**Acceptance Criteria:**
- [ ] Identify all NIS code changes from 2025 municipality mergers
- [ ] Add NIS code mapping to ETL script (old code → new Statbel code)
- [ ] Re-run ETL to generate updated staging CSV
- [ ] Re-import statistics to GIS database
- [ ] Verify affected neighborhoods now have house prices
- [ ] Document mapping for future Statbel imports

**Technical Notes:**

Files involved:
- `database/scripts/statbel/load-statistics.py` — ETL script needing NIS mapping
- `database/data/statbel/vastgoed_2010_9999.xlsx` — Source Statbel data
- `database/data/statbel/neighborhood_statistics_staging.csv` — Generated staging file
- `database/migrations/20250102_005_load-statbel-statistics.sql` — Import script

Proposed fix approach:
1. Add mapping dict in ETL script: `NIS_CODE_MAPPING = {"44040": "44088", "44043": "44088", "44034": "44087", ...}`
2. Apply mapping before join: `pop_df["statbel_nis"] = pop_df["municipality_nis"].map(lambda x: NIS_CODE_MAPPING.get(x, x))`
3. Join on `statbel_nis` instead of `municipality_nis`

Alternative: Update our neighborhoods table to use new NIS codes (larger change, affects more systems)

**After Fix:**
- Re-run `/pipeline regenerate municipality 44040` for Melle
- Re-run for other affected municipalities

---

## Story L5: Reduce Prompt Token Usage via External References

> As a pipeline operator, I want agent prompts to be leaner, so that each neighborhood costs fewer tokens and processes faster.

**Context:** Current agent prompts total 1,473 lines (~20KB) with inline examples, terminology lists, and scoring algorithms. Much of this content is duplicated across agents or could be loaded on-demand from reference files.

**Current State:**
- Researcher: 322 lines
- Writer: 467 lines
- SEO Reviewer: 299 lines
- Brand Reviewer: 385 lines
- Total: ~1,473 lines of prompt content per neighborhood
- Estimated: 4-5K tokens in system prompts alone

**Estimated Savings:**
- 2-3K tokens per neighborhood
- ~15-20% reduction in total token usage
- Faster prompt loading and response times

**Acceptance Criteria:**
- [ ] Audit all agent prompts for content that can be externalized
- [ ] Move inline examples to `references/examples/` folders (read on-demand)
- [ ] Deduplicate terminology.json references (currently loaded by multiple agents)
- [ ] Extract shared scoring algorithm docs to single reference file
- [ ] Measure before/after token counts on sample neighborhoods
- [ ] Verify output quality unchanged after prompt reduction

**Optimization Targets:**

| Content Type | Current Location | Proposed Change |
|--------------|------------------|-----------------|
| Output examples | Inline in prompts | Move to `references/examples/*.json` |
| Terminology list | Duplicated in SEO + Brand | Single `references/terminology.json` |
| Scoring algorithms | Inline in SEO + Brand | Shared `references/scoring.md` |
| Icon mappings | Inline in Writer | Move to `references/icons.json` |
| Character limits | Duplicated across agents | Single `references/constraints.json` |

**Technical Notes:**
- Use `@file` references in prompts to load external content
- Consider lazy loading: only load examples when agent needs clarification
- Measure with `tiktoken` library for accurate token counts
- Keep critical instructions inline; only externalize reference material

---

## Story L6: Materialized Views for POI Aggregates

> As a pipeline operator, I want pre-computed POI counts per neighborhood, so that researcher queries run faster and use fewer database round-trips.

**Context:** The Researcher agent currently makes 7-8 separate PostGIS queries per neighborhood to count POIs by category. These queries are repeated for every neighborhood, even though POI data changes infrequently. Materialized views can pre-compute these aggregates.

**Current State:**
- Each researcher run queries: vets, pet stores, dog parks, parks, supermarkets, schools, pharmacies, bus stops
- Each query: ~100-500ms depending on neighborhood size
- Total: ~2-4 seconds of database time per neighborhood
- For 2,800 neighborhoods: ~2-3 hours of pure query time

**Proposed Materialized Views:**

```sql
-- Pre-computed POI counts per neighborhood
CREATE MATERIALIZED VIEW mv_neighborhood_poi_counts AS
SELECT
    n.nis_code,
    COUNT(*) FILTER (WHERE p.category = 'veterinary') as vet_count,
    COUNT(*) FILTER (WHERE p.category = 'pet_shop') as pet_store_count,
    COUNT(*) FILTER (WHERE p.category = 'dog_park') as dog_park_count,
    COUNT(*) FILTER (WHERE p.category = 'park') as park_count,
    -- ... other categories
FROM neighborhoods n
LEFT JOIN pois p ON ST_Contains(n.geometry, p.geometry)
GROUP BY n.nis_code;

-- Nearest POIs per neighborhood (top 5 each category)
CREATE MATERIALIZED VIEW mv_neighborhood_nearest_pois AS
SELECT DISTINCT ON (n.nis_code, p.category)
    n.nis_code,
    p.category,
    p.name,
    p.address,
    ST_Distance(n.centroid, p.geometry) as distance_meters
FROM neighborhoods n
CROSS JOIN LATERAL (
    SELECT * FROM pois
    WHERE category IN ('veterinary', 'pet_shop', 'dog_park')
    ORDER BY geometry <-> n.centroid
    LIMIT 5
) p;
```

**Acceptance Criteria:**
- [ ] Create `mv_neighborhood_poi_counts` materialized view
- [ ] Create `mv_neighborhood_nearest_pois` materialized view
- [ ] Add refresh script for weekly updates (`REFRESH MATERIALIZED VIEW CONCURRENTLY`)
- [ ] Update Researcher queries to use materialized views
- [ ] Measure query time improvement (target: 80% reduction)
- [ ] Document refresh schedule and manual refresh command

**Estimated Savings:**
- Query time: 2-4 seconds → 200-400ms per neighborhood
- Total for 2,800 neighborhoods: ~3 hours → ~20 minutes of query time
- Reduces database load during batch processing

**Technical Notes:**
- Use `CONCURRENTLY` refresh to avoid locking during updates
- Schedule refresh via cron or after OSM data imports
- Consider partial refresh for changed neighborhoods only
- Index on `nis_code` for fast lookups

---

## Story L7: Qualitative Language in Prose (No Specific Counts) ✅

> As a content reader, I want neighborhood descriptions that use qualitative language instead of specific numbers, so that the content stays accurate even when POI data changes.

**Status:** Completed (2026-01-10)

**Context:** SEO expert feedback indicates that specific counts in prose ("20 parken", "5 dierenartsen") create maintenance burden and can become stale. Qualitative descriptions ("ruim voldoende groen", "voldoende dierenartsen in de buurt") read more naturally and don't require regeneration when POI data updates.

**Scope Decisions:**
- All prose sections use qualitative language (intro, section intros, dailyLife)
- POI names removed from prose entirely
- Population stats from Statbel remain specific (annual update cadence)
- Walking distances remain specific (calculated from fixed centroids)
- valueCards and POI arrays keep specific counts

**Qualitative Language Guide:**

| Count Range | Qualitative Dutch |
|-------------|-------------------|
| 0 | "geen ... in de wijk", "niet aanwezig in de directe omgeving" |
| 1 | "één ...", "de enige ...", "een enkele ..." |
| 2 | "een paar ...", "twee opties" |
| 3-5 | "enkele ...", "een handvol ...", "meerdere opties" |
| 6-10 | "voldoende ...", "ruim voldoende ...", "voldoende keuze" |
| 11-20 | "veel ...", "een ruim aanbod aan ..." |
| 20+ | "talrijke ...", "een uitgebreid aanbod aan ...", "ruim voldoende" |

**Acceptance Criteria:**
- [x] Update Writer prompt to prohibit specific counts in prose (intro, paragraphs)
- [x] Add qualitative language guide to Writer references
- [x] Specific counts remain allowed in `valueCards` (structured UI elements)
- [x] POI names only in structured data, not in prose
- [x] Test on 3-5 sample neighborhoods to verify natural Dutch output
- [x] Document the style change for future prompt iterations

**Implementation Notes:**

Files modified:
- `agents/writer/prompt-v1.md` — Added "CRITICAL: Qualitative language in prose" sections in Steps 7, 9, 11
- `agents/writer/references/content-guidelines.md` — Added qualitative language guide, POI names section, updated examples
- `agents/shared/terminology.json` — Updated `specificity` and `encouragedPatterns` guidelines

Testing:
- Tested on 44021A0 (Begijnhofdries) — dense urban with 20 parks, 41 bus stops, 5 dog parks
- Verified: prose uses "talrijke parken", "meerdere praktijken", "goede busverbindingen"
- Verified: valueCards retain specific counts ("20 parken in de buurt", "41 bushaltes")
- Verified: POI names only in structured arrays, not in prose

**Impact:**
- Prose becomes stable (one-time AI cost)
- POI list updates cost €0 (script refresh only)
- Better SEO (no "outdated content" signals)

**After Implementation:**
- Regenerate all existing content with new style (~€450 one-time)
- Future OSM updates don't require prose regeneration

---

## Story L8: Merge SEO and Brand Reviewers

> As a pipeline operator, I want a single quality review stage instead of two separate ones, so that each neighborhood processes faster and costs fewer tokens.

**Context:** The SEO Reviewer and Brand Reviewer perform similar functions - both read the full Writer output, apply scoring algorithms, and make minor text adjustments. Running them separately means passing the full content twice through Claude.

**Current State:**
- SEO Reviewer: ~15K input tokens, ~6.5K output tokens
- Brand Reviewer: ~16K input tokens, ~7K output tokens
- Combined: ~31K input, ~13.5K output per neighborhood
- Two separate agent invocations = 2× latency

**Proposed Change:**
Merge into single "Quality Reviewer" agent that:
- Applies SEO scoring (keyword density, meta optimization)
- Applies Brand scoring (tone, terminology, authenticity)
- Produces single combined output with both review sections
- Returns unified `qualityScore` (weighted average)

**Acceptance Criteria:**
- [ ] Create combined `quality-reviewer` agent prompt
- [ ] Merge scoring algorithms from SEO + Brand into single reference
- [ ] Output schema includes both `seoReview` and `brandReview` sections
- [ ] Pipeline updated to run 3 stages instead of 4
- [ ] Quality threshold logic unchanged (score >= 70 to publish)
- [ ] Test on 5 neighborhoods, compare output quality to separate reviewers
- [ ] Measure token savings and time improvement

**Estimated Savings:**
- Input tokens: ~31K → ~18K (-40%)
- Time per neighborhood: ~2 min saved (one fewer agent round-trip)
- For 2,800 neighborhoods: ~$150-200 saved

**Technical Notes:**
- Keep separate review sections in output for debugging/transparency
- May need to increase output token limit for combined response
- Consider if any review logic is truly sequential (SEO before Brand)

---

## Dependencies

```
Epic J (Agent Pipeline)
  └── L1 (Dog Park Features)
  └── L2 (Name Casing)
  └── L3 (Ralph Loop)
  └── L4 (Statbel NIS Mapping)
  └── L5 (Prompt Token Reduction)
  └── L6 (Materialized Views)
  └── L7 (Qualitative Language)
  └── L8 (Merge Reviewers)
```

All stories in this epic depend on having a working pipeline from Epic J.

---

## Adding New Stories

When pipeline output issues are identified:
1. Document the current behavior
2. Define the desired behavior
3. Add acceptance criteria
4. Note which agent(s) need modification
