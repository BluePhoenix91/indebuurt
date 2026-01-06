#!/bin/bash
# =============================================================================
# Master Database Setup Script
#
# Runs all data loading scripts in the correct order to set up the database
# from scratch. Run this from WSL after PostgreSQL is running.
#
# Usage:
#   chmod +x database/scripts/setup-all.sh
#   ./database/scripts/setup-all.sh
#
# Prerequisites:
#   - PostgreSQL running with PostGIS extension
#   - Initial schema migration already run (20250101_001_initial-schema.sql)
#   - Statbel GeoJSON already loaded (see database/README.md for H4)
#   - Environment: WSL with jq, curl, ogr2ogr installed
#
# What this script does:
#   1. Fetch POI data from Overpass API
#   2. Convert Overpass JSON to GeoJSON
#   3. Load GeoJSON into PostgreSQL staging table
#
# After running this script:
#   Run database/migrations/20250101_003_load-pois.sql in TablePlus
# =============================================================================

set -e  # Exit on error

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo ""
echo "=============================================="
echo "  Database Setup - All Scripts"
echo "=============================================="
echo ""

# Check prerequisites
echo -e "${YELLOW}Checking prerequisites...${NC}"

if ! command -v jq &> /dev/null; then
    echo -e "${RED}ERROR: jq is not installed. Install with: sudo apt install jq${NC}"
    exit 1
fi

if ! command -v curl &> /dev/null; then
    echo -e "${RED}ERROR: curl is not installed. Install with: sudo apt install curl${NC}"
    exit 1
fi

if ! command -v ogr2ogr &> /dev/null; then
    echo -e "${RED}ERROR: ogr2ogr is not installed. Install with: sudo apt install gdal-bin${NC}"
    exit 1
fi

echo -e "${GREEN}All prerequisites installed.${NC}"
echo ""

# Get database credentials
echo "Enter PostgreSQL connection details:"
read -p "Host [localhost]: " DB_HOST
DB_HOST=${DB_HOST:-localhost}

read -p "Database [buurtkompas]: " DB_NAME
DB_NAME=${DB_NAME:-buurtkompas}

read -p "User [postgres]: " DB_USER
DB_USER=${DB_USER:-postgres}

read -sp "Password: " DB_PASS
echo ""

if [ -z "$DB_PASS" ]; then
    echo -e "${RED}ERROR: Password is required${NC}"
    exit 1
fi

PG_CONN="PG:host=$DB_HOST dbname=$DB_NAME user=$DB_USER password=$DB_PASS"

# Test connection
echo ""
echo -e "${YELLOW}Testing database connection...${NC}"
if ! ogr2ogr --version &> /dev/null; then
    echo -e "${RED}ERROR: Cannot verify ogr2ogr${NC}"
    exit 1
fi
echo -e "${GREEN}ogr2ogr available.${NC}"
echo ""

# =============================================================================
# Step 1: Fetch POI data from Overpass API
# =============================================================================
echo "=============================================="
echo "  Step 1/3: Fetching POI data from Overpass"
echo "=============================================="
echo ""

"$SCRIPT_DIR/pois/fetch.sh"

# Check if fetch was successful
POI_COUNT=$(ls -1 "$PROJECT_ROOT/database/data/pois"/*.json 2>/dev/null | wc -l)
if [ "$POI_COUNT" -lt 6 ]; then
    echo -e "${RED}ERROR: Expected 6 JSON files, found $POI_COUNT${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}Step 1 complete: $POI_COUNT JSON files downloaded${NC}"
echo ""

# =============================================================================
# Step 2: Convert Overpass JSON to GeoJSON
# =============================================================================
echo "=============================================="
echo "  Step 2/3: Converting to GeoJSON"
echo "=============================================="
echo ""

"$SCRIPT_DIR/pois/convert-to-geojson.sh"

# Check if conversion was successful
GEOJSON_COUNT=$(ls -1 "$PROJECT_ROOT/database/data/pois"/*.geojson 2>/dev/null | wc -l)
if [ "$GEOJSON_COUNT" -lt 6 ]; then
    echo -e "${RED}ERROR: Expected 6 GeoJSON files, found $GEOJSON_COUNT${NC}"
    exit 1
fi

echo ""
echo -e "${GREEN}Step 2 complete: $GEOJSON_COUNT GeoJSON files created${NC}"
echo ""

# =============================================================================
# Step 3: Load GeoJSON into PostgreSQL
# =============================================================================
echo "=============================================="
echo "  Step 3/3: Loading GeoJSON into PostgreSQL"
echo "=============================================="
echo ""

LOADED=0
for f in "$PROJECT_ROOT/database/data/pois"/*.geojson; do
    filename=$(basename "$f")
    echo "Loading $filename..."

    if ogr2ogr -f "PostgreSQL" "$PG_CONN" "$f" -nln staging_poi -append 2>/dev/null; then
        LOADED=$((LOADED + 1))
        echo "  -> OK"
    else
        echo -e "${RED}  -> FAILED${NC}"
    fi
done

echo ""
if [ "$LOADED" -eq "$GEOJSON_COUNT" ]; then
    echo -e "${GREEN}Step 3 complete: $LOADED files loaded into staging_poi${NC}"
else
    echo -e "${YELLOW}WARNING: Only $LOADED of $GEOJSON_COUNT files loaded${NC}"
fi

# =============================================================================
# Summary
# =============================================================================
echo ""
echo "=============================================="
echo "  Setup Complete!"
echo "=============================================="
echo ""
echo "Data is now in the staging_poi table."
echo ""
echo -e "${YELLOW}Next steps:${NC} Run these scripts in TablePlus or psql:"
echo ""
echo "  1. Transform staging to pois table:"
echo "     \\i database/scripts/pois/transform-staging-to-pois.sql"
echo ""
echo "  2. Enrich POIs with city/postal_code from statistical sectors:"
echo "     \\i database/scripts/geo/enrich-pois-with-address.sql"
echo ""
echo "Step 1 extracts address fields from OSM tags."
echo "Step 2 fills gaps via spatial join with statistical_sectors (~80% coverage)."
echo ""
