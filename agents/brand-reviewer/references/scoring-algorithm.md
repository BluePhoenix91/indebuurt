# Brand Quality Scoring Algorithm (v1.0)

This document defines how the Brand Reviewer calculates the 0-100 quality score. The score determines `passedBrand` status (threshold: 70).

**Philosophy:** Score pages on brand consistency, authentic local voice, and natural prose. Trust SEO Reviewer for keyword density and clichés — focus on terminology, tone, and authenticity.

---

## Narrative Fields Definition

**IMPORTANT:** All checks apply ONLY to narrative text fields (same as SEO Reviewer).

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
contributionCTA.heading
contributionCTA.intro
valueCards[].title / description / detail
labels[].text
```

### Non-Narrative Fields (excluded from scoring)
```
All POI names, addresses, coordinates
All icon fields
All statistics/numbers
neighboringNeighborhoods[]
```

---

## Score Categories (v1.0)

| Category | Max Points | Weight |
|----------|------------|--------|
| Terminology Compliance | 30 | 30% |
| Tone & Voice | 25 | 25% |
| Local Authenticity | 20 | 20% |
| Narrative Naturalness | 15 | 15% |
| Sparse Data Handling | 10 | 10% |
| **Total** | **100** | **100%** |

---

## 1. Terminology Compliance (30 points)

Checks that content uses the correct Dutch terms per `terminology.json`.

| Criterion | Points |
|-----------|--------|
| No avoided terms used | 15 |
| Preferred terms present where relevant | 10 |
| Allowed exceptions used correctly | 5 |

### Terminology Rules

**Source of truth:** `../shared/terminology.json`

Read `preferredTerms` from that file to get:
- `use` — the preferred term
- `alternativeAllowed` — acceptable synonyms (if present)
- `avoid` — terms that should trigger a violation
- `allowedPhrases` — exceptions where an avoided term is acceptable (e.g., "buurt" in "buurtgevoel")

Do NOT hardcode terminology rules here. Always read from the JSON file.

### Deductions
| Issue | Deduction |
|-------|-----------|
| Each avoided term found | -3 |
| English term where Dutch exists | -2 |

### Examples

**Good (30/30):**
> "Met 5 parken binnen bereik is deze wijk ideaal voor baasjes die dagelijks willen wandelen. De omheinde hondenspeelweide biedt ruimte voor je viervoeter."

**Poor (21/30):**
> "Deze buurt is ideaal voor hondenbezitters die dagelijks wandelen met hun huisdier. Het hondenpark biedt voldoende ruimte."
- "buurt" instead of "wijk" (-3)
- "hondenbezitters" not explicitly wrong but "baasjes" preferred
- "huisdier" instead of "viervoeter/hond" (-3)
- "hondenpark" instead of "hondenspeelweide" (-3)

---

## 2. Tone & Voice (25 points)

Checks for friendly, second-person, non-corporate voice.

| Criterion | Points |
|-----------|--------|
| Second person (je/jouw) used consistently | 10 |
| Friendly, not corporate | 10 |
| Honest, not overselling | 5 |

### Perspective Check
Count occurrences of:
- **je/jouw forms:** je, jij, jouw, je (possessive)
- **u forms:** u, uw
- **wij forms:** wij, ons, onze

Scoring:
- Pure je/jouw: Full 10 points
- Mixed with occasional wij: 8 points
- Mixed with u: 5 points
- Predominantly u or wij: 0-3 points

### Friendly Markers (positive signals)
- "je vindt", "je kunt", "jouw hond"
- "handig voor", "praktisch voor wie"
- "neem water mee", "houd er rekening mee"
- "de route via X is rustiger"

### Anti-patterns (negative signals)
| Type | Examples | Deduction |
|------|----------|-----------|
| Corporate | "wij bieden", "onze services", "ons aanbod" | -3 each |
| Formal | "u kunt", "men dient", "dient men" | -3 each |
| Promotional | "ontdek de mogelijkheden", "profiteer van" | -3 each |

### Honesty Check
Trust SEO Reviewer's trade-off detection. Verify that trade-offs mentioned are genuine limitations, not faux-humble marketing ("De enige nadeel is dat je verwend wordt").

---

## 3. Local Authenticity (20 points)

Checks for specific, local content that only someone who knows the area would mention.

| Criterion | Points |
|-----------|--------|
| Specific place names in narrative | 8 |
| Practical local tips | 7 |
| Neighborhood-specific observations | 5 |

### Place Names Check
Count unique POI/landmark names mentioned in narrative text (not in POI arrays):
- 3+ unique names: 8 points
- 2 names: 6 points
- 1 name: 4 points
- 0 names: 0 points

### Local Tips Check
Presence of patterns like:
- "via de [street/area]"
- "richting [landmark]"
- "de route langs [place]"
- "bij [specific location]"

### Neighborhood Observations Check
Specific character observations:
- "de levendige sfeer rond [street]"
- "de drukte op [market/square]"
- "de rustige straten richting [area]"

### Examples

**Good (20/20):**
> "Dampoort combineer je hier stedelijk gemak met voldoende buitenruimte. De levendige sfeer rond de Dendermondsesteenweg biedt winkels en horeca, maar de drukte betekent ook meer verkeer. Kies voor rustigere wandelingen de routes richting de waterkant bij de Schelde."

**Poor (5/20):**
> "Deze wijk combineert stedelijk gemak met groene ruimte. Er zijn winkels en horeca in de buurt. Voor rustigere wandelingen kun je naar het park."

---

## 4. Narrative Naturalness (15 points)

**Note:** This category accepts LLM subjective judgment.

| Criterion | Points |
|-----------|--------|
| Varied sentence structure | 5 |
| Connected paragraphs | 5 |
| Conversational flow | 5 |

### Sentence Variety Check
Analyze first words of sentences in intro:
- Calculate variety score: unique starters / total sentences
- Score > 0.7: Full 5 points
- Score 0.5-0.7: 3 points
- Score < 0.5: 1 point

### Anti-patterns
| Pattern | Deduction |
|---------|-----------|
| 3+ sentences starting with "De wijk..." | -2 |
| 3+ sentences starting with "Er zijn/is..." | -2 |
| List-like prose: "Er zijn X. Er zijn Y. Er zijn Z." | -3 |
| Robotic transitions: "Daarnaast is er ook nog..." | -1 |

### Naturalness Test (subjective)
Read the intro aloud mentally:
- Does it sound like a person speaking to a friend? → Good
- Does it sound like a Wikipedia article? → Needs work
- Does it sound like a database dump? → Poor

---

## 5. Sparse Data Handling (10 points)

Checks that data gaps are handled gracefully, not apologetically.

| Criterion | Points |
|-----------|--------|
| Acknowledgment pattern used | 4 |
| Pivot to alternative present | 3 |
| Concrete alternative with distance | 3 |

### Good Pattern (acknowledge → pivot → alternative)
> "Een gespecialiseerde dierenwinkel vind je niet in de wijk zelf. De dichtstbijzijnde optie is Tom & Co aan de Dendermondsesteenweg, op 16 minuten wandelen. Als alternatief bieden de supermarkten in de wijk een basisassortiment."

### Bad Pattern (just apologize)
> "Helaas zijn er geen dierenwinkels in de buurt."
> "De wijk heeft jammer genoeg geen hondenspeelweide."

### Detection Heuristics

**Good indicators:**
- "vind je niet in de wijk (zelf)"
- "de dichtstbijzijnde optie is"
- "als alternatief"
- "op X minuten"

**Bad indicators:**
- "helaas"
- "jammer genoeg"
- "ontbreekt volledig"
- Statement ends without alternative

### Scoring
- Each gap handled gracefully: +points based on pattern completeness
- Each gap handled poorly: -2 points
- If no gaps detected: Full 10 points (nothing to handle)

---

## Score Calculation Example

**Input:** Dampoort writer output (post-SEO review)

| Category | Score | Notes |
|----------|-------|-------|
| Terminology | 27/30 | One "buurt" in allowed phrase, all preferred terms used |
| Tone & Voice | 23/25 | Consistent je/jouw, friendly markers present |
| Local Authenticity | 18/20 | Multiple place names, local tips present |
| Narrative Naturalness | 14/15 | Good variety, natural flow |
| Sparse Data Handling | 10/10 | Pet store gap handled gracefully |
| **Total** | **92/100** | **passedBrand: true** |

---

## Pass Threshold

```
passedBrand = qualityScore >= 70
```

| Score Range | Quality | Typical Action |
|-------------|---------|----------------|
| 90-100 | Excellent | No changes needed |
| 80-89 | Good | Minor terminology fixes |
| 70-79 | Acceptable | Passes with some fixes |
| 60-69 | Needs Work | Does not pass, flag for review |
| < 60 | Poor | Significant brand issues |

---

## Output Format

```json
{
  "scoreBreakdown": {
    "terminologyScore": 27,
    "toneVoiceScore": 23,
    "localAuthenticityScore": 18,
    "narrativeNaturalnessScore": 14,
    "sparseDataHandlingScore": 10
  }
}
```

---

## Implementation Priority

If time is limited, implement in this order:
1. **Terminology scanning** — Check against avoided terms list
2. **Perspective counting** — je/jouw vs u/wij forms
3. **Sparse data pattern detection** — acknowledge/pivot/alternative

Naturalness and local authenticity can rely more on LLM judgment.
