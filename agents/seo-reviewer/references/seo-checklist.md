# SEO Checklist for Neighborhood Pages (v2.1)

This checklist guides the SEO Reviewer's analysis. Updated to focus on "living decision" content with deterministic, machine-checkable rules.

**Philosophy:** Neighborhood pages should rank for "wonen in [wijk]" queries while serving dog owners through implicit signals and practical guidance.

---

## Narrative Fields (Where Checks Apply)

All keyword counts and bucket checks apply ONLY to these narrative fields:

```
subtitle, intro, facilities.intro, dogParks.intro, vets.intro,
petStores.intro, statistics.intro, houses.intro,
dailyLife.title, dailyLife.intro, dailyLife.benefits[]
```

**Excluded from checks:** valueCards, POI names, labels, coordinates, icons.

---

## Critical Checks (Must Pass)

### Subtitle / Meta Description

- [ ] **Length:** 80-120 characters
- [ ] **Contains neighborhood name:** The specific neighborhood being described
- [ ] **Contains city name:** Geographic context (e.g., "Gent")
- [ ] **Contains living signal OR dog signal:** At least one of:
  - Living signals: `groen`, `wandelafstand`, `bereikbaar`, `rustig`, `parken`, `station`, `compact`
  - Dog signals: `hond`, `baasjes`, `viervoeter`
- [ ] **Has compelling hook:** Unique angle, not generic statement
- [ ] **No marketing clichés:** `ideaal`, `perfect`, `bruisend`, `uniek`, `dé plek`, `must`

**Note:** "met ruimte voor je hond" is NOT a cliché — it's a factual hook.

### Main Intro (First Paragraph)

- [ ] **Neighborhood name in first sentence:** Establishes topic immediately
- [ ] **City name in first 100 words:** Geographic SEO signal
- [ ] **Living context established (2+ buckets):**

| Bucket | Terms (any match counts) |
|--------|--------------------------|
| Walkability | `wandelafstand`, `te voet`, `op X minuten`, `bereikbaar`, `loopafstand` |
| Green/Outdoor | `groen`, `park`, `parken`, `buiten`, `natuur`, `open ruimte` |
| Mobility | `bereikbaarheid`, `openbaar vervoer`, `station`, `tram`, `bus`, `fiets` |
| Calm/Safety | `rustig`, `autoluw`, `verkeer`, `voetpad`, `drukte`, `stil` |

- [ ] **Specific data point:** Distance, count, or statistic early on
- [ ] **No generic opening:** Avoid "Deze wijk is een leuke plek..."

**Note:** Dog keyword in first paragraph is optional. "Wandeling" or "dagelijkse routine" counts as implicit dog signal.

### Main Intro (Overall)

- [ ] **Word count 400-800:** Sufficient content depth
- [ ] **Neighborhood name 2-4 times:** Natural distribution
- [ ] **Dog keywords LIMITED (in narrative fields only):** Max 4 total occurrences of `hond/honden/baasjes/viervoeter/viervoeters`
- [ ] **Trade-off mentioned:** At least 1 honest limitation (see detection rules below)
- [ ] **Mitigation provided:** Practical advice for trade-off (see detection rules below)
- [ ] **Natural reading flow:** Content reads well, not keyword-stuffed

### Topic Coverage (4 Buckets)

All four topic buckets should be represented in narrative fields:

#### Bucket 1: Wonen & Leefkwaliteit (2+ terms required)
```
wonen, leefkwaliteit, dagelijks leven, dagelijkse routine, woonwijk,
sfeer, praktisch, bereikbaarheid, levendig, residentieel
```

#### Bucket 2: Groen & Buiten (2+ terms required)
```
groen, groene, park, parken, wandelen, wandeling, wandelafstand,
buiten, natuur, open ruimte, speelweide, bos, water
```

#### Bucket 3: Rust & Veiligheid (1+ terms required)
```
rustig, rust, autoluw, verkeer, voetpad, voetpaden, veilig,
drukte, geluidsoverlast, stil, kalm
```

#### Bucket 4: Dog Lens (deterministic rule)

**Passes if EITHER:**

**Option A:** Explicit dog count is 1-3 (inclusive)
- Count: `hond`, `honden`, `baasjes`, `viervoeter`, `viervoeters`
- Must be ≥ 1 AND ≤ 3

**Option B:** Implicit signals ≥ 3 AND includes walking term
- Count: `wandelen`, `wandeling`, `wandelafstand`, `te voet`, `dagelijkse routine`, `ochtendwandeling`, `avondwandeling`, `uitlaten`
- Must have ≥ 3 occurrences
- AND must include at least one of: `wandelen`, `wandeling`, `wandelafstand`, `te voet`

**Why Option B needs a walking term:** Prevents passing purely via "groen/parken" without walking/routine cue.

### Section Intros

Each section intro must answer **"what exists + why it matters"**:

- [ ] **facilities.intro:** What dog-relevant facilities exist + daily life impact
- [ ] **dogParks.intro:** What dog park options exist + what they're like
- [ ] **vets.intro:** What veterinary options exist + accessibility
- [ ] **petStores.intro:** What pet store options exist (practical alternatives allowed)
- [ ] **dailyLife.intro:** Picture of daily routine

#### Length Guidelines (soft targets)
| Section | Suggested | Warning if |
|---------|-----------|------------|
| facilities.intro | 40-80 words | < 20 words |
| dogParks.intro | 40-80 words | < 20 words |
| vets.intro | 30-60 words | < 20 words |
| petStores.intro | 30-60 words | < 20 words |
| dailyLife.intro | 50-100 words | < 30 words |

**Key:** A 30-word intro that answers "what + why" passes. Only flag if < 20 words AND missing usefulness.

### Section Focus (Relaxed Drift Rules)

- [ ] **Primary topic:** Each section intro is primarily about its topic
- [ ] **Practical alternatives allowed:** One sentence mentioning alternatives is OK
- [ ] **No major drift:** Section shouldn't become primarily about another topic

---

## Decision Usefulness Checks (Must Pass)

### Trade-off Detection (checkable heuristics)

**Trade-off present if pattern appears:** Transition word + friction keyword

**Transition words:**
```
maar, wel, toch, keerzijde, nadeel, let op, houd er rekening mee
```

**Friction keywords:**
```
verkeer, drukte, lawaai, smalle, beperkt, ontbreekt, weinig,
geen, verder, buiten de wijk
```

**Examples that pass:**
- "Geen dierenwinkel in de wijk" ✓
- "...maar het verkeer kan druk zijn" ✓
- "Let op: beperkte parkeergelegenheid" ✓
- "Wel wat verder van het centrum" ✓

### Mitigation Detection (checkable heuristics)

**Mitigation present if ANY pattern appears:**
- `maar ... op X minuten`
- `alternatief`
- `kies (de route|een tijdstip)`
- `neem (water|zakjes) mee`
- `voor ... ga je naar`
- `dichtstbijzijnde ... vind je in`

**Examples that pass:**
- "...maar op 8 minuten vind je Tom & Co" ✓
- "Kies de route via de Coupure voor rustiger wandelen" ✓
- "Als alternatief bieden supermarkten basisproducten" ✓

### "Who is this for" Detection (checkable heuristics)

**Present if sentence contains ANY of:**
- `praktisch voor wie`
- `geschikt voor`
- `minder geschikt als`
- `fijn voor wie`
- `niet ideaal als`

**Placement rule:** Should appear in:
- Final 25% of main intro, OR
- dailyLife.intro (preferably near end)

**Examples that pass:**
- "Praktisch voor wie stedelijk wil wonen maar toch buitenruimte zoekt"
- "Minder geschikt als je dagelijks grote losloopgebieden nodig hebt"
- "Fijn voor wie een rustige routine verkiest"

---

## Warning Checks (Should Pass)

### Value Cards

- [ ] **Specific descriptions:** Not generic ("Goed bereikbaar" needs context)
- [ ] **Living-relevant terms:** Include words that serve decision-making
- [ ] **Consistent with data:** Matches POI information

### Labels

- [ ] **Clear and descriptive:** "Stedelijk", "Groen", not vague terms
- [ ] **Reflect actual character:** Based on data, not aspirational
- [ ] **Living-focused:** Terms people use when searching for where to live

### Daily Life Benefits

- [ ] **Specific, not vague:** Include distances, counts, specifics
- [ ] **Mix of practical and emotional:** Both functional and feeling-based
- [ ] **3-7 items:** Appropriate range

### Local SEO Signals

- [ ] **Neighborhood name in dailyLife.title:** "Dagelijks leven in [Neighborhood]"
- [ ] **Specific POI names in narrative:** 2+ mentions (vets, parks, stores)
- [ ] **Local landmarks:** References to known local features
- [ ] **City patterns:** stadsrand, binnenstad, etc. where relevant

---

## Info Checks (Nice to Have)

### Internal Linking

- [ ] **neighboringNeighborhoods populated:** Has related neighborhoods
- [ ] **All links valid:** Each ID exists in database
- [ ] **Logical connections:** Links make geographic sense

### Content Enhancement

- [ ] **Walking route described:** Specific paths or areas mentioned
- [ ] **Seasonal considerations:** If relevant to the neighborhood
- [ ] **Unique neighborhood characteristics:** What makes it different

### Housing Section

- [ ] **Postal code mentioned:** In houses.intro (nice to have, not required)
- [ ] **Housing context:** Brief mention of housing market/character

---

## Anti-Patterns to Flag

### Dog Keyword Stuffing (narrative fields only)
```
❌ "Dampoort is een hondvriendelijke wijk voor baasjes met honden.
    Baasjes kunnen met hun hond wandelen in deze hondvriendelijke wijk.
    Voor baasjes is dit een fijne wijk om met hun hond te wonen."

Count in narrative: hond x3, baasjes x3 = 6 explicit dog terms
→ Exceeds max of 4, triggers -3 deduction
```

### Missing Living Context
```
❌ Opens with: "Voor baasjes biedt deze wijk veel voorzieningen..."
   (Dog-first, no living context buckets)

✓ Opens with: "Dampoort is een compacte stadswijk aan de rand van
   het Gentse centrum. Met groen op wandelafstand en het station
   om de hoek combineer je stedelijk gemak met ruimte voor je
   dagelijkse routine."
   (2+ buckets: Green/Outdoor + Mobility)
```

### Generic Statements
```
❌ "Een leuke wijk om te wonen"
❌ "Ideaal voor iedereen"
❌ "De perfecte buurt"
✓ "Compact maar met voldoende groen voor dagelijkse wandelingen"
```

### Missing Specifics
```
❌ "Er zijn verschillende voorzieningen in de buurt"
✓ "Met 6 parken binnen 15 minuten wandelen en een dierenarts op 7 minuten"
```

### No Trade-offs (Marketing Speak)
```
❌ Only positive statements, no honest limitations
   → No transition word + friction keyword detected
   → Fails Decision Usefulness check

✓ "Geen dierenwinkel in de wijk zelf, maar op 8 minuten vind je
   [naam] in de naburige wijk."
   → "Geen" (friction) detected
   → "maar op 8 minuten" (mitigation) detected
```

### Cross-Topic Drift
```
❌ Major drift: dogParks.intro spends 3+ sentences on general parks
✓ Minor practical mention: petStores.intro notes "supermarkten
   als alternatief voor basisproducten" (one sentence = allowed)
```

---

## Quick Reference: v2.1 Changes

| v2 Rule | v2.1 Tightening |
|---------|-----------------|
| Dog Lens: "1-3 explicit OR 3+ implicit" | Added: Option B requires walking term |
| Keyword caps apply to all content | Now: Narrative fields only (excludes POI names, value cards) |
| Trade-off "should be present" | Now: Checkable heuristics (transition + friction word) |
| Mitigation "should be present" | Now: Checkable patterns ("maar...op X minuten", "alternatief", etc.) |
| "Who is this for" anywhere | Now: Final 25% of intro OR dailyLife.intro |
| Section length ≥ 40 words | Now: Soft target; pass if answers "what + why" even at 30 words |

---

## Writer Micro-Brief (6 lines)

For quick reference, writers should:

1. Open with living context (stad/compact/groen/bereikbaar)
2. Mention 1 honest trade-off early (traffic/noise/limited choice)
3. Give 1 mitigation ("kies route", "alternatief op X min")
4. Use "hond" max 1-3 times; prefer "wandeling" + "groen" signals
5. Name 2-4 concrete local places
6. End intro or dailyLife.intro with "praktisch voor wie / minder geschikt als"

---

## Quick Reference: Character/Word Limits

| Field | Target | Warning |
|-------|--------|---------|
| subtitle | 80-120 chars | < 60 or > 140 |
| intro | 400-800 words | < 350 or > 850 |
| facilities.intro | 40-80 words | < 20 |
| dogParks.intro | 40-80 words | < 20 |
| vets.intro | 30-60 words | < 20 |
| petStores.intro | 30-60 words | < 20 |
| statistics.intro | 30-60 words | < 20 |
| houses.intro | 40-80 words | < 20 |
| dailyLife.intro | 50-100 words | < 30 |
| dailyLife.benefits | 30-80 words each | |
| valueCard.title | max 25 chars | |
| valueCard.description | max 60 chars | |
| labels.text | max 25 chars | |

---

## Example: Good v2.1 Content

### Subtitle
> "Wonen in Dampoort: compact, goed bereikbaar en groen op wandelafstand — met ruimte voor je hond in Gent"

✓ Living signals: `compact`, `bereikbaar`, `groen`, `wandelafstand`
✓ Dog mention: natural, not dominating
✓ Specific traits, not generic
✓ No clichés

### Intro Opening
> "Dampoort is een compacte stadswijk aan de rand van het Gentse centrum. Met het station om de hoek en groen op wandelafstand combineer je hier stedelijk gemak met voldoende buitenruimte voor je dagelijkse routine. Voor een wandeling met je hond bereik je binnen 12 minuten de omheinde hondenweide aan de Schelde."

✓ Neighborhood in first sentence: "Dampoort"
✓ City in first 100 words: "Gentse centrum"
✓ Living buckets (2+): Mobility (`station`), Green (`groen`), Walkability (`wandelafstand`)
✓ Dog lens: 1 explicit (`hond`), implicit signals (`wandeling`, `dagelijkse routine`)
✓ Specific data: "12 minuten"

### Trade-off + Mitigation
> "De wijk heeft geen eigen dierenwinkel, maar bij Pets Place in Sint-Amandsberg — op 8 minuten met de bus — vind je alles wat je nodig hebt. Voor basisbenodigdheden bieden de lokale supermarkten een praktisch alternatief."

✓ Trade-off detected: "geen" (friction keyword)
✓ Mitigation detected: "maar...op 8 minuten" + "alternatief"
✓ POI name: "Pets Place"

### Who is this for
> "Praktisch voor wie stedelijk wil wonen maar toch ruimte zoekt voor dagelijkse wandelingen. Minder geschikt als je grote losloopgebieden nodig hebt."

✓ Pattern detected: "Praktisch voor wie", "Minder geschikt als"
✓ Placement: End of intro (final 25%)
