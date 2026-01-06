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

## Dependencies

```
Epic J (Agent Pipeline)
  └── L1 (Dog Park Features)
  └── L2 (Name Casing)
```

All stories in this epic depend on having a working pipeline from Epic J.

---

## Adding New Stories

When pipeline output issues are identified:
1. Document the current behavior
2. Define the desired behavior
3. Add acceptance criteria
4. Note which agent(s) need modification
