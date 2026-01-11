# Local Authenticity Patterns

Patterns to detect local-specific content in narrative fields.

## Place Names

Count unique names mentioned in prose (not in POI arrays):
- Street names: "Dendermondsesteenweg", "Hogeweg"
- Park names: "La Sapinière", "Cirkelspark"
- Landmarks: "station", "markt", "kerk"
- POI names: "Tom & Co", "dierenarts Maenhout"

## Local Tips

Phrases that indicate insider knowledge:
- "via de [street]"
- "richting [landmark]"
- "de route langs [place]"
- "bij het [location]"

## Neighborhood Observations

Specific character observations:
- "de levendige sfeer rond [street]"
- "de drukte op [market/square]"
- "de rustige straten richting [area]"

## Scoring Guide

| Unique Place Names | Score |
|--------------------|-------|
| 3+ names | 8/8 points |
| 2 names | 6/8 points |
| 1 name | 4/8 points |
| 0 names | 0/8 points |

Local tips present: +7 points
Neighborhood observations present: +5 points

**Total Local Authenticity: 20 points**

## If Too Generic

1. Log as `validationIssue` with severity "warning"
2. Note that neighborhood-specific details are missing
3. Do NOT invent details — only flag the issue
