# Buurtkompas SEO Strategy (v2.1)

> **TL;DR:** Neighborhood pages are "living decision" content that uses dog ownership as a lens — not dog directory pages. Rank for "wonen in [wijk]" queries while naturally serving dog owners through implicit signals and practical guidance.

---

## Philosophy

### Core Principle: Implicit > Explicit

People searching for where to live with a dog typically search:
- "rustige wijk Gent" (not "hondvriendelijke wijk Gent")
- "groene buurt" (not "buurt met hondenspeelweide")
- "wonen in Dampoort" (not "hond in Dampoort")

**Dog ownership is often implied**, not stated. Our pages should rank for living-decision queries while naturally serving dog owners.

### What Changed (v1 → v2.1)

| v1 Approach | v2.1 Approach |
|-------------|---------------|
| Count "hond" occurrences (3-5x) | Dog lens: 1-3 explicit max |
| Keyword density 2-4% | Semantic topic coverage (4 buckets) |
| Force dog keyword in first paragraph | Living context required, dog optional |
| Strict section focus (no alternatives) | Practical alternatives allowed |
| No decision usefulness scoring | Trade-off + mitigation + audience signal required |
| Caps apply to all content | Caps apply to narrative fields only |

---

## Content Pipeline

```
Researcher → Writer → SEO Reviewer → Brand Reviewer → Final JSON
   (data)     (prose)   (optimize)     (tone check)    (validated)
```

The SEO Reviewer optimizes Writer output for search visibility while preserving factual data and brand voice.

---

## Narrative Fields (Where Rules Apply)

All keyword counting, caps, and bucket checks apply ONLY to these fields:

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

**Excluded from checks:**
- valueCards[].title / description / detail
- POI names (parks[].name, practices[].name, stores[].name)
- labels[].text
- neighboringNeighborhoods[]
- All coordinates and icons

**Why:** POI names like "Hondenweide Dampoort" shouldn't count toward keyword caps.

---

## Scoring Algorithm (100 points)

| Category | Max | What It Measures |
|----------|-----|------------------|
| Subtitle | 15 | Living signals, length, no clichés |
| Main Intro | 25 | Living context, trade-offs, natural flow |
| Topic Coverage | 20 | 4 semantic buckets covered |
| Section Intros | 10 | Useful content ("what + why") |
| Decision Usefulness | 15 | Trade-off + mitigation + audience signal |
| Local Relevance | 10 | POI names, landmarks |
| Internal Linking | 5 | Valid neighbor links |

**Pass threshold:** qualityScore ≥ 70

---

## Topic Buckets — Explicit Term Lists

### Bucket 1: Wonen & Leefkwaliteit (5 pts, need 2+)
```
wonen, leefkwaliteit, dagelijks leven, dagelijkse routine, woonwijk,
sfeer, praktisch, bereikbaarheid, levendig, residentieel
```

### Bucket 2: Groen & Buiten (5 pts, need 2+)
```
groen, groene, park, parken, wandelen, wandeling, wandelafstand,
buiten, natuur, open ruimte, speelweide, bos, water
```

### Bucket 3: Rust & Veiligheid (5 pts, need 1+)
```
rustig, rust, autoluw, verkeer, voetpad, voetpaden, veilig,
drukte, geluidsoverlast, stil, kalm
```

### Bucket 4: Dog Lens (5 pts, deterministic rule)

**Explicit dog terms:**
```
hond, honden, baasjes, viervoeter, viervoeters
```

**Implicit dog-friendly signals:**
```
wandelen, wandeling, wandelafstand, te voet, dagelijkse routine,
ochtendwandeling, avondwandeling, uitlaten
```

**Pass condition:**
| Option | Rule |
|--------|------|
| **A** | Explicit dog count is 1-3 (inclusive) |
| **B** | Implicit signals ≥ 3 AND includes walking term (`wandelen`, `wandeling`, `wandelafstand`, `te voet`) |

---

## Decision Usefulness — Checkable Heuristics

### Trade-off Detection (5 pts)

**Pattern:** Transition word + friction keyword

**Transition words:**
```
maar, wel, toch, keerzijde, nadeel, let op, houd er rekening mee
```

**Friction keywords:**
```
verkeer, drukte, lawaai, smalle, beperkt, ontbreekt, weinig,
geen, verder, buiten de wijk
```

**Examples:**
- "Geen dierenwinkel in de wijk" ✓
- "...maar het verkeer kan druk zijn" ✓
- "Let op: beperkte parkeergelegenheid" ✓

### Mitigation Detection (5 pts)

**Patterns:**
- `maar ... op X minuten`
- `alternatief`
- `kies (de route|een tijdstip)`
- `neem (water|zakjes) mee`
- `voor ... ga je naar`
- `dichtstbijzijnde ... vind je in`

**Examples:**
- "...maar op 8 minuten vind je Tom & Co" ✓
- "Kies de route via de Coupure" ✓
- "Als alternatief bieden supermarkten basisproducten" ✓

### "Who is this for" Detection (5 pts)

**Patterns:**
- `praktisch voor wie`
- `geschikt voor`
- `minder geschikt als`
- `fijn voor wie`
- `niet ideaal als`

**Placement:** Final 25% of intro OR dailyLife.intro

---

## Keyword Caps (Narrative Fields Only)

| Keyword Type | Max | Deduction |
|--------------|-----|-----------|
| Any explicit dog term (hond/baasjes/viervoeter) | 4 | -3 pts |
| "baasjes" specifically | 3 | -2 pts |
| Any single keyword | 6 | Warning |

---

## First Paragraph Requirements

### Must contain:
1. Neighborhood name (first sentence)
2. City name (first 100 words)
3. 2+ living context buckets:

| Bucket | Terms |
|--------|-------|
| Walkability | `wandelafstand`, `te voet`, `op X minuten`, `bereikbaar` |
| Green/Outdoor | `groen`, `park`, `parken`, `buiten`, `natuur` |
| Mobility | `bereikbaarheid`, `openbaar vervoer`, `station`, `tram`, `bus` |
| Calm/Safety | `rustig`, `autoluw`, `verkeer`, `voetpad`, `drukte` |

### May contain (not required):
- Dog keyword — if natural
- "Wandeling" or "dagelijkse routine" = implicit dog signal

---

## Marketing Clichés (Avoid)

```
ideaal, perfect, bruisend, uniek, dé plek, must, beste
```

**Note:** "met ruimte voor je hond" is NOT a cliché — it's a factual hook.

---

## Section Intros — Soft Length Targets

| Section | Suggested | Warning if |
|---------|-----------|------------|
| facilities.intro | 40-80 words | < 20 words |
| dogParks.intro | 40-80 words | < 20 words |
| vets.intro | 30-60 words | < 20 words |
| petStores.intro | 30-60 words | < 20 words |
| dailyLife.intro | 50-100 words | < 30 words |

**Key:** A 30-word intro that answers "what exists + why it matters" passes. Only penalize if < 20 words AND missing usefulness.

### Relaxed Drift Rules
- **Allowed:** One sentence of practical alternative (e.g., "supermarkten als alternatief" in petStores.intro)
- **Not allowed:** Section becomes primarily about another topic

---

## Writer Micro-Brief (6 lines)

1. Open with living context (stad/compact/groen/bereikbaar)
2. Mention 1 honest trade-off early (traffic/noise/limited choice)
3. Give 1 mitigation ("kies route", "alternatief op X min")
4. Use "hond" max 1-3 times; prefer "wandeling" + "groen" signals
5. Name 2-4 concrete local places
6. End intro or dailyLife.intro with "praktisch voor wie / minder geschikt als"

---

## Good Example

### Subtitle
> "Wonen in Dampoort: compact, goed bereikbaar en groen op wandelafstand — met ruimte voor je hond in Gent"

✓ Living signals: `compact`, `bereikbaar`, `groen`, `wandelafstand`
✓ Dog mention: natural, not dominating
✓ No clichés

### Intro Opening
> "Dampoort is een compacte stadswijk aan de rand van het Gentse centrum. Met het station om de hoek en groen op wandelafstand combineer je hier stedelijk gemak met voldoende buitenruimte voor je dagelijkse routine. Voor een wandeling met je hond bereik je binnen 12 minuten de omheinde hondenweide aan de Schelde."

✓ Neighborhood in first sentence
✓ City in first 100 words
✓ Living buckets (2+): Mobility, Green, Walkability
✓ Dog lens: 1 explicit, 2 implicit
✓ Specific data: "12 minuten"

### Trade-off + Mitigation
> "De wijk heeft geen eigen dierenwinkel, maar bij Pets Place in Sint-Amandsberg — op 8 minuten met de bus — vind je alles wat je nodig hebt. Voor basisbenodigdheden bieden de lokale supermarkten een praktisch alternatief."

✓ Trade-off: "geen" (friction)
✓ Mitigation: "maar...op 8 minuten" + "alternatief"

### Who is this for
> "Praktisch voor wie stedelijk wil wonen maar toch ruimte zoekt voor dagelijkse wandelingen. Minder geschikt als je grote losloopgebieden nodig hebt."

✓ Patterns: "Praktisch voor wie", "Minder geschikt als"
✓ Placement: End of intro

---

## Bad Example

> "Dampoort is een hondvriendelijke wijk voor baasjes met honden. In deze wijk kunnen baasjes met hun hond wandelen naar de hondenspeelweide. Baasjes vinden hier ook een dierenarts voor hun hond."

✗ No living buckets in first paragraph
✗ Explicit dog count: 6 (hond x3, baasjes x3) — exceeds cap of 4
✗ Dog-first opening
✗ No trade-off or mitigation

---

## Acceptance Criteria

A page should be able to score 80+ **without**:
- Mentioning "baasjes" more than once
- Starting with a dog amenity list
- Meeting any keyword density target

A page should **NOT pass** if:
- It lacks living decision vocabulary (wonen/leefkwaliteit bucket empty)
- It reads like marketing copy (no trade-offs)
- It repeats dog terms as a crutch (> 4 occurrences)

---

## Reference Files

| File | Purpose |
|------|---------|
| `seo-reviewer/references/keyword-strategy.md` | Term lists, bucket definitions |
| `seo-reviewer/references/scoring-algorithm.md` | Point breakdown, detection heuristics |
| `seo-reviewer/references/seo-checklist.md` | Checklist for writers/reviewers |
| `seo-reviewer/prompt-v1.md` | SEO Reviewer agent prompt |
| `writer/prompt-v1.md` | Writer agent prompt |
| `writer/references/content-guidelines.md` | Brand voice, terminology |

---

## Implementation Priority

If time is limited, implement in this order:

1. **Topic Coverage bucket detection** (presence-based, use term lists)
2. **Decision Usefulness checks** (trade-off + mitigation + who-for heuristics)
3. **Narrative-only keyword counting/caps**

Everything else can be iterated later.

---

## Long-tail Keywords (for reference)

### Living-focused (primary target)
```
wonen in [neighborhood]
[neighborhood] [city]
rustige wijk [city]
groene buurt [city]
```

### Dog-implied (secondary)
```
wonen in [neighborhood] met hond
wandelen [neighborhood]
[neighborhood] parken
```

### Explicit dog (tertiary)
```
hondvriendelijke wijk [city]
hondenspeelweide [neighborhood]
dierenarts [neighborhood]
```

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| v1 | Dec 2025 | Initial keyword density approach |
| v2 | Jan 2026 | Living-first philosophy, topic buckets, decision usefulness |
| v2.1 | Jan 2026 | Deterministic rules, narrative-only caps, checkable heuristics |
