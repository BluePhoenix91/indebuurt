import osmium
import pandas as pd
import json
import os
from math import radians, cos, sin, asin, sqrt

# Street types to extract (residential streets where people live)
STREET_TYPES = ['residential', 'tertiary', 'living_street']

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

def point_within_radius(point_lon, point_lat, center_lon, center_lat, radius_m):
    """Check if a point is within radius of center"""
    distance = haversine(point_lon, point_lat, center_lon, center_lat)
    return distance <= radius_m

def street_intersects_radius(node_coords, center_lon, center_lat, radius_m):
    """
    Check if any point of the street (way) is within the radius

    Args:
        node_coords: List of (lon, lat) tuples representing the street's nodes
        center_lon, center_lat: Center point of neighborhood
        radius_m: Radius in meters

    Returns:
        True if any node is within radius
    """
    for lon, lat in node_coords:
        if point_within_radius(lon, lat, center_lon, center_lat, radius_m):
            return True
    return False

class StreetExtractor(osmium.SimpleHandler):
    def __init__(self, neighborhoods_df, radius_m=1000):
        super().__init__()
        self.neighborhoods_df = neighborhoods_df
        self.radius_m = radius_m
        self.streets = []
        self.node_locations = {}  # Cache node locations
        self.ways_to_process = []  # Store ways for second pass
        self.processed_ways = 0

    def node(self, n):
        """First pass: collect all node locations"""
        self.node_locations[n.id] = (n.location.lon, n.location.lat)

    def way(self, w):
        """First pass: identify streets to process"""
        # Check if it's a street type we're interested in
        if 'highway' not in w.tags:
            return

        highway_type = w.tags['highway']
        if highway_type not in STREET_TYPES:
            return

        # Store way for second pass
        self.ways_to_process.append({
            'id': w.id,
            'tags': dict(w.tags),
            'nodes': [n.ref for n in w.nodes]
        })

        if len(self.ways_to_process) % 5000 == 0:
            print(f"  Found {len(self.ways_to_process)} potential streets...")

    def process_streets(self):
        """Second pass: filter streets by proximity to neighborhoods"""
        print(f"\nProcessing {len(self.ways_to_process)} streets...")

        for way_data in self.ways_to_process:
            self.processed_ways += 1

            if self.processed_ways % 5000 == 0:
                print(f"  Processed {self.processed_ways}/{len(self.ways_to_process)} streets, found {len(self.streets)} matches...")

            # Get coordinates for all nodes in this way
            node_coords = []
            for node_ref in way_data['nodes']:
                if node_ref in self.node_locations:
                    node_coords.append(self.node_locations[node_ref])

            # Skip if we couldn't resolve all nodes
            if len(node_coords) < 2:
                continue

            # Check if street intersects any neighborhood radius
            for _, neighborhood in self.neighborhoods_df.iterrows():
                if street_intersects_radius(
                    node_coords,
                    neighborhood['longitude'],
                    neighborhood['latitude'],
                    self.radius_m
                ):
                    # Street intersects this neighborhood - store it
                    street_name = way_data['tags'].get('name', 'Unnamed')

                    # Create GeoJSON LineString geometry
                    geometry = {
                        'type': 'LineString',
                        'coordinates': [[lon, lat] for lon, lat in node_coords]
                    }

                    self.streets.append({
                        'osm_id': way_data['id'],
                        'name': street_name,
                        'highway_type': way_data['tags']['highway'],
                        'neighborhood_id': neighborhood['id'],
                        'neighborhood_name': neighborhood['name'],
                        'city': neighborhood['city'],
                        'geometry': json.dumps(geometry)
                    })

                    # A street can intersect multiple neighborhoods, so we don't break here

        print(f"  Completed: {len(self.streets)} street segments found across all neighborhoods")

def extract_streets(osm_file, neighborhoods_df, radius_m=1000):
    """
    Extract street geometries near neighborhoods

    Args:
        osm_file: Path to OSM PBF file
        neighborhoods_df: DataFrame with neighborhood centers
        radius_m: Radius in meters to search around each neighborhood

    Returns:
        DataFrame with street data
    """
    print(f"\nExtracting streets from: {osm_file}")
    print(f"Target street types: {', '.join(STREET_TYPES)}")
    print(f"Radius: {radius_m}m ({radius_m/1000}km)")
    print(f"Neighborhoods: {len(neighborhoods_df)}")
    print("=" * 70)

    handler = StreetExtractor(neighborhoods_df, radius_m)

    # First pass: collect nodes and identify ways
    print("\nPass 1: Reading OSM data...")
    handler.apply_file(osm_file)

    # Second pass: filter streets by proximity
    print("\nPass 2: Filtering streets by proximity to neighborhoods...")
    handler.process_streets()

    return pd.DataFrame(handler.streets)

def save_to_geojson(streets_df, output_file):
    """
    Save streets as GeoJSON FeatureCollection
    """
    features = []

    for _, street in streets_df.iterrows():
        geometry = json.loads(street['geometry'])

        feature = {
            'type': 'Feature',
            'properties': {
                'osm_id': int(street['osm_id']),
                'name': street['name'],
                'highway_type': street['highway_type'],
                'neighborhood_id': int(street['neighborhood_id']),
                'neighborhood_name': street['neighborhood_name'],
                'city': street['city']
            },
            'geometry': geometry
        }
        features.append(feature)

    geojson = {
        'type': 'FeatureCollection',
        'features': features
    }

    os.makedirs(os.path.dirname(output_file), exist_ok=True)

    with open(output_file, 'w', encoding='utf-8') as f:
        json.dump(geojson, f, ensure_ascii=False, indent=2)

    print(f"\nSaved GeoJSON to: {output_file}")

def save_summary_csv(streets_df, output_file):
    """
    Save summary statistics as CSV (without full geometry)
    """
    summary_df = streets_df[['osm_id', 'name', 'highway_type', 'neighborhood_id', 'neighborhood_name', 'city']].copy()
    summary_df.to_csv(output_file, index=False)
    print(f"Saved summary CSV to: {output_file}")

def main():
    print("=" * 70)
    print("Street Sampling POC - Extract Street Network from OSM")
    print("=" * 70)

    # Configuration
    OSM_FILE = "../poc_smartscore/data/belgium-latest.osm.pbf"  # Reuse from poc_smartscore
    NEIGHBORHOODS_FILE = "data/neighborhoods.csv"
    OUTPUT_GEOJSON = "data/streets/residential_streets.geojson"
    OUTPUT_CSV = "data/streets/residential_streets_summary.csv"
    RADIUS_M = 1000  # 1km radius

    # Load neighborhoods
    print("\n1. Loading neighborhoods...")
    neighborhoods_df = pd.read_csv(NEIGHBORHOODS_FILE)
    print(f"   Loaded {len(neighborhoods_df)} neighborhoods")

    # Extract streets
    print("\n2. Extracting streets from OSM...")
    streets_df = extract_streets(OSM_FILE, neighborhoods_df, radius_m=RADIUS_M)

    # Save results
    print("\n3. Saving results...")
    save_to_geojson(streets_df, OUTPUT_GEOJSON)
    save_summary_csv(streets_df, OUTPUT_CSV)

    # Statistics
    print("\n" + "=" * 70)
    print("EXTRACTION SUMMARY")
    print("=" * 70)
    print(f"Total street segments extracted: {len(streets_df):,}")
    print(f"\nStreets per neighborhood:")

    for _, neighborhood in neighborhoods_df.iterrows():
        count = len(streets_df[streets_df['neighborhood_id'] == neighborhood['id']])
        print(f"  {neighborhood['name']:30s} ({neighborhood['city']:20s}): {count:4d} streets")

    print(f"\nStreet types breakdown:")
    for highway_type in STREET_TYPES:
        count = len(streets_df[streets_df['highway_type'] == highway_type])
        pct = (count / len(streets_df) * 100) if len(streets_df) > 0 else 0
        print(f"  {highway_type:20s}: {count:5d} ({pct:5.1f}%)")

    # Spot check: show some named streets
    print(f"\nSample of named streets (first 10):")
    named_streets = streets_df[streets_df['name'] != 'Unnamed'].head(10)
    for _, street in named_streets.iterrows():
        print(f"  {street['name']:40s} ({street['city']}, {street['highway_type']})")

    print("\n" + "=" * 70)
    print("Street extraction complete!")
    print("=" * 70)
    print("\nFiles created:")
    print(f"  - {OUTPUT_GEOJSON} (full geometry for mapping)")
    print(f"  - {OUTPUT_CSV} (summary for inspection)")

if __name__ == "__main__":
    main()
