#!/usr/bin/env python3
"""
Load Statbel Statistics into Buurtkompas Database

This script processes Statbel open data files and generates a CSV file
ready for loading into the neighborhood_statistics table.

Data sources:
- Population: OPENDATA_SECTOREN_2024.txt (sector-level, aggregated to neighborhoods)
- House prices: vastgoed_2010_9999.xlsx (municipality-level, inherited to neighborhoods)

Usage:
    python load-statistics.py

Output:
    ../data/statbel/neighborhood_statistics_staging.csv

Dependencies:
    pip install pandas openpyxl
"""

import pandas as pd
import os
from pathlib import Path

# Configuration
SCRIPT_DIR = Path(__file__).parent
DATA_DIR = SCRIPT_DIR.parent.parent / "data" / "statbel"
OUTPUT_FILE = DATA_DIR / "neighborhood_statistics_staging.csv"

# Data years
POPULATION_YEAR = 2024
HOUSE_PRICE_YEAR = 2024

# Flanders provinces and Brussels (for filtering)
FLANDERS_PROVINCES = [
    "Antwerpen", "Limburg", "Oost-Vlaanderen",
    "West-Vlaanderen", "Vlaams-Brabant"
]
# Brussels NIS codes start with 21
BRUSSELS_NIS_PREFIX = "21"


def load_population_data() -> pd.DataFrame:
    """
    Load and aggregate population data from sector level to neighborhood level.

    Returns DataFrame with columns:
    - nis_code: 7-char neighborhood code
    - population: total inhabitants
    - area_km2: total area in km2
    - population_density: people per km2
    """
    print("Loading population data...")

    pop_file = DATA_DIR / "OPENDATA_SECTOREN_2024.txt"

    # Read with pipe delimiter, handle BOM
    df = pd.read_csv(
        pop_file,
        sep="|",
        encoding="utf-8-sig",  # Handle BOM
        dtype={"CD_REFNIS": str, "CD_SECTOR": str}
    )

    print(f"  Loaded {len(df)} statistical sectors")

    # Filter to Flanders + Brussels
    # CD_REFNIS is the municipality NIS code (5 chars)
    df["municipality_nis"] = df["CD_REFNIS"].astype(str).str.zfill(5)

    # Brussels starts with 21, Flanders municipalities by province:
    # 1xxxx = Brussels/Vlaams-Brabant area
    # 2xxxx = Vlaams-Brabant
    # 3xxxx = Vlaams-Brabant/Walloon Brabant
    # 4xxxx = Liege/Limburg
    # 7xxxx = Hainaut/West-Flanders
    # We'll filter by checking first 2 digits and known Flemish municipalities

    # Actually, the cleaner approach: use province from municipality lookup
    # But since we have TX_DESCR_NL (municipality name), let's just use all data
    # and filter later when joining with our neighborhoods table

    # Extract neighborhood code (first 7 chars of sector code)
    # CD_SECTOR format: "11001A00-" -> neighborhood is "11001A0"
    df["nis_code"] = df["CD_SECTOR"].str[:7]

    # Surface area is in hectares (hm2), convert to km2
    # Column name has special chars, find it dynamically
    area_col = [c for c in df.columns if "OPPERVLAKKTE" in c or "HM" in c][0]

    # Handle Belgian number format (comma as decimal separator)
    df["area_hm2"] = pd.to_numeric(
        df[area_col].astype(str).str.replace(",", "."),
        errors="coerce"
    )
    df["area_km2"] = df["area_hm2"] / 100  # 1 km2 = 100 hectares

    # Aggregate by neighborhood
    neighborhood_pop = df.groupby("nis_code").agg({
        "TOTAL": "sum",
        "area_km2": "sum",
        "municipality_nis": "first"  # Keep for house price join
    }).reset_index()

    neighborhood_pop.columns = ["nis_code", "population", "area_km2", "municipality_nis"]

    # Calculate population density (handle division by zero)
    neighborhood_pop["population_density"] = neighborhood_pop.apply(
        lambda row: round(row["population"] / row["area_km2"], 2)
        if row["area_km2"] > 0 else 0,
        axis=1
    )

    # Replace any inf values with 0
    neighborhood_pop["population_density"] = neighborhood_pop["population_density"].replace(
        [float("inf"), float("-inf")], 0
    )

    print(f"  Aggregated to {len(neighborhood_pop)} neighborhoods")
    print(f"  Total population: {neighborhood_pop['population'].sum():,}")

    return neighborhood_pop


def load_house_price_data() -> pd.DataFrame:
    """
    Load house prices by municipality from Statbel Excel file.

    Returns DataFrame with columns:
    - municipality_nis: 5-char municipality NIS code
    - median_house_price: Q50 price for regular houses
    """
    print("\nLoading house price data...")

    price_file = DATA_DIR / "vastgoed_2010_9999.xlsx"

    # Read Excel - it has sheets per year (2010, 2011, ..., 2024, 2025)
    xl = pd.ExcelFile(price_file)
    print(f"  Available sheets: {xl.sheet_names}")

    # Read the target year's sheet
    target_sheet = str(HOUSE_PRICE_YEAR)
    if target_sheet not in xl.sheet_names:
        # Fall back to latest available year
        numeric_sheets = [s for s in xl.sheet_names if s.isdigit()]
        target_sheet = max(numeric_sheets)
        print(f"  Sheet '{HOUSE_PRICE_YEAR}' not found, using '{target_sheet}'")

    df = pd.read_excel(price_file, sheet_name=target_sheet)

    print(f"  Columns: {df.columns.tolist()}")
    print(f"  Loaded {len(df)} rows")

    # Find relevant columns (names may be in Dutch/French)
    # Expected: CD_REFNIS (NIS code), CD_YEAR (year), CD_TYPE (property type), Q50 (median)

    # Look for NIS code column
    nis_col = None
    for col in df.columns:
        if "REFNIS" in str(col).upper() or "NIS" in str(col).upper():
            nis_col = col
            break

    # Look for year column
    year_col = None
    for col in df.columns:
        if "YEAR" in str(col).upper() or "JAAR" in str(col).upper():
            year_col = col
            break

    # Look for property type column
    type_col = None
    for col in df.columns:
        if "TYPE" in str(col).upper():
            type_col = col
            break

    # Look for Q50/median column
    q50_col = None
    for col in df.columns:
        col_upper = str(col).upper()
        if "Q50" in col_upper or "P_50" in col_upper or "MEDIAN" in col_upper:
            q50_col = col
            break

    # Look for geographic level column
    level_col = None
    for col in df.columns:
        if "LEVEL" in str(col).upper() or "NIVEAU" in str(col).upper():
            level_col = col
            break

    print(f"  Identified columns: NIS={nis_col}, Year={year_col}, Type={type_col}, Q50={q50_col}, Level={level_col}")

    if not all([nis_col, year_col, q50_col]):
        # Fallback: print first few rows for debugging
        print("  WARNING: Could not identify all columns. Sample data:")
        print(df.head())
        raise ValueError("Could not identify required columns in house price data")

    # Filter to:
    # - Municipality level (CD_niveau_refnis = 5)
    # - Regular houses (maisons d'habitation ordinaires)

    df_filtered = df.copy()
    print(f"  Starting with {len(df_filtered)} rows from sheet '{target_sheet}'")

    # Filter by geographic level (municipality = level 5 or NIS code length = 5)
    if level_col:
        # Check what levels exist
        print(f"  Available levels: {df_filtered[level_col].unique()}")
        # Municipality level is typically 5 or "Gemeente"
        df_filtered = df_filtered[
            (df_filtered[level_col] == 5) |
            (df_filtered[level_col] == "5") |
            (df_filtered[level_col].astype(str).str.contains("gemeente", case=False, na=False))
        ]
    else:
        # Fallback: filter by NIS code length
        df_filtered["nis_len"] = df_filtered[nis_col].astype(str).str.len()
        df_filtered = df_filtered[df_filtered["nis_len"] == 5]

    print(f"  After level filter: {len(df_filtered)} rows")

    # Filter by property type (regular houses)
    if type_col:
        print(f"  Available property types: {df_filtered[type_col].unique()[:10]}")
        # Look for "maison" or "huis" or "woning"
        house_types = df_filtered[type_col].astype(str).str.lower()
        df_houses = df_filtered[
            house_types.str.contains("maison|huis|woning|house", regex=True, na=False) &
            ~house_types.str.contains("appartement|apartment|flat", regex=True, na=False)
        ]
        if len(df_houses) > 0:
            df_filtered = df_houses
        print(f"  After property type filter: {len(df_filtered)} rows")

    # Extract municipality NIS and median price
    result = df_filtered[[nis_col, q50_col]].copy()
    result.columns = ["municipality_nis", "median_house_price"]

    # Clean up
    result["municipality_nis"] = result["municipality_nis"].astype(str).str.zfill(5)
    result["median_house_price"] = pd.to_numeric(result["median_house_price"], errors="coerce")

    # Remove rows with missing prices
    result = result.dropna(subset=["median_house_price"])

    # Take the first (or average) if there are duplicates
    result = result.groupby("municipality_nis").agg({
        "median_house_price": "first"
    }).reset_index()

    result["median_house_price"] = result["median_house_price"].astype(int)

    print(f"  Final: {len(result)} municipalities with house prices")
    print(f"  Price range: {result['median_house_price'].min():,} - {result['median_house_price'].max():,} EUR")

    return result


def merge_and_export(pop_df: pd.DataFrame, price_df: pd.DataFrame) -> None:
    """
    Merge population and price data, export to CSV.
    """
    print("\nMerging datasets...")

    # Join house prices to neighborhoods via municipality
    merged = pop_df.merge(
        price_df,
        on="municipality_nis",
        how="left"
    )

    # Add year column
    merged["year"] = POPULATION_YEAR

    # Select and order columns for output
    output = merged[[
        "nis_code",
        "year",
        "population",
        "population_density",
        "median_house_price"
    ]].copy()

    # Report coverage
    total = len(output)
    with_price = output["median_house_price"].notna().sum()
    with_pop = (output["population"] > 0).sum()

    print(f"  Total neighborhoods: {total}")
    print(f"  With population > 0: {with_pop} ({100*with_pop/total:.1f}%)")
    print(f"  With house prices: {with_price} ({100*with_price/total:.1f}%)")

    # Export to CSV
    output.to_csv(OUTPUT_FILE, index=False)
    print(f"\nExported to: {OUTPUT_FILE}")
    print(f"File size: {OUTPUT_FILE.stat().st_size / 1024:.1f} KB")


def main():
    """Main entry point."""
    print("=" * 60)
    print("Statbel Statistics ETL Pipeline")
    print("=" * 60)

    # Load data
    pop_df = load_population_data()
    price_df = load_house_price_data()

    # Merge and export
    merge_and_export(pop_df, price_df)

    print("\n" + "=" * 60)
    print("Done! Next step:")
    print("  Run: database/migrations/20250102_005_load-statbel-statistics.sql")
    print("=" * 60)


if __name__ == "__main__":
    main()
