# SmartScore Algorithm & Weighting POC

## 1. Goal

Validate that the SmartScore concept produces **meaningful, differentiable, and intuitive results** that accurately reflect neighborhood liveability across different domains.

**Core questions to answer:**

1. **Does the algorithm differentiate neighborhoods effectively?**
   - Do urban centers score differently from suburbs?
   - Do quiet residential areas score differently from vibrant commercial districts?
   - Can the score identify "similar" neighborhoods across different cities?

2. **Do scores align with human intuition?**
   - Does a neighborhood known for excellent shopping score high on the "Winkels" domain?
   - Does a green suburban area score high on "Groen" and lower on "Nightlife"?
   - Would residents recognize their neighborhood in the score breakdown?

3. **Is the scoring methodology defensible?**
   - Can we explain why neighborhood A scores 8.2 and neighborhood B scores 6.5?
   - Are the weights between domains reasonable (e.g., should "Transport" count as much as "Schools")?
   - Does the scoring hold up under scrutiny from real estate professionals?

4. **What's the optimal scoring approach?**
   - Manual weighting vs. machine learning?
   - Normalized 0-10 scale vs. percentile ranking?
   - Absolute scores vs. relative comparisons?

**Success criteria:**

- Scores for 10 test neighborhoods align with local knowledge 80%+ of the time
- Domain breakdowns correctly identify neighborhood characteristics (e.g., city center = high shops, low green space)
- Score differences are large enough to be meaningful (min 0.5 point spread between distinct neighborhoods)
- Methodology can be explained in simple terms to non-technical users

**Failure signals:**

- All neighborhoods score between 6.0-7.0 (insufficient differentiation)
- Counterintuitive results (industrial area scores high on "Family-friendly")
- Inability to reproduce scores with same input data
- No clear correlation between POI density and score outcomes

## 2. Test Neighborhoods

Select 10 diverse neighborhoods across Flanders with well-known characteristics to validate scoring accuracy.

### Urban Centers (High Density, Commercial)
1. **Gent - Korenmarkt/Veldstraat area**
   - Expected profile: High shops, restaurants, nightlife, transport | Low green space, parking
   - Known for: Shopping district, student life, historic center

2. **Antwerpen - Meir district**
   - Expected profile: High retail, restaurants, culture, transport | Low green space, quiet
   - Known for: Main shopping street, commercial hub

### Residential Suburbs (Family-oriented)
3. **Sint-Martens-Latem (near Gent)**
   - Expected profile: High green space, schools, safety | Low nightlife, shops, transport
   - Known for: Affluent, green, family-friendly village

4. **Mortsel (near Antwerpen)**
   - Expected profile: Medium shops, schools, parks | Low nightlife, culture
   - Known for: Middle-class residential suburb

### Mixed Urban Neighborhoods
5. **Gent - Dampoort/Brugse Poort**
   - Expected profile: Medium shops, diverse, transport | Mixed green space, gentrifying area
   - Known for: Multicultural, evolving neighborhood

6. **Leuven - Wijgmaal**
   - Expected profile: Medium shops, schools, community feel | Suburban character
   - Known for: Village feel within city limits

### Student Districts
7. **Leuven - Oude Markt area**
   - Expected profile: High nightlife, restaurants, culture, transport | Low quiet, parking, green
   - Known for: Student nightlife, "longest bar in Europe"

### Green/Park Areas
8. **Edegem - 't Bist area**
   - Expected profile: High green space, quiet, safety | Low density, nightlife
   - Known for: Residential with nature access

### Small Town Centers
9. **Aalst - City center**
   - Expected profile: Medium shops, restaurants, culture | Regional town character
   - Known for: Mid-sized town center with local amenities

### Industrial/Working Class
10. **Gent - Gentbrugge/Ledeberg**
    - Expected profile: Mixed industrial-residential, improving connectivity | Historic working-class area
    - Known for: Former industrial area, affordable housing

## 3. Scoring Domains

Define 7-10 domains that collectively capture neighborhood liveability. Each domain aggregates related POI types.

### Core Domains (MVP)

| Domain | POI Categories (OSM tags) | Weight | Rationale |
|--------|---------------------------|--------|-----------|
| **Winkels** (Shopping) | `shop=supermarket, convenience, bakery, butcher` | 16% | Daily necessity access |
| **Restaurants & Cafés** | `amenity=restaurant, cafe, bar, fast_food` | 11% | Social life, dining options |
| **Groen** (Green Space) | `leisure=park, garden, dog_park`, `natural=wood` | 16% | Health, recreation, family appeal |
| **Onderwijs** (Education) | `amenity=school, kindergarten, library` | 13% | Family-friendly indicator |
| **Transport** | `public_transport=stop_position`, `railway=station` | 14% | Mobility, connectivity |
| **Sport & Fitness** | `leisure=fitness_centre, sports_centre, playground` | 10% | Active lifestyle |
| **Gezondheidszorg** (Healthcare) | `amenity=pharmacy, doctors, hospital, dentist` | 11% | Essential services |
| **Cultuur & Uitgaan** (Culture & Nightlife) | `amenity=cinema, theatre, nightclub`, `tourism=museum` | 9% | Entertainment, vibrancy |

**Notes:**
- Weights are preliminary and should be validated through POC testing
- All weights sum to 100%
- Consider user preference weighting in Phase 3

### Alternative Approaches to Explore

1. **User-adjustable weights:** Let users prioritize domains based on lifestyle
2. **Life-stage profiles:** Different weights for students, families, retirees
3. **Percentile scoring:** Rank neighborhoods relative to each other rather than absolute scale

## 4. Scoring Methodology

### Approach A: Density-Based Scoring (Recommended for POC)

For each domain, calculate a score based on POI density within a defined radius:

```
Domain Score = normalized(
  count(POIs within radius) / area
  OR
  weighted sum of (1/distance) for nearest N POIs
)
```

**Steps:**
1. For each test neighborhood, define a center point (lat/lon)
2. Count POIs per category within 1km radius
3. Normalize counts to 0-10 scale using min-max normalization across all test neighborhoods
4. Apply domain weights to get overall SmartScore

**Example calculation:**
```
Winkels score for Korenmarkt:
- 45 shops within 1km
- Max across all neighborhoods: 50 shops
- Min across all neighborhoods: 5 shops
- Normalized: (45-5)/(50-5) * 10 = 8.9

Overall SmartScore = Σ(domain_score * weight)
```

### Approach B: Distance to Nearest POIs

```
Domain Score = 10 - normalized(avg_distance to nearest 3-5 POIs)
```

Better for neighborhoods where proximity matters more than density.

### Approach C: Composite Metrics

Combine multiple factors:
- Count within 500m (walking distance)
- Count within 1km (cycling distance)
- Average distance to nearest 5 POIs
- Diversity score (variety of POI types)

### Normalization Strategy

POI density follows a power-law distribution (city centers have exponentially more amenities than suburbs). Normal distribution-based methods (z-score, mean-based) are not appropriate. Fixed thresholds require too much domain expertise upfront.

**Approach 1: Min-Max (Linear Scaling)**
- Scale based on best/worst in test set: `(value - min) / (max - min) × 10`
- Pro: Simple to understand, preserves relative differences
- Con: Sensitive to outliers; one mega-dense area can compress all suburbs to 0-2 range

**Approach 2: Logarithmic Scaling**
- Apply log transform before scaling: `log(value + 1) / log(max + 1) × 10`
- Pro: Models diminishing returns (50 vs 100 shops matters less than 5 vs 10); better suburban differentiation
- Con: Less intuitive to explain; can over-compress high values

**Approach 3: Combined (Log + Min-Max)**
- Log transform first to handle power-law distribution, then min-max for full 0-10 range
- Pro: Realistic diminishing returns + good differentiation across full spectrum
- Con: Two-step process adds complexity

**Decision:** Implement both min-max (baseline) and logarithmic (recommended) normalization in the POC. Generate parallel score reports to compare which produces more intuitive results when visualized on the map.

## 5. Data Requirements

### Minimum Viable Dataset

For each test neighborhood:

1. **Geographic definition**
   - Center point (lat/lon)
   - Radius (1km recommended) OR polygon boundary
   - Official name + city

2. **POI data from OSM**
   - Extract from Geofabrik Belgium export
   - Filter by relevant tags (see domain table)
   - Include: name, coordinates, category, tags

3. **Validation data** (manual collection)
   - Local knowledge survey: "On scale 1-10, how would you rate [neighborhood] for shopping?"
   - Resident interviews or online research
   - Real estate descriptions mentioning neighborhood characteristics

### Data Collection Steps

1. Define center coordinates for 10 test neighborhoods
2. Extract OSM data using osmium or Overpass API
3. Process into CSV: `neighborhood, lat, lon, domain, poi_name, poi_lat, poi_lon, distance`
4. Manually research each neighborhood's reputation for validation

## 6. Implementation Plan

### Phase 1: Data Preparation (Week 1)
- [ ] Define 10 test neighborhood center points
- [ ] Extract Belgium OSM data from Geofabrik
- [ ] Filter POIs by category using osmium
- [ ] Create CSV datasets per domain
- [ ] Manually document expected scores per neighborhood

### Phase 2: Algorithm Implementation (Week 1-2)
- [ ] Write Python/C# script to calculate domain scores
- [ ] Implement min-max normalization
- [ ] Apply domain weights for overall SmartScore
- [ ] Generate score reports for all 10 neighborhoods

### Phase 3: Validation & Iteration (Week 2)
- [ ] Compare computed scores to expected profiles
- [ ] Identify counterintuitive results
- [ ] Adjust weights and normalization approach
- [ ] Test alternative scoring methods (density vs. distance)
- [ ] Document findings and recommendations

### Phase 4: Documentation (Week 2)
- [ ] Write methodology explanation for non-technical audience
- [ ] Create visualizations (radar charts per neighborhood)
- [ ] Document assumptions and limitations
- [ ] Provide recommendations for production implementation

## 6.5. User Stories & Acceptance Criteria

### Story 1: Define Test Neighborhoods

**As a POC developer**, I want to identify and document 10 diverse test neighborhoods with their geographic centers, so that I have a consistent dataset to validate the scoring algorithm.

**Acceptance Criteria:**
- [x] 10 neighborhoods selected covering all categories (urban centers, suburbs, student districts, green areas, small towns, industrial)
- [x] Each neighborhood has a defined center point (lat, lon coordinates)
- [x] Coordinates are saved in a CSV or JSON file for reuse
- [x] Can manually verify on OpenStreetMap that coordinates are correctly placed in target neighborhoods

**Validation:** Open OSM, plot each coordinate, confirm it's in the intended neighborhood.

---

### Story 2: Extract Belgium OSM Data

**As a POC developer**, I want to download and prepare Belgium OSM data, so that I have a complete POI dataset to work with.

**Acceptance Criteria:**
- [x] Belgium OSM extract downloaded from Geofabrik (`.osm.pbf` format)
- [x] File is recent (within last 30 days)
- [x] File size is reasonable (~400-600MB for Belgium)
- [x] Can verify file integrity (not corrupted)
- [x] File is stored in a known location accessible to scripts

**Validation:** Run `osmium fileinfo belgium-latest.osm.pbf` to confirm valid OSM data.

---

### Story 3: Filter POIs by Domain

**As a POC developer**, I want to extract POIs for each domain (shops, restaurants, parks, etc.) from the Belgium OSM dataset, so that I can analyze them separately.

**Acceptance Criteria:**
- [x] 8 separate POI datasets created (one per domain: Winkels, Restaurants, Groen, Onderwijs, Transport, Sport, Gezondheidszorg, Cultuur)
- [x] Each dataset includes: POI name, latitude, longitude, OSM tags
- [x] Filters correctly apply OSM tag criteria (e.g., `shop=supermarket` for Winkels)
- [x] Datasets are in CSV format for easy inspection
- [x] Can manually verify that a known POI appears in correct domain dataset (e.g., "Delhaize Korenmarkt" in Winkels)

**Validation:** Open CSV, search for known POIs (e.g., "Gravensteen" in Cultuur), confirm presence and correct coordinates.

---

### Story 4: Create Initial Leaflet Map with POIs

**As a POC developer**, I want to create a basic interactive map showing neighborhoods and POIs, so that I can visually validate data quality BEFORE implementing scoring logic.

**Acceptance Criteria:**
- [x] Map displays all of Belgium with appropriate zoom level to see all 10 neighborhoods
- [x] Each neighborhood center point is marked with a labeled marker
- [x] 1km radius circle drawn around each neighborhood center (for scoring reference)
- [x] POI markers loaded within 2km of neighborhoods for broader context (optimized for performance)
- [x] POI markers plotted for all 8 domains, color-coded by domain type
- [x] POI markers are clickable and show: POI name, domain category, OSM type and ID
- [x] Map can be panned and zoomed
- [x] Map exports to standalone HTML file (5-10 MB, browser-friendly)
- [x] Layer control allows toggling domains on/off
- [x] Can visually confirm POI distribution matches expectations (dense in cities, sparse in suburbs)

**Validation:** Open map, zoom to Korenmarkt, visually confirm many shop markers; zoom to Latem, confirm fewer POIs and more green space markers. Check that POI categories look correct. File should load quickly in browser.

**Why this early?** Visual inspection catches data extraction issues before investing time in scoring calculations.

---

### Story 5: Calculate POIs Within Radius

**As a POC developer**, I want to count POIs within 1km of each neighborhood center point, so that I can score each domain based on amenity density.

**Acceptance Criteria:**
- [x] For each neighborhood × domain combination, count of POIs within 1km radius is calculated (80 combinations: 10 neighborhoods × 8 domains)
- [x] Distance calculation uses correct geospatial formula (haversine formula implemented)
- [x] Results stored in structured format (CSV with neighborhood, domain, count columns)
- [x] Summary table created showing neighborhoods as rows, domains as columns
- [x] Can verify counts by visual inspection on the map created in Story 4
- [x] Edge case handled: POIs exactly at 1km boundary are treated consistently (using <= comparison)
- [x] Statistics calculated: total POIs, averages, min/max neighborhoods

**Validation:** Compare calculated counts to what's visible on the map for 2-3 neighborhoods; numbers should roughly match visual density. Urban centers (Meir: 811 POIs, Korenmarkt: 784 POIs) have significantly more than suburbs (Sint-Martens-Latem: 18 POIs).

---

### Story 6: Implement Scoring with Dual Normalization

**As a POC developer**, I want to calculate domain scores using both min-max and logarithmic normalization, so that I can compare which approach produces more intuitive results.

**Acceptance Criteria:**
- [x] Domain scores calculated for all 10 neighborhoods × 8 domains using min-max normalization
- [x] Domain scores calculated for all 10 neighborhoods × 8 domains using logarithmic normalization
- [x] Scores are on 0-10 scale (min-max: 0.12-7.45, log: 2.08-8.75)
- [x] Overall SmartScore calculated by applying domain weights (16%, 11%, 16%, 13%, 14%, 10%, 11%, 9%)
- [x] Overall SmartScore is on 0-10 scale
- [x] Results stored in comparable format (scores_comparison.csv with both normalization methods side-by-side)
- [x] Detailed scores saved per neighborhood × domain for analysis (scores_minmax.csv, scores_log.csv)

**Validation:** Check that min-max produces values spanning close to 0-10 range (✓ 0.12-7.45), and log scores are generally higher for suburbs (✓ average 2.33 points higher, max 3.44 points).

---

### Story 7: Create Interactive Dashboard with Map and Score Cards

**As a POC reviewer**, I want to see an interactive dashboard combining the map and score cards in one view, so that I can visually validate scores against POI distribution and quickly compare neighborhoods.

**Acceptance Criteria:**
- [x] Split-screen layout: interactive map on left, scrollable score cards on right
- [x] Map displays all neighborhoods, 1km circles, and POIs within 2km (color-coded by domain)
- [x] Map has layer controls to toggle domains on/off
- [x] Score cards show all 10 neighborhoods sorted by SmartScore (high to low)
- [x] Each score card displays: name, city, SmartScore (min-max), SmartScore (log), and compact domain breakdown
- [x] Domain breakdowns show POI counts and visual progress bars (0-10 scale)
- [x] Domain colors consistent between map and score cards
- [x] Single HTML file that works in any browser (smartscore_dashboard.html, 6-8 MB)
- [x] Responsive design with sticky sidebar header

**Validation:** Check that expected patterns hold (✓ Meir scores highest at 7.45 with dense POI coverage on map, ✓ Sint-Martens-Latem scores lowest at 0.12 with sparse POIs, ✓ Visual correlation between map density and scores).

---

### Story 8: Export and Share Results

**As a POC reviewer**, I want to export the visualization and results, so that I can share findings with stakeholders.

**Acceptance Criteria:**
- [x] Leaflet map exports to standalone HTML file that opens in any browser (smartscore_dashboard.html)
- [x] Score results export to CSV files for further analysis (5 CSV files in results/ folder)
- [x] HTML file is self-contained (no external dependencies - Folium embeds all resources)
- [x] Files can be shared via email or file storage (regular files, no server required)
- [x] Opening HTML file shows functional interactive map without additional setup

**Validation:** HTML file opens in any browser with full functionality. CSV files can be opened in Excel/Google Sheets for analysis.

---

### Story 9: Document Findings and Recommendations

**As a POC reviewer**, I want a concise summary of findings, so that I can decide on next steps for MVP development.

**Acceptance Criteria:**
- [x] Document which normalization approach (min-max vs log) is recommended and why (Min-max recommended for stronger differentiation)
- [x] List any domain weight adjustments suggested based on results (Reduce Groen from 16%→10% due to incomplete OSM data)
- [x] Identify data quality issues discovered (Green space under-mapped: only 252 POIs; suburban POI gaps minimal)
- [x] Provide 3-5 key takeaways about algorithm viability (5 key takeaways documented)
- [x] Recommend whether to proceed to MVP or iterate on POC (✅ PROCEED TO MVP - confidence 9/10)

**Validation:** Document is clear enough that someone unfamiliar with the POC can understand outcomes and decisions. See POC_FINDINGS.md for complete summary.

## 7. Tools & Technologies

**Decision: Python**

Given the requirement for Leaflet map visualization and rapid iteration, Python is the optimal choice.

**Tech stack:**
- **Folium** - Python library for Leaflet maps, renders in Jupyter and exports to standalone HTML
- **Pandas** - CSV data manipulation and score calculations
- **GeoPandas** - Spatial operations (distance calculations, radius filtering)
- **Jupyter Notebook** - Interactive development and inline map display

**Required packages:**
```
pip install pandas geopandas folium jupyter
```

**How Folium + Jupyter works:**
- Folium creates Leaflet maps that render **directly inside Jupyter notebook cells** as interactive widgets
- When you create a map object and display it in a cell, it appears inline with full pan/zoom/click functionality
- No separate browser window needed during development
- Maps can also be exported to standalone HTML files using `map.save('output.html')`
- This gives you both: inline development/validation AND shareable HTML files

**Typical workflow:**
1. Create map in notebook: `map = folium.Map(...)`
2. Add markers, circles, etc.: `folium.Marker(...).add_to(map)`
3. Display inline: just type `map` in a cell and run it
4. Export when done: `map.save('smartscore_map.html')`

**Project Structure:**
All POC work should be contained in the `poc_smartscore/` folder at repository root:
```
poc_smartscore/
├── data/
│   ├── neighborhoods.csv          # Test neighborhood definitions
│   ├── belgium-latest.osm.pbf     # OSM data from Geofabrik
│   └── pois/                      # Extracted POI CSVs per domain
│       ├── winkels.csv
│       ├── restaurants.csv
│       └── ...
├── smartscore_analysis.ipynb      # Main Jupyter notebook
├── smartscore_map.html            # Exported map visualization
└── results/
    └── scores.csv                 # Final score results
```

All file creation, data downloads, and exports should use paths relative to `poc_smartscore/`.

## 8. Expected Outputs

### Primary Deliverable: Interactive Map Visualization

**Goal:** Visual inspection of POI distribution and calculated scores (similar to POC 1 approach)

**Core components:**

1. **Leaflet Map**
   - 10 neighborhood center points marked
   - 1km radius circles around each center
   - POI markers color-coded by domain (shops, restaurants, parks, etc.)
   - Clickable markers showing POI details
   - Visual validation: "Does this neighborhood really have more shops?"

2. **Neighborhood Score Cards/List**
   - Simple display showing all 10 neighborhoods
   - Overall SmartScore (0-10) for each
   - Domain breakdown scores visible
   - Side-by-side comparison of min-max vs logarithmic normalization
   - Sorted by score (high to low)

**Implementation:**
- Python-based (fastest for POC)
- Either Jupyter notebook with inline display OR simple HTML file
- Whichever is easier to build and share
- Focus: functional and clear, not polished

### Supporting Outputs

3. **Basic Validation Notes**
   - Quick assessment: do scores align with expected neighborhood profiles?
   - List any surprising/counterintuitive results
   - Simple observations about data quality

4. **Next Steps Recommendation**
   - Which normalization approach to use for MVP?
   - Any domain weight adjustments needed?
   - Data gaps or issues to address

## 9. Success Metrics

The POC is successful if:

- ✅ 8+ out of 10 neighborhoods have intuitive score profiles
- ✅ Score spread is at least 2.0 points (max - min ≥ 2.0)
- ✅ Domain breakdowns correctly identify 1-2 defining characteristics per neighborhood
- ✅ Methodology can be explained in <500 words to non-technical stakeholder
- ✅ Scores are reproducible (same inputs = same outputs)
- ✅ At least one scoring approach shows clear promise for production

## 10. Risks & Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| OSM data gaps in residential areas | Low differentiation between suburbs | Validate POI coverage before committing to methodology |
| All neighborhoods score similarly | Algorithm doesn't differentiate | Test multiple normalization approaches; expand radius |
| Weights feel arbitrary | Stakeholder pushback | Document rationale; prepare to iterate based on feedback |
| Scores don't match intuition | Loss of credibility | Build in validation step; interview locals |
| Over-optimization for test set | Doesn't generalize | Keep methodology simple; avoid overfitting |

## 11. Next Steps After POC

If successful:
- Expand to 50+ neighborhoods across Flanders
- Integrate with PostGIS for production scalability
- Add Statbel socioeconomic data layer
- Build API endpoint exposing SmartScore calculations
- Design frontend visualization of scores

If needs refinement:
- Iterate on domain definitions
- Explore ML-based weighting
- Test different radius sizes
- Consider time-based factors (rush hour transport, evening safety)
