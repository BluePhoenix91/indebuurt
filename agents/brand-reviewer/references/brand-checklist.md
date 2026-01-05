# Brand Reviewer Quick Checklist

Use this checklist for rapid brand review. Each section maps to a scoring category.

---

## 1. Terminology (30 pts)

### Required Checks

**Source of truth:** `../shared/terminology.json`

For each entry in `preferredTerms`:
- [ ] No terms from `avoid` list appear in content
- [ ] Terms from `alternativeAllowed` are acceptable (don't flag)
- [ ] Terms in `allowedPhrases` are exceptions (don't flag those specific phrases)

---

## 2. Tone & Voice (25 pts)

### Perspective Check
- [ ] Uses "je/jouw" consistently (second person)
- [ ] No "u/uw" (formal)
- [ ] Minimal "wij/ons" (first person plural)

### Friendly Markers (should be present)
- [ ] "je vindt", "je kunt", "jouw hond"
- [ ] "handig voor", "praktisch voor wie"
- [ ] "neem water mee", "houd er rekening mee"
- [ ] Local tip phrasing ("de route via X")

### Red Flags (should NOT appear)
- [ ] "wij bieden", "onze services"
- [ ] "u kunt", "men dient"
- [ ] "ontdek de mogelijkheden", "profiteer van"
- [ ] Marketing superlatives ("fantastisch", "ongelooflijk")

---

## 3. Local Authenticity (20 pts)

### Place Names
- [ ] At least 2-3 specific place names in intro
- [ ] Street names mentioned (e.g., Dendermondsesteenweg)
- [ ] Parks named specifically (e.g., La Sapinière)
- [ ] Landmarks referenced (e.g., station, kerk, markt)

### Local Tips
- [ ] "via de [street/area]" pattern present
- [ ] "richting [landmark]" pattern present
- [ ] Route suggestions given
- [ ] Specific neighborhood character described

### Generic (avoid)
- [ ] "het park in de buurt"
- [ ] "de winkels in de straat"
- [ ] "nabijgelegen voorzieningen"

---

## 4. Narrative Naturalness (15 pts)

### Sentence Variety
- [ ] Sentences vary in length
- [ ] Different sentence starters
- [ ] Not all starting with "De wijk...", "Er zijn..."

### Flow
- [ ] Paragraphs connect logically
- [ ] Transitions feel natural
- [ ] Reads like speech, not a report

### Anti-patterns
- [ ] No list-like prose ("Er zijn X. Er zijn Y.")
- [ ] No robotic transitions ("Daarnaast...")
- [ ] No Wikipedia-style formality

---

## 5. Sparse Data Handling (10 pts)

### Good Pattern (full points)
```
[Acknowledge] Een gespecialiseerde dierenwinkel vind je niet in de wijk zelf.
[Pivot] De dichtstbijzijnde optie is Tom & Co aan de Dendermondsesteenweg,
[Alternative] op 16 minuten wandelen. Als alternatief bieden de supermarkten
een basisassortiment.
```

### Bad Pattern (deduct points)
```
Helaas zijn er geen dierenwinkels in de buurt.
```

### Checklist
- [ ] Gap acknowledged without apology
- [ ] Nearest alternative mentioned
- [ ] Distance/time to alternative given
- [ ] No "helaas", "jammer genoeg"

---

## Quick Decision Tree

```
START
  ↓
Scan for avoided terms (eigenaars, hondenpark, buurt, etc.)
  → Found? → Fix terminology → Log change
  ↓
Check perspective (je/jouw vs u/wij)
  → Wrong form? → Fix perspective → Log change
  ↓
Look for corporate/formal phrases
  → Found? → Rewrite friendly → Log change
  ↓
Check for specific place names
  → Generic? → Flag as issue (may need context)
  ↓
Read intro aloud mentally
  → Sounds robotic? → Flag for review
  ↓
Check sparse data sections
  → "Helaas" pattern? → Rewrite with alternative → Log change
  ↓
Calculate score → Output
```

---

## Auto-Fix vs. Flag for Review

### Auto-Fix (simple substitution)
For terminology: replace `avoid` terms with `use` term (check `../shared/terminology.json`)
For perspective: replace "u kunt" → "je kunt", etc.

### Flag for Review (needs judgment)
- Missing local authenticity (needs neighborhood knowledge)
- Narrative flow issues (subjective)
- Sparse data needs complete rewrite
- Tone mismatch requiring paragraph restructure
