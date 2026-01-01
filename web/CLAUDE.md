# Web Coding Guidelines

## Tech Stack

- Astro with TypeScript
- SCSS for styling
- Content Collections for structured data (JSON + Zod schemas in `content/config.ts`)

### Astro information

> Astro is an all-in-one web framework for building websites.

- Astro uses island architecture and server-first design to reduce client-side JavaScript overhead and ship high performance websites.
- Astro’s friendly content-focused features like content collections and built-in Markdown support make it an excellent choice for blogs, marketing, and e-commerce sites amongst others.
- The `.astro` templating syntax provides powerful server rendering in a format that follows HTML standards and will feel very familiar to anyone who has used JSX.
- Astro supports popular UI frameworks like React, Vue, Svelte, Preact, and Solid through official integrations.
- Astro is powered by Vite, comes with a fast development server, bundles your JavaScript and CSS for you, and makes building websites feel fun.

see @docs/astro-llms-full.md for the full information.

## Component Architecture

**Scoped styles in components:**

- Component-specific CSS goes in the component's `<style>` block
- Use CSS nesting (`&` selectors) for related elements
- Keep Props interfaces inline in the frontmatter

**Base components for shared patterns:**

- Use base components with slots for reusable patterns (e.g., `BaseCard.astro`)
- Extract a base component when you have a second use case, not preemptively

**Legacy migration:**
Some existing components use global SCSS + mixins. When modifying these:

1. Move styles into the component's `<style>` block
2. If the pattern is shared, extract a base component with slots
3. Remove the now-unused SCSS from global files

## Global SCSS (limited use)

Only use global SCSS for truly shared concerns:

- `_variables.scss` - design tokens (colors, spacing, breakpoints)
- `_reset.scss` - CSS reset
- `base/` - typography, fonts, base layout

Avoid:

- Adding component styles to global SCSS files
- Creating new mixins for component patterns (use base components instead)

## File Organization

- Components: `src/components/`
- Pages: `src/pages/` (dynamic routes like `[slug]`)
- Content: `src/content/` (JSON validated by Zod)
- Shared logic: `src/lib/`
- Global styles: `src/styles/`

## Language & Naming Conventions

- User-facing content: Dutch (nl-BE)
- Code, comments, documentation: English
- Database tables, columns, variables: English

### Geographic Terminology

Use English terms in code, with Dutch equivalents for user-facing content:

| English (code) | Dutch (UI) | Description |
|----------------|------------|-------------|
| neighborhood | wijk/buurt | Level 6 - primary user-facing geographic unit |
| statistical_sector | statistische sector | Level 7 - finest granularity, for data aggregation |
| sub_municipality | deelgemeente | Level 5 - larger district (future) |
| municipality | gemeente | City/town level |
| province | provincie | Province level |

### Database Tables

| Table | Purpose |
|-------|---------|
| `neighborhoods` | Aggregated neighborhood boundaries (Level 6) - ~2,800 for Flanders + Brussels |
| `statistical_sectors` | Fine-grained sector boundaries (Level 7) - ~9,900 for Flanders + Brussels |
| `pois` | Points of interest from OpenStreetMap |
| `neighborhood_statistics` | Socioeconomic data linked to neighborhoods |

### NIS Code Structure

Belgian statistical areas use NIS codes: `44021A011`
- `44021` = Municipality (Gent)
- `A` = Sub-municipality letter
- `0` = Neighborhood digit
- `11` = Statistical sector

## Database Migration Principles

- **Never modify existing migrations** in `database/migrations/`
- Add new timestamped migrations for any schema or data changes
- Schema changes (ALTER TABLE) go in the migration that needs them
- See `database/README.md` for full details
