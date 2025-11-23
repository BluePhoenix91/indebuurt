import geopandas as gpd
import pandas as pd
from shapely.geometry import Point, LineString
from math import radians, cos, sin, asin, sqrt
import os

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

def calculate_line_length_meters(linestring):
    """
    Calculate approximate length of a LineString in meters using haversine

    Args:
        linestring: Shapely LineString with lon/lat coordinates

    Returns:
        Length in meters
    """
    coords = list(linestring.coords)
    total_length = 0

    for i in range(len(coords) - 1):
        lon1, lat1 = coords[i]
        lon2, lat2 = coords[i + 1]
        segment_length = haversine(lon1, lat1, lon2, lat2)
        total_length += segment_length

    return total_length

def generate_sample_points_for_street(street_geometry, street_data, sample_interval_m=500):
    """
    Generate sample points along a street LineString

    Logic:
    - Streets ≤500m: 1 point at midpoint
    - Streets >500m: points at 0m, 500m, 1000m, etc. along the line

    Args:
        street_geometry: Shapely LineString
        street_data: Dict with street metadata (osm_id, name, neighborhood_id, etc.)
        sample_interval_m: Distance between sample points in meters (default 500m)

    Returns:
        List of sample point dicts
    """
    samples = []

    # Calculate street length
    length_m = calculate_line_length_meters(street_geometry)

    if length_m <= sample_interval_m:
        # Short street: 1 point at midpoint (50% along the line)
        point = street_geometry.interpolate(0.5, normalized=True)

        samples.append({
            'latitude': point.y,
            'longitude': point.x,
            'street_osm_id': street_data['osm_id'],
            'street_name': street_data['name'],
            'highway_type': street_data['highway_type'],
            'neighborhood_id': street_data['neighborhood_id'],
            'neighborhood_name': street_data['neighborhood_name'],
            'city': street_data['city'],
            'street_length_m': length_m,
            'position_on_street': 'midpoint'
        })
    else:
        # Long street: multiple points at intervals
        # Calculate how many sample points we need
        num_points = int(length_m / sample_interval_m) + 1

        for i in range(num_points):
            # Calculate distance along the street for this sample
            distance_along = i * sample_interval_m

            # Stop if we exceed street length
            if distance_along > length_m:
                break

            # Interpolate point at this distance
            # We need to convert distance to normalized position (0-1)
            normalized_position = distance_along / length_m
            point = street_geometry.interpolate(normalized_position, normalized=True)

            samples.append({
                'latitude': point.y,
                'longitude': point.x,
                'street_osm_id': street_data['osm_id'],
                'street_name': street_data['name'],
                'highway_type': street_data['highway_type'],
                'neighborhood_id': street_data['neighborhood_id'],
                'neighborhood_name': street_data['neighborhood_name'],
                'city': street_data['city'],
                'street_length_m': length_m,
                'position_on_street': f'{distance_along:.0f}m'
            })

    return samples

def filter_points_within_radius(samples_df, neighborhoods_df, radius_m=1000):
    """
    Filter sample points to only keep those within radius of their neighborhood center

    Args:
        samples_df: DataFrame with sample points
        neighborhoods_df: DataFrame with neighborhood centers
        radius_m: Radius in meters

    Returns:
        Filtered DataFrame
    """
    print(f"\nFiltering sample points to keep only those within {radius_m}m of neighborhood centers...")

    filtered_samples = []
    initial_count = len(samples_df)

    for _, sample in samples_df.iterrows():
        # Find the neighborhood center for this sample
        neighborhood = neighborhoods_df[neighborhoods_df['id'] == sample['neighborhood_id']].iloc[0]

        # Calculate distance from sample to neighborhood center
        distance = haversine(
            sample['longitude'], sample['latitude'],
            neighborhood['longitude'], neighborhood['latitude']
        )

        # Keep sample if within radius
        if distance <= radius_m:
            filtered_samples.append(sample)

    filtered_df = pd.DataFrame(filtered_samples)
    removed_count = initial_count - len(filtered_df)

    print(f"  Kept {len(filtered_df):,} sample points (removed {removed_count:,} outside radius)")

    return filtered_df

def generate_sample_points(streets_gdf, neighborhoods_df, sample_interval_m=500, radius_m=1000):
    """
    Generate sample points for all streets

    Args:
        streets_gdf: GeoDataFrame with street geometries
        neighborhoods_df: DataFrame with neighborhood centers
        sample_interval_m: Distance between samples (default 500m)
        radius_m: Filter radius around neighborhood centers (default 1000m)

    Returns:
        DataFrame with sample points
    """
    print(f"\nGenerating sample points with {sample_interval_m}m intervals...")
    print(f"Processing {len(streets_gdf):,} streets...")

    all_samples = []
    processed = 0

    for idx, row in streets_gdf.iterrows():
        processed += 1

        if processed % 500 == 0:
            print(f"  Processed {processed:,}/{len(streets_gdf):,} streets, generated {len(all_samples):,} samples so far...")

        # Get street geometry
        geometry = row['geometry']

        # Skip if not a LineString
        if not isinstance(geometry, LineString):
            continue

        # Extract street data
        street_data = {
            'osm_id': row['osm_id'],
            'name': row['name'],
            'highway_type': row['highway_type'],
            'neighborhood_id': row['neighborhood_id'],
            'neighborhood_name': row['neighborhood_name'],
            'city': row['city']
        }

        # Generate sample points for this street
        samples = generate_sample_points_for_street(geometry, street_data, sample_interval_m)
        all_samples.extend(samples)

    print(f"  Total sample points generated: {len(all_samples):,}")

    # Convert to DataFrame
    samples_df = pd.DataFrame(all_samples)

    # Add sample_id
    samples_df.insert(0, 'sample_id', range(1, len(samples_df) + 1))

    # Filter to keep only points within radius
    samples_df = filter_points_within_radius(samples_df, neighborhoods_df, radius_m)

    # Re-index sample_id after filtering
    samples_df['sample_id'] = range(1, len(samples_df) + 1)

    return samples_df

def main():
    print("=" * 70)
    print("Street Sampling POC - Generate Sample Points Along Streets")
    print("=" * 70)

    # Configuration
    STREETS_GEOJSON = "data/streets/residential_streets.geojson"
    NEIGHBORHOODS_CSV = "data/neighborhoods.csv"
    OUTPUT_CSV = "data/samples/street_samples.csv"
    SAMPLE_INTERVAL_M = 500  # Sample every 500m
    RADIUS_M = 1000  # Keep only samples within 1km of neighborhood center

    # Load data
    print("\n1. Loading data...")
    print(f"   Reading streets from: {STREETS_GEOJSON}")
    streets_gdf = gpd.read_file(STREETS_GEOJSON)
    print(f"   Loaded {len(streets_gdf):,} street segments")

    print(f"   Reading neighborhoods from: {NEIGHBORHOODS_CSV}")
    neighborhoods_df = pd.read_csv(NEIGHBORHOODS_CSV)
    print(f"   Loaded {len(neighborhoods_df)} neighborhoods")

    # Generate sample points
    print("\n2. Generating sample points...")
    samples_df = generate_sample_points(
        streets_gdf,
        neighborhoods_df,
        sample_interval_m=SAMPLE_INTERVAL_M,
        radius_m=RADIUS_M
    )

    # Save results
    print("\n3. Saving results...")
    os.makedirs(os.path.dirname(OUTPUT_CSV), exist_ok=True)
    samples_df.to_csv(OUTPUT_CSV, index=False)
    print(f"   Saved to: {OUTPUT_CSV}")

    # Statistics
    print("\n" + "=" * 70)
    print("SAMPLING SUMMARY")
    print("=" * 70)
    print(f"Total sample points generated: {len(samples_df):,}")
    print(f"Sample interval: {SAMPLE_INTERVAL_M}m")
    print(f"Filter radius: {RADIUS_M}m")

    print(f"\nSample points per neighborhood:")
    for _, neighborhood in neighborhoods_df.iterrows():
        count = len(samples_df[samples_df['neighborhood_id'] == neighborhood['id']])
        print(f"  {neighborhood['name']:30s} ({neighborhood['city']:20s}): {count:4d} samples")

    print(f"\nTotal samples across all neighborhoods: {len(samples_df):,}")

    # Check if within expected range (20-200 per neighborhood, 200-2000 total)
    min_samples = samples_df.groupby('neighborhood_id').size().min()
    max_samples = samples_df.groupby('neighborhood_id').size().max()

    print(f"\nValidation:")
    print(f"  Min samples per neighborhood: {min_samples}")
    print(f"  Max samples per neighborhood: {max_samples}")
    print(f"  Expected range per neighborhood: 20-200")

    if min_samples >= 20 and max_samples <= 200:
        print(f"  ✓ All neighborhoods within expected range!")
    else:
        print(f"  ⚠ Some neighborhoods outside expected range")

    if 200 <= len(samples_df) <= 2000:
        print(f"  ✓ Total sample count within expected range (200-2000)")
    else:
        print(f"  ⚠ Total sample count outside expected range")

    # Show sample of data
    print(f"\nSample of generated points (first 5):")
    print(samples_df.head(5)[['sample_id', 'neighborhood_name', 'street_name', 'latitude', 'longitude', 'position_on_street']].to_string(index=False))

    # Show street length distribution
    print(f"\nStreet length statistics:")
    print(f"  Average street length: {samples_df['street_length_m'].mean():.0f}m")
    print(f"  Median street length: {samples_df['street_length_m'].median():.0f}m")
    print(f"  Shortest street: {samples_df['street_length_m'].min():.0f}m")
    print(f"  Longest street: {samples_df['street_length_m'].max():.0f}m")

    print("\n" + "=" * 70)
    print("Sample point generation complete!")
    print("=" * 70)
    print(f"\nOutput file: {OUTPUT_CSV}")

if __name__ == "__main__":
    main()
