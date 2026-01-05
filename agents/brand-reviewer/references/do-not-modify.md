# Fields the Brand Reviewer Must NOT Modify

This document lists all fields that are protected from brand review modifications. These contain factual data, technical identifiers, or presentation choices that must remain unchanged.

**Note:** This is the same protection list as the SEO Reviewer. Brand Reviewer only modifies narrative text fields, never data.

---

## Absolutely Protected (Never Touch)

### Identity & Technical Fields
- `schemaVersion` — Schema version identifier
- `generatedAt` — Will be updated by Brand agent to reflect review timestamp
- `id` — Neighborhood slug for URLs
- `city` — City name (factual)
- `name` — Neighborhood display name (factual)
- `postalCode` — Postal code (factual)
- `dateAdded` — Page creation date

### Geographic Data
- `coordinates.lat` — Latitude
- `coordinates.lon` — Longitude
- `coordinates.zoom` — Map zoom level

### Statistical Data
- `inhabitants` — Population count
- `statistics.medianPrice` — Median house price
- `statistics.inhabitants` — (duplicate of above)
- `statistics.availableHomes` — Homes for sale
- `statistics.pricePerSqm` — Price per square meter

### POI Data (All Arrays)

These represent real-world data from the Researcher agent:

**Dog Parks (`dogParks.parks[*]`):**
- `name`, `distance`, `distanceIcon`, `coordinates`
- `features[*].text`, `features[*].icon`

**Veterinary Practices (`vets.practices[*]`):**
- `name`, `street`, `streetNumber`, `bus`, `municipality`, `postalCode`
- `distance`, `distanceIcon`, `coordinates`

**Pet Stores (`petStores.stores[*]`):**
- `name`, `street`, `streetNumber`, `bus`, `municipality`, `postalCode`
- `distance`, `distanceIcon`, `coordinates`

### Icons
- All `icon` fields throughout the document
- All `distanceIcon` fields
- `labels[*].icon` — Icon assignments are editorial decisions

### System Fields
- `contributionCTA.typeformId` — Fixed form identifier
- `houses.hasOwnPostalCode` — Boolean derived from data

### SEO Review Data
- `seoReview.*` — All SEO review metadata must be preserved

---

## Conditionally Protected

### `neighboringNeighborhoods`
- **Do NOT modify** the array contents
- SEO Reviewer already validated these

---

## Fields Brand Reviewer CAN Modify

### High Priority (Main Brand Targets)
- `subtitle` — Terminology and tone
- `intro` — Main content (terminology, tone, authenticity)
- `facilities.intro` — Facilities section intro
- `dogParks.intro` — Dog parks section intro
- `vets.intro` — Veterinary section intro
- `petStores.intro` — Pet stores section intro
- `statistics.intro` — Statistics section intro
- `houses.intro` — Housing section intro

### Medium Priority
- `dailyLife.title` — Section title
- `dailyLife.intro` — Daily life intro paragraph
- `dailyLife.benefits[*]` — Benefit bullet points (text only)
- `valueCards[*].title` — Card titles (terminology only)
- `valueCards[*].description` — Card descriptions
- `valueCards[*].detail` — Card detail text
- `labels[*].text` — Label text (terminology only)

### Low Priority
- `contributionCTA.heading` — CTA heading text
- `contributionCTA.intro` — CTA intro paragraph

---

## Modification Guidelines

When modifying allowed fields:

1. **Fix terminology only** — Replace avoided terms with preferred equivalents
2. **Fix tone issues** — Replace formal/promotional language with friendly tone
3. **Preserve meaning** — Changes should correct, not alter content meaning
4. **Log all changes** — Every modification requires a changesLog entry
5. **Respect SEO work** — Don't undo SEO optimizations unless they violate brand rules
6. **Dutch only** — All prose modifications must be in Dutch
