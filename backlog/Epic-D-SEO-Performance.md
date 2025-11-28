# Epic D — SEO Hardening & Performance

## Story D1: Structured Data (Schema.org JSON-LD)
> As a search engine, I want structured data for the neighbourhood, so I can better understand and present the page.

**Acceptance Criteria:**
- [ ] JSON-LD `<script type="application/ld+json">` added to the page
- [ ] Uses appropriate schema type (`Place` / `Neighborhood` or closest match)
- [ ] Includes at least:
    - [ ] Neighbourhood name
    - [ ] Postal code
    - [ ] City
    - [ ] Country
    - [ ] Short description
    - [ ] Approximate geo coordinates
- [ ] BreadcrumbList JSON-LD added for site structure (e.g. Home → Wonen met hond → Antwerpen-Zuid)
- [ ] JSON-LD validated with Google Rich Results Test or similar without critical errors

## Story D2: Lighthouse SEO & Performance Pass
> As the site owner, I want the neighbourhood page to score highly on SEO and performance audits, so it ranks well and feels fast.

**Acceptance Criteria:**
- [ ] Lighthouse SEO score ≥ 90 for the neighbourhood page
- [ ] No major accessibility issues reported by Lighthouse (e.g. contrast, labels)
- [ ] Images optimised (reasonable sizes, optional srcset for hero and maps)
- [ ] No unnecessary JavaScript loaded on the page beyond GA and minimal behaviour
- [ ] No horizontal scrolling on common viewports
- [ ] Cumulative layout shift (CLS) kept low (no big jumps during load)

---

## Appendix: Neighborhood JSON Data Structure

Each neighborhood should have a JSON file in `src/data/neighborhoods/` following this structure:

```json
{
  "slug": "gent-centrum",
  "title": "Wonen in Gent-Centrum",
  "subtitle": "Voor baasjes die op zoek zijn naar een buurt die klapt voor hen én hun hond",
  "badges": [
    { "icon": "postcode", "label": "Postcode 9000" },
    { "icon": "population", "label": "12,490 inwoners" },
    { "icon": "tag", "label": "Hondvriendelijk" },
    { "icon": "tag", "label": "Populaire buurt" },
    { "icon": "tag", "label": "Veel groen" }
  ],
  "intro": "Gent-Centrum is een van de meest hondvriendelijke buurten van Gent...",
  "mapCenter": {
    "lat": 51.0543,
    "lng": 3.7174
  },
  "highlights": [
    {
      "title": "Hondenspeelparken",
      "description": "Je vindt hier maar liefst 3 hondenparken binnen wandelafstand",
      "detail": "Maximaal binnen 500 meter",
      "icon": "dog-park",
      "walkTime": "10 mins"
    },
    {
      "title": "Dierenartsen",
      "description": "Altijd één hulp bij de hand",
      "detail": "Maximaal binnen 1.000 meter",
      "icon": "vet",
      "walkTime": "15 mins"
    },
    {
      "title": "Dierenwinkels",
      "description": "Voor alle extra spullen blijf je in de buurt",
      "detail": "Maximaal binnen 600 meter",
      "icon": "pet-store",
      "walkTime": "8 mins"
    },
    {
      "title": "Groene ruimtes",
      "description": "Je moet er even voor lopen",
      "detail": "Maximaal binnen ± 2.000 meter",
      "icon": "park",
      "walkTime": "25 mins"
    },
    {
      "title": "Openbaar vervoer",
      "description": "Je vaart ermee onder auto",
      "detail": "Maximaal binnen 600 meter",
      "icon": "transit",
      "walkTime": "10 mins"
    },
    {
      "title": "Gemiddelde woningprijs",
      "description": "Prijsgemiddelde in de met meeste buurten",
      "detail": "€ 385.000 mediaan",
      "icon": "house"
    }
  ],
  "dogParks": [
    {
      "name": "1. Morekstraat",
      "walkTime": "10 mins",
      "features": [
        "± 50 m² - Een kleine & fijne speelweide",
        "Omheind – Feeling voor honden dat niet altijd zo",
        "Open 24/7 – Elke naar verloop of late wandelsessies"
      ],
      "coordinates": {
        "lat": 51.0543,
        "lng": 3.7174
      }
    },
    {
      "name": "2. Aziëstraat",
      "walkTime": "15 mins",
      "features": [
        "± 90 m² - Een kleine & fijne speelweide",
        "Omheind – Feeling voor honden dat niet altijd zo",
        "Open 24/7 – Elke naar verloop of late wandelsessies"
      ],
      "coordinates": {
        "lat": 51.0560,
        "lng": 3.7200
      }
    }
  ],
  "vets": [
    {
      "name": "Kat en Ko",
      "address": "Maurice Dequekerstraat 25A, 9000 Gent",
      "walkTime": "8 mins",
      "coordinates": {
        "lat": 51.0530,
        "lng": 3.7180
      }
    },
    {
      "name": "WelketPets",
      "address": "Koopvaardijstraat 4-6/5, 9000 Gent",
      "walkTime": "12 mins",
      "coordinates": {
        "lat": 51.0520,
        "lng": 3.7190
      }
    }
  ],
  "petStores": [
    {
      "name": "Berko",
      "address": "Maurice Dequekerstraat 25A, 9000 Gent",
      "walkTime": "8 mins",
      "coordinates": {
        "lat": 51.0540,
        "lng": 3.7185
      }
    },
    {
      "name": "WelketPets",
      "address": "Koopvaardijstraat 4-6/5, 9000 Gent",
      "walkTime": "12 mins",
      "coordinates": {
        "lat": 51.0525,
        "lng": 3.7195
      }
    }
  ],
  "dailyLife": {
    "intro": "Wat dit betekent voor jouw dagelijkse leven met je viervoeter",
    "benefits": [
      "Je dag begint doorgaans wanneer je hond zijn energie kwijt kan in het park in de Morekstraat, nog voor je aan je koffie toe bent.",
      "De buurt voelt sociaal en veitrouwd: baasjes herkennen elkaar, groeten elkaar en delen tips over nieuwe hondvriendelijke plekjes.",
      "Spullen vergeten of noodgeval? Of het nu voer of een snelle trip naar de dierenarts is, alles is nét zo dichtbij dat je leven 'makkelijker zonder dat je het doorhebt'.",
      "De combinatie van korte wandelafstanden en langere groene routes maakt je leven 'makkelijker zonder dat je het doorhebt'."
    ]
  },
  "contributionCTA": {
    "heading": "Baasjes helpen baasjes",
    "intro": "Help andere baasjes en schetsen hoe hondvriendelijk deze buurt is. Vul de enquête hieronder in? Het neemt niet geen 5 minuten in beslag en je wordt hun grote held.",
    "buttonText": "Deel je ervaring over Gent-Centrum",
    "buttonUrl": "https://forms.gle/example"
  },
  "statistics": {
    "medianPrice": 485175,
    "inhabitants": 12250,
    "availableHomes": 29,
    "pricePerSqm": 3650
  }
}
```

**Notes:**
- Coordinates use `{ "lat": number, "lng": number }` format for Leaflet compatibility
- All walk times are strings (e.g., "10 mins", "15 mins")
- Icons are referenced by name (implementation will map to actual icon components)
- The structure supports all content sections defined in Epic B
