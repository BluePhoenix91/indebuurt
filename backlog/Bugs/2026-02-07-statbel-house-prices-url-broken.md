# Statbel House Prices Download URL Returns 404

**Date discovered:** 2026-02-07
**Type:** Bug
**Severity:** Medium
**Component:** Statbel import pipeline (`import-statbel --dataset house-prices`)
**Discovered during:** Story O3 chain test (Epic O - ETL Automation)

---

## Summary

The configured Statbel house prices download URL (`vastgoed_2010_9999.xlsx`) returns HTTP 404. Statbel has reorganized their open data file structure, moving to new filenames and a quarterly data format.

---

## Current Behavior

Running `import-statbel` fails on the house prices step:

```
Error: Failed to download from Statbel
  Response status code does not indicate success: 404 (Not Found).
```

The URL in `appsettings.json`:
```
https://statbel.fgov.be/sites/default/files/files/opendata/vastgoed/vastgoed_2010_9999.xlsx
```

Population import still works fine — only house prices are affected.

---

## Root Cause

Statbel has restructured their real estate open data. The old file path no longer exists. The new files are at:

| File | URL |
|------|-----|
| **Per gemeente (quarterly)** | `https://statbel.fgov.be/sites/default/files/files/documents/Bouwen%20%26%20wonen/2.1%20Vastgoedprijzen/NM/NL_immo_statbel_kwartaal_per_gemeente.xlsx` |
| Per jaar (annual) | `https://statbel.fgov.be/sites/default/files/files/documents/Bouwen%20%26%20wonen/2.1%20Vastgoedprijzen/NM/NL_immo_statbel_jaar.xlsx` |

Source: https://statbel.fgov.be/nl/themas/bouwen-wonen/vastgoedprijzen

---

## Impact

- `import-statbel` completes population import but fails on house prices
- `neighborhood_statistics.median_house_price` stays NULL for all neighborhoods
- Workaround: population data still imports successfully, house prices can be skipped with `--dataset population`

---

## Proposed Fix

### 1. Update the URL in `appsettings.json`

Point to the new quarterly per-gemeente file (4 MB, most granular):
```json
"HousePricesUrl": "https://statbel.fgov.be/sites/default/files/files/documents/Bouwen%20%26%20wonen/2.1%20Vastgoedprijzen/NM/NL_immo_statbel_kwartaal_per_gemeente.xlsx"
```

### 2. Verify `HousePriceDataParser` compatibility

The parser uses dynamic column detection (searches for REFNIS/NIS, TYPE, Q50/MEDIAN, LEVEL/NIVEAU) and year-named sheets. Need to verify:
- Does the new file have year-named sheets or quarter-named sheets?
- Are the column names compatible with the parser's pattern matching?
- How to aggregate quarterly data to annual median prices?

The parser may need updates if the sheet/column structure changed significantly.

---

## Files Involved

- `pipeline/src/Pipeline.Cli/appsettings.json` — URL configuration
- `pipeline/src/Pipeline.Core/Services/Statbel/HousePriceDataParser.cs` — Excel parsing logic
- `pipeline/src/Pipeline.Core/Services/Statbel/StatbelDownloader.cs` — Download logic

---

## Related

- **Epic O** - ETL Automation (discovered during O3 chain test)
- **Story O2** - Statbel Import Command (house prices import)
