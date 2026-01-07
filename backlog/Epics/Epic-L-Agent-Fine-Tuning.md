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

## Story L2: Normalize Neighborhood Name Casing

> As a content reader, I want neighborhood names in normal title case, so that the content looks professional rather than SHOUTING.

**Context:** The GIS database stores neighborhood names in ALL CAPS (e.g., "AALST - STATION"). The writer should transform these to title case (e.g., "Aalst - Station") for display.

**Current State:**
- `name` field in brand-reviewer output: `"AALST - STATION"`
- Appears in headings and titles as all-caps

**Acceptance Criteria:**
- [ ] Writer transforms neighborhood name to title case
- [ ] Handles edge cases: "SINT-NIKLAAS" → "Sint-Niklaas", hyphenated words
- [ ] City name also normalized if needed
- [ ] Updated in writer output schema and transformation rules

**Technical Notes:**
- Simple transformation in writer agent
- May need locale-aware handling for Dutch proper nouns
- Consider maintaining a list of exceptions (e.g., abbreviations that stay uppercase)

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

## Dependencies

```
Epic J (Agent Pipeline)
  └── L1 (Dog Park Features)
  └── L2 (Name Casing)
  └── L3 (Ralph Loop)
  └── L4 (Statbel NIS Mapping)
```

All stories in this epic depend on having a working pipeline from Epic J.

---

## Adding New Stories

When pipeline output issues are identified:
1. Document the current behavior
2. Define the desired behavior
3. Add acceptance criteria
4. Note which agent(s) need modification
