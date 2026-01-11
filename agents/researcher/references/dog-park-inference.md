# Dog Park Feature Inference Logic

OSM data for dog parks is sparse. Use this multi-layered inference to extract features.

## isFenced (boolean)

**Priority 1 — Tag-based (highest confidence):**
```
TRUE if: barrier IN ('fence', 'hedge')
      OR fenced = 'yes'
      OR fence = 'yes'
      OR fence_type IS NOT NULL
```

**Priority 2 — Name-based (if no tag):**
```
TRUE if name contains any of:
  'losloop', 'hondenweide', 'hondenspeelweide',
  'hondenspeelzone', 'hondenpark', 'hondenzone', 'vrijheidszone'
```

**Default:** FALSE

## surface (string, optional)

**Priority 1 — Tag-based:**
```
surface tag → use directly
landuse tag → 'grass'|'meadow' → 'grass', 'forest' → 'mixed'
landcover tag → 'grass' → 'grass'
natural tag → 'sand' → 'sand'
```

**Priority 2 — Name-based:**
```
name contains 'weide' → 'grass'
name contains 'bos' → 'mixed'
```

**Default:** Omit if no information

## hasWater (boolean)

**Priority 1 — Tag-based:**
```
TRUE if swimming:dog = 'yes'
```

**Priority 2 — Name/description-based:**
```
TRUE if name OR description contains 'water' or 'zwem'
```

**Default:** FALSE

## Optional Features (extract when tagged)

| Feature | Tag | Notes |
|---------|-----|-------|
| isAccessible | `wheelchair` | enum: "yes", "no", "limited" (~19% coverage) |
| isLit | `lit` | boolean (~2% coverage) |
| openingHours | `opening_hours` | string, e.g., "24/7" (~3% coverage) |
| hasSmallDogArea | `small_dog` | boolean (~2% coverage) |

See `query-examples.md` for the full SQL query implementing this logic.
