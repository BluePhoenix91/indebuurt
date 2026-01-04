# CLAUDE.md - Agent Pipeline Context

This folder contains AI agent schemas and tooling for generating neighborhood content on indebuurt.be.

## Quick Reference

**Pipeline:** Researcher → Writer → SEO Reviewer → Brand Reviewer → Final JSON (Astro)

**Key principle:** Zod schemas in `scripts/schemas.ts` are the **source of truth**. JSON schemas are generated from Zod, never edited directly.

## Common Tasks

### Regenerate JSON schemas after Zod changes

```bash
cd agents
npm run schemas:generate
```

### Validate schemas match Zod (CI check)

```bash
npm run schemas:validate
```

### Validate a JSON file against a schema

```bash
npm run validate:json -- researcher path/to/file.json
npm run validate:json -- writer path/to/file.json
npm run validate:json -- final path/to/file.json
```

## Schema Responsibilities

| Schema               | Contains                                                                          | Does NOT contain                             |
| -------------------- | --------------------------------------------------------------------------------- | -------------------------------------------- |
| **ResearcherOutput** | Factual data: POIs, coordinates, distances (meters), statistics, data sources     | Icons, formatted distances, prose, narrative |
| **WriterOutput**     | All narrative content, icons, formatted distances ("4 mins"), editorial decisions | —                                            |
| **FinalOutput**      | Must match `web/src/content/config.ts` exactly                                    | —                                            |

## File Locations

- **Zod schemas (source of truth):** `scripts/schemas.ts`
- **Generated JSON schemas:** `researcher/output-schema.json`, `writer/output-schema.json`, `shared/final-output-schema.json`
- **Brand terminology:** `shared/terminology.json`
- **Character limits:** `shared/character-limits.json`
- **Examples:** `researcher/examples/*.json`

## When Editing Schemas

1. Edit `scripts/schemas.ts` (the Zod definitions)
2. Bump `schemaVersion` if breaking change
3. Run `npm run schemas:generate`
4. Update examples if structure changed
5. Commit both Zod changes AND generated JSON schemas

## Detailed Documentation

See [README.md](./README.md) for full documentation including versioning policy and CI integration.
See [SEO-STRATEGY.md](./SEO-STRATEGY.md) for full documentation on SEO strategy.
