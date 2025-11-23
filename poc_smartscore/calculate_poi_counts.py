import pandas as pd
import os
from math import radians, cos, sin, asin, sqrt

# Domain definitions
DOMAINS = ['winkels', 'restaurants', 'groen', 'onderwijs', 'transport', 'sport', 'gezondheidszorg', 'cultuur']

DOMAIN_NAMES = {
    'winkels': 'Winkels (Shopping)',
    'restaurants': 'Restaurants & Cafes',
    'groen': 'Groen (Green Space)',
    'onderwijs': 'Onderwijs (Education)',
    'transport': 'Transport',
    'sport': 'Sport & Fitness',
    'gezondheidszorg': 'Gezondheidszorg (Healthcare)',
    'cultuur': 'Cultuur (Culture & Nightlife)'
}

def haversine(lon1, lat1, lon2, lat2):
    """
    Calculate the great circle distance in meters between two points
    on the earth (specified in decimal degrees)

    The haversine formula determines the great-circle distance between
    two points on a sphere given their longitudes and latitudes.

    Args:
        lon1, lat1: Longitude and latitude of point 1 (in decimal degrees)
        lon2, lat2: Longitude and latitude of point 2 (in decimal degrees)

    Returns:
        Distance in meters
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

def load_neighborhoods(file_path):
    """Load neighborhood data from CSV"""
    df = pd.read_csv(file_path)
    print(f"Loaded {len(df)} neighborhoods")
    return df

def load_pois(domain):
    """Load POI data for a specific domain"""
    file_path = f"data/pois/{domain}.csv"
    if not os.path.exists(file_path):
        print(f"Warning: {file_path} not found")
        return pd.DataFrame()

    df = pd.read_csv(file_path)
    return df

def count_pois_within_radius(neighborhood, pois_df, radius_m=1000):
    """
    Count POIs within a given radius of a neighborhood center point

    Args:
        neighborhood: Row from neighborhoods dataframe with lat/lon
        pois_df: DataFrame of POIs with latitude/longitude columns
        radius_m: Radius in meters (default 1000m = 1km)

    Returns:
        Count of POIs within radius
    """
    if pois_df.empty:
        return 0

    nbh_lat = neighborhood['latitude']
    nbh_lon = neighborhood['longitude']

    count = 0
    for _, poi in pois_df.iterrows():
        distance = haversine(nbh_lon, nbh_lat, poi['longitude'], poi['latitude'])
        if distance <= radius_m:
            count += 1

    return count

def calculate_all_counts(neighborhoods_df, radius_m=1000):
    """
    Calculate POI counts for all neighborhood × domain combinations

    Args:
        neighborhoods_df: DataFrame of neighborhoods
        radius_m: Radius in meters for counting

    Returns:
        DataFrame with columns: neighborhood_id, neighborhood_name, domain, count
    """
    results = []

    print(f"\nCalculating POI counts within {radius_m}m for each neighborhood...\n")

    for _, neighborhood in neighborhoods_df.iterrows():
        print(f"Processing: {neighborhood['name']} ({neighborhood['city']})")

        for domain in DOMAINS:
            # Load POIs for this domain
            pois_df = load_pois(domain)

            # Count POIs within radius
            count = count_pois_within_radius(neighborhood, pois_df, radius_m)

            # Store result
            results.append({
                'neighborhood_id': neighborhood['id'],
                'neighborhood_name': neighborhood['name'],
                'city': neighborhood['city'],
                'category': neighborhood['category'],
                'domain': domain,
                'domain_name': DOMAIN_NAMES[domain],
                'poi_count': count,
                'radius_m': radius_m
            })

            print(f"  {DOMAIN_NAMES[domain]}: {count} POIs")

        print()

    return pd.DataFrame(results)

def create_summary_table(counts_df):
    """
    Create a pivot table showing neighborhoods × domains

    Args:
        counts_df: DataFrame from calculate_all_counts

    Returns:
        Pivot table with neighborhoods as rows, domains as columns
    """
    pivot = counts_df.pivot_table(
        index=['neighborhood_id', 'neighborhood_name', 'city'],
        columns='domain',
        values='poi_count',
        aggfunc='sum'
    )

    # Add total column
    pivot['TOTAL'] = pivot.sum(axis=1)

    return pivot

def main():
    print("=" * 70)
    print("SmartScore POC - Calculate POIs Within Radius")
    print("=" * 70)

    # Configuration
    RADIUS_M = 1000  # 1km radius for scoring

    # Load neighborhoods
    print("\n1. Loading neighborhoods...")
    neighborhoods_df = load_neighborhoods("data/neighborhoods.csv")

    # Calculate counts for all combinations
    print(f"\n2. Calculating POI counts (radius: {RADIUS_M}m = {RADIUS_M/1000}km)...")
    counts_df = calculate_all_counts(neighborhoods_df, radius_m=RADIUS_M)

    # Save detailed results
    output_file = "results/poi_counts.csv"
    os.makedirs(os.path.dirname(output_file), exist_ok=True)
    counts_df.to_csv(output_file, index=False)
    print(f"\n3. Detailed results saved to: {output_file}")

    # Create and save summary table
    print("\n4. Creating summary table...")
    summary = create_summary_table(counts_df)
    summary_file = "results/poi_counts_summary.csv"
    summary.to_csv(summary_file)
    print(f"   Summary table saved to: {summary_file}")

    # Display summary
    print("\n" + "=" * 70)
    print("SUMMARY: POI Counts by Neighborhood")
    print("=" * 70)
    print(summary.to_string())

    # Calculate some statistics
    print("\n" + "=" * 70)
    print("STATISTICS")
    print("=" * 70)
    total_pois_in_scope = counts_df['poi_count'].sum()
    print(f"Total POIs within {RADIUS_M}m of all neighborhoods: {total_pois_in_scope:,}")
    print(f"Average POIs per neighborhood: {total_pois_in_scope / len(neighborhoods_df):.1f}")

    # Find neighborhoods with most/least POIs
    neighborhood_totals = counts_df.groupby('neighborhood_name')['poi_count'].sum().sort_values(ascending=False)
    print(f"\nMost POIs: {neighborhood_totals.index[0]} ({neighborhood_totals.iloc[0]:,} POIs)")
    print(f"Least POIs: {neighborhood_totals.index[-1]} ({neighborhood_totals.iloc[-1]:,} POIs)")

    # Find most common domain
    domain_totals = counts_df.groupby('domain_name')['poi_count'].sum().sort_values(ascending=False)
    print(f"\nMost common domain: {domain_totals.index[0]} ({domain_totals.iloc[0]:,} POIs)")

    print("\n" + "=" * 70)
    print("Calculation complete!")
    print("=" * 70)

if __name__ == "__main__":
    main()
