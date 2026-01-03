# POI Categories Reference

This document lists all POI categories available in the PostGIS database.
Use these exact category names in your queries.

## Available Categories

| Category | DB Value | Count | Description |
|----------|----------|-------|-------------|
| Veterinarians | `vet` | 612 | Veterinary practices |
| Pet Stores | `pet_store` | 328 | Pet shops and supplies |
| Dog Parks | `dog_park` | 764 | Dedicated dog exercise areas |
| Parks | `park` | 5,396 | General parks and green spaces |
| Supermarkets | `supermarket` | 2,927 | Grocery stores |
| Pharmacies | `pharmacy` | 2,803 | Apotheek/pharmacies |
| Schools | `school` | 5,869 | Educational institutions |
| Bus Stops | `bus_stop` | 43,850 | Public bus stops |
| Train Stations | `train_station` | 494 | Railway stations |

## Category Notes

### Dog-specific categories (primary focus)
- `vet` - Critical for dog owners, query with adaptive radius
- `pet_store` - Daily needs, query within reasonable walking distance
- `dog_park` - Off-leash areas, very important for quality of life

### General livability categories
- `park` - Green spaces (not specifically for dogs)
- `supermarket` - Daily shopping convenience
- `pharmacy` - Health services proximity
- `school` - Family-friendliness indicator

### Transport categories
- `bus_stop` - Public transport accessibility
- `train_station` - Regional connectivity

## Query Patterns

```sql
-- Count POIs by category in neighborhood
SELECT COUNT(*) FROM pois
WHERE category = 'vet'
AND ST_Contains(
  (SELECT boundary FROM neighborhoods WHERE id = 'gent-binnenstad'),
  location
);

-- Using helper function
SELECT * FROM get_pois_in_neighborhood('gent-binnenstad', 'vet');
```
