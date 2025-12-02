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
