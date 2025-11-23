# indebuurt.be — Business Concept & Product Evolution

_(Internal concept note – updated November 2025)_

---

## 0. Context Update

In the initial concept, indebuurt.be explored **SmartScores** to quantify neighbourhood livability.  
After UX research and experimentation, we’ve reframed the concept toward **human-centred proximity and belonging** — helping people find areas that _feel_ close enough to what matters to them, instead of abstract numeric rankings.

The **SmartScore POC** remains valuable as a foundation for the data model and domain weighting logic, but the product direction now prioritises **explainable, intuitive “neighbourhood experience labels”** such as:

> “Daily groceries around the corner,” “Short bike to work,” or “Quiet but well-connected.”

---

## 1. Vision

**indebuurt.be** helps people discover _where they truly belong_ — not by scoring neighbourhoods, but by showing where life fits their daily rhythm.

It translates data about shops, schools, parks, and transport into **human language**, bridging the gap between **objective data** and **subjective feeling**.  
The goal remains the same: give future residents confidence in where to live, and give real-estate partners richer, data-driven storytelling.

---

## 2. The Opportunity

Most real-estate searches focus on the **house**, not the **context**.  
Yet in surveys, over 70 % of respondents say the **neighbourhood** is their main deciding factor.

Belgian portals today provide only surface-level context — basic statistics or distances.  
There’s a clear opportunity to add value with **smarter, data-backed but human-readable insights**.

**indebuurt.be** can fill this gap by combining:

- **OpenStreetMap (OSM)** → points of interest (shops, cafés, schools, parks, etc.)
- **Public data (Statbel)** → house prices, income, ownership, demographics
- **First-party data** → user sentiment about local experience

These sources allow indebuurt.be to describe not just _how far things are_, but _what life feels like there_.

This creates a dual advantage:

- 💡 **SEO advantage** — unique, data-rich pages that answer high-intent searches like “Wonen in Gentbrugge” or “Beste buurten in Leuven”.
- 💰 **B2B value** — contextual data layers and widgets for real-estate portals and agents.

---

## 3. MVP / POC Goal

**Objective:**  
Prove that contextual, proximity-based neighbourhood descriptions increase **SEO visibility** and **user engagement**.

**Output:**  
A public web app where visitors can describe what matters (“near work”, “close to a Delhaize”, “green streets”) and instantly see which neighbourhoods fit those wishes.

**SmartScore heritage:**  
The earlier SmartScore POC provided validated sub-domain weights now reused to define **proximity thresholds** and **label rules**.

**Success criteria:**

- Indexed, ranking SEO pages (“Wonen in [stad]”)
- Engagement with discovery interactions
- Proven data aggregation workflow

---

## 4. Data Foundation

| Source                       | Example Data                                                         | Usage                                                                                   |
| ---------------------------- | -------------------------------------------------------------------- | --------------------------------------------------------------------------------------- |
| **OpenStreetMap / POI data** | Shops, cafés, schools, parks, gyms                                   | Proximity & convenience indicators (“Groceries within walking distance”, “Park nearby”) |
| **Statbel**                  | Median house price, income, ownership vs rental                      | Socio-economic enrichment                                                               |
| **First-party survey data**  | Residents’ perceived strengths (“rust & zekerheid”, “verbondenheid”) | Sentiment & local authenticity                                                          |
| **Address registry (BEST)**  | Official address structure, coordinates                              | Reliable area mapping and geocoding                                                     |

All sources are publicly available or self-generated, ensuring **compliance and repeatability** while allowing unique **first-party differentiation**.

---

## 5. Core Features (MVP Scope)

**User-facing features**

- 🔎 _Neighbourhood discovery_ — “Where could I live that’s close to…?”
- 🌿 _Proximity & convenience labels_ — human-readable summaries (“Groceries nearby”, “Good transit access”)
- ⚖️ _Compare neighbourhoods_ — side-by-side narrative and Statbel stats
- 💬 _Resident sentiment_ — “Wat mensen hier fijn vinden”
- 🏡 _Contextual housing insight_ — price, income, and ownership data

**Partner-facing features (Phase 2)**

- Embeddable **neighbourhood summary widget** for listings
- **API access** for contextual search and lead routing (“Toon buurten die passen bij dit pand”)
- **Market insights dashboard** for realtors (“Uw regio scoort hoog op verbondenheid en rust”)

---

## 6. Product Evolution & Monetization

| Phase                          | Focus                                            | Output                                 | Monetization Path                       |
| ------------------------------ | ------------------------------------------------ | -------------------------------------- | --------------------------------------- |
| **Phase 1 — MVP**              | SEO validation & data integration                | indebuurt.be comfort-zone app          | Proof of SEO uplift and engagement      |
| **Phase 2 — B2B Integrations** | Realtor & portal embedding                       | Widgets + API                          | B2B licensing or SaaS model             |
| **Phase 3 — Personalization**  | Matchmaking between user wishes & neighbourhoods | “Welke buurt past bij jou?” tool       | Co-branded experiences, lead generation |
| **Phase 4 — Insights Suite**   | Realtor dashboards                               | Regional benchmarks, sentiment reports | Premium data subscriptions              |

---

## 7. Next Steps

- ✅ Retain SmartScore findings as baseline for domain weights
- ⚙️ Develop OSM → street-sample → neighbourhood aggregation pipeline
- 🧮 Integrate Statbel socio-economic datasets
- 🧠 Launch first-party survey model for local insights
- 🚀 Launch pilot for Gent (city-level focus)
- 📈 Measure SEO performance and engagement

---

## 8. Preliminary Technical Approach

The indebuurt.be platform remains modular and data-driven but now centres on **pre-computed proximity labels** rather than real-time scoring.

**Guiding principles**

- **Data modularity:** combine OSM, Statbel, and survey data in a unified geospatial model.
- **Street-based sampling:** evaluate distances from realistic living points (street midpoints).
- **Offline computation:** pre-generate neighbourhood labels for instant lookup.
- **Caching and pre-rendering:** static pages for SEO; dynamic filters for exploration.
- **API-first:** expose labels, POI counts, and stats for frontend & B2B.
- **Scalable enrichment:** incremental data updates (monthly) to keep insights fresh.

**Initial focus**

- ETL pipeline for OSM POI extraction and Statbel enrichment
- Batch process for street-sample generation and proximity classification
- PostGIS database for geospatial aggregation
- Lightweight API exposing pre-computed neighbourhood features
- Basic frontend for search, comparison, and storytelling pages

---

## 9. Technical Validation POCs

Before building the full MVP, the following proof-of-concepts validate technical feasibility and de-risk key architecture choices.

### 9.1. SmartScore Algorithm & Weighting POC

**Status:** ✅ **COMPLETE** - [See POC Findings](../poc_smartscore/POC_FINDINGS.md)

**Goal:** Validate that the scoring concept produces meaningful, differentiable results

**What to test:**

- Define scoring algorithm for 3-5 domains (shops, education, green space, transport)
- Test on 5-10 known neighborhoods with different characteristics
- Validate that scores align with intuitive expectations (city centers score high on shops, suburbs on green space)
- Determine if manual weighting or ML-based weighting works better

**Technical scope:** Python/C# script processing CSV data  
**Risk if skipped:** Might build a full system that produces useless scores

**Results:**

- ✅ Algorithm validated with 8 domains across 10 diverse neighborhoods
- ✅ Score range: 0.12 – 7.45 (strong differentiation)
- ✅ Scores align with human intuition (10/10 neighborhoods match expectations)
- ✅ Manual weighting recommended (16 % Winkels/Groen, 14 % Transport, etc.)
- ✅ **Recommendation:** PROCEED TO MVP with min-max normalization
- 📊 [Interactive Dashboard](../poc_smartscore/smartscore_dashboard.html)
- 📄 [Full POC Specification](../analyse/poc_smartscore.md)

---

### 9.2. PostGIS Spatial Aggregation Performance POC

**Goal:** Prove the database can handle spatial queries and street-sample aggregation at scale.

**What to test:**

- Import full Belgium OSM dataset into PostGIS
- Extract residential street segments and sample every ~100 m
- Test KNN queries for “nearest 5 supermarkets” at 10 000 points
- Measure aggregation from street → neighbourhood → city levels
- Validate index strategies (GiST, BRIN)

**Technical scope:** Docker PostGIS + benchmark scripts  
**Risk if skipped:** Performance issues could make batch jobs too slow or costly

---

### 9.3. Street Sampling & Label Aggregation POC

**Status:** ✅ **COMPLETE** - [See POC Findings](../poc_street_sampling/POC_FINDINGS.md)

**Goal:** Validate feasibility of the street-midpoint sampling and label-generation pipeline.

**What to test:**

- Extract residential streets from OSM
- Generate sample points every 500 m (minimum 1 per street)
- Compute nearest POIs (supermarkets, PT stops, parks/green spaces) per point
- Aggregate per neighbourhood using median distances → classify into human-readable labels
  ("Groceries within walking distance", "Excellent PT access")
- Evaluate runtime & storage size

**Technical scope:** Python + GeoPandas + Folium (PostGIS for production)
**Risk if skipped:** Neighbourhood summaries may be inaccurate or unscalable

**Results:**

- ✅ Methodology validated: Street sampling with 500m intervals provides representative data
- ✅ Performance excellent: ~2.4 min for 10 neighborhoods, ~1-2 hours for all Flanders (with 8-core parallelization)
- ✅ Urban center accuracy: 100% match rate (Korenmarkt, Dampoort, Wijgmaal, Gentbrugge)
- ⚠️ Overall label accuracy: 66.7% (below 80% target, but fixable with threshold adjustments)
- ✅ Scalability confirmed: 4,416 sample points generated, 883 calculations/second, 18 MB storage
- ⚠️ Data quality issues: PT threshold too lenient (400m → 250-300m), green space data incomplete (38 POIs, includes gardens)
- ✅ **Recommendation:** PROCEED TO MVP with PT threshold adjustment + green space filtering
- 🗺️ [Interactive Map](../poc_street_sampling/street_sampling_map.html)
- 📄 [Full POC Specification](../analyse/poc_street_sampling.md)

---

### 9.4. Statbel Data Integration POC

**Goal:** Verify Statbel data is accessible, structured, and mappable to geographic areas.

**What to test:**

- Download & parse Statbel datasets (house prices, income, ownership)
- Map data to postal codes / statistical sectors / NIS codes
- Join with OSM neighbourhood geometry
- Validate completeness & update frequency
- Test API availability (if any)

**Technical scope:** ETL script + data quality report  
**Risk if skipped:** Data gaps or mismatched geography discovered too late

---

### 9.5. SEO Content Generation POC

**Goal:** Validate that qualitative, proximity-based content can rank and attract organic traffic.

**What to test:**

- Generate 10–20 static HTML pages for different neighbourhoods
- Include data-driven narratives (“Daily groceries nearby”, “Short bike to center”)
- Deploy to test domain with schema.org markup and meta tags
- Monitor Google Search Console for indexing & impressions (4–6 weeks)
- A/B test narrative vs score-based versions

**Technical scope:** Static site generator + Search Console tracking  
**Risk if skipped:** SEO traction is core to the business model

---

### 9.6. OSRM Routing Integration POC (Medium)

**Goal:** Validate real travel times for future precision upgrade.

**What to test:**

- Set up OSRM Docker with Belgium data
- Compare straight-line vs walking/driving distances for 1 000 pairs
- Measure latency & memory footprint
- Evaluate if precomputation of travel tables is practical

**Technical scope:** OSRM Docker + batch script  
**Risk if skipped:** Later accuracy improvements may be harder to add

---

### 9.7. Data Freshness Pipeline POC (Medium)

**Goal:** Prove automated incremental data updates are feasible.

**What to test:**

- Set up automated Geofabrik download (weekly)
- Implement delta detection (what changed since last update)
- Test incremental recalculation vs full rebuild
- Measure processing time & resources

**Technical scope:** Cron job + diff logic + ETL  
**Risk if skipped:** Manual updates won’t scale; stale data hurts credibility

---

## 10. Summary

indebuurt.be now focuses on **discovering fit, not scores**.  
By combining pre-computed proximity data, intuitive neighbourhood labels, and SEO-friendly storytelling, the MVP will show that people don’t need numbers to understand where they belong — they need **context that feels real**.

The validated SmartScore foundations ensure methodological rigour, while the new comfort-zone model delivers clarity, speed, and emotional resonance — bridging data and belonging in the Belgian housing journey.
