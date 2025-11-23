# Belgian POI & Routing POC — Summary

## 1. Context & Objective

This proof-of-concept explored how to build a **local, structured database of Points of Interest (POIs)** in Belgium — starting with supermarkets — and how to integrate proximity logic (e.g. “nearest supermarket to an address”) into a lightweight map viewer.

The goal was to assess **data quality**, **technical feasibility**, and **integration paths** for potential future features such as contextual property insights (“supermarkets within 1 km”) or neighborhood scoring.

---

## 2. What We Built

### 🗺️ Front-end POC

- A lightweight **Leaflet viewer** displaying:
  - Uploaded address datasets (`lat,lon`).
  - Uploaded POI datasets (e.g. `supermarkets.csv`).
  - Real-time geocoding via **Nominatim**.
  - “Find nearest supermarket” function computing **straight-line distance** (“vogelvlucht”).
- Added sample data and CSV parsing to allow fully offline testing.

### 🧰 Data Extraction Pipeline

- Used **OpenStreetMap (OSM)** data via the **Geofabrik Belgium extract** (`.osm.pbf`).
- Converted and filtered with **osmium-tool** and **GDAL**:
  1. `osmium tags-filter` for category extraction (`shop=supermarket`, etc.).
  2. Export to GeoJSON.
  3. Convert to centroid CSV (with lon/lat) for ingestion.
- Resulting CSVs (supermarkets, gyms, parks, …) are lightweight and easily visualized or joined with address data.

### 🧩 Data Validation

- Visual inspection confirmed OSM coverage in Belgium is **sufficiently dense for supermarkets** and **reasonable for other amenities**.
- Some rural gaps exist; expected from volunteer-contributed OSM data.

---

## 3. What We Learned

| Area                   | Key Insight                                                                                                                   |
| ---------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| **Data availability**  | OSM + Geofabrik gives good nationwide coverage for most amenity types.                                                        |
| **Legal position**     | OSM’s ODbL license allows **commercial use** if we only expose derived insights (e.g. distances) and include attribution.     |
| **Performance**        | Local CSV or DB queries are fast; live Overpass API is not suitable for production.                                           |
| **Routing**            | Straight-line distance works for early prototypes, but real travel distance/time requires a routing engine (OSRM or similar). |
| **Tooling**            | Osmium + GDAL is enough for data preparation; OSRM (Docker) can provide production-grade routing.                             |
| **Developer friction** | Setup is reproducible on WSL / Linux with minimal dependencies.                                                               |

---

## 4. Next Technical Steps (Follow-up POCs)

### A. Routing & Distance Validation

- **Host OSRM locally** for Belgium and compare _vogelvlucht_ vs. driving/walking distances.
- Evaluate query latency and memory footprint.
- Test the `/table` endpoint for batch nearest-neighbor lookups.

### B. Data Enrichment & Scoring

- Extend beyond supermarkets:
  - `leisure=fitness_centre`
  - `leisure=park`, `garden`, `dog_park`
  - `amenity=pharmacy`, etc.
- Aggregate into per-address context metrics:
  - Distances
  - Count within radius (e.g. 1 km)
  - Weighted “amenity score”

### C. Backend Integration

- Move from browser CSV parsing to a small service layer:
  - Simple API: `POST /nearest?lat=&lon=&type=supermarket`
  - Cache POI coordinates in PostGIS for fast KNN queries.
- Optionally expose a bulk enrichment endpoint for property datasets.

### D. Infrastructure & Data Updates

- Automate weekly OSM updates (Geofabrik sync + osmium pipeline).
- Consider delta imports to avoid full rebuilds.

### E. Product-Level Exploration (still early)

- Map possible use-cases:
  - Real-estate context (nearest amenities, livability indexes).
  - B2B POI API for portals or CRMs.
- Validate potential data gaps (e.g. retail chains missing in OSM).
- Assess privacy and attribution requirements for production.

---

## 5. Open Questions Before Productization

- **Coverage accuracy:** Is OSM sufficient for commercial reliability across all POI categories?
- **Data licensing:** Is ODbL attribution sufficient, or do clients expect closed-data sources?
- **Performance at scale:** How many concurrent POI lookups per second can OSRM or PostGIS handle?
- **Integration model:** API, enrichment service, or embedded dataset?
- **Value layer:** What unique value does aggregation or scoring add beyond raw POI proximity?

---

## 6. Summary

We successfully demonstrated that:

- Open-data sources (OSM + Geofabrik) provide a **viable foundation** for Belgian POI enrichment.
- A fully local pipeline — **Osmium → CSV → OSRM → Viewer** — is technically straightforward.
- Next focus should be **routing accuracy**, **scalability**, and **data quality validation** before any product framing.

This POC establishes the technical base from which we can explore product opportunities confidently — still early-stage, still exploratory, but on solid technical ground.
