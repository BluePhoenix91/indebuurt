using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace Pipeline.Core.Services.Statbel;

/// <summary>
/// Parses Statbel population data from pipe-delimited text files.
/// </summary>
public interface IPopulationDataParser
{
    /// <summary>
    /// Parse population data file and aggregate to neighborhoods.
    /// </summary>
    List<NeighborhoodPopulation> Parse(string filePath);
}

/// <summary>
/// Parses Statbel population data from pipe-delimited text files.
/// Aggregates sector-level data to neighborhood level.
/// </summary>
public class PopulationDataParser(ILogger<PopulationDataParser> logger) : IPopulationDataParser
{
    /// <summary>
    /// Parse population data file and aggregate to neighborhoods.
    /// </summary>
    /// <param name="filePath">Path to OPENDATA_SECTOREN_{year}.txt</param>
    /// <returns>List of neighborhood population records</returns>
    public List<NeighborhoodPopulation> Parse(string filePath)
    {
        logger.LogInformation("Parsing population data from {FilePath}", filePath);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = "|",
            HasHeaderRecord = true,
            BadDataFound = null, // Ignore bad data
            MissingFieldFound = null
        };

        // Read with BOM handling
        using var reader = new StreamReader(filePath, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        using var csv = new CsvReader(reader, config);

        // Read all records
        csv.Read();
        csv.ReadHeader();

        // Find the area column (name varies: "OPPERVLAKKTE IN HM²" or similar)
        var headers = csv.HeaderRecord ?? [];
        var areaColumnName = headers.FirstOrDefault(h =>
            h.Contains("OPPERVLAKKTE", StringComparison.OrdinalIgnoreCase) ||
            h.Contains("HM", StringComparison.OrdinalIgnoreCase));

        if (areaColumnName == null)
        {
            throw new InvalidOperationException(
                $"Could not find area column in population file. Available columns: {string.Join(", ", headers)}");
        }

        logger.LogDebug("Using area column: {AreaColumn}", areaColumnName);

        // Parse sectors and aggregate to neighborhoods
        var sectorData = new List<(string NisCode, int Population, decimal AreaHm2)>();

        while (csv.Read())
        {
            var sectorCode = csv.GetField<string>("CD_SECTOR");
            var populationStr = csv.GetField<string>("TOTAL");
            var areaStr = csv.GetField<string>(areaColumnName);

            if (string.IsNullOrWhiteSpace(sectorCode))
                continue;

            // Parse population (handle empty/null)
            if (!int.TryParse(populationStr, out var population))
                population = 0;

            // Parse area (handle Belgian comma decimal separator)
            var areaHm2 = ParseBelgianDecimal(areaStr);

            sectorData.Add((sectorCode, population, areaHm2));
        }

        logger.LogInformation("Parsed {SectorCount} statistical sectors", sectorData.Count);

        // Aggregate to neighborhoods (first 7 chars of sector code)
        var neighborhoods = sectorData
            .GroupBy(s => s.NisCode[..7])
            .Select(g =>
            {
                var totalPopulation = g.Sum(s => s.Population);
                var totalAreaHm2 = g.Sum(s => s.AreaHm2);
                var areaKm2 = totalAreaHm2 / 100m; // 1 km² = 100 hectares

                var density = areaKm2 > 0
                    ? Math.Round(totalPopulation / areaKm2, 2)
                    : 0m;

                return new NeighborhoodPopulation(
                    g.Key,
                    totalPopulation,
                    Math.Round(areaKm2, 4),
                    density);
            })
            .ToList();

        var totalPopulation = neighborhoods.Sum(n => n.Population);
        logger.LogInformation(
            "Aggregated to {NeighborhoodCount} neighborhoods, total population: {TotalPopulation:N0}",
            neighborhoods.Count, totalPopulation);

        return neighborhoods;
    }

    /// <summary>
    /// Parse a decimal value that may use Belgian formatting (comma as decimal separator).
    /// </summary>
    private static decimal ParseBelgianDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return 0m;

        // Check if the value contains a comma - likely Belgian format
        if (value.Contains(','))
        {
            // Try Belgian culture first (comma as decimal separator)
            var belgianCulture = new CultureInfo("nl-BE");
            if (decimal.TryParse(value, NumberStyles.Number, belgianCulture, out var belgianResult))
                return belgianResult;

            // Fallback: replace comma with period
            var normalized = value.Replace(",", ".");
            if (decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out belgianResult))
                return belgianResult;
        }

        // No comma - try invariant culture (period as decimal)
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var result))
            return result;

        return 0m;
    }
}
