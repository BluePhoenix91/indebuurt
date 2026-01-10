# Writer Agent Content Guidelines

This document defines the brand voice, terminology, and writing patterns for neighborhood content.

---

## Brand Voice

### Core Principles

- **Friendly** — Like advice from a fellow dog owner who knows the area
- **Informative** — Specific facts and numbers, not vague descriptions
- **Honest** — Acknowledge trade-offs; no neighborhood is perfect

### Perspective

Always write in second person, directly addressing the reader:
- Use: "je", "jouw", "je hond"
- Avoid: "men", "de eigenaar", "de bewoner"

### Tone Examples

**Good:**
> Met voldoende parken binnen handbereik en een hondenspeelweide op 12 minuten wandelen, is Dampoort een fijne wijk voor baasjes die van groene ruimte houden.

**Bad:**
> Dampoort is een ideale wijk voor hondenliefhebbers met veel groene ruimte.

The good example uses qualitative language for counts ("voldoende parken"), specific distances (12 minutes), correct terminology (baasjes, hondenspeelweide), and doesn't overclaim (says "fijne wijk" not "ideale wijk").

---

## Terminology

Use these terms consistently. Source: `shared/terminology.json`

### Required Terms

| Use | Avoid |
|-----|-------|
| **baasjes** | eigenaars, eigenaren, bezitters, houders |
| **viervoeter**, **hond** | huisdier, dier |
| **hondenspeelweide** | hondenpark, dog park |
| **wijk** | buurt (except in "buurtgevoel") |
| **dierenarts** | veterinair, veearts |
| **uitlaten** | wandelen met |
| **wandeling** | wandeltocht |

### Usage Examples

- "Als baasje in Dampoort..."
- "Je viervoeter kan hier..."
- "De dichtstbijzijnde hondenspeelweide..."
- "Voor de dagelijkse wandeling..."

---

## Patterns to Avoid

Never use these clichés or marketing phrases:

- "ideaal voor iedereen"
- "perfecte wijk"
- "unieke locatie"
- "bruisende buurt"
- "gezellige sfeer"
- "op een steenworp afstand"
- "in het hart van"
- "een must voor..."
- "dé plek voor..."

### Why?

These phrases are:
1. Vague — they don't tell the reader anything specific
2. Overused — they sound like generic marketing copy
3. Untrustworthy — if everything is "perfect" or "ideal", nothing is

---

## POI Names in Prose

Do NOT mention specific POI names in narrative text:

**Avoid:**
- "Tom & Co op de Martelaarslaan"
- "Dierenarts Heughebaert Anne"
- "Het Appelbrugparkje ligt om de hoek"

**Use instead:**
- "een dierenwinkel op 18 minuten wandelen"
- "de dichtstbijzijnde praktijk"
- "het dichtstbijzijnde park ligt om de hoek"

POI names belong in structured data only (vets.practices, petStores.stores, dogParks.parks arrays).

**Why?** Names change (businesses close, rename). Generic references stay accurate over time.

---

## Patterns to Encourage

Use these approaches instead:

### Specific Distances

Keep walking times and distances specific — these add precision and don't become stale:
- "op 4 minuten wandelen"
- "binnen 500 meter"
- "de dichtstbijzijnde dierenarts ligt op 7 minuten"

### Qualitative Counts (for prose)

Use qualitative language instead of specific numbers in all prose sections (intro, section intros, dailyLife):

| Count | Dutch Phrasing |
|-------|----------------|
| 0 | "geen ... in de wijk", "niet aanwezig in de directe omgeving" |
| 1 | "één ...", "de enige ...", "een enkele ..." |
| 2 | "een paar ...", "twee opties" |
| 3-5 | "enkele ...", "een handvol ...", "meerdere opties" |
| 6-10 | "voldoende ...", "ruim voldoende ...", "voldoende keuze" |
| 11-20 | "veel ...", "een ruim aanbod aan ..." |
| 20+ | "talrijke ...", "een uitgebreid aanbod aan ...", "ruim voldoende" |

**Good:**
- "voldoende dierenartsen binnen bereik" (not "3 dierenartsen")
- "ruim voldoende parken" (not "23 parken")
- "een enkele hondenspeelweide" (not "1 hondenspeelweide")
- "meerdere praktijken in de omgeving"
- "goede OV-bereikbaarheid" (not "41 bushaltes")

**Why?** OSM data changes regularly. Qualitative prose stays accurate; specific counts belong only in structured data (valueCards, POI arrays).

### Specific Counts (for structured data only)

Keep specific counts in these locations:
- `valueCards.description`: "20 parken in de buurt"
- `valueCards.detail`: "5 praktijken binnen 30 min"
- POI arrays (`dogParks.parks`, `vets.practices`, `petStores.stores`)

### Honest Trade-offs

- "geen hondenspeelweide in de wijk zelf, maar..."
- "de dichtstbijzijnde dierenarts ligt buiten de wijk"
- "met 5951 inwoners per km² is het hier drukker"

### Practical Scenarios

- "voor de ochtendwandeling..."
- "op een regenachtige dag..."
- "als je hond plots ziek wordt..."

---

## Handling Sparse Data

When data is missing or sparse, be honest and pivot to alternatives.

### Principle

1. Acknowledge the gap honestly
2. Pivot to what IS available
3. Give a concrete alternative with distance

### Example: No Dog Parks

**Bad:**
> Helaas zijn er geen hondenparken in de wijk.

**Good:**
> Een officiële hondenspeelweide vind je niet in de wijk zelf, maar met voldoende parken en groene pleinen heb je meer dan genoeg ruimte voor de dagelijkse wandeling. De dichtstbijzijnde omheinde speelweide ligt op 12 minuten wandelen.

### Example: No Vets

**Bad:**
> Er is geen dierenarts in de buurt.

**Good:**
> In de wijk zelf is geen dierenartspraktijk gevestigd. De dichtstbijzijnde optie ligt op 8 minuten wandelen in een aangrenzende wijk. Voor spoedgevallen zijn meerdere praktijken bereikbaar met de auto.

### Example: No Pet Stores

**Bad:**
> Er zijn geen dierenwinkels.

**Good:**
> Een gespecialiseerde dierenwinkel vind je niet in de wijk, maar de dichtstbijzijnde optie ligt op 16 minuten te voet. De meeste supermarkten in de wijk hebben ook een basisassortiment voor je viervoeter.

---

## Section-Specific Guidelines

### Subtitle (80-120 chars)

- One compelling line that captures the neighborhood's essence for **living**, with dog ownership as a lens
- Start with living context (wonen, bereikbaar, groen, rustig) rather than dog amenities
- Include a specific hook (unique trait or key data point)
- Should work as a meta description for SEO

**Good examples:**
- "Wonen in Dampoort: compact, goed bereikbaar en groen op wandelafstand — met ruimte voor je hond in Gent"
- "Rustige stadswijk met parken op wandelafstand en snelle toegang tot voorzieningen — fijn voor je dagelijkse routine"

**Avoid:**
- "Ideale wijk voor baasjes met honden" (dog-first, generic, cliché)
- "Hondvriendelijke wijk met veel voorzieningen" (dog-first, vague)

### Intro (400-800 words)

**First paragraph requirements:**
1. Neighborhood name in first sentence
2. City name within first 100 words
3. Living context established (2+ of: walkability, green space, character, mobility)
4. Dog lens can come later — don't force it into the opening

**Must cover (flexible order after opening):**
- [ ] Neighborhood character/vibe — what's it like to live here?
- [ ] Key amenities summary — what's nearby?
- [ ] At least one honest trade-off with mitigation — what's the downside and how do you deal with it?
- [ ] "Who is this for" signal — who would thrive here vs who might not?
- [ ] Specific data points woven naturally

**Living-first approach:**
Open with the neighborhood's character and livability, not with dog amenities. Dog ownership is a lens, not the entire frame.

**Good opening:**
> "Dampoort is een compacte stadswijk aan de rand van het Gentse centrum. Met het station om de hoek en groen op wandelafstand combineer je hier stedelijk gemak met voldoende buitenruimte voor je dagelijkse routine."

**Bad opening:**
> "Als baasje in Dampoort heb je toegang tot een hondenspeelweide en meerdere parken."

Don't pad to reach word count. Quality over quantity.

### Daily Life Benefits (3-7 items)

Each benefit should be:
- **Qualitative counts** — use "voldoende", "meerdere", "ruim aanbod" (not specific numbers)
- **Specific distances** — walking times are allowed ("op 15 minuten wandelen")
- **Practical** — describe real scenarios
- **30-80 words** — substantive but concise
- **No POI names** — don't mention specific parks, vets, or stores by name

**Good:**
> Dankzij de voldoende parken binnen wandelafstand heb je altijd een groene plek voor de ochtendwandeling. Sommige parken hebben schaduwrijke paden voor warme dagen.

**Bad:**
> Dankzij de 6 parken... Het Cirkelspark is populair... (uses specific count and POI name)

---

## Quality Checklist

Before finalizing content, verify:

- [ ] All terminology matches the guide (baasjes, hondenspeelweide, etc.)
- [ ] No forbidden phrases used
- [ ] At least one honest trade-off mentioned
- [ ] **No specific counts in prose** — use qualitative language ("voldoende", "meerdere")
- [ ] **No POI names in prose** — save names for structured data
- [ ] **Distances remain specific** — "op X minuten wandelen" is encouraged
- [ ] **Population stats remain specific** — Statbel data (inhabitants, density) keeps exact numbers
- [ ] Content reads naturally, not like a list of facts
- [ ] Second person consistently used (je, jouw)
- [ ] Dutch spelling and grammar correct
