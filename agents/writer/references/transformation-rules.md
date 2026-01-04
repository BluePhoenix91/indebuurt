# Writer Agent Transformation Rules

This document defines how to transform ResearcherOutput data into WriterOutput format.

---

## Distance Formatting

The Researcher provides `walkingTimeMinutes` (already calculated). Format it for display.

### Format Rules

| Minutes | Output |
|---------|--------|
| 1 | "1 min" |
| 2-59 | "X mins" |

### Examples

```
walkingTimeMinutes: 4  → distance: "4 mins"
walkingTimeMinutes: 12 → distance: "12 mins"
walkingTimeMinutes: 1  → distance: "1 min"
```

### In Dutch Prose

When writing narrative text, use natural Dutch:
- "op 4 minuten wandelen"
- "binnen 10 minuten te voet"
- "slechts 2 minuten lopen"

---

## Zoom Level Calculation

Calculate map zoom from the neighborhood's bounding box.

### Formula

```
latSpan = boundingBox.north - boundingBox.south
lonSpan = boundingBox.east - boundingBox.west
maxSpan = max(latSpan, lonSpan)

Zoom levels:
- maxSpan < 0.01  → zoom = 16  (very small, <1km)
- maxSpan < 0.02  → zoom = 15  (small, 1-2km)
- maxSpan < 0.04  → zoom = 14  (medium, 2-4km)
- maxSpan < 0.08  → zoom = 13  (large, 4-8km)
- maxSpan >= 0.08 → zoom = 12  (very large, >8km)
```

### Example

For Dampoort:
```json
"boundingBox": {
  "north": 51.077512,
  "south": 51.046655,
  "east": 3.756195,
  "west": 3.738679
}

latSpan = 51.077512 - 51.046655 = 0.030857
lonSpan = 3.756195 - 3.738679 = 0.017516
maxSpan = 0.030857

→ zoom = 14 (medium neighborhood)
```

---

## POI Selection Criteria

Select the most relevant POIs for each category.

### Maximum Items Per Category

| Category | Max Items |
|----------|-----------|
| vets | 4 |
| petStores | 4 |
| dogParks | 5 |
| parks | 5 (for value card reference only) |

### Selection Priority

1. **Distance** — closest first
2. **Named** — POIs with names over "Unnamed"
3. **Features** — for dog parks, prefer those with features (fenced, water)

### Example

If ResearcherOutput contains 8 vets, select the 4 closest ones. If two are equidistant, prefer the one with a name.

---

## Feature Extraction (Dog Parks)

Convert ResearcherOutput boolean flags to a features array with icons.

### Mapping Table

| ResearcherOutput Field | Feature Text (Dutch) | Icon |
|------------------------|---------------------|------|
| `isFenced: true` | "Omheind terrein" | fa-solid fa-fence |
| `hasWater: true` | "Drinkwater aanwezig" | fa-solid fa-droplet |
| `surface: "grass"` | "Grasondergrond" | fa-solid fa-seedling |
| `surface: "gravel"` | "Grindondergrond" | fa-solid fa-circle-dot |
| `surface: "mixed"` | "Gemengde ondergrond" | fa-solid fa-layer-group |

### Example Transformation

**Input (ResearcherOutput):**
```json
{
  "name": "Hondenweide Dampoort",
  "isFenced": true,
  "hasWater": false,
  "surface": "grass"
}
```

**Output (WriterOutput):**
```json
{
  "name": "Hondenweide Dampoort",
  "icon": "fa-solid fa-dog",
  "features": [
    { "text": "Omheind terrein", "icon": "fa-solid fa-fence" },
    { "text": "Grasondergrond", "icon": "fa-solid fa-seedling" }
  ]
}
```

Note: Only include features for `true` values or present strings.

---

## Value Card Selection

Create 4-8 value cards highlighting key amenities.

### Mandatory Cards (if data exists)

1. **Dog parks** — always first priority
2. **Vets** — healthcare is essential
3. **Pet stores** — supplies matter

### Conditional Cards (based on counts)

| Condition | Card |
|-----------|------|
| `poiCounts.parks > 5` | Parks/Green space |
| `poiCounts.supermarkets > 3` | Supermarkets |
| `poiCounts.busStops + poiCounts.trainStations > 10` | Transport |
| `poiCounts.schools > 5` | Schools (family-friendly) |

### Card Structure

Each value card needs:
- `icon` — from valueCardIcons in icon-mappings.json
- `title` — max 25 chars, Dutch (e.g., "Hondenparken", "Dierenartsen")
- `distance` — formatted distance to nearest
- `distanceIcon` — typically "fa-solid fa-person-walking"
- `description` — max 60 chars (e.g., "3 hondenparken in de buurt")
- `detail` — max 50 chars (e.g., "Dichtstbijzijnde op 5 min")

### Target Count

Aim for **4-6 cards**. Never exceed 8.

---

## Neighboring Neighborhoods

Extract and format neighboring neighborhood IDs.

### Rules

1. Take from `context.neighboringNeighborhoods`
2. Select top 5 by distance
3. Output as array of ID strings only (not full objects)

### Example

**Input:**
```json
"neighboringNeighborhoods": [
  { "id": "gent-groot-begijnhof", "name": "GROOT BEGIJNHOF", "distanceMeters": 677 },
  { "id": "gent-afrikalaan", "name": "AFRIKALAAN", "distanceMeters": 706 },
  { "id": "gent-blaisantvest", "name": "BLAISANTVEST", "distanceMeters": 1187 }
]
```

**Output:**
```json
"neighboringNeighborhoods": [
  "gent-groot-begijnhof",
  "gent-afrikalaan",
  "gent-blaisantvest"
]
```

---

## Identity Field Mapping

Direct mappings from ResearcherOutput to WriterOutput:

| ResearcherOutput | WriterOutput |
|------------------|--------------|
| `neighborhoodId` | `id` |
| `neighborhoodName` | `name` |
| `city` | `city` |
| `postalCode` | `postalCode` |
| `statistics.inhabitants` | `inhabitants` |
| `centerCoordinates.lat` | `coordinates.lat` |
| `centerCoordinates.lon` | `coordinates.lon` |
| (calculated) | `coordinates.zoom` |
| (current date) | `dateAdded` |

---

## Statistics Mapping

| ResearcherOutput | WriterOutput |
|------------------|--------------|
| `statistics.medianHousePrice` | `statistics.medianPrice` |
| `statistics.inhabitants` | `statistics.inhabitants` |
| `statistics.availableHomes` | `statistics.availableHomes` (null if 0 in source) |
| `statistics.pricePerSqm` | `statistics.pricePerSqm` (often null) |

Note: Fields may be `null` if data is unavailable. This is valid.
