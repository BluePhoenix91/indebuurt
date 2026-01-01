# Database Scripts

Scripts for loading and processing data into the Buurtkompas database.

## Structure

```
scripts/
├── setup-all.sh          # Master script - runs all domain scripts in order
└── {domain}/             # Domain-specific scripts (e.g., pois/, statistics/)
    ├── fetch.sh          # Download data from external source
    └── convert-*.sh      # Transform data for loading
```

## Conventions

### Folder Organization
- **Master script** (`setup-all.sh`) lives at root level
- **Domain scripts** live in subfolders named after their data domain (e.g., `pois/`, `statistics/`)
- Each domain folder contains scripts for that data pipeline

### Script Naming
- `fetch.sh` - Downloads data from external APIs/sources
- `convert-to-{format}.sh` - Converts between data formats (e.g., `convert-to-geojson.sh`)
- `load.sh` - Loads data into PostgreSQL (if not handled by master script)

### Script Requirements
- All scripts run from WSL (not Windows CMD)
- Use `set -e` to exit on error
- Include usage documentation in header comments
- Print "Next steps" at the end when run individually
- Use relative paths from PROJECT_ROOT

### Adding a New Domain

1. Create folder: `scripts/{domain}/`
2. Add scripts following naming conventions
3. Update `setup-all.sh` to call the new scripts in order
4. Update `database/README.md` folder structure section

Example for future H6 (Statistics):
```
scripts/
├── setup-all.sh
├── pois/
│   ├── fetch.sh
│   └── convert-to-geojson.sh
└── statistics/           # New domain
    └── fetch.sh
```

### Master Script Updates

When adding new domain scripts, update `setup-all.sh`:
1. Add new step section with clear header
2. Call domain scripts in correct order
3. Add verification check after each step
4. Update step numbering (e.g., "Step 1/4" instead of "Step 1/3")

## Data Flow

```
External API → fetch.sh → raw files (database/data/{domain}/)
                              ↓
                    convert-*.sh (if needed)
                              ↓
                    ogr2ogr → staging table
                              ↓
                    migration SQL → final table
```

## Related Files

- Migrations: `database/migrations/`
- Overpass queries: `database/queries/`
- Output data: `database/data/` (gitignored)
- Main docs: `database/README.md`
