#!/bin/bash
# =============================================================================
# Fetch POI data from Overpass API
#
# This script downloads POI data for Flanders + Brussels from OpenStreetMap
# via the Overpass API. Run this from WSL.
#
# Usage:
#   chmod +x database/scripts/fetch-pois.sh
#   ./database/scripts/fetch-pois.sh           # Download missing/stale files
#   ./database/scripts/fetch-pois.sh --force   # Re-download all files
#
# Options:
#   --force    Re-download all files regardless of age
#   --max-age  Maximum age in days before re-downloading (default: 7)
#
# Output: JSON files in database/data/pois/
# =============================================================================

set -e  # Exit on error

OVERPASS_URL="https://overpass-api.de/api/interpreter"
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
QUERIES_DIR="$PROJECT_ROOT/database/queries"
OUTPUT_DIR="$PROJECT_ROOT/database/data/pois"

# Default settings
FORCE_DOWNLOAD=false
MAX_AGE_DAYS=7

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --force)
            FORCE_DOWNLOAD=true
            shift
            ;;
        --max-age)
            MAX_AGE_DAYS="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            echo "Usage: $0 [--force] [--max-age DAYS]"
            exit 1
            ;;
    esac
done

# Create output directory if it doesn't exist
mkdir -p "$OUTPUT_DIR"

echo "==================================="
echo "Fetching POI data from Overpass API"
echo "==================================="
echo "Queries directory: $QUERIES_DIR"
echo "Output directory: $OUTPUT_DIR"
echo "Max age: $MAX_AGE_DAYS days"
echo "Force download: $FORCE_DOWNLOAD"
echo ""

# Function to check if file needs download
needs_download() {
    local output_file="$1"

    # If force mode, always download
    if [ "$FORCE_DOWNLOAD" = true ]; then
        return 0
    fi

    # If file doesn't exist, download
    if [ ! -f "$output_file" ]; then
        return 0
    fi

    # If file is empty or invalid JSON, download
    if [ ! -s "$output_file" ]; then
        return 0
    fi

    # Check if file has valid elements array
    ELEMENT_COUNT=$(jq '.elements | length' "$output_file" 2>/dev/null || echo "0")
    if [ "$ELEMENT_COUNT" -eq 0 ]; then
        return 0
    fi

    # Check file age
    local file_age_days
    if [[ "$OSTYPE" == "darwin"* ]]; then
        # macOS
        file_age_days=$(( ($(date +%s) - $(stat -f %m "$output_file")) / 86400 ))
    else
        # Linux/WSL
        file_age_days=$(( ($(date +%s) - $(stat -c %Y "$output_file")) / 86400 ))
    fi

    if [ "$file_age_days" -ge "$MAX_AGE_DAYS" ]; then
        return 0
    fi

    # File is fresh and valid
    return 1
}

# Count total queries
TOTAL=$(ls -1 "$QUERIES_DIR"/*.overpassql 2>/dev/null | wc -l)
CURRENT=0
DOWNLOADED=0
SKIPPED=0

for query_file in "$QUERIES_DIR"/*.overpassql; do
    if [ ! -f "$query_file" ]; then
        echo "No .overpassql files found in $QUERIES_DIR"
        exit 1
    fi

    CURRENT=$((CURRENT + 1))
    domain=$(basename "$query_file" .overpassql)
    output_file="$OUTPUT_DIR/${domain}.json"

    if needs_download "$output_file"; then
        echo "[$CURRENT/$TOTAL] Fetching $domain..."

        # Rate limiting - wait before request (except first)
        if [ "$DOWNLOADED" -gt 0 ]; then
            echo "         Waiting 10 seconds (rate limiting)..."
            sleep 10
        fi

        # Call Overpass API
        HTTP_STATUS=$(curl -s -w "%{http_code}" -d "@$query_file" "$OVERPASS_URL" -o "$output_file")

        if [ "$HTTP_STATUS" -eq 200 ]; then
            # Check if we got valid JSON with elements
            ELEMENT_COUNT=$(jq '.elements | length' "$output_file" 2>/dev/null || echo "0")
            if [ "$ELEMENT_COUNT" -gt 0 ]; then
                echo "         -> $output_file ($ELEMENT_COUNT elements)"
                DOWNLOADED=$((DOWNLOADED + 1))
            else
                echo "         -> WARNING: No elements in response"
                rm -f "$output_file"
            fi
        else
            echo "         -> ERROR: HTTP $HTTP_STATUS"
            rm -f "$output_file"
        fi
    else
        ELEMENT_COUNT=$(jq '.elements | length' "$output_file" 2>/dev/null || echo "0")
        echo "[$CURRENT/$TOTAL] Skipping $domain (fresh file with $ELEMENT_COUNT elements)"
        SKIPPED=$((SKIPPED + 1))
    fi
done

echo ""
echo "==================================="
echo "Download complete!"
echo "==================================="
echo "Downloaded: $DOWNLOADED files"
echo "Skipped: $SKIPPED files (already fresh)"
echo ""
echo "Files in output directory:"
ls -lh "$OUTPUT_DIR"/*.json 2>/dev/null || echo "No JSON files found"
echo ""

# Check if all files exist
MISSING=0
for query_file in "$QUERIES_DIR"/*.overpassql; do
    domain=$(basename "$query_file" .overpassql)
    output_file="$OUTPUT_DIR/${domain}.json"
    if [ ! -f "$output_file" ] || [ ! -s "$output_file" ]; then
        echo "WARNING: Missing or empty: $domain.json"
        MISSING=$((MISSING + 1))
    fi
done

if [ "$MISSING" -gt 0 ]; then
    echo ""
    echo "Some files are missing. Run again or check for errors above."
    exit 1
fi

echo ""
echo "Next steps:"
echo "1. Convert Overpass JSON to GeoJSON:"
echo "   ./database/scripts/pois/convert-to-geojson.sh"
echo ""
echo "2. Load GeoJSON files into PostgreSQL:"
echo ""
echo "   for f in database/data/pois/*.geojson; do"
echo "     ogr2ogr -f \"PostgreSQL\" \\"
echo "       \"PG:host=localhost dbname=buurtkompas user=postgres password=YOUR_PASSWORD\" \\"
echo "       \"\$f\" -nln staging_poi -append"
echo "   done"
echo ""
echo "3. Run the migration SQL in TablePlus:"
echo "   database/migrations/20250101_003_load-pois.sql"
echo ""
echo "Or run the master script to do all steps:"
echo "   ./database/scripts/setup-all.sh"
