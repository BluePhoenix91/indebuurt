namespace Pipeline.Core.Services.Statbel;

/// <summary>
/// Configuration options for Statbel data imports.
/// </summary>
public class StatbelOptions
{
    public const string SectionName = "Statbel";

    /// <summary>
    /// URL template for population data. Use {year} placeholder.
    /// Example: https://statbel.fgov.be/.../OPENDATA_SECTOREN_{year}.zip
    /// </summary>
    public string PopulationUrlTemplate { get; set; } =
        "https://statbel.fgov.be/sites/default/files/files/opendata/bevolking/sectoren/OPENDATA_SECTOREN_{year}.zip";

    /// <summary>
    /// URL for house prices Excel file (contains all years as sheets).
    /// </summary>
    public string HousePricesUrl { get; set; } =
        "https://statbel.fgov.be/sites/default/files/files/opendata/vastgoed/vastgoed_2010_9999.xlsx";

    /// <summary>
    /// HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;
}
