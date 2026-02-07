# Improvement: Additional Statbel Datasets for SmartScore

## Summary

Expand the Statbel data imports (Epic O, Story O2) to include additional datasets that enrich neighborhood profiles and feed into the SmartScore livability system. These datasets complement the existing population and house price data with socioeconomic, mobility, and demographic indicators.

## Current State

The Statbel import command currently supports two datasets:
- **Population** — Inhabitants per statistical sector (loaded)
- **House Prices** — Median prices per municipality (loaded)

## Proposed Datasets

### Priority 1: Statistical Sector Level (Highest Value)

| Dataset | Source URL | Key Fields | SmartScore Use |
|---------|-----------|------------|----------------|
| **Fiscal Income** | [statbel.fgov.be/en/open-data/fiscal-statistics-income-statistical-sector](https://statbel.fgov.be/en/open-data/fiscal-statistics-income-statistical-sector) | Mean/median income, income deciles | Affordability context, socioeconomic profile |
| **Cars per Household** | [statbel.fgov.be/en/open-data/number-cars-household-statistical-sector-2022](https://statbel.fgov.be/en/open-data/number-cars-household-statistical-sector-2022) | % households with 0/1/2+ cars | Mobility profile, urban accessibility indicator |

### Priority 2: Municipality Level (Aggregate to Neighborhoods)

| Dataset | Source URL | Key Fields | SmartScore Use |
|---------|-----------|------------|----------------|
| **Education Level** | [statbel.fgov.be/en/themes/datalab/datalab-census-education](https://statbel.fgov.be/en/themes/datalab/datalab-census-education) | % higher education, % secondary, % primary | Education profile of residents |
| **Household Composition** | [statbel.fgov.be/en/themes/population/structure-population/households](https://statbel.fgov.be/en/themes/population/structure-population/households) | % singles, % families with children, % lone parents | Family-friendliness indicator |
| **Age Distribution** | [statbel.fgov.be/en/themes/population/structure-population](https://statbel.fgov.be/en/themes/population/structure-population) | % youth (<18), % working age, % elderly (65+) | Life stage fit |
| **Origin/Nationality** | [statbel.fgov.be/en/themes/population/structure-population/origin](https://statbel.fgov.be/en/themes/population/structure-population/origin) | % Belgian background, % foreign background | Diversity metric |
| **Building Permits** | [statbel.fgov.be/en/open-data/building-permits-month-region-province-district-and-municipality](https://statbel.fgov.be/en/open-data/building-permits-month-region-province-district-and-municipality) | New construction permits, renovations | Growth/development indicator |
| **Employment Rate** | [statbel.fgov.be/en/themes/census/labour-market/situation-labour-market-employment-and-unemployment](https://statbel.fgov.be/en/themes/census/labour-market/situation-labour-market-employment-and-unemployment) | Employment rate, unemployment rate | Economic health |

### Note: Education Level Granularity

Education level data from Statbel is currently available at **municipality level only** (not statistical sector). Census 2021 data may offer finer granularity in the future. For now:
- Import at municipality level
- All neighborhoods in a municipality inherit the same education profile
- Consider requesting sector-level data from Statbel (contact: statbel@economie.fgov.be)

## Implementation Approach

### Database Schema Changes

Add columns to `gis.neighborhood_statistics`:

```sql
-- Income (sector level)
ALTER TABLE gis.neighborhood_statistics ADD COLUMN median_income_eur DECIMAL(10,2);
ALTER TABLE gis.neighborhood_statistics ADD COLUMN mean_income_eur DECIMAL(10,2);

-- Mobility (sector level)
ALTER TABLE gis.neighborhood_statistics ADD COLUMN pct_households_no_car DECIMAL(5,2);
ALTER TABLE gis.neighborhood_statistics ADD COLUMN avg_cars_per_household DECIMAL(4,2);

-- Demographics (municipality level, inherited)
ALTER TABLE gis.neighborhood_statistics ADD COLUMN pct_higher_education DECIMAL(5,2);
ALTER TABLE gis.neighborhood_statistics ADD COLUMN pct_singles DECIMAL(5,2);
ALTER TABLE gis.neighborhood_statistics ADD COLUMN pct_families_with_children DECIMAL(5,2);
ALTER TABLE gis.neighborhood_statistics ADD COLUMN pct_elderly_65plus DECIMAL(5,2);
ALTER TABLE gis.neighborhood_statistics ADD COLUMN pct_youth_under18 DECIMAL(5,2);
ALTER TABLE gis.neighborhood_statistics ADD COLUMN pct_foreign_background DECIMAL(5,2);
ALTER TABLE gis.neighborhood_statistics ADD COLUMN employment_rate DECIMAL(5,2);
```

### CLI Command Extension

Extend the `import statbel` command:

```bash
dotnet run -- import statbel                        # All datasets
dotnet run -- import statbel --dataset income       # Fiscal income only
dotnet run -- import statbel --dataset cars         # Cars per household only
dotnet run -- import statbel --dataset education    # Education level only
dotnet run -- import statbel --dataset households   # Household composition only
dotnet run -- import statbel --dataset age          # Age distribution only
dotnet run -- import statbel --dataset employment   # Employment rate only
```

### New Parsers Required

| Parser | Input Format | Statbel File |
|--------|--------------|--------------|
| `IncomeDataParser.cs` | XLSX | `fiscaal-inkomen-per-sector-*.xlsx` |
| `CarsPerHouseholdParser.cs` | XLSX | `wagens-per-huishouden-sector-*.xlsx` |
| `EducationLevelParser.cs` | XLSX | `onderwijsniveau-gemeente-*.xlsx` |
| `HouseholdCompositionParser.cs` | XLSX | `huishoudsamenstelling-*.xlsx` |
| `AgeDistributionParser.cs` | ZIP/TXT | Population files (reuse existing) |
| `EmploymentParser.cs` | XLSX | `werkgelegenheid-gemeente-*.xlsx` |

## SmartScore Integration

These datasets enable richer SmartScore dimensions:

| Dimension | Current Data | With New Datasets |
|-----------|--------------|-------------------|
| **Affordability** | House prices only | + Income context (price-to-income ratio) |
| **Accessibility** | POI distances | + Car dependency indicator |
| **Demographics** | Population count | + Age profile, household types, diversity |
| **Education** | School POIs nearby | + Education level of residents |
| **Economic Health** | None | Employment rate, income levels |

## Data Refresh Frequency

| Dataset | Statbel Update Frequency | Recommended Import |
|---------|--------------------------|-------------------|
| Fiscal Income | Annual (2-year lag) | Yearly |
| Cars per Household | Annual | Yearly |
| Education Level | Census (every 10 years) | After each census |
| Household Composition | Annual | Yearly |
| Age Distribution | Annual | Yearly |
| Employment | Quarterly | Quarterly |

## Considerations

### Data Licensing
All Statbel open data is published under **CC BY 4.0** — free to use with attribution.

### Municipality-Level Inheritance
For datasets only available at municipality level:
- All statistical sectors within a municipality inherit the same value
- This is acceptable for slow-changing metrics (education, employment)
- Consider weighting by sector population when aggregating

### Missing Data Handling
Some statistical sectors may lack data (too few inhabitants for privacy). Strategy:
- Log warnings for missing sectors
- Use municipality average as fallback
- Mark inherited values in the data (`is_inherited` flag)

## Related

- Epic O — ETL Automation (parent epic)
- Story O2 — Statbel Statistics Import Command (extend this story)
- [StatbelOptions.cs](pipeline/src/Pipeline.Core/Services/Statbel/StatbelOptions.cs) — Add new URLs
- [StatbelImportService.cs](pipeline/src/Pipeline.Core/Services/Statbel/StatbelImportService.cs) — Add new dataset support

## References

- [Statbel Open Data Portal](https://statbel.fgov.be/en/open-data)
- [Fiscal Income by Statistical Sector](https://statbel.fgov.be/en/open-data/fiscal-statistics-income-statistical-sector)
- [Cars per Household by Statistical Sector](https://statbel.fgov.be/en/open-data/number-cars-household-statistical-sector-2022)
- [Census Education Datalab](https://statbel.fgov.be/en/themes/datalab/datalab-census-education)
