import folium
import pandas as pd
import os

# POI category styling
POI_STYLES = {
    'supermarkets': {
        'name': 'Groceries (Supermarkets)',
        'color': 'red',
        'icon': 'shopping-cart',
        'prefix': 'fa'
    },
    'pt_stops': {
        'name': 'Public Transport',
        'color': 'blue',
        'icon': 'bus',
        'prefix': 'fa'
    },
    'green_spaces': {
        'name': 'Parks & Green Spaces',
        'color': 'green',
        'icon': 'tree',
        'prefix': 'fa'
    }
}

def load_sample_distances(samples_df, distances_df):
    """
    Merge sample points with their distance data

    Args:
        samples_df: DataFrame with sample points
        distances_df: DataFrame with distance records

    Returns:
        DataFrame with samples enriched with distance info
    """
    print("\n  Merging sample points with distance data...")

    # Pivot distances to have one row per sample with columns for each category
    distances_pivot = distances_df.pivot_table(
        index='sample_id',
        columns='category',
        values=['nearest_poi_name', 'distance_m'],
        aggfunc='first'
    )

    # Flatten column names
    distances_pivot.columns = ['_'.join(col).strip() for col in distances_pivot.columns.values]
    distances_pivot = distances_pivot.reset_index()

    # Merge with samples
    samples_enriched = samples_df.merge(distances_pivot, on='sample_id', how='left')

    print(f"  Enriched {len(samples_enriched):,} sample points with distance data")

    return samples_enriched

def create_enhanced_map(neighborhoods_df, samples_enriched_df, pois_dict, output_file="street_sampling_map.html"):
    """
    Create interactive map with enhanced sample point popups showing distances

    Args:
        neighborhoods_df: DataFrame with neighborhood centers
        samples_enriched_df: DataFrame with sample points + distance data
        pois_dict: Dictionary of {category_key: pois_df}
        output_file: Output HTML file path

    Returns:
        Folium map object
    """
    print("\nCreating enhanced map...")

    # Calculate center point for initial map view
    center_lat = 50.85
    center_lon = 4.35

    # Create map
    m = folium.Map(
        location=[center_lat, center_lon],
        zoom_start=9,
        tiles='OpenStreetMap',
        control_scale=True
    )

    # Add neighborhoods layer
    print("  Adding neighborhood centers and 1km radius circles...")
    for _, neighborhood in neighborhoods_df.iterrows():
        # Add 1km radius circle
        folium.Circle(
            location=[neighborhood['latitude'], neighborhood['longitude']],
            radius=1000,
            color='#3388ff',
            fill=True,
            fillColor='#3388ff',
            fillOpacity=0.1,
            weight=2,
            popup=f"{neighborhood['name']} - 1km radius",
            tooltip=f"{neighborhood['name']} (1km radius)"
        ).add_to(m)

        # Add neighborhood center marker
        folium.Marker(
            location=[neighborhood['latitude'], neighborhood['longitude']],
            popup=f"<b>{neighborhood['name']}</b><br>{neighborhood['city']}<br>Category: {neighborhood['category']}",
            tooltip=f"{neighborhood['name']}",
            icon=folium.Icon(color='red', icon='home', prefix='fa')
        ).add_to(m)

    # Add POI layers (with toggle controls)
    print(f"  Adding POIs by category...")
    for category_key, pois_df in pois_dict.items():
        if pois_df.empty:
            print(f"    - {category_key}: No POIs to add")
            continue

        style = POI_STYLES[category_key]
        layer = folium.FeatureGroup(name=style['name'], show=True)

        print(f"    - {style['name']}: Adding {len(pois_df)} POIs...")

        for idx, poi in pois_df.iterrows():
            if (idx + 1) % 200 == 0:
                print(f"      Added {idx + 1}/{len(pois_df)} {category_key}...")

            popup_html = f"""
            <b>{poi['name']}</b><br>
            <b>Type:</b> {poi['poi_type']}<br>
            <b>Category:</b> {style['name']}<br>
            <b>OSM ID:</b> {poi['osm_id']}
            """

            folium.Marker(
                location=[poi['latitude'], poi['longitude']],
                popup=folium.Popup(popup_html, max_width=250),
                tooltip=poi['name'],
                icon=folium.Icon(
                    color=style['color'],
                    icon=style['icon'],
                    prefix=style['prefix']
                )
            ).add_to(layer)

        layer.add_to(m)

    # Add sample points layer with enhanced popups
    sample_points_layer = folium.FeatureGroup(name='Street Sample Points', show=True)

    print(f"  Adding {len(samples_enriched_df):,} street sample points with distance data...")
    for idx, sample in samples_enriched_df.iterrows():
        if (idx + 1) % 1000 == 0:
            print(f"    Added {idx + 1:,}/{len(samples_enriched_df):,} sample points...")

        # Build popup with distance information
        popup_html = f"""
        <div style="font-family: Arial, sans-serif; font-size: 12px;">
            <b style="font-size: 14px;">Sample Point #{sample['sample_id']}</b><br>
            <b>Street:</b> {sample['street_name']}<br>
            <b>Neighborhood:</b> {sample['neighborhood_name']}<br>
            <b>Position:</b> {sample['position_on_street']}<br>
            <hr style="margin: 5px 0;">
            <b>Distances to nearest POIs:</b><br>
            <span style="color: red;">🛒 Groceries:</span> {sample['distance_m_supermarkets']:.0f}m<br>
            <span style="padding-left: 20px; font-size: 11px;">→ {sample['nearest_poi_name_supermarkets']}</span><br>
            <span style="color: blue;">🚌 Public Transport:</span> {sample['distance_m_pt_stops']:.0f}m<br>
            <span style="padding-left: 20px; font-size: 11px;">→ {sample['nearest_poi_name_pt_stops']}</span><br>
            <span style="color: green;">🌳 Parks:</span> {sample['distance_m_green_spaces']:.0f}m<br>
            <span style="padding-left: 20px; font-size: 11px;">→ {sample['nearest_poi_name_green_spaces']}</span>
        </div>
        """

        folium.CircleMarker(
            location=[sample['latitude'], sample['longitude']],
            radius=3,
            color='#ff7800',
            fill=True,
            fillColor='#ff7800',
            fillOpacity=0.6,
            weight=1,
            popup=folium.Popup(popup_html, max_width=300),
            tooltip=f"Sample: {sample['street_name']}"
        ).add_to(sample_points_layer)

    sample_points_layer.add_to(m)

    # Add layer control
    folium.LayerControl(collapsed=False).add_to(m)

    # Add title
    title_html = '''
    <div style="position: fixed;
                top: 10px; left: 50px; width: 550px; height: 90px;
                background-color: white; border:2px solid grey; z-index:9999;
                font-size:14px; padding: 10px">
        <b>Street Sampling POC - Enhanced Map with Distances</b><br>
        <span style="font-size:11px">
        <span style="color:red">●</span> Neighborhoods |
        <span style="color:#ff7800">●</span> Sample Points (click to see distances) |
        <span style="color:red">●</span> Supermarkets |
        <span style="color:blue">●</span> PT Stops |
        <span style="color:green">●</span> Green Spaces<br>
        Use layer controls (top right) to toggle layers on/off
        </span>
    </div>
    '''
    m.get_root().html.add_child(folium.Element(title_html))

    # Save to file
    print(f"\n  Saving map to: {output_file}")
    m.save(output_file)

    # Get file size
    file_size_mb = os.path.getsize(output_file) / (1024 * 1024)
    print(f"  File size: {file_size_mb:.2f} MB")

    return m

def main():
    print("=" * 70)
    print("Street Sampling POC - Create Enhanced Map with Sample Point Distances")
    print("=" * 70)

    # Configuration
    NEIGHBORHOODS_FILE = "data/neighborhoods.csv"
    SAMPLES_FILE = "data/samples/street_samples.csv"
    DISTANCES_FILE = "results/distances_per_sample.csv"
    OUTPUT_FILE = "street_sampling_map.html"

    POI_FILES = {
        'supermarkets': 'data/pois/supermarkets.csv',
        'pt_stops': 'data/pois/pt_stops.csv',
        'green_spaces': 'data/pois/green_spaces.csv'
    }

    # Load data
    print("\n1. Loading data...")

    print(f"   Reading neighborhoods from: {NEIGHBORHOODS_FILE}")
    neighborhoods_df = pd.read_csv(NEIGHBORHOODS_FILE)
    print(f"   Loaded {len(neighborhoods_df)} neighborhoods")

    print(f"   Reading sample points from: {SAMPLES_FILE}")
    samples_df = pd.read_csv(SAMPLES_FILE)
    print(f"   Loaded {len(samples_df):,} sample points")

    print(f"   Reading distances from: {DISTANCES_FILE}")
    distances_df = pd.read_csv(DISTANCES_FILE)
    print(f"   Loaded {len(distances_df):,} distance records")

    print(f"   Reading POIs...")
    pois_dict = {}
    total_pois = 0
    for category_key, file_path in POI_FILES.items():
        pois_df = pd.read_csv(file_path)
        pois_dict[category_key] = pois_df
        total_pois += len(pois_df)
        print(f"     - {POI_STYLES[category_key]['name']}: {len(pois_df)} POIs")
    print(f"   Total POIs: {total_pois}")

    # Enrich samples with distance data
    print("\n2. Enriching sample points with distance data...")
    samples_enriched_df = load_sample_distances(samples_df, distances_df)

    # Create enhanced map
    print("\n3. Creating enhanced interactive map...")
    m = create_enhanced_map(neighborhoods_df, samples_enriched_df, pois_dict, OUTPUT_FILE)

    # Summary
    print("\n" + "=" * 70)
    print("ENHANCED MAP CREATION COMPLETE")
    print("=" * 70)
    print(f"Interactive map saved to: {OUTPUT_FILE}")
    print(f"\nMap contents:")
    print(f"  - {len(neighborhoods_df)} neighborhood centers (red home icons)")
    print(f"  - {len(neighborhoods_df)} × 1km radius circles (blue)")
    print(f"  - {len(samples_enriched_df):,} street sample points (orange dots) with distance data")
    print(f"  - {total_pois} POIs in 3 categories")

    print(f"\nNew features (Story 7):")
    print(f"  ✓ Sample point popups now show distances to nearest POIs")
    print(f"  ✓ Distance to nearest supermarket + POI name")
    print(f"  ✓ Distance to nearest PT stop + stop name")
    print(f"  ✓ Distance to nearest park + park name")
    print(f"  ✓ Color-coded distance display (red/blue/green)")

    print(f"\nNext steps:")
    print(f"  1. Open {OUTPUT_FILE} in your browser")
    print(f"  2. Click on sample points (orange dots) to see distance popups")
    print(f"  3. Verify distances and POI names are displayed correctly")
    print(f"  4. Compare urban vs suburban sample points (urban should have shorter distances)")

    print("\n" + "=" * 70)

if __name__ == "__main__":
    main()
