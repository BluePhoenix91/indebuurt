# Writer Agent Examples

This folder contains annotated WriterOutput examples that demonstrate how to transform ResearcherOutput into engaging Dutch content.

## Status

**Currently empty** — Examples will be added after validating the first Writer agent outputs.

## Purpose

Once populated, these examples will:
1. Show the complete ResearcherOutput → WriterOutput transformation
2. Demonstrate correct brand voice and terminology usage
3. Illustrate how to handle edge cases (sparse data, missing POIs)
4. Serve as few-shot examples for the Writer agent

## Planned Examples

After initial testing, we plan to add examples for:

| Neighborhood | Type | Key Characteristics |
|--------------|------|---------------------|
| gent-dampoort | Urban | Medium density, good amenities, 1 dog park |
| gent-binnenstad | Historic urban | Dense, many parks, no dog parks |
| gent-mendonk | Rural | Large area, sparse POIs |

## File Format

Each example should be a complete WriterOutput JSON file following the schema in `../output-schema.json`.

Annotations can be added as `$comment` or `_annotation_*` fields (which will be stripped before validation).

## Adding New Examples

When adding an example:
1. Run the Writer agent on a ResearcherOutput test file
2. Validate the output against the schema
3. Review for quality (terminology, specificity, honesty)
4. Add annotations explaining key editorial decisions
5. Save as `{neighborhood-id}.json`

## Related Files

- `../output-schema.json` — Schema that all examples must match
- `../references/content-guidelines.md` — Brand voice and terminology rules
- `../../researcher/test-outputs/` — Source ResearcherOutput files
