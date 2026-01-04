# SEO Quality Scoring Algorithm (v2.1)

This document defines how the SEO Reviewer calculates the 0-100 quality score. The score determines `passedSEO` status (threshold: 70).

**Philosophy:** Score pages as "living decision" content, not "dog directory" content. Reward semantic topic coverage and decision usefulness over keyword density.

---

## Narrative Fields Definition

**IMPORTANT:** All keyword counting, caps, and bucket checks apply ONLY to narrative text fields.

### Narrative Fields (included in scoring)
```
subtitle
intro
facilities.intro
dogParks.intro
vets.intro
petStores.intro
statistics.intro
houses.intro
dailyLife.title
dailyLife.intro
dailyLife.benefits[]
```

### Non-Narrative Fields (excluded from scoring)
```
valueCards[].title / description / detail
dogParks.parks[].name
vets.practices[].name / street / municipality
petStores.stores[].name
labels[].text
neighboringNeighborhoods[]
All coordinate fields
All icon fields
```

**Rationale:** The reviewer judges writing quality and semantic coverage, not structured UI elements or POI data.

---

## Score Categories (v2.1)

| Category | Max Points | Weight |
|----------|------------|--------|
| Subtitle | 15 | 15% |
| Main Intro | 25 | 25% |
| Topic Coverage | 20 | 20% |
| Section Intros | 10 | 10% |
| Decision Usefulness | 15 | 15% |
| Local Relevance | 10 | 10% |
| Internal Linking | 5 | 5% |
| **Total** | **100** | **100%** |

---

## 1. Subtitle Score (15 points)

The subtitle serves as the meta description in search results.

| Criterion | Points |
|-----------|--------|
| Length 80-120 characters | 3 |
| Contains neighborhood name | 4 |
| Contains city name | 2 |
| Contains dog keyword OR implicit signal keyword | 3 |
| Has compelling hook (not generic) | 3 |

### Implicit Signal Keywords (count for 3 pts)
Any of: `groen`, `parken`, `wandelafstand`, `autoluw`, `rustig`, `bereikbaar`, `wandelen`, `station`, `compact`

### Deductions
| Issue | Deduction |
|-------|-----------|
| Length < 60 characters | -3 |
| Length > 140 characters | -2 |
| Marketing clichés (see list below) | -2 per cliché |
| Generic statement (could apply to any neighborhood) | -2 |

### Marketing Clichés (trigger deduction)
`ideaal`, `perfect`, `bruisend`, `uniek`, `dé plek`, `must`

**Note:** "met ruimte voor je hond" is NOT a cliché — it's a factual hook.

### Examples

**Good (15/15):**
> "Wonen in Dampoort: compact, goed bereikbaar en groen op wandelafstand — met ruimte voor je hond in Gent"

**Good (14/15) — no explicit dog keyword:**
> "Wonen in Dampoort: compacte stadswijk met station om de hoek en parken op wandelafstand — Gent"

**Poor (6/15):**
> "Een ideale wijk voor hondenbezitters"
- Length: 35 chars ✗, No neighborhood ✗, Cliché "ideale" ✗

---

## 2. Main Intro Score (25 points)

The intro is the primary SEO content.

| Criterion | Points |
|-----------|--------|
| Neighborhood name in first sentence | 4 |
| City name in first 100 words | 2 |
| Living context in first paragraph (2+ buckets) | 5 |
| Trade-off/honest limitation mentioned | 5 |
| Specific data point in first 150 words | 3 |
| Natural flow (readable, not stuffed) | 3 |
| Dog lens present (explicit or via walking terms) | 3 |

### Living Context Buckets (first paragraph must have 2+)

| Bucket | Terms (any match counts) |
|--------|--------------------------|
| Walkability | `wandelafstand`, `te voet`, `op X minuten`, `bereikbaar`, `loopafstand` |
| Green/Outdoor | `groen`, `park`, `parken`, `buiten`, `natuur`, `open ruimte` |
| Mobility | `bereikbaarheid`, `openbaar vervoer`, `station`, `tram`, `bus`, `fiets` |
| Calm/Safety | `rustig`, `autoluw`, `verkeer`, `voetpad`, `drukte`, `stil` |

### Deductions
| Issue | Deduction |
|-------|-----------|
| Word count < 350 | -3 |
| Word count > 850 | -2 |
| Dog keyword stuffing (> 4 in narrative fields) | -5 |
| No specific POI/distance mentioned | -3 |
| Generic opening ("Deze wijk is een leuke plek...") | -2 |

---

## 3. Topic Coverage Score (20 points)

Score based on presence of **concept buckets** in narrative fields, not word counts.

### Explicit Term Lists

**Wonen & Leefkwaliteit terms:**
```
wonen, leefkwaliteit, dagelijks leven, dagelijkse routine, woonwijk,
sfeer, praktisch, bereikbaarheid, levendig, residentieel
```

**Groen & Buiten terms:**
```
groen, groene, park, parken, wandelen, wandeling, wandelafstand,
buiten, natuur, open ruimte, speelweide, bos, water
```

**Rust & Veiligheid terms:**
```
rustig, rust, autoluw, verkeer, voetpad, voetpaden, veilig,
drukte, geluidsoverlast, stil, kalm
```

**Explicit Dog terms:**
```
hond, honden, baasjes, viervoeter, viervoeters
```

**Implicit Dog-Friendly signals:**
```
wandelen, wandeling, wandelafstand, te voet, dagelijkse routine,
ochtendwandeling, avondwandeling, uitlaten
```

### Bucket Scoring

| Topic Bucket | Points | Pass Condition |
|--------------|--------|----------------|
| Wonen & Leefkwaliteit | 5 | 2+ terms from list present |
| Groen & Buiten | 5 | 2+ terms from list present |
| Rust & Veiligheid | 5 | 1+ terms from list present |
| Dog Lens | 5 | See deterministic rule below |

### Dog Lens Bucket — Deterministic Rule

**Passes if EITHER:**

**Option A:** `explicit_dog_count` is between 1 and 3 (inclusive)
- Count occurrences of: hond, honden, baasjes, viervoeter, viervoeters
- Must be ≥ 1 AND ≤ 3

**Option B:** `implicit_signal_count` ≥ 3 AND at least one walking term present
- Count occurrences of implicit signals (wandelen, wandeling, wandelafstand, te voet, dagelijkse routine, ochtendwandeling, avondwandeling, uitlaten)
- Must have ≥ 3 occurrences
- AND must include at least one of: `wandelen`, `wandeling`, `wandelafstand`, `te voet`

**Why Option B requires a walking term:** Prevents pages from passing dog lens purely via "groen/parken" without any walking/routine cue.

### Deductions (narrative fields only)
| Issue | Deduction |
|-------|-----------|
| Explicit dog terms > 4 total | -3 |
| "baasjes" > 3 occurrences | -2 |
| Wonen/Leefkwaliteit bucket empty | -5 |
| Groen/Buiten bucket empty | -3 |

---

## 4. Section Intros Score (10 points)

Each section intro should be useful, not just long.

| Criterion | Points |
|-----------|--------|
| Each intro answers "what exists + why it matters" | 5 |
| Section-relevant keywords present | 3 |
| No major cross-topic drift | 2 |

### "What + Why" Usefulness Test
An intro passes if it answers BOTH:
1. **What exists:** counts, distances, availability
2. **Why it matters:** impact on daily life

A 30-35 word intro that answers both questions should pass. Only penalize if extremely short (<20 words) AND missing usefulness.

### Length Guidelines (soft targets, not hard rules)
| Section | Suggested | Warning if |
|---------|-----------|------------|
| facilities.intro | 40-80 words | < 20 words |
| dogParks.intro | 40-80 words | < 20 words |
| vets.intro | 30-60 words | < 20 words |
| petStores.intro | 30-60 words | < 20 words |
| dailyLife.intro | 50-100 words | < 30 words |

### Drift Rules
- **Major drift (penalize):** Section becomes primarily about another topic
- **Allowed:** One sentence of practical alternative

---

## 5. Decision Usefulness Score (15 points)

This is Buurtkompas's moat: help people decide where to live.

| Criterion | Points |
|-----------|--------|
| Trade-off present | 5 |
| Mitigation present | 5 |
| "Who is this for" signal present | 5 |

### Trade-off Detection (checkable heuristics)

**Trade-off present if ANY of these patterns appear:**

Transition word + friction keyword:
- Transition words: `maar`, `wel`, `toch`, `keerzijde`, `nadeel`, `let op`, `houd er rekening mee`
- Friction keywords: `verkeer`, `drukte`, `lawaai`, `smalle`, `beperkt`, `ontbreekt`, `weinig`, `geen`, `verder`, `buiten de wijk`

Examples that trigger detection:
- "Geen dierenwinkel in de wijk" ✓
- "...maar het verkeer kan druk zijn" ✓
- "Let op: beperkte parkeergelegenheid" ✓

### Mitigation Detection (checkable heuristics)

**Mitigation present if ANY of these patterns appear:**
- `maar ... op X minuten`
- `alternatief`
- `kies (de route|een tijdstip)`
- `neem (water|zakjes) mee`
- `voor ... ga je naar`
- `dichtstbijzijnde ... vind je in`

Examples:
- "...maar op 8 minuten vind je Tom & Co" ✓
- "Kies de route via de Coupure voor rustiger wandelen" ✓
- "Als alternatief bieden supermarkten basisproducten" ✓

### "Who is this for" Detection (checkable heuristics)

**Present if sentence contains ANY of:**
- `praktisch voor wie`
- `geschikt voor`
- `minder geschikt als`
- `fijn voor wie`
- `niet ideaal als` (note: "ideaal" allowed here as contrast, not marketing)

### Placement Rule
The "who is this for" signal should appear in:
- Final 25% of main intro, OR
- dailyLife.intro (preferably near end)

---

## 6. Local Relevance Score (10 points)

| Criterion | Points |
|-----------|--------|
| 2+ specific POI names mentioned in narrative | 4 |
| 1+ local landmark or city pattern referenced | 3 |
| Neighborhood name in dailyLife.title | 3 |

### Examples of Local Signals
- POI names: "Dierenarts Maenhout", "Hondenweide Dampoort", "Tom & Co"
- Landmarks: "Blaarmeersen", "Citadelpark", "bij het station"
- City patterns: "stadsrand", "binnenstad", "universiteitswijk"

Postal code in houses.intro = nice to have, not scored.

---

## 7. Internal Linking Score (5 points)

| Criterion | Points |
|-----------|--------|
| `neighboringNeighborhoods` is populated | 3 |
| All linked neighborhoods exist in database | 2 |

### Deductions
| Issue | Deduction |
|-------|-----------|
| Each invalid neighborhood ID | -1 (logged as warning) |

---

## Score Calculation Example

**Input:** Dampoort writer output (v2.1 optimized)

| Category | Score | Notes |
|----------|-------|-------|
| Subtitle | 14/15 | Living context + dog signal, good length |
| Intro | 23/25 | Strong living context, trade-off + mitigation present |
| Topic Coverage | 18/20 | All buckets covered, dog lens via Option A (2 explicit) |
| Section Intros | 9/10 | Useful intros, answers what+why |
| Decision Usefulness | 15/15 | Trade-off ✓, mitigation ✓, who-for ✓ |
| Local Relevance | 9/10 | POI names, local landmarks |
| Internal Links | 4/5 | 4 valid links |
| **Total** | **92/100** | **passedSEO: true** |

---

## Pass Threshold

```
passedSEO = qualityScore >= 70
```

| Score Range | Quality | Typical Action |
|-------------|---------|----------------|
| 90-100 | Excellent | No changes needed |
| 80-89 | Good | Minor optimizations |
| 70-79 | Acceptable | Passes, but with improvements |
| 60-69 | Needs Work | Does not pass, review recommended |
| < 60 | Poor | Significant issues |

---

## Acceptance Criteria (v2.1 validation)

A page should be able to score 80+ **without**:
- Mentioning "baasjes" more than once
- Starting with a dog amenity list
- Meeting any keyword density target

A page should **NOT pass** if:
- It lacks living decision vocabulary (wonen/leefkwaliteit bucket empty)
- It reads like marketing copy (no trade-offs)
- It repeats dog terms as a crutch (> 4 occurrences)

---

## Output Format

```json
{
  "scoreBreakdown": {
    "subtitleScore": 14,
    "introScore": 23,
    "topicCoverageScore": 18,
    "sectionIntrosScore": 9,
    "decisionUsefulnessScore": 15,
    "localRelevanceScore": 9,
    "internalLinkingScore": 4
  }
}
```

---

## Implementation Priority

If time is limited, implement in this order:
1. **Topic Coverage bucket detection** (presence-based, use term lists)
2. **Decision Usefulness checks** (trade-off + mitigation + who-for heuristics)
3. **Narrative-only keyword counting/caps**

Everything else can be iterated later.
