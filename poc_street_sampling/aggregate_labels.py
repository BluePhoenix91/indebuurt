import pandas as pd
import os

# Label thresholds (from POC specification)
LABEL_THRESHOLDS = {
    'supermarkets': {
        'category_name': 'Groceries',
        'threshold_m': 1000,
        'label_pass': 'Groceries within walking distance',
        'label_fail': 'Limited grocery access'
    },
    'pt_stops': {
        'category_name': 'Public Transport',
        'threshold_m': 400,
        'label_pass': 'Excellent PT access',
        'label_fail': 'Limited PT access'
    },
    'green_spaces': {
        'category_name': 'Parks & Green Spaces',
        'threshold_m': 1000,
        'label_pass': 'Park within walking distance',
        'label_fail': 'Limited green access'
    }
}

def calculate_median_distances(distances_df):
    """
    Calculate median distance per neighborhood × category

    Args:
        distances_df: DataFrame with distance records per sample point

    Returns:
        DataFrame with median distances per neighborhood × category
    """
    print("\nCalculating median distances per neighborhood × category...")

    # Group by neighborhood and category, calculate median
    medians = distances_df.groupby(['neighborhood_name', 'category'])['distance_m'].median().reset_index()
    medians.rename(columns={'distance_m': 'median_distance_m'}, inplace=True)

    print(f"  Calculated medians for {len(medians)} neighborhood × category combinations")

    return medians

def assign_labels(medians_df):
    """
    Assign human-readable labels based on median distances and thresholds

    Args:
        medians_df: DataFrame with median distances

    Returns:
        DataFrame with labels assigned
    """
    print("\nAssigning labels based on thresholds...")

    results = []

    for _, row in medians_df.iterrows():
        neighborhood = row['neighborhood_name']
        category = row['category']
        median_distance = row['median_distance_m']

        # Get threshold info for this category
        threshold_info = LABEL_THRESHOLDS[category]
        threshold = threshold_info['threshold_m']

        # Assign label based on threshold
        if median_distance <= threshold:
            label = threshold_info['label_pass']
            meets_threshold = True
        else:
            label = threshold_info['label_fail']
            meets_threshold = False

        results.append({
            'neighborhood_name': neighborhood,
            'category': category,
            'category_name': threshold_info['category_name'],
            'median_distance_m': median_distance,
            'threshold_m': threshold,
            'label': label,
            'meets_threshold': meets_threshold
        })

    labels_df = pd.DataFrame(results)
    print(f"  Assigned labels for {len(labels_df)} neighborhood × category combinations")

    return labels_df

def create_summary_table(labels_df, neighborhoods_df):
    """
    Create a summary table with one row per neighborhood showing all labels

    Args:
        labels_df: DataFrame with labels per neighborhood × category
        neighborhoods_df: DataFrame with neighborhood info

    Returns:
        DataFrame with summary table (one row per neighborhood)
    """
    print("\nCreating summary table...")

    summary_rows = []

    for _, neighborhood in neighborhoods_df.iterrows():
        nbh_name = neighborhood['name']
        nbh_labels = labels_df[labels_df['neighborhood_name'] == nbh_name]

        if nbh_labels.empty:
            continue

        # Extract labels for each category
        groceries = nbh_labels[nbh_labels['category'] == 'supermarkets'].iloc[0]
        pt = nbh_labels[nbh_labels['category'] == 'pt_stops'].iloc[0]
        parks = nbh_labels[nbh_labels['category'] == 'green_spaces'].iloc[0]

        summary_rows.append({
            'neighborhood_name': nbh_name,
            'city': neighborhood['city'],
            'category': neighborhood['category'],
            'groceries_label': groceries['label'],
            'groceries_median_m': groceries['median_distance_m'],
            'groceries_meets_threshold': groceries['meets_threshold'],
            'pt_label': pt['label'],
            'pt_median_m': pt['median_distance_m'],
            'pt_meets_threshold': pt['meets_threshold'],
            'parks_label': parks['label'],
            'parks_median_m': parks['median_distance_m'],
            'parks_meets_threshold': parks['meets_threshold']
        })

    summary_df = pd.DataFrame(summary_rows)
    print(f"  Created summary for {len(summary_df)} neighborhoods")

    return summary_df

def main():
    print("=" * 70)
    print("Street Sampling POC - Aggregate to Neighborhood-Level Labels")
    print("=" * 70)

    # Configuration
    DISTANCES_FILE = "results/distances_per_sample.csv"
    NEIGHBORHOODS_FILE = "data/neighborhoods.csv"
    OUTPUT_LABELS_FILE = "results/neighborhood_labels.csv"
    OUTPUT_SUMMARY_FILE = "results/neighborhood_labels_summary.csv"

    # Load data
    print("\n1. Loading data...")
    distances_df = pd.read_csv(DISTANCES_FILE)
    print(f"   Loaded {len(distances_df):,} distance records")

    neighborhoods_df = pd.read_csv(NEIGHBORHOODS_FILE)
    print(f"   Loaded {len(neighborhoods_df)} neighborhoods")

    # Calculate medians
    print("\n2. Calculating median distances...")
    medians_df = calculate_median_distances(distances_df)

    # Assign labels
    print("\n3. Assigning labels based on thresholds...")
    print("\n   Thresholds:")
    for category, info in LABEL_THRESHOLDS.items():
        print(f"   - {info['category_name']}: ≤{info['threshold_m']}m → '{info['label_pass']}'")
        print(f"     {'':23s} >{info['threshold_m']}m → '{info['label_fail']}'")

    labels_df = assign_labels(medians_df)

    # Create summary table
    print("\n4. Creating summary table...")
    summary_df = create_summary_table(labels_df, neighborhoods_df)

    # Save results
    print("\n5. Saving results...")
    os.makedirs(os.path.dirname(OUTPUT_LABELS_FILE), exist_ok=True)

    labels_df.to_csv(OUTPUT_LABELS_FILE, index=False)
    print(f"   Detailed labels saved to: {OUTPUT_LABELS_FILE}")

    summary_df.to_csv(OUTPUT_SUMMARY_FILE, index=False)
    print(f"   Summary table saved to: {OUTPUT_SUMMARY_FILE}")

    # Display results
    print("\n" + "=" * 70)
    print("NEIGHBORHOOD LABELS SUMMARY")
    print("=" * 70)

    # Sort by urban/suburban for better readability
    summary_display = summary_df.copy()

    for _, row in summary_display.iterrows():
        print(f"\n{row['neighborhood_name']} ({row['city']}) - {row['category']}")
        print(f"  {'Groceries:':20s} {row['groceries_label']:40s} (median: {row['groceries_median_m']:.0f}m)")
        print(f"  {'Public Transport:':20s} {row['pt_label']:40s} (median: {row['pt_median_m']:.0f}m)")
        print(f"  {'Parks:':20s} {row['parks_label']:40s} (median: {row['parks_median_m']:.0f}m)")

    # Statistics
    print("\n" + "=" * 70)
    print("LABEL DISTRIBUTION")
    print("=" * 70)

    for category, info in LABEL_THRESHOLDS.items():
        cat_labels = labels_df[labels_df['category'] == category]
        pass_count = cat_labels['meets_threshold'].sum()
        fail_count = len(cat_labels) - pass_count

        print(f"\n{info['category_name']}:")
        print(f"  '{info['label_pass']}': {pass_count}/{len(cat_labels)} neighborhoods")
        print(f"  '{info['label_fail']}': {fail_count}/{len(cat_labels)} neighborhoods")

    # Validation against expected profiles
    print("\n" + "=" * 70)
    print("VALIDATION: Labels vs Expected Profiles")
    print("=" * 70)
    print("\nExpected patterns:")
    print("  Urban centers (Korenmarkt, Meir, Oude Markt):")
    print("    → Should have: Good groceries, Excellent PT")
    print("  Suburbs (Sint-Martens-Latem):")
    print("    → Should have: Limited groceries, Limited PT")

    print("\nActual results for key neighborhoods:")
    key_neighborhoods = ['Korenmarkt/Veldstraat', 'Meir', 'Oude Markt', 'Sint-Martens-Latem']
    for nbh in key_neighborhoods:
        row = summary_df[summary_df['neighborhood_name'] == nbh]
        if not row.empty:
            row = row.iloc[0]
            print(f"\n  {nbh}:")
            print(f"    Groceries: {row['groceries_label']} ({row['groceries_median_m']:.0f}m)")
            print(f"    PT: {row['pt_label']} ({row['pt_median_m']:.0f}m)")
            print(f"    Parks: {row['parks_label']} ({row['parks_median_m']:.0f}m)")

    print("\n" + "=" * 70)
    print("Aggregation complete!")
    print("=" * 70)
    print(f"\nOutput files:")
    print(f"  - {OUTPUT_LABELS_FILE} (detailed)")
    print(f"  - {OUTPUT_SUMMARY_FILE} (summary table)")

if __name__ == "__main__":
    main()
