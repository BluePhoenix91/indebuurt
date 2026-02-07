namespace Pipeline.Core.Services.Boundaries;

/// <summary>
/// A single statistical sector feature parsed from the Statbel GeoJSON source.
/// Geometry stored in source CRS (EPSG:31370) as WKB for efficient PostgreSQL COPY.
/// </summary>
public record SectorFeature(
    string CdSector,
    string SectorNameNl,
    string CityNameNl,
    string? ProvinceNl,
    string? RegionNl,
    byte[] GeometryWkb);

/// <summary>
/// Aggregated neighborhood metadata queried from the staging table,
/// before slug generation in C#.
/// </summary>
public record NeighborhoodMetadata(
    string NisCode,
    string Name,
    string City,
    string? Province,
    int SectorCount);

/// <summary>
/// Result of the boundary import operation.
/// </summary>
public record BoundaryImportResult(
    int TotalFeaturesInFile,
    int SectorsAfterFilter,
    int SectorsImported,
    int NeighborhoodsCreated,
    int PreviousNeighborhoodCount,
    int PreviousSectorCount,
    List<string> Warnings)
{
    public bool HasWarnings => Warnings.Count > 0;
}
