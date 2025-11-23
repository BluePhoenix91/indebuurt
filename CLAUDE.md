# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**indebuurt.be** is a neighborhood discovery platform for Belgium/Flanders that combines objective data with personal insights to help people find where they belong. The platform turns neighborhood data into intuitive insights, helping future residents decide where to live.

**Current Status:** Early planning/design phase. Repository contains analysis and POC documentation but no production code yet.

## Core Concept

The platform provides a **SmartScore** system that quantifies neighborhood liveability by combining:
- **OpenStreetMap (OSM)** data for points of interest (shops, cafes, schools, parks, gyms)
- **Statbel** (Belgian public statistics) for socioeconomic data (house prices, income, ownership rates, population)
- **First-party survey data** for resident sentiment and perceived neighborhood strengths

## Planned Architecture

### Data Foundation

| Data Source | Purpose | Technical Integration |
|-------------|---------|---------------------|
| OpenStreetMap via Geofabrik | POI extraction (shops, amenities, green spaces) | Extract using `osmium-tool`, convert to GeoJSON/CSV with GDAL |
| Statbel | Socioeconomic indicators | ETL pipeline for periodic ingestion |
| BEST (Belgian address registry) | Official address geocoding | Reliable area mapping |
| First-party surveys | Resident sentiment | Custom data collection and aggregation |

### Technical Stack

**Backend (.NET based on .gitignore):**
- API-first design (REST API for frontend and B2B integrations)
- Geospatial database (likely PostGIS) for spatial aggregation
- ETL pipeline for OSM and Statbel data processing
- Support for multiple spatial resolutions (city, neighborhood, postal code, address level)

**Geospatial Processing:**
- `osmium-tool` for filtering OSM data by tags (e.g., `shop=supermarket`)
- GDAL for GeoJSON conversion and coordinate extraction
- OSRM (Open Source Routing Machine) for routing and distance calculations
- PostGIS for KNN (k-nearest neighbors) queries and spatial operations

**Data Update Strategy:**
- Periodic refresh via incremental updates from Geofabrik
- Precomputed SmartScores cached for SEO-optimized static pages
- Dynamic comparison endpoints for interactive features

### Key Architectural Principles

1. **Flexible spatial aggregation:** Compute scores at multiple resolutions (city → neighborhood → postal code → address)
2. **Data modularity:** Keep OSM, Statbel, and survey data sources separable and composable
3. **Cache and pre-render:** Precompute static SmartScores for SEO while supporting dynamic queries
4. **API-first:** All data accessible via REST API for both web frontend and B2B integrations

## Data Processing Pipeline (from POC)

### OSM POI Extraction Workflow
```
1. Download Belgium extract from Geofabrik (.osm.pbf)
2. Filter by tags using osmium:
   osmium tags-filter belgium-latest.osm.pbf shop=supermarket -o supermarkets.osm.pbf
3. Convert to GeoJSON using GDAL
4. Extract centroids to CSV (lon, lat, name, tags)
5. Import to geospatial database for queries
```

### Distance Calculations
- **Straight-line ("vogelvlucht"):** Simple for prototypes, fast
- **Real routing:** Use OSRM Docker container for walking/driving distances
- **Batch queries:** OSRM `/table` endpoint for nearest-neighbor lookups at scale

## Core Features (MVP Scope)

**User-facing:**
- Neighborhood search (address, neighborhood, or city)
- SmartScore breakdown by domain (shops, education, green spaces, transport)
- Side-by-side neighborhood comparison
- Socioeconomic context (prices, income, ownership rates)
- Resident sentiment display

**Future B2B features:**
- Embeddable SmartScore widgets
- API access for contextual search
- Market insights dashboards for realtors

## Compliance & Licensing

**OpenStreetMap:** ODbL license allows commercial use when exposing derived insights (distances, scores) rather than raw OSM data. Attribution required.

## Phase Roadmap

1. **Phase 1 (MVP):** SEO validation, data integration, public web app with SmartScore
2. **Phase 2:** B2B integrations, widgets, API licensing
3. **Phase 3:** Personalization and neighborhood matchmaking
4. **Phase 4:** Premium insights suite for real estate professionals

## Key Terminology

- **SmartScore:** Aggregate livability score based on weighted sub-scores per domain (amenities, transport, green space, etc.)
- **POI:** Point of Interest from OpenStreetMap
- **Spatial resolution:** Level of geographic granularity (city/neighborhood/postal code/address)
- **Geofabrik:** Service providing regional OSM data extracts (updated daily/weekly)
- **OSRM:** Open Source Routing Machine for calculating real travel distances/times