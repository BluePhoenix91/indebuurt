import folium
import pandas as pd
import json
import os

# POI category styling
POI_STYLES = {
    'supermarkets': {
        'name': 'Groceries (Supermarkets)',
        'color': 'red',
        'icon': 'shopping-cart',
        'prefix': 'fa',
        'line_color': '#ff0000'
    },
    'pt_stops': {
        'name': 'Public Transport',
        'color': 'blue',
        'icon': 'bus',
        'prefix': 'fa',
        'line_color': '#0000ff'
    },
    'green_spaces': {
        'name': 'Parks & Green Spaces',
        'color': 'green',
        'icon': 'tree',
        'prefix': 'fa',
        'line_color': '#00aa00'
    }
}

def prepare_poi_data(pois_dict):
    """
    Convert POI dataframes to JavaScript-ready format

    Returns:
        Dictionary of POI ID -> {lat, lon, name, category}
    """
    poi_lookup = {}

    for category_key, pois_df in pois_dict.items():
        for _, poi in pois_df.iterrows():
            poi_id = f"{category_key}_{poi['osm_id']}"
            poi_lookup[poi_id] = {
                'lat': poi['latitude'],
                'lon': poi['longitude'],
                'name': poi['name'],
                'category': category_key
            }

    return poi_lookup

def prepare_sample_poi_links(distances_df):
    """
    Create mapping of sample_id -> nearest POI IDs for each category

    Returns:
        Dictionary of sample_id -> {category: poi_id}
    """
    sample_links = {}

    for _, row in distances_df.iterrows():
        sample_id = row['sample_id']
        category = row['category']
        poi_id = f"{category}_{row['nearest_poi_id']}"

        if sample_id not in sample_links:
            sample_links[sample_id] = {}

        sample_links[sample_id][category] = poi_id

    return sample_links

def load_sample_distances(samples_df, distances_df):
    """Merge sample points with their distance data"""
    print("\n  Merging sample points with distance data...")

    distances_pivot = distances_df.pivot_table(
        index='sample_id',
        columns='category',
        values=['nearest_poi_name', 'distance_m'],
        aggfunc='first'
    )

    distances_pivot.columns = ['_'.join(col).strip() for col in distances_pivot.columns.values]
    distances_pivot = distances_pivot.reset_index()

    samples_enriched = samples_df.merge(distances_pivot, on='sample_id', how='left')

    print(f"  Enriched {len(samples_enriched):,} sample points with distance data")

    return samples_enriched

def create_map_with_lines(neighborhoods_df, samples_enriched_df, pois_dict, distances_df, labels_df, output_file="street_sampling_map.html"):
    """
    Create interactive map with JavaScript-powered connection lines
    """
    print("\nCreating map with interactive connection lines...")

    # Calculate center point
    center_lat = 50.85
    center_lon = 4.35

    # Create map
    m = folium.Map(
        location=[center_lat, center_lon],
        zoom_start=9,
        tiles='OpenStreetMap',
        control_scale=True
    )

    # Add neighborhoods with enhanced popups
    print("  Adding neighborhood centers with label popups...")
    for _, neighborhood in neighborhoods_df.iterrows():
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

        # Get label data for this neighborhood
        label_row = labels_df[labels_df['neighborhood_name'] == neighborhood['name']]

        if not label_row.empty:
            label_row = label_row.iloc[0]

            # Count samples in this neighborhood
            sample_count = len(samples_enriched_df[samples_enriched_df['neighborhood_name'] == neighborhood['name']])

            # Helper function to color-code labels
            def get_label_color(label_text):
                if 'Limited' in label_text:
                    return '#d9534f'  # Red/orange for limited
                elif 'Excellent' in label_text or 'within walking distance' in label_text:
                    return '#5cb85c'  # Green for good access
                else:
                    return '#f0ad4e'  # Orange for moderate

            # Build enhanced popup
            popup_html = f"""
            <div style="font-family: Arial, sans-serif; font-size: 13px; min-width: 300px;">
                <div style="background-color: #3388ff; color: white; padding: 10px; margin: -10px -10px 10px -10px; border-radius: 3px 3px 0 0;">
                    <b style="font-size: 16px;">{neighborhood['name']}</b><br>
                    <span style="font-size: 12px;">{neighborhood['city']} • {neighborhood['category']}</span>
                </div>

                <div style="margin-bottom: 8px;">
                    <b style="font-size: 14px;">Neighborhood SmartLabels:</b>
                </div>

                <div style="margin-bottom: 6px; padding: 6px; background-color: #f9f9f9; border-left: 4px solid {get_label_color(label_row['groceries_label'])};">
                    <span style="font-size: 12px; color: #666;">🛒 Groceries</span><br>
                    <b style="color: {get_label_color(label_row['groceries_label'])};">{label_row['groceries_label']}</b><br>
                    <span style="font-size: 11px; color: #888;">Median: {label_row['groceries_median_m']:.0f} m</span>
                </div>

                <div style="margin-bottom: 6px; padding: 6px; background-color: #f9f9f9; border-left: 4px solid {get_label_color(label_row['pt_label'])};">
                    <span style="font-size: 12px; color: #666;">🚌 Public Transport</span><br>
                    <b style="color: {get_label_color(label_row['pt_label'])};">{label_row['pt_label']}</b><br>
                    <span style="font-size: 11px; color: #888;">Median: {label_row['pt_median_m']:.0f} m</span>
                </div>

                <div style="margin-bottom: 6px; padding: 6px; background-color: #f9f9f9; border-left: 4px solid {get_label_color(label_row['parks_label'])};">
                    <span style="font-size: 12px; color: #666;">🌳 Parks & Green Spaces</span><br>
                    <b style="color: {get_label_color(label_row['parks_label'])};">{label_row['parks_label']}</b><br>
                    <span style="font-size: 11px; color: #888;">Median: {label_row['parks_median_m']:.0f} m</span>
                </div>

                <hr style="margin: 8px 0; border: none; border-top: 1px solid #ddd;">

                <div style="font-size: 11px; color: #666;">
                    <b>Sample Points:</b> {sample_count}<br>
                    <i>Based on median distances from street-level sampling</i>
                </div>
            </div>
            """

            folium.Marker(
                location=[neighborhood['latitude'], neighborhood['longitude']],
                popup=folium.Popup(popup_html, max_width=350),
                tooltip=f"{neighborhood['name']} - Click for labels",
                icon=folium.Icon(color='red', icon='home', prefix='fa')
            ).add_to(m)
        else:
            # Fallback if no label data
            folium.Marker(
                location=[neighborhood['latitude'], neighborhood['longitude']],
                popup=f"<b>{neighborhood['name']}</b><br>{neighborhood['city']}<br>Category: {neighborhood['category']}",
                tooltip=f"{neighborhood['name']}",
                icon=folium.Icon(color='red', icon='home', prefix='fa')
            ).add_to(m)

    # Add POI layers
    print(f"  Adding POIs by category...")
    for category_key, pois_df in pois_dict.items():
        if pois_df.empty:
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

    # Add sample points with custom IDs for JavaScript interaction
    sample_points_layer = folium.FeatureGroup(name='Street Sample Points', show=True)

    print(f"  Adding {len(samples_enriched_df):,} street sample points...")
    for idx, sample in samples_enriched_df.iterrows():
        if (idx + 1) % 1000 == 0:
            print(f"    Added {idx + 1:,}/{len(samples_enriched_df):,} sample points...")

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
            <span style="padding-left: 20px; font-size: 11px;">→ {sample['nearest_poi_name_green_spaces']}</span><br>
            <hr style="margin: 5px 0;">
            <i style="font-size: 10px;">Click to draw lines to nearest POIs</i>
        </div>
        """

        # Create circle marker with unique class for JavaScript access
        marker = folium.CircleMarker(
            location=[sample['latitude'], sample['longitude']],
            radius=3,
            color='#ff7800',
            fill=True,
            fillColor='#ff7800',
            fillOpacity=0.6,
            weight=1,
            popup=folium.Popup(popup_html, max_width=300),
            tooltip=f"Sample: {sample['street_name']}",
            # Add custom class name with sample ID encoded
            class_name=f"sample-marker sample-{sample['sample_id']}"
        )
        marker.add_to(sample_points_layer)

    sample_points_layer.add_to(m)

    # Add layer control
    folium.LayerControl(collapsed=False).add_to(m)

    # Prepare data for JavaScript
    print("\n  Preparing JavaScript data...")
    poi_lookup = prepare_poi_data(pois_dict)
    sample_links = prepare_sample_poi_links(distances_df)

    # Prepare sample coordinates lookup
    sample_coords = {}
    for _, sample in samples_enriched_df.iterrows():
        sample_coords[str(sample['sample_id'])] = {
            'lat': sample['latitude'],
            'lon': sample['longitude'],
            'name': sample['street_name']
        }

    # Create JavaScript for interactive lines
    javascript_code = f'''
    <script>
    // POI lookup data
    var poiData = {json.dumps(poi_lookup)};

    // Sample -> POI links
    var sampleLinks = {json.dumps(sample_links)};

    // Sample coordinates
    var sampleCoords = {json.dumps(sample_coords)};

    // Line colors by category
    var lineColors = {{
        'supermarkets': '{POI_STYLES["supermarkets"]["line_color"]}',
        'pt_stops': '{POI_STYLES["pt_stops"]["line_color"]}',
        'green_spaces': '{POI_STYLES["green_spaces"]["line_color"]}'
    }};

    // Store current lines
    var currentLines = [];
    var linesLayerGroup = null;

    // Function to clear existing lines
    function clearLines() {{
        if (linesLayerGroup) {{
            linesLayerGroup.clearLayers();
        }}
    }}

    // Function to draw lines from sample to POIs
    function drawLines(sampleId, sampleLat, sampleLon, sampleName) {{
        console.log('>>> drawLines() called for sample', sampleId);

        // Clear previous lines
        clearLines();
        console.log('  Previous lines cleared');

        // Get POI links for this sample
        var links = sampleLinks[sampleId];
        if (!links) {{
            console.log('  ✗ ERROR: No links found for sample', sampleId);
            return;
        }}

        console.log('  ✓ Found links:', Object.keys(links));
        console.log('  Drawing lines from [', sampleLat, ',', sampleLon, ']');

        // Draw line to each category's nearest POI
        var lineCount = 0;
        Object.keys(links).forEach(function(category) {{
            var poiId = links[category];
            var poi = poiData[poiId];

            console.log('    Category:', category, '| POI ID:', poiId);

            if (poi) {{
                console.log('      ✓ Drawing line to [', poi.lat, ',', poi.lon, ']', poi.name);

                // Create polyline
                var line = L.polyline(
                    [[sampleLat, sampleLon], [poi.lat, poi.lon]],
                    {{
                        color: lineColors[category],
                        weight: 2,
                        opacity: 0.7,
                        dashArray: '8, 4'
                    }}
                );

                // Add popup to line
                var categoryName = category.replace('_', ' ');
                line.bindPopup('<b>' + categoryName + '</b><br>From: ' + sampleName + '<br>To: ' + poi.name);

                line.addTo(linesLayerGroup);
                lineCount++;
                console.log('      ✓ Line added to layer group');
            }} else {{
                console.log('      ✗ ERROR: POI not found in lookup:', poiId);
            }}
        }});

        console.log('  ✓ Drew', lineCount, 'lines total');
        console.log('  Layer group now has', linesLayerGroup.getLayers().length, 'layers');
    }}

    // Function to find nearest sample to a click location
    function findNearestSample(clickLat, clickLon) {{
        var minDist = Infinity;
        var nearestSampleId = null;

        Object.keys(sampleCoords).forEach(function(sampleId) {{
            var sample = sampleCoords[sampleId];
            var dist = Math.sqrt(
                Math.pow(sample.lat - clickLat, 2) +
                Math.pow(sample.lon - clickLon, 2)
            );

            if (dist < minDist) {{
                minDist = dist;
                nearestSampleId = sampleId;
            }}
        }});

        console.log('Nearest sample:', nearestSampleId, 'at distance:', minDist.toFixed(6));

        // Only return if click was reasonably close (within ~0.005 degrees = ~500m)
        // Increased threshold to make it easier to click near samples
        if (minDist < 0.005) {{
            console.log('Sample is within threshold, returning:', nearestSampleId);
            return nearestSampleId;
        }}
        console.log('Sample too far away (threshold: 0.005), ignoring click');
        return null;
    }}

    // Wait for map to be ready
    setTimeout(function() {{
        // Find the Folium map object
        var foliumMap = null;
        var mapDivs = document.querySelectorAll('.folium-map');

        if (mapDivs.length > 0) {{
            var mapId = mapDivs[0].id;
            console.log('Found map div ID:', mapId);
            foliumMap = window[mapId];
        }}

        if (!foliumMap) {{
            console.error('Could not find Folium map object');
            return;
        }}

        console.log('Successfully found Folium map');

        // Create layer group for lines
        linesLayerGroup = L.layerGroup().addTo(foliumMap);

        // Find all sample markers and add click handlers
        console.log('Searching for sample markers...');
        var sampleMarkerCount = 0;

        // Wait a bit more for all layers to be added
        setTimeout(function() {{
            foliumMap.eachLayer(function(layer) {{
                // Check if this is a CircleMarker (sample point)
                if (layer instanceof L.CircleMarker && layer.options.className && layer.options.className.includes('sample-marker')) {{
                    sampleMarkerCount++;

                    // Extract sample ID from class name
                    var classMatch = layer.options.className.match(/sample-(\\d+)/);
                    if (classMatch) {{
                        var sampleId = classMatch[1];
                        var sample = sampleCoords[sampleId];

                        if (sample) {{
                            // Add click handler to this marker
                            layer.on('click', function(e) {{
                                console.log('=== Sample marker clicked:', sampleId, '===');
                                L.DomEvent.stopPropagation(e); // Prevent map click
                                drawLines(sampleId, sample.lat, sample.lon, sample.name);
                            }});
                        }}
                    }}
                }}
            }});

            console.log('✓ Attached click handlers to', sampleMarkerCount, 'sample markers');
        }}, 500);

        // Also add map click handler for clicking near (but not on) samples
        foliumMap.on('click', function(e) {{
            var clickLat = e.latlng.lat;
            var clickLon = e.latlng.lng;

            console.log('=== Map clicked at:', clickLat.toFixed(6), clickLon.toFixed(6), '===');

            // Find nearest sample point
            var sampleId = findNearestSample(clickLat, clickLon);

            if (sampleId) {{
                var sample = sampleCoords[sampleId];
                console.log('✓ Found nearby sample', sampleId, '- drawing lines...');
                drawLines(sampleId, sample.lat, sample.lon, sample.name);
            }} else {{
                console.log('✗ No nearby sample found - clearing lines');
                clearLines();
            }}
        }});

        console.log('Interactive connection lines enabled');
        console.log('POI data loaded:', Object.keys(poiData).length, 'POIs');
        console.log('Sample links loaded:', Object.keys(sampleLinks).length, 'samples');
        console.log('Sample coordinates loaded:', Object.keys(sampleCoords).length, 'samples');
        console.log('Click on orange sample points to draw lines!');
    }}, 2000);
    </script>
    '''

    # Add title
    title_html = '''
    <div style="position: fixed;
                top: 10px; left: 50px; width: 700px; height: 130px;
                background-color: white; border:2px solid grey; z-index:9999;
                font-size:14px; padding: 10px">
        <b>Street Sampling POC - Interactive Map with SmartLabels</b><br>
        <span style="font-size:11px">
        <span style="color:red">●</span> Neighborhoods (click for SmartLabels) |
        <span style="color:#ff7800">●</span> Sample Points (click to draw lines) |
        <span style="color:red">●</span> Supermarkets |
        <span style="color:blue">●</span> PT Stops |
        <span style="color:green">●</span> Green Spaces<br>
        <b>NEW:</b> Click neighborhood centers (red house icons) to see aggregated SmartLabels<br>
        <b>ALSO:</b> Click sample points to see lines to nearest POIs (color-coded)<br>
        Use layer controls (top right) to toggle layers on/off
        </span>
    </div>
    '''
    m.get_root().html.add_child(folium.Element(title_html))

    # Save to file
    print(f"\n  Saving map to: {output_file}")
    m.save(output_file)

    # Inject JavaScript into saved file
    print(f"  Injecting JavaScript for interactive lines...")
    with open(output_file, 'r', encoding='utf-8') as f:
        html_content = f.read()

    # Insert JavaScript before </body>
    html_content = html_content.replace('</body>', f'{javascript_code}</body>')

    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(html_content)

    # Get file size
    file_size_mb = os.path.getsize(output_file) / (1024 * 1024)
    print(f"  File size: {file_size_mb:.2f} MB")

    return m

def main():
    print("=" * 70)
    print("Street Sampling POC - Create Map with Interactive Connection Lines")
    print("=" * 70)

    # Configuration
    NEIGHBORHOODS_FILE = "data/neighborhoods.csv"
    SAMPLES_FILE = "data/samples/street_samples.csv"
    DISTANCES_FILE = "results/distances_per_sample.csv"
    LABELS_FILE = "results/neighborhood_labels_summary.csv"
    OUTPUT_FILE = "street_sampling_map.html"

    POI_FILES = {
        'supermarkets': 'data/pois/supermarkets.csv',
        'pt_stops': 'data/pois/pt_stops.csv',
        'green_spaces': 'data/pois/green_spaces.csv'
    }

    # Load data
    print("\n1. Loading data...")

    neighborhoods_df = pd.read_csv(NEIGHBORHOODS_FILE)
    print(f"   Loaded {len(neighborhoods_df)} neighborhoods")

    samples_df = pd.read_csv(SAMPLES_FILE)
    print(f"   Loaded {len(samples_df):,} sample points")

    distances_df = pd.read_csv(DISTANCES_FILE)
    print(f"   Loaded {len(distances_df):,} distance records")

    labels_df = pd.read_csv(LABELS_FILE)
    print(f"   Loaded neighborhood labels for {len(labels_df)} neighborhoods")

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

    # Create map with interactive lines and neighborhood labels
    print("\n3. Creating map with JavaScript-powered connection lines...")
    m = create_map_with_lines(neighborhoods_df, samples_enriched_df, pois_dict, distances_df, labels_df, OUTPUT_FILE)

    # Summary
    print("\n" + "=" * 70)
    print("INTERACTIVE MAP CREATION COMPLETE")
    print("=" * 70)
    print(f"Interactive map saved to: {OUTPUT_FILE}")
    print(f"\nMap contents:")
    print(f"  - {len(neighborhoods_df)} neighborhood centers")
    print(f"  - {len(samples_enriched_df):,} street sample points")
    print(f"  - {total_pois} POIs in 3 categories")

    print(f"\nNew features (Story 8):")
    print(f"  ✓ Click any sample point to draw lines to its nearest 3 POIs")
    print(f"  ✓ Lines are color-coded: red (groceries), blue (PT), green (parks)")
    print(f"  ✓ Lines have dashed style and tooltips showing POI names")
    print(f"  ✓ Previous lines auto-clear when clicking new sample")
    print(f"  ✓ JavaScript-powered for dynamic interaction")

    print(f"\nNext steps:")
    print(f"  1. Open {OUTPUT_FILE} in your browser")
    print(f"  2. Click on sample points (orange dots)")
    print(f"  3. Watch colored dashed lines appear to nearest POIs")
    print(f"  4. Click different samples to see lines update")
    print(f"  5. Verify lines point to visibly nearest POIs")

    print("\n" + "=" * 70)
    print("\nNote: JavaScript line drawing is a beta feature.")
    print("If lines don't appear, check browser console for errors.")

if __name__ == "__main__":
    main()
