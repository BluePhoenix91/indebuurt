# indebuurt.be Agent Pipeline

AI agents for generating neighborhood content. This pipeline transforms PostGIS data into engaging Dutch content for dog owners.

## Pipeline Overview

```
┌─────────────┐     ┌────────────┐     ┌──────────────┐     ┌────────────────┐     ┌─────────────┐
│  Researcher │ ──▶ │   Writer   │ ──▶ │ SEO Reviewer │ ──▶ │ Brand Reviewer │ ──▶ │ Final JSON  │
│  (PostGIS)  │     │  (Content) │     │  (Optimize)  │     │   (Quality)    │     │ (Astro)     │
└─────────────┘     └────────────┘     └──────────────┘     └────────────────┘     └─────────────┘
     │                   │                   │                      │
     ▼                   ▼                   ▼                      ▼
 Factual data       Narrative +         SEO-optimized         Brand-checked
 No prose           Icons               content               final output
```

## Folder Structure

```
agents/
├── researcher/           # Queries PostGIS, outputs factual data
│   ├── prompt-v1.md      # System prompt (Story I2)
│   ├── output-schema.json
│   └── examples/
├── writer/               # Transforms data into engaging content
│   ├── prompt-v1.md      # System prompt (Story I3)
│   ├── output-schema.json
│   └── examples/
├── seo-reviewer/         # Optimizes for search visibility
│   ├── prompt-v1.md      # System prompt (Story I4)
│   └── examples/
├── brand-reviewer/       # Ensures voice consistency
│   ├── prompt-v1.md      # System prompt (Story I5)
│   └── examples/
├── shared/               # Shared resources
│   ├── final-output-schema.json  # Matches Astro Content Collections
│   ├── terminology.json          # Brand vocabulary guide
│   └── character-limits.json     # Target lengths for content
├── scripts/              # Build tooling
│   ├── schemas.ts        # Zod schema definitions (source of truth)
│   ├── generate-schemas.ts
│   └── validate-schemas.ts
└── package.json
```

## Quick Start

```bash
# Install dependencies
cd agents
npm install

# Generate JSON schemas from Zod definitions
npm run schemas:generate

# Validate schemas match Zod (use in CI)
npm run schemas:validate
```

## Schema Versioning

### Version Format

Schemas use semantic versioning: `MAJOR.MINOR.PATCH`

- **MAJOR**: Breaking changes (fields removed, types changed)
- **MINOR**: Additions (new optional fields)
- **PATCH**: Documentation, descriptions, examples

### Current Versions

| Schema | Version | Last Updated |
|--------|---------|--------------|
| ResearcherOutput | 1.0.0 | 2025-12-14 |
| WriterOutput | 1.0.0 | 2025-12-14 |
| FinalOutput | 1.0.0 | 2025-12-14 |

### Making Schema Changes

1. **Edit the Zod source** in `scripts/schemas.ts`
2. **Bump version** in the schema's `schemaVersion` literal
3. **Regenerate JSON schemas**: `npm run schemas:generate`
4. **Update examples** if structure changed
5. **Commit both** Zod changes and generated JSON schemas

### Schema Compatibility

- **Agents**: Check `schemaVersion` field in their output
- **Validation**: CI runs `npm run schemas:validate` to catch drift
- **Migration**: When MAJOR version bumps, update agent prompts

## Agent Responsibilities

### Researcher (Story I2)
- Queries PostGIS for POIs, statistics, distances
- Outputs **factual data only** - no prose, no icons
- Includes data source references for transparency
- Does NOT make editorial decisions

### Writer (Story I3)
- Receives Researcher output
- Generates all narrative content in Dutch
- Assigns icons based on POI types
- Formats distances ("4 mins" vs raw meters)
- Makes editorial decisions (what to highlight, tone)

### SEO Reviewer (Story I4)
- Checks title length, meta description
- Suggests internal linking opportunities
- Validates heading structure
- Does NOT change factual data

### Brand Reviewer (Story I5)
- Enforces terminology (see `shared/terminology.json`)
- Catches generic/cliché content
- Ensures local authenticity
- Outputs quality score (0-100)

## Shared Resources

### terminology.json
Brand vocabulary guide defining preferred terms:
- "baasjes" not "eigenaars"
- "viervoeter" not "huisdier"
- "hondenspeelweide" not "dog park"

### character-limits.json
Target ranges for content fields:
- subtitle: 80-120 characters
- intro: 400-800 words
- dailyLife.benefits: 3-7 items

These are **soft limits** for agent guidance, not hard validation.

## Relationship to Astro

The `shared/final-output-schema.json` must match the Zod schema in:
```
web/src/content/config.ts
```

When Content Collections schema changes:
1. Update `scripts/schemas.ts` (finalOutputSchema)
2. Regenerate: `npm run schemas:generate`
3. Both schemas stay in sync

## CI Integration

Add to your CI pipeline:

```yaml
- name: Validate agent schemas
  run: |
    cd agents
    npm ci
    npm run schemas:validate
```

This fails the build if generated schemas don't match Zod definitions.
