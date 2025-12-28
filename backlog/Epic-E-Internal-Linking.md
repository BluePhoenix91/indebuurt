# Epic E — Internal Linking & Neighborhood Connections

## Overview

This epic covers the implementation of internal linking between neighborhood pages through a "neighboring neighborhoods" feature. Internal linking is a critical SEO strategy that helps search engines discover and understand the relationship between pages, while also improving user navigation and engagement.

### SEO Benefits
- **Improved crawlability**: Search engines can discover all neighborhood pages through link paths
- **PageRank distribution**: Authority flows between related pages
- **Topical relevance**: Links between geographically related neighborhoods signal content relationships
- **Reduced bounce rate**: Users can easily navigate to related content
- **Increased session duration**: More pages viewed per visit

---

## Story E1: Add Neighboring Neighborhoods to Data Structure ✅

> As a developer, I want to store neighboring neighborhood relationships in the data model, so I can render links between related neighborhoods.

**Acceptance Criteria:**
- [x] `Neighborhood` interface updated with optional `neighboringNeighborhoods` field
- [x] Field stores array of neighborhood IDs (strings) — not duplicated data
- [x] All existing neighborhood data files updated with their neighbors
- [x] Neighbor IDs reference valid entries in the `neighborhoods` Record
- [x] TypeScript compilation passes without errors

**Implementation Notes:**
- Added `neighboringNeighborhoods?: string[]` to `Neighborhood` interface in `web/src/data/neighborhoods.ts`
- Initial implementation used `{ id: string; name: string }` objects but was refactored to simple `string[]` to avoid data duplication
- All 12 Gent neighborhood files updated with their neighboring district IDs:
  - `gent-binnenstad.ts`: 6 neighbors
  - `gent-brugse-poort.ts`: 5 neighbors
  - `gent-dampoort.ts`: 5 neighbors
  - `gent-elisabethbegijnhof.ts`: 6 neighbors
  - `gent-kanaaldorpen.ts`: 4 neighbors
  - `gent-macharius-heirnis.ts`: 5 neighbors
  - `gent-muide.ts`: 4 neighbors
  - `gent-rabot.ts`: 5 neighbors
  - `gent-sluizeken-tolhuis-ham.ts`: 6 neighbors
  - `gent-watersportbaan-ekkergem.ts`: 5 neighbors
  - `gent-wondelgem.ts`: 3 neighbors
  - `gent-zwijnaarde.ts`: 3 neighbors
- Neighbor data sourced from official Gent district boundaries (25 districts total, 12 currently with pages)

---

## Story E2: Display Neighboring Neighborhoods Section ✅

> As a user browsing a neighborhood page, I want to see links to adjacent neighborhoods, so I can explore related areas without returning to the homepage.

**Acceptance Criteria:**
- [x] "Aangrenzende buurten" (Adjacent neighborhoods) section added to neighborhood detail page
- [x] Section appears after the statistics section
- [x] Section only renders if neighborhood has neighbors with existing pages
- [x] Neighbors without pages are filtered out (graceful degradation)
- [x] Each neighbor displayed as a clickable card with:
  - [x] Neighborhood name
  - [x] First paragraph of intro text
  - [x] Label tags (icons + text)
  - [x] "Lees meer" CTA
- [x] Cards link to the neighbor's detail page (`/buurt/{neighbor-id}/`)
- [x] Section includes intro text explaining the feature
- [x] Sidebar navigation updated with "Aangrenzende buurten" link

**Implementation Notes:**
- Added to `web/src/pages/buurt/[slug]/index.astro`
- Reuses `NeighborhoodCard` component from homepage for consistency
- Neighbor lookup at render time:
  ```typescript
  const neighboringNeighborhoods = neighborhood.neighboringNeighborhoods
    ?.map((id) => neighborhoods[id])
    .filter(Boolean);
  ```
- Uses `color="accent-2"` variant (green) to differentiate from homepage cards
- Conditional rendering ensures section only appears when neighbors exist
- Links are standard `<a href>` tags — fully SEO-friendly (server-side rendered, crawlable)

---

## Story E3: Reusable NeighborhoodCard Component ✅

> As a developer, I want the NeighborhoodCard component to be configurable, so I can reuse it across different contexts with appropriate display options.

**Acceptance Criteria:**
- [x] `NeighborhoodCard` component supports optional `date` prop
- [x] `showDate` boolean prop controls date visibility (default: `false`)
- [x] `dateIcon` prop allows custom icon for date display
- [x] Component supports multiple color variants: `primary`, `secondary`, `accent-1`, `accent-2`
- [x] Homepage "Alle buurten" section does not show dates
- [x] Homepage "Recentste buurten" section shows dates with `showDate={true}`
- [x] Neighboring neighborhoods section does not show dates

**Implementation Notes:**
- Updated `web/src/components/NeighborhoodCard.astro`:
  ```typescript
  interface Props {
    title: string;
    date?: string;
    dateIcon?: string;
    showDate?: boolean;
    description: string;
    tags: Array<{ icon: string; text: string }>;
    color?: "primary" | "secondary" | "accent-1" | "accent-2";
  }
  ```
- Default values: `showDate = false`, `dateIcon = "fa-regular fa-clock"`
- Date only renders when both `showDate` is `true` AND `date` has a value
- Component used in three contexts:
  1. Homepage "Alle buurten": `color="primary"`, no date
  2. Homepage "Recentste buurten": `color="accent-2"`, `showDate={true}`
  3. Neighboring neighborhoods: `color="accent-2"`, no date

---

## Story E4: Neighboring Section Styling ✅

> As a user, I want the neighboring neighborhoods section to be visually consistent with the rest of the page, so the experience feels cohesive.

**Acceptance Criteria:**
- [x] New SCSS partial created for neighboring section styles
- [x] Section uses consistent spacing with other page sections
- [x] Responsive layout adapts to mobile viewports
- [x] Cards use existing `neighborhood-cards-grid` layout
- [x] Intro text styled consistently with other section intros

**Implementation Notes:**
- Created `web/src/styles/sections/_neighboring.scss`:
  ```scss
  @use "../variables" as *;

  .neighboring-section {
    margin-top: $spacing-xxl;
    @media (max-width: $breakpoint-mobile) {
      margin-top: $spacing-lg;
    }
  }

  .neighboring-intro {
    margin-top: $spacing-md;
    margin-bottom: $spacing-lg;
    font-size: $size-body;
    color: $black;
  }
  ```
- Added import to `web/src/styles/styles.scss`
- Reuses existing `.neighborhood-cards-grid` from `_neighborhood-grid.scss`

---

## Future Enhancements (Not Yet Implemented)

### Story E5: Visual Map of Neighborhood Connections

> As a user, I want to see a visual map showing how neighborhoods connect, so I can understand the geographic layout.

**Acceptance Criteria:**
- [ ] Interactive map shows all neighborhoods as nodes
- [ ] Lines connect neighboring neighborhoods
- [ ] Clicking a node navigates to that neighborhood
- [ ] Current neighborhood highlighted differently

### Story E6: "You Might Also Like" Recommendations

> As a user, I want to see neighborhood recommendations based on similar characteristics, so I can find alternatives beyond just geographic proximity.

**Acceptance Criteria:**
- [ ] Algorithm identifies neighborhoods with similar scores/features
- [ ] "Similar neighborhoods" section shows 2-3 recommendations
- [ ] Similarity based on: price range, dog park count, green space, etc.
- [ ] Clearly differentiated from geographic neighbors

---

## Technical Reference

### Data Structure

```typescript
// web/src/data/neighborhoods.ts
export interface Neighborhood {
  id: string;
  name: string;
  // ... other fields
  neighboringNeighborhoods?: string[]; // Array of neighbor IDs
}
```

### Usage Example

```typescript
// In Astro component
const neighboringNeighborhoods = neighborhood.neighboringNeighborhoods
  ?.map((id) => neighborhoods[id])
  .filter(Boolean); // Filters out neighbors without pages

// Render
{neighboringNeighborhoods?.map((neighbor) => (
  <a href={`/buurt/${neighbor.id}/`}>
    <NeighborhoodCard
      title={neighbor.name}
      description={neighbor.intro.split("\n\n")[0]}
      tags={neighbor.labels}
      color="accent-2"
    />
  </a>
))}
```

### Files Modified

| File | Changes |
|------|---------|
| `web/src/data/neighborhoods.ts` | Added `neighboringNeighborhoods?: string[]` to interface |
| `web/src/data/neighborhoods/*.ts` | Added neighbor IDs to all 12 neighborhood files |
| `web/src/pages/buurt/[slug]/index.astro` | Added neighboring section and sidebar link |
| `web/src/components/NeighborhoodCard.astro` | Added `showDate` and `date` props |
| `web/src/pages/index.astro` | Updated to use `showDate` prop |
| `web/src/styles/sections/_neighboring.scss` | New file for section styles |
| `web/src/styles/styles.scss` | Added import for neighboring styles |
