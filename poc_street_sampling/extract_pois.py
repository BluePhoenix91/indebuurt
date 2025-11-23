import osmium
import pandas as pd
import os
from math import radians, cos, sin, asin, sqrt

# POI categories for the street sampling POC (3 categories only)
POI_CATEGORIES = {
    'supermarkets': {
        'name': 'Supermarkets (Groceries)',
        'filters': [
            ('shop', ['supermarket'])
        ]
    },
    'pt_stops': {
        'name': 'Public Transport Stops',
        'filters': [
            ('public_transport', ['stop_position', 'station'])
        ]
    },
    'green_spaces': {
        'name': 'Parks & Green Spaces',
        'filters': [
            ('leisure', ['park', 'garden', 'dog_park']),
            ('natural', ['wood']),
            ('landuse', ['forest', 'recreation_ground'])
        ]
    }
}

def haversine(lon1, lat1, lon2, lat2):
    """
    Calculate the great circle distance in meters between two points
    on the earth (specified in decimal degrees)
    """
    # Convert decimal degrees to radians
    lon1, lat1, lon2, lat2 = map(radians, [lon1, lat1, lon2, lat2])

    # Haversine formula
    dlon = lon2 - lon1
    dlat = lat2 - lat1
    a = sin(dlat/2)**2 + cos(lat1) * cos(lat2) * sin(dlon/2)**2
    c = 2 * asin(sqrt(a))
    r = 6371000  # Radius of earth in meters
    return c * r

def point_within_any_neighborhood(lon, lat, neighborhoods_df, radius_m):
    """
    Check if a point is within radius of ANY neighborhood center

    Args:
        lon, lat: Point coordinates
        neighborhoods_df: DataFrame with neighborhood centers
        radius_m: Radius in meters

    Returns:
        True if within radius of at least one neighborhood
    """
    for _, neighborhood in neighborhoods_df.iterrows():
        distance = haversine(lon, lat, neighborhood['longitude'], neighborhood['latitude'])
        if distance <= radius_m:
            return True
    return False

class POIExtractor(osmium.SimpleHandler):
    def __init__(self, category_key, filters, neighborhoods_df, radius_m=2000):
        super().__init__()
        self.category_key = category_key
        self.filters = filters
        self.neighborhoods_df = neighborhoods_df
        self.radius_m = radius_m
        self.pois = []
        self.processed = 0

    def matches_filter(self, tags):
        """Check if tags match any of the category filters"""
        for key, values in self.filters:
            if key in tags and tags[key] in values:
                return True
        return False

    def extract_poi(self, obj):
        """Extract POI information from OSM object"""
        if self.matches_filter(obj.tags):
            # Get location (only process nodes for simplicity)
            if hasattr(obj, 'location'):
                lat = obj.location.lat
                lon = obj.location.lon
            else:
                # Skip ways/areas for now (would need centroid calculation)
                return

            # Check if within radius of any neighborhood
            if not point_within_any_neighborhood(lon, lat, self.neighborhoods_df, self.radius_m):
                return

            # Extract relevant tags
            name = obj.tags.get('name', 'Unnamed')
            osm_type = 'node'
            osm_id = obj.id

            # Get specific type from tags
            poi_type = None
            for key, values in self.filters:
                if key in obj.tags:
                    poi_type = f"{key}={obj.tags[key]}"
                    break

            self.pois.append({
                'osm_type': osm_type,
                'osm_id': osm_id,
                'name': name,
                'latitude': lat,
                'longitude': lon,
                'poi_type': poi_type,
                'category': self.category_key
            })

            if len(self.pois) % 100 == 0:
                print(f"  {self.category_key}: Found {len(self.pois)} POIs...")

        self.processed += 1
        if self.processed % 1000000 == 0:
            print(f"  {self.category_key}: Processed {self.processed:,} objects...")

    def node(self, n):
        self.extract_poi(n)

def extract_category_pois(osm_file, category_key, category_info, neighborhoods_df, radius_m=2000):
    """Extract POIs for a specific category"""
    print(f"\nExtracting {category_info['name']}...")
    print(f"  Radius: {radius_m}m around {len(neighborhoods_df)} neighborhoods")

    handler = POIExtractor(category_key, category_info['filters'], neighborhoods_df, radius_m)
    handler.apply_file(osm_file)

    print(f"  Found {len(handler.pois)} POIs")

    return handler.pois

def save_to_csv(pois, output_file):
    """Save POIs to CSV file"""
    os.makedirs(os.path.dirname(output_file), exist_ok=True)

    df = pd.DataFrame(pois)
    df.to_csv(output_file, index=False)

    print(f"  Saved to {output_file}")

def main():
    print("=" * 70)
    print("Street Sampling POC - Extract POIs for Distance Calculations")
    print("=" * 70)

    # Configuration
    OSM_FILE = "../poc_smartscore/data/belgium-latest.osm.pbf"  # Reuse from poc_smartscore
    NEIGHBORHOODS_FILE = "data/neighborhoods.csv"
    OUTPUT_DIR = "data/pois"
    RADIUS_M = 2000  # 2km radius for broader context

    print("\nPOC Categories (3 simplified categories):")
    for key, info in POI_CATEGORIES.items():
        print(f"  - {info['name']}")

    # Load neighborhoods
    print("\n1. Loading neighborhoods...")
    neighborhoods_df = pd.read_csv(NEIGHBORHOODS_FILE)
    print(f"   Loaded {len(neighborhoods_df)} neighborhoods")

    # Extract POIs for each category
    print(f"\n2. Extracting POIs from OSM (within {RADIUS_M}m of neighborhoods)...")
    all_pois = {}

    for category_key, category_info in POI_CATEGORIES.items():
        pois = extract_category_pois(OSM_FILE, category_key, category_info, neighborhoods_df, RADIUS_M)
        all_pois[category_key] = pois

        # Save to CSV
        output_file = os.path.join(OUTPUT_DIR, f"{category_key}.csv")
        save_to_csv(pois, output_file)

    # Summary
    print("\n" + "=" * 70)
    print("EXTRACTION SUMMARY")
    print("=" * 70)
    print(f"Search radius: {RADIUS_M}m ({RADIUS_M/1000}km) around {len(neighborhoods_df)} neighborhoods")
    print(f"\nPOIs extracted by category:")

    total_pois = 0
    for category_key, pois in all_pois.items():
        count = len(pois)
        total_pois += count
        print(f"  {POI_CATEGORIES[category_key]['name']:35s}: {count:5d} POIs")

    print(f"\n  {'TOTAL':35s}: {total_pois:5d} POIs")

    # Show sample from each category
    print(f"\nSample POIs (first 3 from each category):")
    for category_key, pois in all_pois.items():
        print(f"\n  {POI_CATEGORIES[category_key]['name']}:")
        for poi in pois[:3]:
            print(f"    - {poi['name']:40s} ({poi['poi_type']})")

    print("\n" + "=" * 70)
    print("POI extraction complete!")
    print("=" * 70)
    print("\nFiles created:")
    for category_key in POI_CATEGORIES.keys():
        output_file = os.path.join(OUTPUT_DIR, f"{category_key}.csv")
        print(f"  - {output_file}")

if __name__ == "__main__":
    main()
