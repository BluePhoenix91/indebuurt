# Statbel Statistical Sectors Dataset

**Source file:** `sh_statbel_statistical_sectors_31370_20240101`
**Reference date:** January 1, 2024
**Coordinate system:** Belgian Lambert 1972 (EPSG: 31370)
**Accuracy:** 1/10,000

## Overview

This dataset contains the statistical sectors of Belgium as of 01/01/2024. Statistical sectors are the smallest administrative units used for statistical purposes in Belgium.

**Important notes:**
- Valid until the next update/correction of municipal boundaries
- Since 01/01/2019, the municipality code can **no longer** be derived from the statistical sector code
- Municipal boundaries were improved in 2024 by the General Administration of Patrimonial Documentation (FPS Finance)

## Field Reference

### Statistical Sector Level

| Field | Description |
|-------|-------------|
| `cd_sector` | Statistical sector code (01/01/2024) |
| `tx_sector_descr_nl` | Sector name (Dutch) |
| `tx_sector_descr_fr` | Sector name (French) |
| `tx_sector_descr_de` | Sector name (German) |

### Sub-Municipality Level (NIS6)

| Field | Description |
|-------|-------------|
| `cd_sub_munty` | Aggregation of sectors sharing first 6 positions in code |
| `tx_sub_munty_nl` | NIS6 name (Dutch) |
| `tx_sub_munty_fr` | NIS6 name (French) |

### Municipality Level

| Field | Description |
|-------|-------------|
| `tx_munty_dstr` | Municipality district name |
| `cd_munty_refnis` | Municipality NIS code (text) |
| `tx_munty_descr_nl` | Municipality name (Dutch) |
| `tx_munty_descr_fr` | Municipality name (French) |
| `tx_munty_descr_de` | Municipality name (German) |

### District Level

| Field | Description |
|-------|-------------|
| `cd_dstr_refnis` | District NIS code (text) |
| `tx_adm_dstr_descr_nl` | District name (Dutch) |
| `tx_adm_dstr_descr_fr` | District name (French) |
| `tx_adm_dstr_descr_de` | District name (German) |

### Province Level

| Field | Description |
|-------|-------------|
| `cd_prov_refnis` | Province NIS code (text) |
| `tx_prov_descr_nl` | Province name (Dutch) |
| `tx_prov_descr_fr` | Province name (French) |
| `tx_prov_descr_de` | Province name (German) |

### Region Level

| Field | Description |
|-------|-------------|
| `cd_rgn_refnis` | Region NIS code (text) |
| `tx_rgn_descr_nl` | Region name (Dutch) |
| `tx_rgn_descr_fr` | Region name (French) |
| `tx_rgn_descr_de` | Region name (German) |

### EU/Country Level

| Field | Description |
|-------|-------------|
| `cd_country` | Country code |
| `cd_nuts_lvl1` | NUTS1 code (Eurostat 2021) |
| `cd_nuts_lvl2` | NUTS2 code (Eurostat 2021) |
| `cd_nuts_lvl3` | NUTS3 code (Eurostat 2021) |

### Geometry Measurements

| Field | Description | Notes |
|-------|-------------|-------|
| `ms_area_ha` | Area in hectares | Lambert 2008 |
| `ms_perimeter_m` | Perimeter in meters | Lambert 2008 |

## Administrative Hierarchy

```
Country (BE)
└── Region (cd_rgn_refnis)
    └── Province (cd_prov_refnis)
        └── District (cd_dstr_refnis)
            └── Municipality (cd_munty_refnis)
                └── Sub-municipality/NIS6 (cd_sub_munty)
                    └── Statistical Sector (cd_sector)
```

## Usage for indebuurt.be

This dataset is essential for:
- **Spatial aggregation** at multiple resolution levels (sector → municipality → province → region)
- **Geographic lookups** to map addresses to statistical sectors
- **Joining with Statbel socioeconomic data** which is published at statistical sector level
- **NUTS codes** enable EU-level comparisons via Eurostat

---
*Original documentation by Hadewych De Sadeleer, GIS Manager, Statbel (04/07/2024)*
