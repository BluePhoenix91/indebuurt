import folium
import pandas as pd
import os

def create_base_map(neighborhoods_df, samples_df, output_file="street_sampling_map.html"):
    """
    Create base interactive map showing neighborhoods and sample points

    Args:
        neighborhoods_df: DataFrame with neighborhood centers
        samples_df: DataFrame with street sample points
        output_file: Output HTML file path

    Returns:
        Folium map object
    """
    print("\nCreating base map...")

    # Calculate center point for initial map view (center of Belgium approximately)
    center_lat = 50.85
    center_lon = 4.35

    # Create map
    m = folium.Map(
        location=[center_lat, center_lon],
        zoom_start=9,  # Show region around all neighborhoods
        tiles='OpenStreetMap',
        control_scale=True
    )

    # Add neighborhoods layer
    print("  Adding neighborhood centers and 1km radius circles...")
    for _, neighborhood in neighborhoods_df.iterrows():
        # Add 1km radius circle
        folium.Circle(
            location=[neighborhood['latitude'], neighborhood['longitude']],
            radius=1000,  # 1km in meters
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

    # Create a feature group for sample points (allows toggling on/off)
    sample_points_layer = folium.FeatureGroup(name='Street Sample Points', show=True)

    print(f"  Adding {len(samples_df):,} street sample points...")

    # Add sample points (using CircleMarker for better performance)
    for idx, sample in samples_df.iterrows():
        # Progress indicator
        if (idx + 1) % 1000 == 0:
            print(f"    Added {idx + 1:,}/{len(samples_df):,} sample points...")

        # Create popup with sample info
        popup_html = f"""
        <b>Sample Point #{sample['sample_id']}</b><br>
        <b>Street:</b> {sample['street_name']}<br>
        <b>Neighborhood:</b> {sample['neighborhood_name']}<br>
        <b>Position:</b> {sample['position_on_street']}<br>
        <b>Street length:</b> {sample['street_length_m']:.0f}m
        """

        folium.CircleMarker(
            location=[sample['latitude'], sample['longitude']],
            radius=3,  # Small circle
            color='#ff7800',
            fill=True,
            fillColor='#ff7800',
            fillOpacity=0.6,
            weight=1,
            popup=folium.Popup(popup_html, max_width=250),
            tooltip=f"Sample: {sample['street_name']}"
        ).add_to(sample_points_layer)

    # Add sample points layer to map
    sample_points_layer.add_to(m)

    # Add layer control
    folium.LayerControl(collapsed=False).add_to(m)

    # Add title
    title_html = '''
    <div style="position: fixed;
                top: 10px; left: 50px; width: 400px; height: 60px;
                background-color: white; border:2px solid grey; z-index:9999;
                font-size:16px; padding: 10px">
        <b>Street Sampling POC - Base Map</b><br>
        <span style="font-size:12px">Red markers: Neighborhood centers | Orange dots: Sample points</span>
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
    print("Street Sampling POC - Create Base Interactive Map")
    print("=" * 70)

    # Configuration
    NEIGHBORHOODS_FILE = "data/neighborhoods.csv"
    SAMPLES_FILE = "data/samples/street_samples.csv"
    OUTPUT_FILE = "street_sampling_map.html"

    # Load data
    print("\n1. Loading data...")
    print(f"   Reading neighborhoods from: {NEIGHBORHOODS_FILE}")
    neighborhoods_df = pd.read_csv(NEIGHBORHOODS_FILE)
    print(f"   Loaded {len(neighborhoods_df)} neighborhoods")

    print(f"   Reading sample points from: {SAMPLES_FILE}")
    samples_df = pd.read_csv(SAMPLES_FILE)
    print(f"   Loaded {len(samples_df):,} sample points")

    # Create map
    print("\n2. Creating interactive map...")
    m = create_base_map(neighborhoods_df, samples_df, OUTPUT_FILE)

    # Summary
    print("\n" + "=" * 70)
    print("MAP CREATION COMPLETE")
    print("=" * 70)
    print(f"Interactive map saved to: {OUTPUT_FILE}")
    print(f"\nMap contents:")
    print(f"  - {len(neighborhoods_df)} neighborhood centers (red home icons)")
    print(f"  - {len(neighborhoods_df)} × 1km radius circles (blue)")
    print(f"  - {len(samples_df):,} street sample points (orange dots)")
    print(f"\nFeatures:")
    print(f"  ✓ Layer control to toggle sample points on/off")
    print(f"  ✓ Clickable markers with detailed info")
    print(f"  ✓ Pan and zoom enabled")
    print(f"  ✓ Tooltips on hover")
    print(f"\nNext steps:")
    print(f"  1. Open {OUTPUT_FILE} in your browser")
    print(f"  2. Verify sample points are distributed along streets")
    print(f"  3. Check that urban areas have denser samples than suburbs")
    print(f"  4. Confirm all neighborhoods are visible and correctly placed")

    print("\n" + "=" * 70)

if __name__ == "__main__":
    main()
