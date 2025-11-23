import pandas as pd
import os
from math import radians, cos, sin, asin, sqrt

# POI categories
POI_CATEGORIES = {
    'supermarkets': 'Groceries (Supermarkets)',
    'pt_stops': 'Public Transport',
    'green_spaces': 'Parks & Green Spaces'
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

def find_nearest_poi(sample_lat, sample_lon, pois_df):
    """
    Find the nearest POI to a sample point

    Args:
        sample_lat, sample_lon: Sample point coordinates
        pois_df: DataFrame with POIs (must have latitude, longitude columns)

    Returns:
        Tuple of (nearest_poi_row, distance_m) or (None, None) if no POIs
    """
    if pois_df.empty:
        return None, None

    min_distance = float('inf')
    nearest_poi = None

    for _, poi in pois_df.iterrows():
        distance = haversine(sample_lon, sample_lat, poi['longitude'], poi['latitude'])

        if distance < min_distance:
            min_distance = distance
            nearest_poi = poi

    return nearest_poi, min_distance

def calculate_all_distances(samples_df, pois_dict):
    """
    Calculate nearest POI distance for all sample points across all categories

    Args:
        samples_df: DataFrame with sample points
        pois_dict: Dictionary of {category_key: pois_df}

    Returns:
        DataFrame with distance results
    """
    print(f"\nCalculating distances for {len(samples_df):,} sample points × {len(pois_dict)} categories...")
    print(f"Total distance calculations: {len(samples_df) * len(pois_dict):,}")

    results = []
    processed = 0

    for _, sample in samples_df.iterrows():
        processed += 1

        # Progress indicator
        if processed % 500 == 0:
            print(f"  Processed {processed:,}/{len(samples_df):,} sample points ({processed/len(samples_df)*100:.1f}%)...")

        sample_lat = sample['latitude']
        sample_lon = sample['longitude']
        sample_id = sample['sample_id']

        # Calculate distance to nearest POI in each category
        for category_key, pois_df in pois_dict.items():
            nearest_poi, distance = find_nearest_poi(sample_lat, sample_lon, pois_df)

            if nearest_poi is not None:
                results.append({
                    'sample_id': sample_id,
                    'neighborhood_name': sample['neighborhood_name'],
                    'street_name': sample['street_name'],
                    'sample_latitude': sample_lat,
                    'sample_longitude': sample_lon,
                    'category': category_key,
                    'category_name': POI_CATEGORIES[category_key],
                    'nearest_poi_id': nearest_poi['osm_id'],
                    'nearest_poi_name': nearest_poi['name'],
                    'nearest_poi_type': nearest_poi['poi_type'],
                    'distance_m': distance
                })
            else:
                # No POIs found in this category (shouldn't happen with our data)
                results.append({
                    'sample_id': sample_id,
                    'neighborhood_name': sample['neighborhood_name'],
                    'street_name': sample['street_name'],
                    'sample_latitude': sample_lat,
                    'sample_longitude': sample_lon,
                    'category': category_key,
                    'category_name': POI_CATEGORIES[category_key],
                    'nearest_poi_id': None,
                    'nearest_poi_name': 'No POI found',
                    'nearest_poi_type': None,
                    'distance_m': None
                })

    print(f"  Completed: {processed:,} sample points processed")

    return pd.DataFrame(results)

def main():
    print("=" * 70)
    print("Street Sampling POC - Calculate Distances to Nearest POIs")
    print("=" * 70)

    # Configuration
    SAMPLES_FILE = "data/samples/street_samples.csv"
    POI_FILES = {
        'supermarkets': 'data/pois/supermarkets.csv',
        'pt_stops': 'data/pois/pt_stops.csv',
        'green_spaces': 'data/pois/green_spaces.csv'
    }
    OUTPUT_FILE = "results/distances_per_sample.csv"

    # Load sample points
    print("\n1. Loading sample points...")
    samples_df = pd.read_csv(SAMPLES_FILE)
    print(f"   Loaded {len(samples_df):,} sample points")

    # Load POIs
    print("\n2. Loading POIs...")
    pois_dict = {}
    total_pois = 0
    for category_key, file_path in POI_FILES.items():
        pois_df = pd.read_csv(file_path)
        pois_dict[category_key] = pois_df
        total_pois += len(pois_df)
        print(f"   - {POI_CATEGORIES[category_key]}: {len(pois_df)} POIs")
    print(f"   Total POIs: {total_pois}")

    # Calculate distances
    print("\n3. Calculating distances...")
    distances_df = calculate_all_distances(samples_df, pois_dict)

    # Save results
    print("\n4. Saving results...")
    os.makedirs(os.path.dirname(OUTPUT_FILE), exist_ok=True)
    distances_df.to_csv(OUTPUT_FILE, index=False)
    print(f"   Saved to: {OUTPUT_FILE}")

    # Statistics
    print("\n" + "=" * 70)
    print("DISTANCE CALCULATION SUMMARY")
    print("=" * 70)
    print(f"Total distance records: {len(distances_df):,}")
    print(f"Sample points processed: {len(samples_df):,}")
    print(f"Categories: {len(pois_dict)}")
    print(f"Expected records: {len(samples_df) * len(pois_dict):,}")

    # Check for missing data
    missing = distances_df['distance_m'].isna().sum()
    if missing > 0:
        print(f"\n⚠ Warning: {missing} records have no distance (no POI found)")
        print("Categories with missing POIs:")
        for category_key in pois_dict.keys():
            cat_missing = distances_df[(distances_df['category'] == category_key) & (distances_df['distance_m'].isna())].shape[0]
            if cat_missing > 0:
                print(f"  - {POI_CATEGORIES[category_key]}: {cat_missing} samples")
    else:
        print(f"\n✓ All sample points have distances to nearest POIs in all categories")

    # Distance statistics per category
    print(f"\nDistance statistics per category:")
    for category_key, category_name in POI_CATEGORIES.items():
        cat_data = distances_df[distances_df['category'] == category_key]['distance_m']
        if cat_data.notna().any():
            print(f"\n  {category_name}:")
            print(f"    Min distance: {cat_data.min():.0f}m")
            print(f"    Max distance: {cat_data.max():.0f}m")
            print(f"    Mean distance: {cat_data.mean():.0f}m")
            print(f"    Median distance: {cat_data.median():.0f}m")

    # Show sample results
    print(f"\nSample distance records (first 10):")
    sample_cols = ['sample_id', 'neighborhood_name', 'category_name', 'nearest_poi_name', 'distance_m']
    print(distances_df[sample_cols].head(10).to_string(index=False))

    # Validation checks
    print(f"\n" + "=" * 70)
    print("VALIDATION CHECKS")
    print("=" * 70)

    # Check for negative distances
    negative = (distances_df['distance_m'] < 0).sum()
    if negative > 0:
        print(f"❌ Found {negative} negative distances!")
    else:
        print(f"✓ No negative distances")

    # Check for unreasonably large distances (>10km for urban areas)
    very_large = (distances_df['distance_m'] > 10000).sum()
    if very_large > 0:
        print(f"⚠ Found {very_large} distances > 10km (may indicate sparse POI coverage)")
        # Show which categories
        for category_key in pois_dict.keys():
            cat_large = distances_df[(distances_df['category'] == category_key) & (distances_df['distance_m'] > 10000)].shape[0]
            if cat_large > 0:
                print(f"  - {POI_CATEGORIES[category_key]}: {cat_large} samples > 10km")
    else:
        print(f"✓ All distances ≤ 10km")

    # Check that all samples have 3 entries
    samples_per_point = distances_df.groupby('sample_id').size()
    incomplete = (samples_per_point != len(pois_dict)).sum()
    if incomplete > 0:
        print(f"❌ {incomplete} sample points don't have {len(pois_dict)} category entries")
    else:
        print(f"✓ All {len(samples_df):,} sample points have {len(pois_dict)} category entries")

    print("\n" + "=" * 70)
    print("Distance calculation complete!")
    print("=" * 70)
    print(f"\nOutput file: {OUTPUT_FILE}")
    print(f"Ready for Story 6: Aggregate to neighborhood-level labels")

if __name__ == "__main__":
    main()
