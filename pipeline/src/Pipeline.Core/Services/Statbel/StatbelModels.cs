namespace Pipeline.Core.Services.Statbel;

/// <summary>
/// Population data aggregated to neighborhood level.
/// </summary>
public record NeighborhoodPopulation(
    string NisCode,          // 7-char neighborhood code
    int Population,
    decimal AreaKm2,
    decimal PopulationDensity);

/// <summary>
/// House price data at municipality level.
/// </summary>
public record MunicipalityHousePrice(
    string MunicipalityNis,  // 5-char municipality code (Statbel's new codes for 2025)
    int MedianHousePrice);

/// <summary>
/// Result of a single dataset import.
/// </summary>
public record DatasetImportResult(
    int RowsProcessed,
    int NeighborhoodsUpdated,
    int NeighborhoodsSkipped,
    List<string> Warnings);

/// <summary>
/// Result of the full Statbel import operation.
/// </summary>
public record StatbelImportResult(
    int Year,
    DatasetImportResult? PopulationResult,
    DatasetImportResult? HousePriceResult)
{
    public bool HasWarnings =>
        (PopulationResult?.Warnings.Count > 0) == true ||
        (HousePriceResult?.Warnings.Count > 0) == true;

    public IEnumerable<string> AllWarnings =>
        (PopulationResult?.Warnings ?? []).Concat(HousePriceResult?.Warnings ?? []);
}
