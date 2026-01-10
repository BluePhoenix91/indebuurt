# Fields the Quality Reviewer Must NOT Modify

This document lists all fields that are protected from modification. These contain factual data, technical identifiers, or presentation choices that must remain unchanged.

---

## Absolutely Protected (Never Touch)

### Identity & Technical Fields
- `schemaVersion` — Schema version identifier
- `generatedAt` — Will be updated by Quality agent to reflect review timestamp
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

---

## Conditionally Protected

### `neighboringNeighborhoods`
- **Validate** that each ID exists in the database
- **Log warning** if an ID is invalid
- **Do NOT remove or modify** the array contents
- Reason: May reference future neighborhoods not yet in database

---

## Fields Quality Reviewer CAN Modify

### High Priority (Main Targets)
- `subtitle` — Meta description + terminology
- `intro` — Main content (SEO + brand)
- `facilities.intro` — Facilities section intro
- `dogParks.intro` — Dog parks section intro
- `vets.intro` — Veterinary section intro
- `petStores.intro` — Pet stores section intro
- `statistics.intro` — Statistics section intro
- `houses.intro` — Housing section intro

### Medium Priority
- `dailyLife.title` — Section title (should include neighborhood name)
- `dailyLife.intro` — Daily life intro paragraph
- `dailyLife.benefits[*]` — Benefit bullet points (text only)
- `valueCards[*].title` — Card titles
- `valueCards[*].description` — Card descriptions
- `valueCards[*].detail` — Card detail text
- `labels[*].text` — Label text (not icons)

### Low Priority (Only If Issues Found)
- `contributionCTA.heading` — CTA heading text
- `contributionCTA.intro` — CTA intro paragraph

---

## Modification Guidelines

When modifying allowed fields:

1. **Apply Brand terminology FIRST** — Replace avoided terms with preferred equivalents before SEO checks
2. **Fix tone issues** — Replace formal/promotional language with friendly tone
3. **Preserve meaning** — Changes should optimize, not alter content meaning
4. **Log all changes** — Every modification requires a changesLog entry with reason
5. **Dutch only** — All prose modifications must be in Dutch
6. **Respect data** — Reference factual data accurately when mentioned in prose
