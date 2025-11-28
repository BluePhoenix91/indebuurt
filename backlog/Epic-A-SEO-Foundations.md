# Epic A — SEO & Tracking Foundations

## Story A1: Initialize Astro Project
> As a developer, I want to initialize a clean Astro project with TypeScript, so I can build a fast, SEO-ready static site.

**Acceptance Criteria:**
- [x] Astro project initialized with `npm create astro@latest`
- [x] TypeScript enabled and working
- [x] Base folders created: `src/pages`, `src/layouts`, `src/components`, `public`
- [x] Dev server runs without errors using `npm run dev`
- [x] `npm run build` outputs static HTML files to `dist/`
- [x] Generated HTML is minimal (no unnecessary JS for static content)

## Story A2: Create Base Layout With SEO Structure
> As a developer, I want a shared layout with semantic HTML and SEO hooks, so all neighbourhood pages have a consistent, SERP-ready structure.

**Acceptance Criteria:**
- [x] `NeighborhoodLayout.astro` created
- [x] `<html lang="nl">` set at document level
- [x] Layout exposes props/slots for:
    - [x] `<title>`
    - [x] `<meta name="description">`
    - [x] `<link rel="canonical">`
    - [x] Open Graph / social tags
- [x] Semantic HTML structure used: `<header>`, `<main>`, `<footer>`
- [x] Simple placeholder header and footer render correctly on a test page

## Story A3: Add Google Analytics 4 (GA4)
> As the site owner, I want GA4 tracking so I can measure traffic and engagement on the neighbourhood page.

**Acceptance Criteria:**
- [x] GA4 snippet added in the shared layout
- [x] Measurement ID read from an environment variable (not hardcoded)
- [x] GA4 code only included in production builds
- [x] At least one custom event implemented (e.g. CTA button click)
- [x] GA4 DebugView or Realtime shows:
    - [x] Pageview events for the neighbourhood page
    - [x] Custom CTA event when the button is clicked

**Testing Note:** To verify GA4 events in DebugView:
1. Build and deploy to production (or use `npm run preview` with production build)
2. Open GA4 DebugView (Admin → DebugView in Google Analytics)
3. Visit the page in a browser
4. Click the CTA button to trigger the `test_cta_click` event
5. Verify both pageview and custom event appear in DebugView

## Story A4: SEO Plumbing (Robots, Sitemap, Open Graph)
> As a search engine, I want crawl and metadata signals, so I can discover and index the neighbourhood page correctly.

**Acceptance Criteria:**
- [x] `robots.txt` created (in `public/` or generated) that allows crawling
- [x] `sitemap.xml` generated and includes the neighbourhood page URL
- [x] `<link rel="canonical">` present on the neighbourhood page
- [x] Basic Open Graph tags added:
    - [x] `og:title`
    - [x] `og:description`
    - [x] `og:url`
    - [x] `og:image` (placeholder)
- [x] `<html lang="nl">` confirmed in the final output HTML
