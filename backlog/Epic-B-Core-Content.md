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

## Story B2: "What Dog Owners Value Here" Cards ✅

> As a dog owner, I want to quickly scan the main advantages of the neighbourhood, so I know whether it fits my lifestyle.

**Acceptance Criteria:**

- [x] Section heading for "Wat baasjes hier zo waarderen" rendered as an H2
- [x] 6 cards implemented:
  - [x] Dog parks
  - [x] Vets
  - [x] Pet stores
  - [x] Green spaces
  - [x] Public transport
  - [x] Average house price
- [x] Each card contains:
  - [x] Icon placeholder
  - [x] Title
  - [x] Short description
  - [x] Supporting detail line (distance or price)
- [x] Card data loaded from a data structure (e.g. JSON or frontmatter), not hardcoded inline
- [x] Responsive layout: 3 columns on desktop, 1–2 columns on smaller viewports
- [x] Semantic HTML used (`<section>`, `<article>`, headings, paragraphs)

## Story B3: Dog Parks & Off-Leash Zones Section ✅

> As a dog owner, I want to see nearby dog parks and their characteristics, so I know where I can go with my dog.

**Acceptance Criteria:**

- [x] Section heading rendered for dog parks/off-leash zones
- [x] Introductory text paragraph explains the section
- [x] At least two park cards implemented (e.g. Morekstraat, Aziëstraat) with:
  - [x] Park name
  - [x] Estimated walk time or distance
  - [x] Key features displayed (e.g. fenced, off-leash allowed, opening hours)
- [x] Static map placeholder image placed to the right on desktop
- [x] Map image has descriptive alt text (e.g. "Locaties van hondenspeelweides in Antwerpen-Zuid")
- [x] On mobile, section stacks vertically in a sensible order (text, cards, then image)

**Implementation Notes:**

- Created `ParkCard.astro` component with semantic HTML structure
- Added `DogPark` interface to `neighborhoods.ts` with park data structure
- Implemented two park cards: Morekstraat and Aziëstraat with features
- Created `_dog-parks.scss` with responsive grid layout (2 columns on desktop, stacked on mobile)
- Generated placeholder SVG map with numbered markers at `web/public/images/maps/antwerpen-zuid-dogparks.svg`
- All content server-rendered, no client-side JavaScript required
- Verified build output contains all required elements

## Story B4: Vets Section ✅

> As a dog owner, I want to know which veterinarians are nearby, so I feel secure for emergencies.

**Acceptance Criteria:**

- [x] Vets section has its own subheading
- [x] List of vet cards implemented (minimum two), each including:
  - [x] Practice name
  - [x] Address
  - [x] Estimated walk time or distance
- [x] Static map placeholder image rendered next to the list on desktop
- [x] Map image has descriptive alt text indicating vet locations
- [x] Layout pattern consistent with the dog parks section
- [x] All text content server-rendered and indexable

**Implementation Notes:**

- Created `VetCard.astro` component using established card mixins (`base-card`, `card-header`, `distance-display`)
- Uses primary color scheme (`$primary`) for visual distinction from dog parks (secondary) and value cards (accent-2)
- Address formatted on two lines: street + number + bus (optional) on first line, postal code + municipality on second line
- Added `Vet` interface to `neighborhoods.ts` with structured address fields (street, streetNumber, bus?, municipality, postalCode)
- Implemented three vet practices: Dierenkliniek Antwerpen-Zuid, Dierenarts Van den Berghe, Dierenkliniek Deurne
- Created `_vets.scss` following same pattern as `_dog-parks.scss`:
  - Grid layout (2 columns on desktop, stacked on mobile)
  - Horizontal scroll container mixin for mobile
  - Sticky map with dynamic height matching list content (max-height for viewport constraint)
- Generated placeholder SVG map with numbered markers at `web/public/images/maps/antwerpen-zuid-vets.svg`
- Map height dynamically matches list height when shorter than viewport, scrolls correctly when longer
- All content server-rendered, no client-side JavaScript required
- Verified build output contains all required elements

## Story B5: Pet Stores Section ✅

> As a dog owner, I want to know where I can buy food and supplies, so I can plan my daily life.

**Acceptance Criteria:**

- [x] Pet stores section has a subheading
- [x] List of pet store cards implemented (minimum two), each including:
  - [x] Store name
  - [x] Address
  - [x] Estimated walk time or distance
- [x] Static map placeholder image rendered next to the list on desktop
- [x] Map image has descriptive alt text indicating pet store locations
- [x] Layout matches the vets section pattern for consistency
- [x] Card content loaded from a data structure

**Implementation Notes:**

- Created `PetStoreCard.astro` component using enhanced card mixins (`base-card`, `card-header` with wrapper support, `address-display`)
- Uses accent-2 color scheme (`$accent-2`) matching value cards for visual consistency
- Address formatted on two lines: street + number + bus (optional) on first line, postal code + municipality on second line
- Address text styled in black (`$black`) matching value card description text
- Added `PetStore` interface to `neighborhoods.ts` with structured address fields (same as Vet interface)
- Implemented three pet stores: Dierenwinkel Antwerpen-Zuid, Pet Shop Deurne, Dierenwinkel Het Huisdier
- Created `_pet-stores.scss` following same pattern as `_vets.scss`:
  - Grid layout (2 columns on desktop, stacked on mobile)
  - Horizontal scroll container mixin for mobile
  - Sticky map with dynamic height matching list content (max-height for viewport constraint)
- CTA button uses `btn-accent-2-outline` matching card color scheme
- Generated placeholder SVG map with numbered markers at `web/public/images/maps/antwerpen-zuid-pet-stores.svg`
- Map height dynamically matches list height when shorter than viewport, scrolls correctly when longer
- All content server-rendered, no client-side JavaScript required
- Verified build output contains all required elements

## Story B6: Daily Life Narrative Block ✅

> As a dog owner, I want a narrative about daily life in this neighbourhood, so I can imagine living there.

**Acceptance Criteria:**

- [x] Section heading for "Wat dit betekent voor jouw dagelijkse leven…" rendered as H2
- [x] One main explanatory paragraph rendered
- [x] Bullet list of daily-life benefits (e.g. morning walks, quick access to parks, etc.)
- [x] Semantic HTML used (`<p>`, `<ul>`, `<li>`)
- [x] Wording includes relevant SEO phrases (e.g. "hondvriendelijke buurt") without keyword stuffing

**Implementation Notes:**

- Added `DailyLife` interface to `neighborhoods.ts` with `title`, `intro`, and `benefits` array
- Implemented section in `web/src/pages/index.astro` with semantic HTML structure
- Section heading uses `SectionHeading` component with level={2}
- Main paragraph renders `neighborhood.dailyLife.intro` content
- Bullet list implemented with `<ul>` and `<li>` elements mapping over `neighborhood.dailyLife.benefits` array
- Created `_daily-life.scss` with styled bullet points using primary color
- Content includes natural SEO phrases: "hondvriendelijke plekjes", "met een hond", "viervoeter", "baasjes"
- All content server-rendered, no client-side JavaScript required
- Verified build output contains all required elements

## Story B7: "Baasjes Helpen Baasjes" Contribution Block ✅

> As a dog owner, I want to share my experience of the neighbourhood, so I can help other owners.

**Acceptance Criteria:**

- [x] Section heading for "Baasjes helpen baasjes" rendered
- [x] Short intro text explains why feedback is useful
- [x] Visual grey block styled according to design (or close MVP)
- [x] Primary CTA button created (e.g. "Deel je ervaring over Antwerpen-Zuid")
- [x] CTA navigates to an external survey page (Typeform/Google Forms) or a simple internal feedback page
- [x] GA4 custom event fired on CTA click (using the implementation from Story A3)

**Implementation Notes:**

- Added `ContributionCTA` interface to `neighborhoods.ts` with `heading`, `intro`, and `typeformId`
- Created `TypeformEmbed.astro` component with SEO-friendly lazy loading:
  - Reserves space with configurable `initialHeight` (default: 400px) to prevent layout shift (CLS)
  - Uses Intersection Observer to load Typeform script only when section enters viewport
  - Typeform auto-resizes after loading, container adapts dynamically
  - Tracks GA4 `typeform_loaded` event when embed loads
  - Optimized for performance with deferred script loading
- Implemented contribution section in `web/src/pages/index.astro`:
  - Grid layout: 2/3 for Typeform embed, 1/3 for decorative image
  - Responsive: stacks vertically on mobile with image shown first
  - Grey background block (`$calm-grey-200`) with rounded corners
- Created `_contribution.scss` with grid layout and sticky image positioning
- Added section to sidebar navigation
- Image uses lazy loading and proper alt text for accessibility
- All content server-rendered, Typeform script loads client-side only when needed

## Story B8: Buurtstatistieken Section ✅

> As a user, I want key numbers about the neighbourhood, so I can compare areas quickly.

**Acceptance Criteria:**

- [x] Section heading for "Buurtstatistieken" rendered
- [x] Statistic cards implemented for at least:
  - [x] Median house price
  - [x] Number of inhabitants
  - [x] Available homes
  - [x] Price per m²
- [x] Each card shows:
  - [x] Label
  - [x] Large number/value
  - [x] Short explanatory text if needed
- [x] All values loaded from a structured data source (not copy-pasted literals)
- [x] Responsive layout so cards remain readable on mobile
- [x] Semantic HTML structure used

**Implementation Notes:**

- Added `Statistics` interface to `neighborhoods.ts` with `intro`, `medianPrice`, `inhabitants`, `availableHomes`, and `pricePerSqm`
- Created `StatisticCard.astro` component with:
  - Icon and value displayed side-by-side in a header row (centered)
  - Label displayed below the value
  - Number formatting for Dutch locale (nl-BE) with dot as thousand separator
  - Price formatting with euro symbol (€) and per m² support
  - Primary color border and text for visual consistency
  - Body size text for value and icon (not large/heading size)
  - Equal width cards in grid layout
  - Hover effects with subtle lift and shadow
- Implemented statistics section in `web/src/pages/index.astro`:
  - Section heading "Buurtstatistieken" as H2
  - Intro paragraph explaining the statistics
  - Four statistic cards displaying all required metrics:
    - Mediaan woningprijs (median house price) with coins icon
    - Aantal inwoners (number of inhabitants) with users icon
    - Beschikbare woningen (available homes) with house icon
    - Prijs per m² (price per m²) with ruler icon
  - Responsive grid: 4 columns on tablet+, 2 columns on mobile, 1 column on small screens
  - All cards have equal width using `width: 100%` in grid
  - Semantic HTML with `<article>` elements
- Created `_statistics.scss` with responsive grid layout and intro text styling
- Created `_statistic-card.scss` component styles using SCSS variables:
  - Primary color border (`$primary`)
  - Primary color for value and icon text
  - Body size font for main content
  - Centered layout with flexbox
- Added section to sidebar navigation (positioned after contribution section)
- All values loaded from `neighborhood.statistics` data structure
- All content server-rendered, no client-side JavaScript required

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
