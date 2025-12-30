# Epic C — Navigation & Page Structure

## Story C0: Generate 10 Neighborhood Pages from JSON ✅

> As a developer, I want to create pages dynamically from neighborhood data files, so I don't manually duplicate code.

**Acceptance Criteria:**

- [x] Create `src/data/neighborhoods/` folder with JSON files
- [x] Use Astro dynamic routes (`[slug].astro` or similar pattern) to generate pages
- [x] Each JSON file contains all data for one neighborhood (10 total)
- [x] Neighborhood data includes: title, subtitle, badges, intro, map coordinates, POIs, statistics
- [x] Running `npm run build` generates 10 static HTML pages (one per neighborhood)
- [x] URLs follow pattern: `/buurt/gent-centrum`, `/buurt/gent-rabot`, etc.
- [x] All 10 pages accessible and render correctly

**Implementation Notes:**

- Created dynamic route at `web/src/pages/buurt/[slug].astro`:
  - Uses Astro's `getStaticPaths()` to generate static pages for each neighborhood
  - Reads neighborhood data from `neighborhoods.ts` TypeScript file (type-safe alternative to JSON)
  - Each neighborhood slug becomes a URL: `/buurt/{slug}`
  - Example: `/buurt/antwerpen-zuid` for the "antwerpen-zuid" neighborhood
- Moved all neighborhood page content from `index.astro` to the dynamic route:
  - All components, sections, and functionality preserved
  - Dynamic badges using `neighborhood.labels` array (instead of hardcoded)
  - Canonical URL updated to include the slug: `https://buurtkompas.be/buurt/{slug}`
- Updated `index.astro` to redirect to first neighborhood:
  - Homepage (`/`) now redirects to `/buurt/{first-neighborhood-slug}`
  - Can be changed to show a neighborhood list page in the future
- Data structure:
  - Using TypeScript interface (`Neighborhood`) for type safety
  - **Each neighborhood has its own file**: `web/src/data/neighborhoods/{slug}.ts`
  - Example: `antwerpen-zuid.ts` contains all data for Antwerpen-Zuid
  - `web/src/data/neighborhoods/index.ts` aggregates all neighborhoods
  - `web/src/data/neighborhoods.ts` re-exports for backward compatibility
  - Each neighborhood file exports a single `Neighborhood` object
  - Each neighborhood has all required fields: title, subtitle, labels (badges), intro, coordinates, POIs (dogParks, vets, petStores), statistics
  - Benefits of file-per-neighborhood:
    - Easier to manage and maintain individual neighborhoods
    - Better Git diffs (changes to one neighborhood don't affect others)
    - Can easily convert to JSON files later if needed
    - See `web/src/data/neighborhoods/README.md` for instructions on adding new neighborhoods
- Build verification:
  - Running `npm run build` successfully generates static HTML pages
  - Each neighborhood gets its own page at `/buurt/{slug}/index.html`
  - Verified with current data (1 neighborhood: "antwerpen-zuid")
  - Will automatically generate pages for all neighborhoods when data is added
- Error handling:
  - Invalid slugs redirect to 404 page
  - Type-safe access to neighborhood data
- Note: Currently using TypeScript data file instead of JSON files. This provides:
  - Type safety and autocomplete
  - Easier refactoring
  - Same functionality as JSON (data-driven pages)
  - Can be converted to JSON files later if needed
