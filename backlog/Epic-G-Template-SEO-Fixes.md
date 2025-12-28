# Epic G — Template-Level SEO Fixes

> Based on SEO crawler analysis of buurtkompas.be (December 2024). These are **template/system-level** fixes only. Content fixes (meta descriptions, titles, page content) will be addressed during content regeneration.

## Context

The SEO crawler identified issues across 37 pages. This epic focuses on **structural template fixes**:
- 0 pages with Schema.org (missing from all templates)
- OG image using favicon on all pages (template default)
- Demografie pages too thin to index (template not ready)

**Out of scope (content work for later):**
- Meta descriptions too short (21 pages) → fix during content regeneration
- Title tags too long (3 pages) → fix during content regeneration
- Huizen page content enrichment → fix during content regeneration

**Reference:** See `/agents/seo/seo_analysis_report.md` for full analysis.

---

## Story G1: Implement Schema.org in Base Layout
> As a search engine, I want structured data, so I can display rich snippets and better understand page content.

**Problem:** 0% of pages have Schema.org markup. This is a template-level addition.

**Note:** This overlaps with Epic D Story D1. Consider this story the implementation ticket.

**Acceptance Criteria:**
- [ ] `Place` or `Neighborhood` schema added to neighborhood overview pages
- [ ] `BreadcrumbList` schema added to all pages (Home > City > Neighborhood)
- [ ] Schema component created: `SchemaOrg.astro` that accepts typed props
- [ ] Schema includes: name, address (city, postalCode, country), geo coordinates, description
- [ ] All schema validates in Google Rich Results Test without errors
- [ ] Schema generation is automatic from neighborhood data (not manual per page)

**Example schema structure:**
```json
{
  "@context": "https://schema.org",
  "@type": "Place",
  "name": "Binnenstad, Gent",
  "description": "...",
  "address": {
    "@type": "PostalAddress",
    "addressLocality": "Gent",
    "postalCode": "9000",
    "addressCountry": "BE"
  },
  "geo": {
    "@type": "GeoCoordinates",
    "latitude": 51.0543,
    "longitude": 3.7174
  }
}
```

---

## Story G2: Improve Open Graph Image Template
> As a social media user, I want compelling preview images when pages are shared, so I'm more likely to click.

**Problem:** All pages use `/favicon.svg` as OG image. This displays poorly on social platforms.

**Acceptance Criteria:**
- [ ] Default OG image created (1200x630px) with buurtkompas branding
- [ ] OG image template supports dynamic text overlay (neighborhood name)
- [ ] Option A: Static branded image per city (simpler)
- [ ] Option B: Dynamic image generation via service like `og-image` or Vercel OG (more work)
- [ ] `og:image` tag updated in layout to use new images
- [ ] Images tested in Facebook Sharing Debugger and Twitter Card Validator

**MVP approach:** Create 1 default branded OG image. Dynamic per-neighborhood images can come later.

---

## Story G3: Disable Demografie Pages Until Template is Ready
> As a site owner, I want to avoid publishing thin demografie pages, so I don't dilute site quality.

**Problem:** Demografie pages have only 77-91 words - essentially raw data tables with no explanatory content. These won't rank and signal low quality to search engines.

**Acceptance Criteria:**
- [ ] Demografie page generation disabled until template is content-rich
- [ ] Configuration option: `{ demografie: false }` in page type config
- [ ] Demografie URLs return 404 (not generated)
- [ ] Sitemap excludes demografie pages
- [ ] Internal links to demografie pages hidden (tab/nav removed)
- [ ] When template is ready, flip flag to `true` and pages auto-generate

**Implementation approach:**
```typescript
// src/config/pageTypes.ts
export const PAGE_TYPE_CONFIG = {
  overview: { enabled: true },
  demografie: { enabled: false },  // Enable when template has 300+ words of context
  huizen: { enabled: true },
};
```

**Re-enable criteria for demografie:**
- Template includes explanatory content about statistics (what they mean, how to interpret)
- Template includes neighborhood-specific context (how this compares to city average)
- Word count reaches 300+ words
- Content serves user intent beyond raw numbers

---

## Story G4: SEO Validation in Build Pipeline
> As a developer, I want automated SEO checks on build, so I catch template regressions before deploy.

**Acceptance Criteria:**
- [ ] Build script includes SEO validation step
- [ ] Validation checks:
  - [ ] All pages have title tags
  - [ ] All pages have meta descriptions
  - [ ] All pages have canonical URLs
  - [ ] No duplicate titles or descriptions
  - [ ] Schema.org JSON-LD is valid JSON
- [ ] Build fails (or warns) if SEO rules are violated
- [ ] Can run standalone: `npm run validate:seo`

**Implementation options:**
- Custom script using the SEO crawler logic
- Lighthouse CI in GitHub Actions
- Simple regex/AST checks on generated HTML

---

## Priority Order

1. **G3** - Disable demografie pages (stop the bleeding on thin content)
2. **G1** - Schema.org (enables rich snippets)
3. **G2** - OG images (social sharing improvement)
4. **G4** - Build validation (prevents regressions)

---

## Dependencies

- G1 (Schema.org) depends on neighborhood data having geo coordinates
- G2 (OG images) may need design input for branded template
- G3 (disable demografie) should be done first to stop bleeding

---

## Success Metrics

After implementing these fixes:
- [ ] Demografie pages: Not indexed until template is ready (currently 12 thin pages)
- [ ] Schema.org: 100% of published pages have valid structured data (currently 0%)
- [ ] OG images: No pages using favicon as OG image
