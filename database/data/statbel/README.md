# Statbel Open Data for Neighborhood Statistics

This directory contains socioeconomic data from Statbel (Belgian Statistics Office) used to populate the `neighborhood_statistics` table.

## Data Files

### Population by Statistical Sector (2024)
- **File:** `OPENDATA_SECTOREN_2024.txt`
- **Source:** [Statbel - Population by Statistical Sector 2024](https://statbel.fgov.be/en/open-data/population-statistical-sector-2024)
- **Download:** https://statbel.fgov.be/sites/default/files/files/opendata/bevolking/sectoren/OPENDATA_SECTOREN_2024.zip
- **Reference date:** January 1, 2024
- **Key columns:** CD_SECTOR (sector code), TOTAL (population), SUPERFICIE (surface area in hectares)

### House Prices by Municipality (2024)
- **File:** `vastgoed_2010_9999.xlsx`
- **Source:** [Statbel - Real Estate Sales](https://statbel.fgov.be/en/open-data/real-estate-sales-according-nature-property-deed-sale-belgium)
- **Reference period:** Transactions during 2024
- **Key columns:** CD_REFNIS (municipality NIS code), CD_TYPE_NL (property type), Q50 (median price)

### Fiscal Income by Statistical Sector (2023) - Optional
- **File:** `fisc2023_D_NL.xlsx`
- **Source:** [Statbel - Taxable Income](https://statbel.fgov.be/en/themes/households/taxable-income)
- **Download:** https://statbel.fgov.be/sites/default/files/files/documents/Huishoudens/10.9%20Fiscale%20inkomens/fisc2023_D_NL.xlsx
- **Reference period:** Tax year 2022 (filed 2023)
- **Key columns:** Sector code, mean/median taxable income

## Data Vintage

| Metric | Source Year | Reference |
|--------|-------------|-----------|
| Population | 2024 | As of Jan 1, 2024 |
| Population Density | 2024 | Calculated from population and area |
| Median House Price | 2024 | Transactions during 2024 |
| Median Income | 2023 | Tax year 2022 |

## Geographic Level Mapping

| Statbel Level | NIS Code | Example | Our Table |
|---------------|----------|---------|-----------|
| Statistical Sector | 9 chars | 44021A001 | statistical_sectors |
| Neighborhood (Wijk) | 7 chars | 44021A0 | neighborhoods |
| Municipality | 5 chars | 44021 | (derived) |

**Processing:**
- Population: Aggregated UP from sectors to neighborhoods (SUM)
- House prices: Inherited DOWN from municipality to all its neighborhoods
- Income: Aggregated UP from sectors with weighted average

## 2025 Municipality Mergers

Belgium merged 28 municipalities on January 1, 2025. Statbel's data uses the **new merged NIS codes**, but our neighborhood boundaries retain the **original statistical sector codes** (which remain unchanged for historical comparability).

### NIS Code Mapping File

**File:** `nis_code_mapping_2025.csv`

This file maps old municipality NIS codes to the new Statbel NIS codes. The ETL script uses this mapping when joining house price data.

| Old NIS | New NIS | Old Name | New Name |
|---------|---------|----------|----------|
| 44040 | 44088 | Melle | Merelbeke-Melle |
| 44043 | 44088 | Merelbeke | Merelbeke-Melle |
| 44034 | 44087 | Lochristi | Lochristi |
| 71022 | 71072 | Hasselt | Hasselt |
| ... | ... | ... | ... |

See the full mapping in `nis_code_mapping_2025.csv` (27 entries).

### Source

- [Statbel: Modification of NSI codes from 1 January 2025](https://statbel.fgov.be/en/news/modification-nsi-codes-municipalities-1-january-2025-onwards)
- [REFNIS-NUTS 2025 PDF](https://statbel.fgov.be/sites/default/files/files/opendata/Nuts/Note%20REFNIS-NUTS%202025-NL.pdf)

### Municipalities Without House Price Data

Some small municipalities have no Statbel house price data (privacy/sample size):

| Municipality | NIS | Population |
|--------------|-----|------------|
| Herstappe | 73028 | 76 |
| Mesen | 33016 | 1,070 |
| Horebeke | 45062 | 2,012 |
| Spiere-Helkijn | 34043 | 2,063 |
| Bever | 23009 | 2,274 |

## Update Process

Statbel typically releases updated data annually:
- Population: Early each year for Jan 1 reference
- House prices: Quarterly, with annual summary
- Income: Mid-year for previous tax year

To update:
1. Download new files from URLs above
2. Update filenames if needed
3. Run `database/scripts/statbel/load-statistics.py`
4. Run `database/migrations/20250102_005_load-statbel-statistics.sql`

## License

All Statbel open data is published under [Creative Commons Attribution 4.0 (CC BY 4.0)](https://creativecommons.org/licenses/by/4.0/).

Attribution: Data from Statbel, the Belgian Statistical Office.
