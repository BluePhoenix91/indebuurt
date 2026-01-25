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

## Service Classes

### Constructor Style

Use **primary constructors** (C# 12) for all service classes:

```csharp
public class MyService(
    ILogger<MyService> logger,
    IOptions<MyOptions> options) : IMyService
{
    // Reference parameters directly: logger, options
    // Use readonly field only if you need to unwrap (e.g., options.Value)
    private readonly MyOptions _options = options.Value;
}
```

### HttpClient Configuration

Configure `HttpClient` settings (timeout, base address) in DI registration, not in the service class:

```csharp
// Program.cs
builder.Services.AddHttpClient<IMyClient, MyClient>((sp, client) =>
{
    var options = sp.GetRequiredService<IOptions<MyOptions>>().Value;
    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
});
```

### Interface Extraction

Extract interfaces for:
- Services that have external dependencies (HTTP clients, databases)
- Repository classes used by other services

This enables testing and follows dependency inversion.

### File Organization

For services with multiple concerns, split into focused files:

```
Services/
├── MyService.cs              # Orchestrator (implements IMyService)
├── MyOptions.cs              # Configuration class
└── MyFeature/
    ├── FeatureConverter.cs   # Data transformation
    └── FeatureRepository.cs  # Database operations (implements IFeatureRepository)
```

## CLI Commands

### Structure

CLI commands live in `Pipeline.Cli/Commands/`. Each command:
- Is a static class with an `ExecuteAsync` method
- Receives `IServiceProvider` to resolve dependencies
- Uses `IConsole` for output (testable)
- Returns `int` exit code (0 = success)

```csharp
public static class MyCommand
{
    public static async Task<int> ExecuteAsync(
        IServiceProvider services,
        bool dryRun,
        CancellationToken cancellationToken)
    {
        var service = services.GetRequiredService<IMyService>();
        // ...
        return 0;
    }
}
```

### Progress Reporting

Use `IProgress<T>` for reporting progress from services to CLI:

```csharp
// In command
var progress = new Progress<string>(msg => console.WriteLine($"  {msg}"));
await service.DoWorkAsync(progress: progress);

// In service
public async Task DoWorkAsync(IProgress<string>? progress = null)
{
    progress?.Report("Starting...");
}
```

## Database Operations

### Bulk Inserts

For large inserts (1000+ rows), use PostgreSQL binary COPY via Npgsql:

```csharp
await using var writer = await conn.BeginBinaryImportAsync(
    "COPY my_table (col1, col2) FROM STDIN (FORMAT BINARY)",
    cancellationToken);

foreach (var item in items)
{
    await writer.StartRowAsync(cancellationToken);
    await writer.WriteAsync(item.Col1, NpgsqlDbType.Bigint, cancellationToken);
    await writer.WriteAsync(item.Col2, NpgsqlDbType.Varchar, cancellationToken);
}

await writer.CompleteAsync(cancellationToken);
```

### Geometry Support

When using NetTopologySuite geometries with raw Npgsql (not EF), configure the data source:

```csharp
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.UseNetTopologySuite();
var dataSource = dataSourceBuilder.Build();

// Then use dataSource.OpenConnectionAsync() for connections
```

### Staging Table Pattern

For atomic data refreshes:
1. Create staging table with same structure
2. Bulk insert into staging
3. Swap tables in a transaction:
   ```sql
   BEGIN;
   DROP TABLE IF EXISTS my_table_old;
   ALTER TABLE my_table RENAME TO my_table_old;
   ALTER TABLE my_table_staging RENAME TO my_table;
   DROP TABLE IF EXISTS my_table_old;
   COMMIT;
   ```

### Column Sizing

For OSM/external data, use generous column sizes — data quality varies:
- Names, addresses, URLs: `TEXT`
- Codes (postal, phone): `VARCHAR(50)`
- Categories, enums: `VARCHAR(50)`

## External APIs

### Rate Limiting

For APIs with rate limits (like Overpass), implement delays between requests:

```csharp
private async Task EnforceRateLimitAsync(CancellationToken ct)
{
    var elapsed = DateTime.UtcNow - _lastRequestTime;
    var delay = TimeSpan.FromMilliseconds(_options.DelayMs) - elapsed;

    if (delay > TimeSpan.Zero)
        await Task.Delay(delay, ct);

    _lastRequestTime = DateTime.UtcNow;
}
```

### Retry with Backoff

For transient failures (429, 503, timeouts), retry with exponential backoff:

```csharp
var retryDelay = baseDelay * Math.Pow(2, attempt - 1);  // 5s, 10s, 20s...
await Task.Delay(retryDelay, cancellationToken);
```
