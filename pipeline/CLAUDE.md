# Pipeline Project — Claude Code Instructions

This document provides guidance for Claude Code when working on the Pipeline project.

## Project Structure

```
pipeline/
├── Pipeline.sln
└── src/
    ├── Pipeline.Api/          # ASP.NET Core Web API host
    │   ├── Program.cs
    │   └── appsettings.json
    │
    ├── Pipeline.Cli/          # Console app with System.CommandLine
    │   └── Program.cs
    │
    └── Pipeline.Core/         # Shared class library
        ├── Data/
        │   ├── PipelineDbContext.cs
        │   └── Configurations/
        │       └── Content/   # One config file per entity
        │           ├── NeighborhoodProseConfiguration.cs
        │           ├── ValueCardTemplateConfiguration.cs
        │           └── LabelRuleConfiguration.cs
        │
        ├── Entities/
        │   └── Content/       # Content schema entities
        │       ├── NeighborhoodProse.cs
        │       ├── ValueCardTemplate.cs
        │       └── LabelRule.cs
        │
        ├── Enums/
        │   ├── CardType.cs
        │   ├── LabelCategory.cs
        │   ├── ConditionField.cs
        │   └── ConditionOperator.cs
        │
        ├── Mappers/           # Convention-based mappings
        │   ├── IconMapper.cs
        │   └── PoiCategoryMapper.cs
        │
        └── Migrations/        # EF Core migrations (generated)
```

## Code Standards

### Entity Classes

- Place in `Pipeline.Core/Entities/{Schema}/` folder
- Use `required` modifier for non-nullable properties
- Add XML documentation for each property
- Keep entities as POCOs — no business logic

```csharp
namespace Pipeline.Core.Entities.Content;

public class ExampleEntity
{
    /// <summary>
    /// Brief description of the property.
    /// </summary>
    public required string Name { get; set; }

    public string? OptionalField { get; set; }
}
```

### Entity Configurations

- Place in `Pipeline.Core/Data/Configurations/{Schema}/` folder
- One file per entity: `{EntityName}Configuration.cs`
- Implement `IEntityTypeConfiguration<TEntity>`
- Always specify table name and schema explicitly
- Define all constraints (max length, precision, indexes) in configuration, not attributes

```csharp
namespace Pipeline.Core.Data.Configurations.Content;

public class ExampleEntityConfiguration : IEntityTypeConfiguration<ExampleEntity>
{
    public void Configure(EntityTypeBuilder<ExampleEntity> builder)
    {
        builder.ToTable("example_entities", "content");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
    }
}
```

### Primary Keys

- Use `Guid` for all entity primary keys (consistency across all tables)
- Generate using `Guid.CreateVersion7()` in the entity (time-ordered, no index fragmentation)
- Configure as `ValueGeneratedNever()` in EF (app generates, not DB)

```csharp
// Entity
public class ExampleEntity
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
}

// Configuration
builder.HasKey(e => e.Id);
builder.Property(e => e.Id).ValueGeneratedNever();
```

**Exception:** Entities with natural keys (like `NeighborhoodProse` with `NisCode` or `ValueCardTemplate` with `CardType`) don't need a surrogate Guid.

### Enums

- Place in `Pipeline.Core/Enums/` folder
- Store as strings in database using `.HasConversion<string>()`
- Add XML documentation explaining the enum's purpose

### DbContext

- `PipelineDbContext` is the single context for all schemas
- Use `DbSet<T> PropertyName => Set<T>()` syntax
- Configurations are auto-discovered via `ApplyConfigurationsFromAssembly()`

## Database Schemas

The PostgreSQL database uses three schemas:

| Schema | Purpose |
|--------|---------|
| `gis` | Geographic/spatial data (neighborhoods, POIs, statistics) |
| `pipeline` | Pipeline job tracking |
| `content` | AI-generated content (prose, templates, rules) |

## EF Core Migrations

### IMPORTANT: Human-Only Operation

**Claude Code must NOT run `dotnet ef migrations` commands directly.**

EF Core migrations modify the database schema and should only be executed by the human developer after reviewing the generated code.

### Workflow

1. **Claude Code**: Create/modify entities and configurations
2. **Claude Code**: Ensure the solution builds (`dotnet build`)
3. **Claude Code**: Ask the human to generate and review the migration
4. **Human**: Run the migration command and review the generated files
5. **Human**: Apply the migration if satisfied

### Migration Commands

Run from the `pipeline/` solution root:

Generate a new migration:
```bash
cd pipeline
dotnet ef migrations add <MigrationName> --project src/Pipeline.Core --startup-project src/Pipeline.Api
```

Apply migrations to database:
```bash
cd pipeline
dotnet ef database update --project src/Pipeline.Core --startup-project src/Pipeline.Api
```

Review pending migrations:
```bash
cd pipeline
dotnet ef migrations list --project src/Pipeline.Core --startup-project src/Pipeline.Api
```

Remove last migration (if not applied):
```bash
cd pipeline
dotnet ef migrations remove --project src/Pipeline.Core --startup-project src/Pipeline.Api
```

### Migration Naming Convention

Use descriptive names that indicate what the migration does:
- `CreateContentSchema` — Initial content tables
- `AddQualityScoreToNeighborhoodProse` — Adding a column
- `AddIndexOnSlug` — Adding an index

## Connection Strings

- Development: `buurtkompas_dev` on localhost:5432
- Production: Lightsail via environment variable override

Connection string is in `appsettings.json` — password must be updated locally (not committed).

## Current Content Schema Entities

### NeighborhoodProse
AI-generated prose content per neighborhood. Keyed by `nis_code` (Belgian NIS code, 7 chars).

| Column | Type | Description |
|--------|------|-------------|
| nis_code | VARCHAR(7) PK | Belgian neighborhood identifier |
| slug | VARCHAR(200) UNIQUE | URL-friendly slug |
| city | VARCHAR(200) | City name |
| name | VARCHAR(200) | Neighborhood name |
| intro | TEXT | Long-form AI-generated intro |
| subtitle | VARCHAR(400) | Short tagline |
| quality_score | DECIMAL(4,1) | AI review score (0-100) |
| prompt_version | VARCHAR(20) | Prompt version used |
| generated_at | TIMESTAMP | Generation timestamp |
| modified_at | TIMESTAMP | Manual edit timestamp |
| modified_by | VARCHAR(100) | Who edited |

### ValueCardTemplate
Global templates (~6 rows) for generating value cards from GIS data.

| Column | Type | Description |
|--------|------|-------------|
| card_type | VARCHAR(50) PK | Enum: DogParks, Parks, Vets, etc. |
| title | VARCHAR(100) | Display title |
| description_template | VARCHAR(200) | Template with {count} placeholder |
| detail_template | VARCHAR(200) | Template with {nearest_*} placeholders |
| sort_order | INT | Display order |

Icons and POI categories are determined by `CardType` in code — not stored in database.

### LabelRule
Rules for automatically generating labels based on neighborhood statistics.

| Column | Type | Description |
|--------|------|-------------|
| id | GUID PK | Generated by app using Guid v7 |
| category | VARCHAR(50) | Enum: Character, Amenities, Transport, Demographics |
| label_text | VARCHAR(100) | Display text (e.g., "Veel groen") |
| label_icon | VARCHAR(100) | Icon CSS class |
| condition_field | VARCHAR(50) | Enum: ParkCount, DogParkCount, VetCount, etc. |
| condition_operator | VARCHAR(20) | Enum: GreaterThan, LessThan, Between, etc. |
| condition_value | VARCHAR(50) | Value(s) to compare |

**Related enums:**
- `LabelCategory` — grouping for display order
- `ConditionField` — available statistics to evaluate
- `ConditionOperator` — comparison operators

## Mappers

Convention-based mappings that derive values from enums. Keeps DB schema lean.

### IconMapper
Maps `CardType` to Font Awesome icon classes.

```csharp
IconMapper.GetIcon(CardType.DogParks)  // "fa-solid fa-dog"
IconMapper.GetDistanceIcon()           // "fa-solid fa-person-walking"
```

### PoiCategoryMapper
Maps `CardType` to POI category strings for GIS queries.

```csharp
PoiCategoryMapper.GetPoiCategory(CardType.Vets)  // "veterinary"
PoiCategoryMapper.GetPoiCategory(CardType.Transit)  // null (not POI-based)
```
