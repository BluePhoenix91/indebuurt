import pandas as pd
import folium
import os
from math import radians, cos, sin, asin, sqrt

# Domain colors for visual distinction
DOMAIN_COLORS = {
    'winkels': '#e74c3c',
    'restaurants': '#f39c12',
    'groen': '#27ae60',
    'onderwijs': '#3498db',
    'transport': '#9b59b6',
    'sport': '#1abc9c',
    'gezondheidszorg': '#e91e63',
    'cultuur': '#ff9800'
}

DOMAIN_NAMES = {
    'winkels': 'Winkels',
    'restaurants': 'Restaurants',
    'groen': 'Groen',
    'onderwijs': 'Onderwijs',
    'transport': 'Transport',
    'sport': 'Sport',
    'gezondheidszorg': 'Healthcare',
    'cultuur': 'Cultuur'
}

def haversine(lon1, lat1, lon2, lat2):
    """Calculate distance between two points on Earth"""
    lon1, lat1, lon2, lat2 = map(radians, [lon1, lat1, lon2, lat2])
    dlon = lon2 - lon1
    dlat = lat2 - lat1
    a = sin(dlat/2)**2 + cos(lat1) * cos(lat2) * sin(dlon/2)**2
    c = 2 * asin(sqrt(a))
    r = 6371000
    return c * r

def filter_pois_within_radius(pois_df, neighborhoods_df, radius_m=2000):
    """Filter POIs to only those within radius of any neighborhood"""
    filtered_pois = []
    for _, poi in pois_df.iterrows():
        poi_lat = poi['latitude']
        poi_lon = poi['longitude']
        for _, nbh in neighborhoods_df.iterrows():
            distance = haversine(poi_lon, poi_lat, nbh['longitude'], nbh['latitude'])
            if distance <= radius_m:
                filtered_pois.append(poi)
                break
    return pd.DataFrame(filtered_pois)

def create_map_html(neighborhoods_df, pois_dict):
    """Create the Folium map and return its HTML"""
    center_lat = neighborhoods_df['latitude'].mean()
    center_lon = neighborhoods_df['longitude'].mean()

    m = folium.Map(
        location=[center_lat, center_lon],
        zoom_start=8,
        tiles='OpenStreetMap'
    )

    # Add neighborhoods
    for _, row in neighborhoods_df.iterrows():
        folium.Marker(
            location=[row['latitude'], row['longitude']],
            popup=folium.Popup(
                f"<b>{row['name']}</b><br>"
                f"City: {row['city']}<br>"
                f"Category: {row['category']}",
                max_width=300
            ),
            tooltip=row['name'],
            icon=folium.Icon(color='darkblue', icon='home', prefix='fa')
        ).add_to(m)

        folium.Circle(
            location=[row['latitude'], row['longitude']],
            radius=1000,
            color='blue',
            fill=True,
            fillColor='blue',
            fillOpacity=0.1,
            popup=f"1km radius around {row['name']}"
        ).add_to(m)

    # Add POIs by domain
    for domain, pois_df in pois_dict.items():
        if pois_df.empty:
            continue

        fg = folium.FeatureGroup(name=DOMAIN_NAMES[domain])

        for idx, poi in pois_df.iterrows():
            folium.CircleMarker(
                location=[poi['latitude'], poi['longitude']],
                radius=3,
                popup=folium.Popup(
                    f"<b>{poi['name']}</b><br>"
                    f"Domain: {DOMAIN_NAMES[domain]}",
                    max_width=250
                ),
                tooltip=poi['name'],
                color=DOMAIN_COLORS[domain],
                fill=True,
                fillColor=DOMAIN_COLORS[domain],
                fillOpacity=0.7
            ).add_to(fg)

        fg.add_to(m)

    folium.LayerControl(collapsed=False).add_to(m)

    return m._repr_html_()

def create_scorecard_html(nbh_row, minmax_df, log_df):
    """Create compact HTML for a single neighborhood score card"""
    nbh_name = nbh_row['neighborhood_name']
    city = nbh_row['city']

    smartscore_minmax = nbh_row['smartscore_minmax']
    smartscore_log = nbh_row['smartscore_log']

    # Get domain scores
    nbh_minmax = minmax_df[minmax_df['neighborhood_name'] == nbh_name]

    # Build compact domain list
    domain_items = ''
    for domain in DOMAIN_COLORS.keys():
        domain_row = nbh_minmax[nbh_minmax['domain'] == domain].iloc[0]
        poi_count = domain_row['poi_count']
        score_minmax = domain_row['domain_score']

        domain_items += f'''
        <div style="display: flex; align-items: center; margin: 3px 0;">
            <span style="width: 10px; height: 10px; background: {DOMAIN_COLORS[domain]}; border-radius: 2px; margin-right: 6px;"></span>
            <span style="flex: 1; font-size: 11px;">{DOMAIN_NAMES[domain]}</span>
            <span style="font-size: 11px; color: #7f8c8d; margin-right: 8px;">{poi_count}</span>
            <div style="width: 60px; height: 8px; background: #ecf0f1; border-radius: 4px; overflow: hidden;">
                <div style="width: {score_minmax * 10}%; background: {DOMAIN_COLORS[domain]}; height: 100%;"></div>
            </div>
        </div>
        '''

    card_html = f'''
    <div style="background: white; border-radius: 6px; padding: 12px; margin-bottom: 12px; box-shadow: 0 1px 3px rgba(0,0,0,0.1); cursor: pointer;"
         onmouseover="this.style.boxShadow='0 3px 8px rgba(0,0,0,0.2)'"
         onmouseout="this.style.boxShadow='0 1px 3px rgba(0,0,0,0.1)'">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px;">
            <div>
                <div style="font-weight: bold; font-size: 14px; color: #2c3e50;">{nbh_name}</div>
                <div style="font-size: 11px; color: #7f8c8d;">{city}</div>
            </div>
            <div style="text-align: right;">
                <div style="font-size: 20px; font-weight: bold; color: #2c3e50;">{smartscore_minmax:.1f}</div>
                <div style="font-size: 10px; color: #27ae60;">Log: {smartscore_log:.1f}</div>
            </div>
        </div>
        <div style="border-top: 1px solid #ecf0f1; padding-top: 8px;">
            {domain_items}
        </div>
    </div>
    '''

    return card_html

def main():
    print("=" * 70)
    print("SmartScore POC - Create Combined Map + Score Cards View")
    print("=" * 70)

    # Load data
    print("\n1. Loading data...")
    neighborhoods_df = pd.read_csv("data/neighborhoods.csv")
    minmax_df = pd.read_csv("results/scores_minmax.csv")
    log_df = pd.read_csv("results/scores_log.csv")
    comparison_df = pd.read_csv("results/scores_comparison.csv")
    comparison_df = comparison_df.sort_values('smartscore_minmax', ascending=False)

    # Load and filter POIs
    print("\n2. Loading POIs...")
    pois_dict = {}
    for domain in DOMAIN_COLORS.keys():
        file_path = f"data/pois/{domain}.csv"
        if os.path.exists(file_path):
            pois_df = pd.read_csv(file_path)
            pois_dict[domain] = filter_pois_within_radius(pois_df, neighborhoods_df)

    # Create map HTML
    print("\n3. Generating map...")
    map_html = create_map_html(neighborhoods_df, pois_dict)

    # Create score cards HTML
    print("\n4. Generating score cards...")
    scorecards_html = ''
    for _, nbh_row in comparison_df.iterrows():
        scorecards_html += create_scorecard_html(nbh_row, minmax_df, log_df)

    # Combine everything
    print("\n5. Creating combined HTML...")

    combined_html = f'''
    <!DOCTYPE html>
    <html>
    <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>SmartScore POC - Interactive Dashboard</title>
        <style>
            * {{
                margin: 0;
                padding: 0;
                box-sizing: border-box;
            }}
            body {{
                font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
                height: 100vh;
                overflow: hidden;
            }}
            .container {{
                display: flex;
                height: 100vh;
            }}
            .map-panel {{
                flex: 1;
                position: relative;
            }}
            .sidebar {{
                width: 380px;
                background: #f5f6fa;
                overflow-y: auto;
                border-left: 1px solid #ddd;
            }}
            .sidebar-header {{
                background: #2c3e50;
                color: white;
                padding: 20px;
                position: sticky;
                top: 0;
                z-index: 1000;
            }}
            .sidebar-header h1 {{
                font-size: 20px;
                margin-bottom: 5px;
            }}
            .sidebar-header p {{
                font-size: 12px;
                opacity: 0.8;
            }}
            .sidebar-content {{
                padding: 16px;
            }}
            .map-container {{
                width: 100%;
                height: 100%;
            }}
        </style>
    </head>
    <body>
        <div class="container">
            <div class="map-panel">
                <div class="map-container">
                    {map_html}
                </div>
            </div>
            <div class="sidebar">
                <div class="sidebar-header">
                    <h1>SmartScore Rankings</h1>
                    <p>10 neighborhoods scored by amenity density</p>
                </div>
                <div class="sidebar-content">
                    {scorecards_html}
                </div>
            </div>
        </div>
    </body>
    </html>
    '''

    # Save
    output_file = "smartscore_dashboard.html"
    with open(output_file, 'w', encoding='utf-8') as f:
        f.write(combined_html)

    file_size = os.path.getsize(output_file) / (1024 * 1024)
    print(f"\n6. Combined dashboard saved to: {output_file}")
    print(f"   File size: {file_size:.1f} MB")

    print("\n" + "=" * 70)
    print("Combined dashboard creation complete!")
    print("=" * 70)
    print(f"\nOpen {output_file} in your browser to view the interactive dashboard")

if __name__ == "__main__":
    main()
