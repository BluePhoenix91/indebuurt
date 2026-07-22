# buurtkompas

**Neighborhood discovery for Flanders** — [buurtkompas.be](https://www.buurtkompas.be)

> Formerly known as **indebuurt**. The GitHub repository was renamed to `buurtkompas`; some local paths or older references may still use the previous name.

Buurtkompas helps people decide where to live by turning open geospatial and statistical data into clear, neighborhood-level insights. Objective signals (amenities, prices, demographics) meet editorial content so future residents can compare places that actually fit how they want to live.

**Status:** PostGIS data foundation in place (Flanders neighborhoods, 63k+ POIs, Statbel statistics). Astro frontend serving neighborhood pages via Content Collections. Content and data pipelines under active development.

## What it does

- **Neighborhood pages** — SEO-oriented guides with maps, amenity context, and socioeconomic background
- **SmartScore** — liveability scoring from OpenStreetMap POIs and Statbel statistics (shops, green space, transport, and more)
- **Content pipeline** — AI agents research PostGIS data, write Dutch copy, then SEO- and brand-review before publishing as static JSON
- **Data platform** — PostGIS as the system of record; .NET pipeline for schema, imports, and processing

## Tech stack

| Layer | Technologies |
| --- | --- |
| Frontend | [Astro](https://astro.build), Content Collections, Leaflet, Sass |
| Database | PostgreSQL 14+ / PostGIS (`buurtkompas_dev`) |
| Data pipeline | .NET (EF Core, CLI + API) |
| Content agents | TypeScript, Zod schemas |
| Sources | OpenStreetMap, Statbel (boundaries, population, house prices) |

## Repository structure

```
buurtkompas/
├── web/          # Astro site — neighborhood pages & static generation
├── agents/       # Content agent prompts, Zod schemas, validation tooling
├── pipeline/     # .NET data/content pipeline (EF Core, CLI, API)
├── database/     # Legacy SQL/reference materials (prefer EF migrations)
├── designs/      # Design artifacts
└── poc_*/        # Earlier experiments (SEO, SmartScore, street sampling)
```

## Getting started

### Web app

```bash
cd web
npm install
npm run dev
```

Build: `npm run build` · Preview: `npm run preview`

### Content agents

```bash
cd agents
npm install
npm run schemas:generate   # Zod → JSON Schema
npm run schemas:validate   # CI / pre-commit check
```

See [agents/README.md](./agents/README.md) for the researcher → writer → SEO → brand pipeline.

### Database & .NET pipeline

- Database: PostgreSQL with PostGIS; primary DB is `buurtkompas_dev`. Schema changes go through EF Core migrations in `pipeline/`, not ad-hoc SQL.
- Pipeline: open `pipeline/Pipeline.sln` (.NET). Details in `pipeline/CLAUDE.md` and [database/README.md](./database/README.md) (legacy reference).

### Git hooks (optional)

[Lefthook](https://github.com/evilmartians/lefthook) runs a pre-commit check that agent Zod schemas stay in sync with generated JSON schemas.

```bash
# macOS
brew install lefthook

# Windows (scoop)
scoop install lefthook

# Or via npm
npm install -g lefthook

lefthook install
```

## License

Licensed under the [Apache License 2.0](./LICENSE).

OpenStreetMap data is © OpenStreetMap contributors, available under the [ODbL](https://www.openstreetmap.org/copyright). Attribution is required when using derived insights.
