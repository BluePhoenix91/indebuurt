# Epic C — Navigation & Page Structure

## Story C0: Generate 10 Neighborhood Pages from JSON
> As a developer, I want to create pages dynamically from neighborhood data files, so I don't manually duplicate code.

**Acceptance Criteria:**
- [ ] Create `src/data/neighborhoods/` folder with JSON files
- [ ] Use Astro dynamic routes (`[slug].astro` or similar pattern) to generate pages
- [ ] Each JSON file contains all data for one neighborhood (10 total)
- [ ] Neighborhood data includes: title, subtitle, badges, intro, map coordinates, POIs, statistics
- [ ] Running `npm run build` generates 10 static HTML pages (one per neighborhood)
- [ ] URLs follow pattern: `/buurt/gent-centrum`, `/buurt/gent-rabot`, etc.
- [ ] All 10 pages accessible and render correctly

## Story C1: In-Page Section Navigation
> As a visitor, I want to jump quickly to the main sections, so I can explore the page efficiently.

**Acceptance Criteria:**
- [ ] In-page navigation component created after all sections exist
- [ ] Navigation includes links for at least:
    - [ ] Inleiding
    - [ ] Voorzieningen
    - [ ] Voor jou / Dagelijks leven
    - [ ] Statistieken
- [ ] Each nav item links to an anchor ID on the page (e.g. `#voorzieningen`)
- [ ] Smooth scrolling enabled (CSS or small JS)
- [ ] Navigation works with keyboard (tab focus, Enter/Space to activate)
- [ ] Navigation implemented in a way that does not harm SEO (pure anchor links, no JS-only routing)
