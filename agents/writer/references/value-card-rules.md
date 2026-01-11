# Value Card Rules

Create 4-8 value proposition cards highlighting key amenities.

## Allowed Categories

Only create value cards for these categories:

| Category | When to Include | Icon |
|----------|-----------------|------|
| Dog parks / Hondenspeelweiden | Always if data exists | `fa-solid fa-dog` |
| Vets / Dierenartsen | Always if data exists | `fa-solid fa-stethoscope` |
| Pet stores / Dierenwinkels | Always if data exists | `fa-solid fa-store` |
| Supermarkets | If count > 3 | `fa-solid fa-cart-shopping` |
| Transport / Openbaar vervoer | If busStops + trainStations > 10 | `fa-solid fa-bus` |
| Green space summary | **Fallback only** when no dog parks | `fa-solid fa-tree` |

## Do NOT Create Cards For

- General parks (parken) — these are NOT a separate category
- Schools, pharmacies, or other POI types

## Fallback Rule

If there are no dog parks, you may create ONE "Groene ruimte" card showing the nearest park or green space. But do not add a separate "Parken" card alongside a dog parks card.

## Card Structure

```json
{
  "icon": "fa-solid fa-dog",
  "title": "Hondenparken",
  "distance": "12 mins",
  "distanceIcon": "fa-solid fa-person-walking",
  "description": "1 hondenspeelweide",
  "detail": "Omheind, grasondergrond"
}
```

## Field Constraints

| Field | Max Length | Guidance |
|-------|------------|----------|
| `title` | 25 chars | Category name in Dutch |
| `description` | 60 chars | Count + brief summary |
| `detail` | 50 chars | Key feature or nearest POI name |

## Examples

**Dog Parks:**
```json
{
  "icon": "fa-solid fa-dog",
  "title": "Hondenspeelweide",
  "distance": "12 mins",
  "distanceIcon": "fa-solid fa-person-walking",
  "description": "1 omheinde speelweide",
  "detail": "Hondenweide Dampoort"
}
```

**Vets:**
```json
{
  "icon": "fa-solid fa-stethoscope",
  "title": "Dierenartsen",
  "distance": "7 mins",
  "distanceIcon": "fa-solid fa-person-walking",
  "description": "4 praktijken binnen bereik",
  "detail": "Dichtstbijzijnde op 7 min"
}
```

**Green Space Fallback (only when no dog parks):**
```json
{
  "icon": "fa-solid fa-tree",
  "title": "Groene ruimte",
  "distance": "5 mins",
  "distanceIcon": "fa-solid fa-person-walking",
  "description": "8 parken en pleinen",
  "detail": "Dichtstbijzijnde op 5 min"
}
```
