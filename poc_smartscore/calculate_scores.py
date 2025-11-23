import pandas as pd
import numpy as np
import os

# Domain weights (from POC specification)
DOMAIN_WEIGHTS = {
    'winkels': 0.16,              # 16% - Daily necessity access
    'restaurants': 0.11,          # 11% - Social life, dining options
    'groen': 0.16,                # 16% - Health, recreation, family appeal
    'onderwijs': 0.13,            # 13% - Family-friendly indicator
    'transport': 0.14,            # 14% - Mobility, connectivity
    'sport': 0.10,                # 10% - Active lifestyle
    'gezondheidszorg': 0.11,      # 11% - Essential services
    'cultuur': 0.09               # 9% - Entertainment, vibrancy
}

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

def min_max_normalize(value, min_val, max_val):
    """
    Min-Max normalization to 0-10 scale

    Formula: (value - min) / (max - min) * 10

    Args:
        value: The value to normalize
        min_val: Minimum value in the dataset
        max_val: Maximum value in the dataset

    Returns:
        Normalized score from 0-10
    """
    if max_val == min_val:
        # All values are the same - return middle score
        return 5.0

    normalized = (value - min_val) / (max_val - min_val) * 10
    return normalized

def log_normalize(value, max_val):
    """
    Logarithmic normalization to 0-10 scale

    Formula: log(value + 1) / log(max + 1) * 10

    This models diminishing returns: the difference between 5 and 10 POIs
    matters more than between 50 and 55 POIs.

    Args:
        value: The value to normalize
        max_val: Maximum value in the dataset

    Returns:
        Normalized score from 0-10
    """
    if max_val == 0:
        return 0.0

    # Add 1 to avoid log(0)
    normalized = np.log(value + 1) / np.log(max_val + 1) * 10
    return normalized

def calculate_domain_scores(counts_df, normalization='minmax'):
    """
    Calculate normalized domain scores for all neighborhoods

    Args:
        counts_df: DataFrame with poi_count column
        normalization: 'minmax' or 'log'

    Returns:
        DataFrame with domain_score column added
    """
    df = counts_df.copy()

    # Calculate scores per domain
    for domain in DOMAIN_WEIGHTS.keys():
        domain_mask = df['domain'] == domain
        domain_counts = df.loc[domain_mask, 'poi_count']

        if normalization == 'minmax':
            min_val = domain_counts.min()
            max_val = domain_counts.max()
            df.loc[domain_mask, 'domain_score'] = domain_counts.apply(
                lambda x: min_max_normalize(x, min_val, max_val)
            )
        elif normalization == 'log':
            max_val = domain_counts.max()
            df.loc[domain_mask, 'domain_score'] = domain_counts.apply(
                lambda x: log_normalize(x, max_val)
            )
        else:
            raise ValueError(f"Unknown normalization: {normalization}")

    return df

def calculate_smartscore(scores_df):
    """
    Calculate overall SmartScore by applying domain weights

    Formula: SmartScore = Σ(domain_score × domain_weight)

    Args:
        scores_df: DataFrame with domain_score column

    Returns:
        DataFrame with weighted_score column and SmartScore per neighborhood
    """
    df = scores_df.copy()

    # Add weighted score column
    df['domain_weight'] = df['domain'].map(DOMAIN_WEIGHTS)
    df['weighted_score'] = df['domain_score'] * df['domain_weight']

    # Calculate SmartScore per neighborhood (sum of weighted scores)
    smartscores = df.groupby('neighborhood_id')['weighted_score'].sum().reset_index()
    smartscores.rename(columns={'weighted_score': 'smartscore'}, inplace=True)

    # Merge back to main dataframe
    df = df.merge(smartscores, on='neighborhood_id', how='left')

    return df

def create_comparison_table(minmax_df, log_df):
    """
    Create side-by-side comparison of both normalization methods

    Args:
        minmax_df: Scored DataFrame using min-max normalization
        log_df: Scored DataFrame using logarithmic normalization

    Returns:
        DataFrame comparing both methods
    """
    # Extract SmartScores
    minmax_scores = minmax_df.groupby(['neighborhood_id', 'neighborhood_name', 'city']).first()[['smartscore']].reset_index()
    minmax_scores.rename(columns={'smartscore': 'smartscore_minmax'}, inplace=True)

    log_scores = log_df.groupby(['neighborhood_id', 'neighborhood_name', 'city']).first()[['smartscore']].reset_index()
    log_scores.rename(columns={'smartscore': 'smartscore_log'}, inplace=True)

    # Merge
    comparison = minmax_scores.merge(log_scores, on=['neighborhood_id', 'neighborhood_name', 'city'])

    # Calculate difference
    comparison['difference'] = comparison['smartscore_log'] - comparison['smartscore_minmax']

    # Sort by min-max score
    comparison = comparison.sort_values('smartscore_minmax', ascending=False)

    return comparison

def main():
    print("=" * 80)
    print("SmartScore POC - Calculate Normalized Scores with Dual Normalization")
    print("=" * 80)

    # Load POI counts
    print("\n1. Loading POI counts...")
    counts_df = pd.read_csv("results/poi_counts.csv")
    print(f"   Loaded {len(counts_df)} neighborhood × domain combinations")

    # Verify weights sum to 100%
    total_weight = sum(DOMAIN_WEIGHTS.values())
    print(f"\n2. Domain weights sum to: {total_weight:.2%} (should be 100%)")
    assert abs(total_weight - 1.0) < 0.001, "Weights must sum to 1.0!"

    # Calculate scores with Min-Max normalization
    print("\n3. Calculating domain scores with MIN-MAX normalization...")
    minmax_df = calculate_domain_scores(counts_df, normalization='minmax')
    minmax_df = calculate_smartscore(minmax_df)
    print("   Min-Max scoring complete")

    # Calculate scores with Logarithmic normalization
    print("\n4. Calculating domain scores with LOGARITHMIC normalization...")
    log_df = calculate_domain_scores(counts_df, normalization='log')
    log_df = calculate_smartscore(log_df)
    print("   Logarithmic scoring complete")

    # Save detailed results
    print("\n5. Saving detailed results...")
    minmax_df.to_csv("results/scores_minmax.csv", index=False)
    print("   Saved: results/scores_minmax.csv")

    log_df.to_csv("results/scores_log.csv", index=False)
    print("   Saved: results/scores_log.csv")

    # Create comparison table
    print("\n6. Creating comparison table...")
    comparison = create_comparison_table(minmax_df, log_df)
    comparison.to_csv("results/scores_comparison.csv", index=False)
    print("   Saved: results/scores_comparison.csv")

    # Display comparison
    print("\n" + "=" * 80)
    print("SMARTSCORE COMPARISON: Min-Max vs Logarithmic Normalization")
    print("=" * 80)
    print(comparison.to_string(index=False))

    # Display statistics
    print("\n" + "=" * 80)
    print("STATISTICS")
    print("=" * 80)
    print(f"Min-Max scores range: {comparison['smartscore_minmax'].min():.2f} - {comparison['smartscore_minmax'].max():.2f}")
    print(f"Log scores range: {comparison['smartscore_log'].min():.2f} - {comparison['smartscore_log'].max():.2f}")
    print(f"Average difference: {comparison['difference'].mean():.2f} points")
    print(f"Max difference: {comparison['difference'].max():.2f} points")

    # Show domain breakdown for top neighborhood
    print("\n" + "=" * 80)
    print("EXAMPLE: Domain Breakdown for Top Neighborhood (Min-Max)")
    print("=" * 80)
    top_neighborhood = comparison.iloc[0]['neighborhood_name']
    top_id = comparison.iloc[0]['neighborhood_id']

    breakdown = minmax_df[minmax_df['neighborhood_id'] == top_id][
        ['domain_name', 'poi_count', 'domain_score', 'domain_weight', 'weighted_score']
    ].copy()
    breakdown = breakdown.sort_values('weighted_score', ascending=False)
    print(f"\n{top_neighborhood}:")
    print(breakdown.to_string(index=False))
    print(f"\nTotal SmartScore: {breakdown['weighted_score'].sum():.2f}")

    print("\n" + "=" * 80)
    print("Scoring calculation complete!")
    print("=" * 80)
    print("\nNext steps:")
    print("1. Review scores_comparison.csv to see which normalization method is more intuitive")
    print("2. Check if urban centers score higher than suburbs (validation)")
    print("3. Verify domain breakdowns match neighborhood characteristics")

if __name__ == "__main__":
    main()
