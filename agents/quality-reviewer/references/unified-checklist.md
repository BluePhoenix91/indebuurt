# Quality Reviewer Quick Checklist

Use this checklist for rapid quality review. Combines SEO and Brand checks in optimal execution order.

---

## Execution Order

**Brand checks run FIRST** (so SEO counts run on clean text):

1. Terminology scan -> fix avoided terms
2. Tone check -> fix formal/corporate language
3. Perspective check -> ensure je/jouw consistency

**SEO checks run SECOND:**

4. Keyword optimization (on now-clean text)
5. Subtitle optimization
6. Section intro optimization
7. Local Relevance scoring

**Final checks:**

8. Local Authenticity scoring (Brand)
9. Narrative naturalness (Brand)
10. Sparse data handling (Brand)
11. Internal link validation (SEO - GIS query)
12. Calculate scores

---

## Brand Checks

### 1. Terminology (30 pts)

**Source of truth:** `../shared/terminology.json`

| Preferred | Avoid | Allowed Exceptions |
|-----------|-------|-------------------|
| baasjes | eigenaars, bezitters, houders | |
| viervoeter/hond | huisdier, dier | |
| wijk | buurt | buurtgevoel, de juiste buurt, buurtbewoners, in de buurt |
| hondenspeelweide | dog park | hondenpark (allowed alternative) |
| dierenarts | veterinair, veearts | |
| wandeling | wandeltocht | |

### 2. Tone & Voice (25 pts)

**Perspective:**
- Use: je, jij, jouw
- Avoid: u, uw, wij, ons, onze

**Friendly markers (should be present):**
- "je vindt", "je kunt", "jouw hond"
- "handig voor", "praktisch voor wie"
- "neem water mee", "houd er rekening mee"

**Red flags (should NOT appear):**
- "wij bieden", "onze services" (corporate)
- "u kunt", "men dient" (formal)
- "ontdek de mogelijkheden" (promotional)

### 3. Local Authenticity (20 pts)

- [ ] 2-3 specific place names in prose
- [ ] "via de [street]" or "richting [landmark]" pattern
- [ ] Specific neighborhood character observations

### 4. Narrative Naturalness (15 pts)

- [ ] Varied sentence starters (not all "De wijk...")
- [ ] No list-like prose ("Er zijn X. Er zijn Y.")
- [ ] Reads conversationally

### 5. Sparse Data Handling (10 pts)

**Good pattern:**
```
[Acknowledge] Een dierenwinkel vind je niet in de wijk zelf.
[Pivot] De dichtstbijzijnde optie is Tom & Co,
[Alternative] op 16 minuten wandelen.
```

**Bad pattern:**
```
Helaas zijn er geen dierenwinkels in de buurt.
```

---

## SEO Checks

### 1. Subtitle (15 pts)

- [ ] 80-120 characters
- [ ] Contains neighborhood name
- [ ] Contains city name
- [ ] Contains living signal OR dog signal
- [ ] No clichés: ideaal, perfect, bruisend, uniek

### 2. Main Intro (25 pts)

- [ ] Neighborhood in first sentence
- [ ] City in first 100 words
- [ ] 2+ living context buckets (walkability, green, mobility, calm)
- [ ] Trade-off mentioned
- [ ] 400-800 words total
- [ ] Max 4 explicit dog terms (hond/baasjes/viervoeter)

### 3. Topic Coverage (20 pts)

| Bucket | Requirement |
|--------|-------------|
| Wonen & Leefkwaliteit | 2+ terms |
| Groen & Buiten | 2+ terms |
| Rust & Veiligheid | 1+ terms |
| Dog Lens | 1-3 explicit OR 3+ implicit with walking term |

### 4. Section Intros (10 pts)

Each must answer "what exists + why it matters"
- Minimum ~30 words if useful
- Flag if < 20 words AND missing usefulness

### 5. Decision Usefulness (15 pts)

- [ ] Trade-off pattern: transition word + friction keyword
- [ ] Mitigation pattern: "maar op X minuten", "alternatief"
- [ ] Who-for pattern: "praktisch voor wie", "minder geschikt als"

### 6. Local Relevance (10 pts)

- [ ] 2+ POI names in narrative
- [ ] Local landmark reference
- [ ] Neighborhood name in dailyLife.title

### 7. Internal Linking (5 pts)

- [ ] neighboringNeighborhoods populated
- [ ] All IDs exist in GIS database

---

## Scoring Summary

| Domain | Categories | Max Points |
|--------|------------|------------|
| **SEO** | 7 categories | 100 |
| **Brand** | 5 categories | 100 |
| **Quality Score** | (SEO + Brand) / 2 | 100 |
| **Pass Threshold** | | >= 70 |

---

## Change Reasons

When logging changes, use these reason codes:

**SEO reasons:**
- `subtitle_length` — Subtitle too short/long
- `keyword_density` — Keywords missing/insufficient
- `intro_structure` — Intro missing SEO elements
- `section_intro_thin` — Section intro too short
- `local_keyword_missing` — Missing city/neighborhood
- `readability` — Sentence structure improved

**Brand reasons:**
- `terminology_violation` — Used avoided term
- `tone_formal` — Too corporate/formal
- `tone_promotional` — Too salesy
- `perspective_inconsistent` — Mixed je/u/wij
- `narrative_list_like` — Reads like a list
- `sparse_data_unhandled` — Gap not handled gracefully
- `english_term_used` — English where Dutch preferred
