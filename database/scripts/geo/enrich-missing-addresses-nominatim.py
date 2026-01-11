#!/usr/bin/env python3
"""
Enrich missing POI addresses via reverse geocoding.

Two-step process:
1. Nominatim (OSM) - Get street names
2. Flemish Geolocation API - Get house numbers (for POIs still missing them)

Usage:
    python enrich-missing-addresses-nominatim.py [--category vet|pet_store|all]
    python enrich-missing-addresses-nominatim.py --skip-nominatim  # Only run Flemish API

Nominatim usage policy: max 1 request/second, identify with User-Agent.
https://operations.osmfoundation.org/policies/nominatim/

Flemish Geolocation API: Official Belgian address registry
https://geo.api.vlaanderen.be/
"""

import argparse
import json
import os
import time
import urllib.request
import urllib.error
from datetime import datetime
from pathlib import Path

# Database connection via psycopg2
try:
    import psycopg2
    from psycopg2.extras import RealDictCursor
except ImportError:
    print("Error: psycopg2 not installed. Run: pip install psycopg2-binary")
    exit(1)


def get_database_url() -> str:
    """Get database URL from environment or .env file."""
    # Check environment first
    if os.environ.get("GIS_DATABASE_URL"):
        return os.environ["GIS_DATABASE_URL"]
    if os.environ.get("DATABASE_URL"):
        return os.environ["DATABASE_URL"]

    # Try to load from .env files
    for env_path in [
        Path(__file__).parent.parent.parent.parent / "web" / ".env",
        Path(__file__).parent.parent.parent.parent / ".env",
    ]:
        if env_path.exists():
            with open(env_path) as f:
                for line in f:
                    if line.startswith("GIS_DATABASE_URL=") or line.startswith("DATABASE_URL="):
                        return line.split("=", 1)[1].strip()

    # Default for local development (buurtkompas database)
    return "postgresql://postgres:admin@localhost:5432/buurtkompas"


# API Configuration
NOMINATIM_URL = "https://nominatim.openstreetmap.org/reverse"
FLEMISH_GEO_URL = "https://geo.api.vlaanderen.be/geolocation/v4/Location"
USER_AGENT = "buurtkompas-address-enrichment/1.0 (https://buurtkompas.be)"
REQUEST_DELAY = 1.1  # seconds between requests (Nominatim policy: 1/sec max)
FLEMISH_REQUEST_DELAY = 0.2  # Flemish API is faster, but be polite


def get_pois_missing_street(conn, category: str = "all"):
    """Fetch POIs with missing street data."""
    query = """
        SELECT id, osm_id, name, category,
               ST_Y(location::geometry) as lat,
               ST_X(location::geometry) as lon,
               postal_code, city, street, house_number
        FROM pois
        WHERE street IS NULL
    """
    if category != "all":
        query += f" AND category = '{category}'"
    query += " ORDER BY category, city, name"

    with conn.cursor(cursor_factory=RealDictCursor) as cur:
        cur.execute(query)
        return cur.fetchall()


def get_pois_missing_house_number(conn, category: str = "all"):
    """Fetch POIs that have street but missing house number."""
    query = """
        SELECT id, osm_id, name, category,
               ST_Y(location::geometry) as lat,
               ST_X(location::geometry) as lon,
               postal_code, city, street, house_number
        FROM pois
        WHERE street IS NOT NULL
          AND (house_number IS NULL OR house_number = '')
    """
    if category != "all":
        query += f" AND category = '{category}'"
    query += " ORDER BY category, city, name"

    with conn.cursor(cursor_factory=RealDictCursor) as cur:
        cur.execute(query)
        return cur.fetchall()


def reverse_geocode_nominatim(lat: float, lon: float) -> dict:
    """Call Nominatim reverse geocoding API."""
    params = f"?lat={lat}&lon={lon}&format=json&addressdetails=1"
    url = NOMINATIM_URL + params

    req = urllib.request.Request(url)
    req.add_header("User-Agent", USER_AGENT)

    try:
        with urllib.request.urlopen(req, timeout=10) as response:
            data = json.loads(response.read().decode("utf-8"))
            return data.get("address", {})
    except urllib.error.URLError as e:
        print(f"  Error: {e}")
        return {}


def reverse_geocode_flemish(lat: float, lon: float) -> dict:
    """Call Flemish Geolocation API for reverse geocoding.

    Returns dict with 'street' and 'house_number' keys.
    API docs: https://geo.api.vlaanderen.be/
    """
    params = f"?latlon={lat},{lon}"
    url = FLEMISH_GEO_URL + params

    req = urllib.request.Request(url)
    req.add_header("User-Agent", USER_AGENT)

    try:
        with urllib.request.urlopen(req, timeout=10) as response:
            data = json.loads(response.read().decode("utf-8"))

            # Parse the FormattedAddress field (e.g., "Weibroekdreef 107, 9990 Maldegem")
            locations = data.get("LocationResult", [])
            if not locations:
                return {}

            # Get the first (closest) result
            location = locations[0]
            formatted = location.get("FormattedAddress", "")

            # Parse: "Street HouseNumber, PostalCode City"
            if not formatted or "," not in formatted:
                return {}

            street_part = formatted.split(",")[0].strip()

            # Try to extract house number from end of street part
            parts = street_part.rsplit(" ", 1)
            if len(parts) == 2 and parts[1] and parts[1][0].isdigit():
                return {
                    "street": parts[0],
                    "house_number": parts[1]
                }
            else:
                # No house number in response
                return {
                    "street": street_part,
                    "house_number": None
                }

    except urllib.error.URLError as e:
        print(f"  Error: {e}")
        return {}
    except (json.JSONDecodeError, KeyError, IndexError) as e:
        print(f"  Parse error: {e}")
        return {}


def generate_sql_update(poi: dict, street: str = None, house_number: str = None) -> str | None:
    """Generate SQL UPDATE statement for a POI."""
    if not street and not house_number:
        return None

    parts = []

    if street:
        street_escaped = street.replace("'", "''")
        parts.append(f"street = '{street_escaped}'")

    if house_number:
        house_number_escaped = house_number.replace("'", "''")
        parts.append(f"house_number = '{house_number_escaped}'")

    parts.append("updated_at = NOW()")

    set_clause = ", ".join(parts)

    # Use osm_id for stability across re-imports
    return f"UPDATE pois SET {set_clause} WHERE osm_id = '{poi['osm_id']}'; -- {poi['name'] or 'Unnamed'}"


def run_nominatim_step(conn, args) -> tuple[list[str], dict]:
    """Step 1: Enrich missing streets via Nominatim."""
    print("\n" + "=" * 60)
    print("STEP 1: Nominatim (OSM) - Getting street names")
    print("=" * 60)

    pois = get_pois_missing_street(conn, args.category)

    if args.limit:
        pois = pois[:args.limit]

    print(f"Found {len(pois)} POIs with missing street data")

    if not pois:
        return [], {"success": 0, "no_street": 0, "errors": 0}

    sql_statements = []
    success_count = 0
    no_street_count = 0
    error_count = 0

    # Track POIs that got street but no house number (for step 2)
    needs_house_number = []

    for i, poi in enumerate(pois, 1):
        name = poi['name'] or 'Unnamed'
        city = poi['city'] or 'Unknown city'
        print(f"[{i}/{len(pois)}] {name} ({city})...", end=" ", flush=True)

        address = reverse_geocode_nominatim(poi['lat'], poi['lon'])

        if not address:
            print("ERROR")
            error_count += 1
        elif not address.get("road"):
            print("no street found")
            no_street_count += 1
        else:
            street = address.get("road")
            house_number = address.get("house_number")
            print(f"{street} {house_number or '(no number)'}".strip())

            sql = generate_sql_update(poi, street=street, house_number=house_number)
            if sql:
                sql_statements.append(sql)
                success_count += 1

                # Track if we need house number
                if not house_number:
                    poi_copy = dict(poi)
                    poi_copy['street'] = street
                    needs_house_number.append(poi_copy)

        # Rate limiting
        time.sleep(REQUEST_DELAY)

    stats = {
        "success": success_count,
        "no_street": no_street_count,
        "errors": error_count,
        "needs_house_number": needs_house_number
    }

    print(f"\nNominatim: {success_count} streets found, {no_street_count} no street, {error_count} errors")
    print(f"           {len(needs_house_number)} POIs still need house numbers")

    return sql_statements, stats


def run_flemish_step(conn, args, pois_from_step1: list = None) -> tuple[list[str], dict]:
    """Step 2: Enrich missing house numbers via Flemish Geolocation API."""
    print("\n" + "=" * 60)
    print("STEP 2: Flemish Geolocation API - Getting house numbers")
    print("=" * 60)

    # If we have POIs from step 1, use those; otherwise query DB
    if pois_from_step1:
        pois = pois_from_step1
        print(f"Using {len(pois)} POIs from step 1 that need house numbers")
    else:
        pois = get_pois_missing_house_number(conn, args.category)
        if args.limit:
            pois = pois[:args.limit]
        print(f"Found {len(pois)} POIs in database with street but no house number")

    if not pois:
        return [], {"success": 0, "no_number": 0, "errors": 0}

    sql_statements = []
    success_count = 0
    no_number_count = 0
    error_count = 0

    for i, poi in enumerate(pois, 1):
        name = poi['name'] or 'Unnamed'
        city = poi['city'] or 'Unknown city'
        current_street = poi.get('street', '')
        print(f"[{i}/{len(pois)}] {name} ({city}, {current_street})...", end=" ", flush=True)

        address = reverse_geocode_flemish(poi['lat'], poi['lon'])

        if not address:
            print("ERROR")
            error_count += 1
        elif not address.get("house_number"):
            print("no house number found")
            no_number_count += 1
        else:
            house_number = address.get("house_number")
            flemish_street = address.get("street")
            print(f"→ {house_number}")

            # Only update house_number (street already set from Nominatim or DB)
            sql = generate_sql_update(poi, house_number=house_number)
            if sql:
                sql_statements.append(sql)
                success_count += 1

        # Rate limiting (Flemish API is faster but be polite)
        time.sleep(FLEMISH_REQUEST_DELAY)

    stats = {
        "success": success_count,
        "no_number": no_number_count,
        "errors": error_count
    }

    print(f"\nFlemish API: {success_count} house numbers found, {no_number_count} not found, {error_count} errors")

    return sql_statements, stats


def main():
    parser = argparse.ArgumentParser(
        description="Enrich POI addresses via Nominatim and Flemish Geolocation API",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  python enrich-missing-addresses-nominatim.py --category vet
      Run both steps for vets: Nominatim for streets, then Flemish API for house numbers

  python enrich-missing-addresses-nominatim.py --skip-nominatim --category vet
      Skip Nominatim, only run Flemish API for POIs that already have streets

  python enrich-missing-addresses-nominatim.py --category all --output addresses.sql
      Process all categories and save SQL to file
        """
    )
    parser.add_argument("--category", choices=["vet", "pet_store", "all"], default="vet",
                        help="POI category to process (default: vet)")
    parser.add_argument("--skip-nominatim", action="store_true",
                        help="Skip step 1 (Nominatim), only run step 2 (Flemish API)")
    parser.add_argument("--dry-run", action="store_true",
                        help="Only print SQL, don't execute")
    parser.add_argument("--limit", type=int, default=None,
                        help="Limit number of POIs to process per step")
    parser.add_argument("--output", type=str, default=None,
                        help="Output SQL file path")
    args = parser.parse_args()

    db_url = get_database_url()
    print(f"Connecting to database...")
    conn = psycopg2.connect(db_url)

    all_sql_statements = []
    all_sql_statements.append(f"-- Address enrichment via reverse geocoding")
    all_sql_statements.append(f"-- Generated: {datetime.now().isoformat()}")
    all_sql_statements.append(f"-- Category: {args.category}")
    all_sql_statements.append(f"-- Skip Nominatim: {args.skip_nominatim}")
    all_sql_statements.append("")

    needs_house_number = []

    # Step 1: Nominatim (unless skipped)
    if not args.skip_nominatim:
        nominatim_sql, nominatim_stats = run_nominatim_step(conn, args)
        all_sql_statements.extend(nominatim_sql)
        needs_house_number = nominatim_stats.get("needs_house_number", [])

        all_sql_statements.append("")
        all_sql_statements.append(f"-- Nominatim: {nominatim_stats['success']} streets, {nominatim_stats['no_street']} no street, {nominatim_stats['errors']} errors")
        all_sql_statements.append("")

    # Step 2: Flemish Geolocation API
    flemish_sql, flemish_stats = run_flemish_step(
        conn, args,
        pois_from_step1=needs_house_number if not args.skip_nominatim else None
    )
    all_sql_statements.extend(flemish_sql)

    all_sql_statements.append("")
    all_sql_statements.append(f"-- Flemish API: {flemish_stats['success']} house numbers, {flemish_stats['no_number']} not found, {flemish_stats['errors']} errors")

    # Final summary
    all_sql_statements.append("")
    all_sql_statements.append(f"-- Total SQL statements: {len([s for s in all_sql_statements if s.startswith('UPDATE')])}")

    sql_output = "\n".join(all_sql_statements)

    if args.output:
        with open(args.output, "w", encoding="utf-8") as f:
            f.write(sql_output)
        print(f"\nSQL written to: {args.output}")
    else:
        print("\n" + "=" * 60)
        print(sql_output)
        print("=" * 60)

    total_updates = len([s for s in all_sql_statements if s.startswith('UPDATE')])
    print(f"\nDone. {total_updates} total SQL UPDATE statements generated.")

    conn.close()


if __name__ == "__main__":
    main()
