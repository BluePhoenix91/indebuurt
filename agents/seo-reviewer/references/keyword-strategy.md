# Keyword Strategy for buurtkompas.be (v2.1)

This document defines the target keywords for SEO optimization. The SEO Reviewer uses this as a reference for topic coverage checks.

**Philosophy:** Buurtkompas pages are *living decision* pages that use dog ownership as a lens — not dog directory pages. Keywords should reflect how people actually search when deciding where to live.

**Note:** This file is updated periodically via separate keyword research (not part of per-neighborhood SEO review).

---

## Core Principle: Implicit > Explicit

People searching for where to live with a dog typically search:
- "rustige wijk Gent" (not "hondvriendelijke wijk Gent")
- "groene buurt" (not "buurt met hondenspeelweide")
- "wonen in Dampoort" (not "hond in Dampoort")

**Dog ownership is often implied**, not stated. Our pages should rank for living-decision queries while naturally serving dog owners.

---

## Narrative Fields (Where Checks Apply)

All keyword counts apply ONLY to narrative text fields:

```
subtitle, intro, facilities.intro, dogParks.intro, vets.intro,
petStores.intro, statistics.intro, houses.intro,
dailyLife.title, dailyLife.intro, dailyLife.benefits[]
```

**Excluded:** valueCards, POI names (parks[].name, practices[].name, stores[].name), labels, coordinates, icons.

---

## Topic Buckets — Explicit Term Lists

Instead of vague "presence checks," use these explicit term lists for deterministic bucket detection.

### Bucket 1: Wonen & Leefkwaliteit

**Terms (match any):**
```
wonen, leefkwaliteit, dagelijks leven, dagelijkse routine, woonwijk,
sfeer, praktisch, bereikbaarheid, levendig, residentieel
```

**Pass condition:** 2+ terms present in narrative fields.

### Bucket 2: Groen & Buiten

**Terms (match any):**
```
groen, groene, park, parken, wandelen, wandeling, wandelafstand,
buiten, natuur, open ruimte, speelweide, bos, water
```

**Pass condition:** 2+ terms present in narrative fields.

### Bucket 3: Rust & Veiligheid

**Terms (match any):**
```
rustig, rust, autoluw, verkeer, voetpad, voetpaden, veilig,
drukte, geluidsoverlast, stil, kalm
```

**Pass condition:** 1+ terms present in narrative fields.

### Bucket 4: Dog Lens

**Explicit dog terms:**
```
hond, honden, baasjes, viervoeter, viervoeters
```

**Implicit dog-friendly signals:**
```
wandelen, wandeling, wandelafstand, te voet, dagelijkse routine,
ochtendwandeling, avondwandeling, uitlaten
```

**Pass condition (deterministic):**

| Option | Rule |
|--------|------|
| **A** | `explicit_dog_count` is 1-3 (inclusive) |
| **B** | `implicit_signal_count` ≥ 3 AND includes at least one of: `wandelen`, `wandeling`, `wandelafstand`, `te voet` |

**Why Option B requires a walking term:** Prevents pages from passing dog lens purely via "groen/parken" without any walking/routine cue.

---

## Geographic Keywords (Always Include)

| Keyword | Target Count | Where |
|---------|--------------|-------|
| `[neighborhood name]` | 2-4x | First sentence, headings, natural mentions |
| `[city name]` | 1-2x | Intro, geographic context |
| `wijk` | 1-2x | Generic neighborhood term |

---

## First Paragraph Requirements

The intro's first paragraph should establish **living context**, not dog amenities.

### Must contain:
- Neighborhood name (first sentence)
- City name (first 100 words)

### Must have 2+ of these living context buckets:

| Bucket | Terms |
|--------|-------|
| Walkability | `wandelafstand`, `te voet`, `op X minuten`, `bereikbaar`, `loopafstand` |
| Green/Outdoor | `groen`, `park`, `parken`, `buiten`, `natuur`, `open ruimte` |
| Mobility | `bereikbaarheid`, `openbaar vervoer`, `station`, `tram`, `bus`, `fiets` |
| Calm/Safety | `rustig`, `autoluw`, `verkeer`, `voetpad`, `drukte`, `stil` |

### May contain (not required):
- Dog keyword — if natural, but not forced
- "Wandeling" or "dagelijkse routine" counts as implicit dog signal

---

## Keyword Caps (Narrative Fields Only)

| Keyword Type | Max Occurrences | Deduction if Exceeded |
|--------------|-----------------|----------------------|
| Any single explicit dog term | 4 | -3 pts |
| "baasjes" specifically | 3 | -2 pts |
| Any single keyword | 6 | Warning |

**Important:** These caps apply only to narrative fields. POI names like "Hondenweide Dampoort" don't count.

---

## Long-tail Keyword Patterns

These search patterns should rank organically when content is good:

### Living-focused (primary target)
```
wonen in [neighborhood]
[neighborhood] [city]
rustige wijk [city]
groene buurt [city]
[neighborhood] leefkwaliteit
woonwijk [city] groen
```

### Dog-implied (secondary)
```
wonen in [neighborhood] met hond
wandelen [neighborhood]
[neighborhood] parken
[neighborhood] groen
```

### Explicit dog (tertiary — will rank anyway)
```
hondvriendelijke wijk [city]
hondenspeelweide [neighborhood]
dierenarts [neighborhood]
```

---

## Implicit Dog-Friendly Signals

These keywords signal dog-friendliness without saying "hond":

| Signal | Why It Matters for Dog Owners |
|--------|-------------------------------|
| `wandelafstand` | Walking is the primary dog activity |
| `wandelen` / `wandeling` | Direct walking reference |
| `groen` / `parken` | Where dogs get exercised |
| `rustig` / `autoluw` | Safe for walking, less stress |
| `voetpaden` | Quality of walking infrastructure |
| `open ruimte` | Space for dogs to run |
| `dagelijkse routine` | Frames daily dog care activities |

**Recommendation:** Use these freely. They improve SEO for living queries AND serve dog owners implicitly.

---

## Decision Usefulness Keywords

These patterns signal practical, decision-useful content:

### Trade-off indicators
**Transition words:** `maar`, `wel`, `toch`, `keerzijde`, `nadeel`, `let op`, `houd er rekening mee`
**Friction keywords:** `verkeer`, `drukte`, `lawaai`, `smalle`, `beperkt`, `ontbreekt`, `weinig`, `geen`, `verder`, `buiten de wijk`

### Mitigation patterns
- `maar ... op X minuten`
- `alternatief`
- `kies (de route|een tijdstip)`
- `dichtstbijzijnde ... vind je in`

### Audience signals
- `praktisch voor wie`
- `geschikt voor`
- `minder geschikt als`
- `fijn voor wie`

---

## Natural Integration Examples

**Good (living context first):**
> "Dampoort is een compacte stadswijk aan de rand van het Gentse centrum. Met het station om de hoek en groen op wandelafstand combineer je hier stedelijk gemak met voldoende buitenruimte voor je dagelijkse routine. Voor een wandeling met je hond bereik je binnen 12 minuten een omheinde speelweide."

**Analysis:**
- Living buckets: Mobility (`station`), Green (`groen`), Walkability (`wandelafstand`)
- Dog lens: 1 explicit (`hond`), 2 implicit (`wandeling`, `dagelijkse routine`)
- Passes all checks ✓

**Bad (dog-first, repetitive):**
> "Dampoort is een hondvriendelijke wijk voor baasjes met honden. In deze wijk kunnen baasjes met hun hond wandelen naar de hondenspeelweide. Baasjes vinden hier ook een dierenarts voor hun hond."

**Analysis:**
- No living buckets in first paragraph
- Explicit dog count: 6 (hond x3, baasjes x3) — exceeds cap
- Fails ✗

---

## Dutch Language Notes

### Preferred Terms
- `wijk` over `buurt` (except in compound words)
- `hond` over `honden` (singular is more natural)
- `baasjes` — use 1-2x max (brand term, can feel repetitive)
- `hondenspeelweide` over `hondenpark` (Flemish standard)
- `wandelen` over other activity verbs

### Marketing Clichés (Avoid)
```
ideaal, perfect, bruisend, uniek, dé plek, must, beste
```

**Note:** "met ruimte voor je hond" is NOT a cliché — it's a factual hook.

---

## City-Specific Considerations

### Gent (Current Focus)
- Include "Gent" in intro
- Reference city patterns: "stadsrand", "binnenstad", "universiteitswijk"
- Postal code mention in houses section (nice to have, not required)

### Future Cities
When expanding, update this file with:
- City-specific living vocabulary
- Local neighborhood terminology
- Regional search patterns

---

## Summary: v2.1 Approach

| Aspect | Rule |
|--------|------|
| Primary target | "wonen in [wijk]" queries |
| Dog lens | 1-3 explicit OR 3+ implicit with walking term |
| Keyword caps | Apply to narrative fields only |
| First paragraph | 2+ living context buckets required |
| Trade-offs | Must include transition + friction word |
| Mitigation | Must include practical alternative pattern |
| Audience signal | "praktisch voor wie" / "minder geschikt als" |

The goal is to rank for **living decision queries** while naturally serving dog owners through implicit signals and practical content.
