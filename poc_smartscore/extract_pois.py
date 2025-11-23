import osmium
import csv
import os

# Define domain filters
DOMAINS = {
    'winkels': {
        'name': 'Winkels (Shopping)',
        'filters': [
            ('shop', ['supermarket', 'convenience', 'bakery', 'butcher'])
        ]
    },
    'restaurants': {
        'name': 'Restaurants & Cafes',
        'filters': [
            ('amenity', ['restaurant', 'cafe', 'bar', 'fast_food'])
        ]
    },
    'groen': {
        'name': 'Groen (Green Space)',
        'filters': [
            ('leisure', ['park', 'garden', 'dog_park']),
            ('natural', ['wood'])
        ]
    },
    'onderwijs': {
        'name': 'Onderwijs (Education)',
        'filters': [
            ('amenity', ['school', 'kindergarten', 'library'])
        ]
    },
    'transport': {
        'name': 'Transport',
        'filters': [
            ('public_transport', ['stop_position']),
            ('railway', ['station'])
        ]
    },
    'sport': {
        'name': 'Sport & Fitness',
        'filters': [
            ('leisure', ['fitness_centre', 'sports_centre', 'playground'])
        ]
    },
    'gezondheidszorg': {
        'name': 'Gezondheidszorg (Healthcare)',
        'filters': [
            ('amenity', ['pharmacy', 'doctors', 'hospital', 'dentist'])
        ]
    },
    'cultuur': {
        'name': 'Cultuur & Uitgaan (Culture & Nightlife)',
        'filters': [
            ('amenity', ['cinema', 'theatre', 'nightclub']),
            ('tourism', ['museum'])
        ]
    }
}

class POIExtractor(osmium.SimpleHandler):
    def __init__(self, domain_key, filters):
        super().__init__()
        self.domain_key = domain_key
        self.filters = filters
        self.pois = []
        self.processed = 0

    def matches_filter(self, tags):
        """Check if tags match any of the domain filters"""
        for key, values in self.filters:
            if key in tags and tags[key] in values:
                return True
        return False

    def extract_poi(self, obj):
        """Extract POI information from OSM object"""
        if self.matches_filter(obj.tags):
            # Get location (use centroid for ways)
            if hasattr(obj, 'location'):
                lat = obj.location.lat
                lon = obj.location.lon
            else:
                # For ways/areas, we'll skip them for now (would need centroid calculation)
                return

            # Extract relevant tags
            name = obj.tags.get('name', 'Unnamed')
            osm_type = 'node'
            osm_id = obj.id

            # Collect all relevant tags
            tags_str = ';'.join([f"{k}={v}" for k, v in obj.tags])

            self.pois.append({
                'osm_type': osm_type,
                'osm_id': osm_id,
                'name': name,
                'latitude': lat,
                'longitude': lon,
                'tags': tags_str
            })

            if len(self.pois) % 1000 == 0:
                print(f"  {self.domain_key}: Found {len(self.pois)} POIs...")

        self.processed += 1
        if self.processed % 1000000 == 0:
            print(f"  {self.domain_key}: Processed {self.processed:,} objects...")

    def node(self, n):
        self.extract_poi(n)

def extract_domain_pois(osm_file, domain_key, domain_info):
    """Extract POIs for a specific domain"""
    print(f"\nExtracting {domain_info['name']}...")

    handler = POIExtractor(domain_key, domain_info['filters'])
    handler.apply_file(osm_file)

    print(f"  Found {len(handler.pois)} POIs")

    return handler.pois

def save_to_csv(pois, output_file):
    """Save POIs to CSV file"""
    os.makedirs(os.path.dirname(output_file), exist_ok=True)

    with open(output_file, 'w', newline='', encoding='utf-8') as f:
        if pois:
            fieldnames = ['osm_type', 'osm_id', 'name', 'latitude', 'longitude', 'tags']
            writer = csv.DictWriter(f, fieldnames=fieldnames)
            writer.writeheader()
            writer.writerows(pois)

    print(f"  Saved to {output_file}")

def main():
    osm_file = "data/belgium-latest.osm.pbf"
    output_dir = "data/pois"

    print(f"Processing OSM file: {osm_file}")
    print(f"Output directory: {output_dir}")
    print("=" * 60)

    # Extract POIs for each domain
    for domain_key, domain_info in DOMAINS.items():
        pois = extract_domain_pois(osm_file, domain_key, domain_info)
        output_file = os.path.join(output_dir, f"{domain_key}.csv")
        save_to_csv(pois, output_file)

    print("\n" + "=" * 60)
    print("Extraction complete!")
    print("\nSummary:")
    for domain_key in DOMAINS.keys():
        csv_file = os.path.join(output_dir, f"{domain_key}.csv")
        if os.path.exists(csv_file):
            with open(csv_file, 'r', encoding='utf-8') as f:
                line_count = sum(1 for line in f) - 1  # Subtract header
                print(f"  {domain_key}.csv: {line_count:,} POIs")

if __name__ == "__main__":
    main()
