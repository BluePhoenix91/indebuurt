#!/bin/bash
# =============================================================================
# Convert Overpass API JSON to GeoJSON format
#
# Overpass API returns OSM JSON format which ogr2ogr doesn't recognize.
# This script converts it to GeoJSON using jq.
#
# Usage:
#   ./convert-osm-to-geojson.sh
#
# Input: database/data/pois/*.json (OSM JSON format)
# Output: database/data/pois/*.geojson (GeoJSON format)
# =============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
INPUT_DIR="$PROJECT_ROOT/database/data/pois"

echo "Converting OSM JSON to GeoJSON..."

for json_file in "$INPUT_DIR"/*.json; do
    if [ ! -f "$json_file" ]; then
        echo "No JSON files found"
        exit 1
    fi

    domain=$(basename "$json_file" .json)
    geojson_file="$INPUT_DIR/${domain}.geojson"

    echo "Converting $domain..."

    # Convert OSM JSON to GeoJSON using jq
    # Handles both nodes (lat/lon) and ways/relations (center.lat/center.lon)
    jq '{
        "type": "FeatureCollection",
        "features": [
            .elements[] |
            {
                "type": "Feature",
                "properties": {
                    "id": .id,
                    "osm_type": .type,
                    "tags": .tags
                },
                "geometry": {
                    "type": "Point",
                    "coordinates": [
                        (if .lon then .lon else .center.lon end),
                        (if .lat then .lat else .center.lat end)
                    ]
                }
            }
        ] | map(select(.geometry.coordinates[0] != null))
    }' "$json_file" > "$geojson_file"

    feature_count=$(jq '.features | length' "$geojson_file")
    echo "  -> $geojson_file ($feature_count features)"
done

echo ""
echo "Conversion complete!"
echo ""
echo "Next steps:"
echo "1. Load GeoJSON files into PostgreSQL:"
echo ""
echo "   for f in database/data/pois/*.geojson; do"
echo "     ogr2ogr -f \"PostgreSQL\" \\"
echo "       \"PG:host=localhost dbname=buurtkompas user=postgres password=YOUR_PASSWORD\" \\"
echo "       \"\$f\" -nln staging_poi -append"
echo "   done"
echo ""
echo "2. Run the migration SQL in TablePlus:"
echo "   database/migrations/20250101_003_load-pois.sql"
