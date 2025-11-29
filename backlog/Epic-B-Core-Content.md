# Epic B — Core Neighbourhood Page Content

## Story B1: Hero Section With Static Map Image ✅
> As a dog owner, I want a clear hero section, so I immediately understand what this neighbourhood is about.

**Acceptance Criteria:**
- [x] H1 renders the neighbourhood title (e.g. "Wonen in Antwerpen-Zuid")
- [x] Subtitle rendered directly under the title
- [x] Badges displayed for:
    - [x] Postal code
    - [x] Number of inhabitants
    - [x] Popularity / "hondvriendelijk" / "veel groen" labels
- [x] Static map placeholder image displayed on the right (no interactivity yet)
- [x] Map image has descriptive alt text (e.g. "Kaart van Antwerpen-Zuid")
- [x] Introductory paragraph rendered below the hero
- [x] All hero content is server-rendered in the HTML (no client-side fetch)

**Implementation Notes:**
- Created `web/src/data/neighborhoods.ts` with TypeScript interface for neighborhood data
- Implemented hero section in `web/src/pages/index.astro` with semantic HTML
- Created `web/src/styles/_hero.scss` with responsive grid layout matching design
- SVG placeholder map created at `web/public/images/maps/antwerpen-zuid-hero.svg`
- All content server-rendered, no client-side JavaScript required
- Verified build output contains all required elements

## Story B2: "What Dog Owners Value Here" Cards
> As a dog owner, I want to quickly scan the main advantages of the neighbourhood, so I know whether it fits my lifestyle.

**Acceptance Criteria:**
- [ ] Section heading for "Wat baasjes hier zo waarderen" rendered as an H2
- [ ] 6 cards implemented:
    - [ ] Dog parks
    - [ ] Vets
    - [ ] Pet stores
    - [ ] Green spaces
    - [ ] Public transport
    - [ ] Average house price
- [ ] Each card contains:
    - [ ] Icon placeholder
    - [ ] Title
    - [ ] Short description
    - [ ] Supporting detail line (distance or price)
- [ ] Card data loaded from a data structure (e.g. JSON or frontmatter), not hardcoded inline
- [ ] Responsive layout: 3 columns on desktop, 1–2 columns on smaller viewports
- [ ] Semantic HTML used (`<section>`, `<article>`, headings, paragraphs)

## Story B3: Dog Parks & Off-Leash Zones Section
> As a dog owner, I want to see nearby dog parks and their characteristics, so I know where I can go with my dog.

**Acceptance Criteria:**
- [ ] Section heading rendered for dog parks/off-leash zones
- [ ] Introductory text paragraph explains the section
- [ ] At least two park cards implemented (e.g. Morekstraat, Aziëstraat) with:
    - [ ] Park name
    - [ ] Estimated walk time or distance
    - [ ] Bullet list of key features (e.g. fenced, off-leash allowed, opening hours)
- [ ] Static map placeholder image placed to the right on desktop
- [ ] Map image has descriptive alt text (e.g. "Locaties van hondenspeelweides in Antwerpen-Zuid")
- [ ] On mobile, section stacks vertically in a sensible order (text, cards, then image)

## Story B4: Vets Section
> As a dog owner, I want to know which veterinarians are nearby, so I feel secure for emergencies.

**Acceptance Criteria:**
- [ ] Vets section has its own subheading
- [ ] List of vet cards implemented (minimum two), each including:
    - [ ] Practice name
    - [ ] Address
    - [ ] Estimated walk time or distance
- [ ] Static map placeholder image rendered next to the list on desktop
- [ ] Map image has descriptive alt text indicating vet locations
- [ ] Layout pattern consistent with the dog parks section
- [ ] All text content server-rendered and indexable

## Story B5: Pet Stores Section
> As a dog owner, I want to know where I can buy food and supplies, so I can plan my daily life.

**Acceptance Criteria:**
- [ ] Pet stores section has a subheading
- [ ] List of pet store cards implemented (minimum two), each including:
    - [ ] Store name
    - [ ] Address
    - [ ] Estimated walk time or distance
- [ ] Static map placeholder image rendered next to the list on desktop
- [ ] Map image has descriptive alt text indicating pet store locations
- [ ] Layout matches the vets section pattern for consistency
- [ ] Card content loaded from a data structure

## Story B6: Daily Life Narrative Block
> As a dog owner, I want a narrative about daily life in this neighbourhood, so I can imagine living there.

**Acceptance Criteria:**
- [ ] Section heading for "Wat dit betekent voor jouw dagelijkse leven…" rendered as H2
- [ ] One main explanatory paragraph rendered
- [ ] Bullet list of daily-life benefits (e.g. morning walks, quick access to parks, etc.)
- [ ] Semantic HTML used (`<p>`, `<ul>`, `<li>`)
- [ ] Wording includes relevant SEO phrases (e.g. "hondvriendelijke buurt") without keyword stuffing

## Story B7: "Baasjes Helpen Baasjes" Contribution Block
> As a dog owner, I want to share my experience of the neighbourhood, so I can help other owners.

**Acceptance Criteria:**
- [ ] Section heading for "Baasjes helpen baasjes" rendered
- [ ] Short intro text explains why feedback is useful
- [ ] Visual grey block styled according to design (or close MVP)
- [ ] Primary CTA button created (e.g. "Deel je ervaring over Antwerpen-Zuid")
- [ ] CTA navigates to an external survey page (Typeform/Google Forms) or a simple internal feedback page
- [ ] GA4 custom event fired on CTA click (using the implementation from Story A3)

## Story B8: Buurtstatistieken Section
> As a user, I want key numbers about the neighbourhood, so I can compare areas quickly.

**Acceptance Criteria:**
- [ ] Section heading for "Buurtstatistieken" rendered
- [ ] Statistic cards implemented for at least:
    - [ ] Median house price
    - [ ] Number of inhabitants
    - [ ] Available homes
    - [ ] Price per m²
- [ ] Each card shows:
    - [ ] Label
    - [ ] Large number/value
    - [ ] Short explanatory text if needed
- [ ] All values loaded from a structured data source (not copy-pasted literals)
- [ ] Responsive layout so cards remain readable on mobile
- [ ] Semantic HTML structure used

## Story B9: Interactive Maps with Location Markers
> As a user, I want to see POIs on an interactive map, so I can zoom and explore locations visually.

**Acceptance Criteria:**
- [ ] Leaflet.js integrated for map rendering (lightweight ~40kb library)
- [ ] Maps display OpenStreetMap tiles
- [ ] Numbered markers placed for each POI type (dog parks, vets, pet stores)
- [ ] Map center coordinates and POI markers loaded from neighborhood JSON data
- [ ] Maps work on mobile (touch zoom/pan enabled)
- [ ] Maps are client-side hydrated (page content remains server-rendered for SEO)
- [ ] At least 3 maps implemented matching the design:
    - [ ] Dog parks & off-leash zones map
    - [ ] Vets map
    - [ ] Pet stores map
- [ ] Marker click/hover shows POI name (optional tooltip)
