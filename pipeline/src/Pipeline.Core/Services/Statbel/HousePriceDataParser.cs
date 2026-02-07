using System.Data;
using ExcelDataReader;
using Microsoft.Extensions.Logging;

namespace Pipeline.Core.Services.Statbel;

/// <summary>
/// Parses Statbel house price data from Excel files.
/// </summary>
public interface IHousePriceDataParser
{
    /// <summary>
    /// Parse house price data from Excel file.
    /// </summary>
    (List<MunicipalityHousePrice> Prices, int DetectedYear) Parse(string filePath, int? year = null);
}

/// <summary>
/// Parses Statbel house price data from Excel files.
/// Filters to municipality level and regular houses only.
/// </summary>
public class HousePriceDataParser(ILogger<HousePriceDataParser> logger) : IHousePriceDataParser
{
    // Required for ExcelDataReader to work on non-Windows platforms
    static HousePriceDataParser()
    {
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// Parse house price data from Excel file.
    /// </summary>
    /// <param name="filePath">Path to vastgoed_2010_9999.xlsx</param>
    /// <param name="year">Target year (uses latest sheet if null)</param>
    /// <returns>List of municipality house prices and the detected year</returns>
    public (List<MunicipalityHousePrice> Prices, int DetectedYear) Parse(string filePath, int? year = null)
    {
        logger.LogInformation("Parsing house price data from {FilePath}", filePath);

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = true
            }
        });

        // Find target sheet
        var sheetNames = dataSet.Tables.Cast<DataTable>().Select(t => t.TableName).ToList();
        logger.LogDebug("Available sheets: {Sheets}", string.Join(", ", sheetNames));

        var targetSheet = FindTargetSheet(dataSet, year);
        if (targetSheet == null)
        {
            throw new InvalidOperationException(
                $"Could not find sheet for year {year}. Available sheets: {string.Join(", ", sheetNames)}");
        }

        var detectedYear = int.Parse(targetSheet.TableName);
        logger.LogInformation("Using sheet: {SheetName}", targetSheet.TableName);

        // Find column indices dynamically
        var columns = FindColumns(targetSheet);

        // Parse and filter data
        var prices = new List<MunicipalityHousePrice>();
        var rowsProcessed = 0;
        var rowsFiltered = 0;

        foreach (DataRow row in targetSheet.Rows)
        {
            rowsProcessed++;

            // Get NIS code
            var nisCode = row[columns.NisCodeIndex]?.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(nisCode))
                continue;

            // Normalize to 5 chars (pad with leading zeros if needed)
            nisCode = nisCode.PadLeft(5, '0');

            // Filter by geographic level (municipality = level 5 or NIS code length = 5)
            if (columns.LevelIndex >= 0)
            {
                var levelStr = row[columns.LevelIndex]?.ToString();
                if (!IsValidLevel(levelStr))
                {
                    rowsFiltered++;
                    continue;
                }
            }
            else if (nisCode.Length != 5)
            {
                rowsFiltered++;
                continue;
            }

            // Filter by property type (regular houses, exclude apartments)
            if (columns.TypeIndex >= 0)
            {
                var propertyType = row[columns.TypeIndex]?.ToString()?.ToLowerInvariant() ?? "";
                if (!IsRegularHouse(propertyType))
                {
                    rowsFiltered++;
                    continue;
                }
            }

            // Get median price (Q50)
            var priceStr = row[columns.Q50Index]?.ToString();
            if (!TryParsePrice(priceStr, out var medianPrice))
                continue;

            prices.Add(new MunicipalityHousePrice(nisCode, medianPrice));
        }

        // Deduplicate (take first per municipality, shouldn't happen but be safe)
        var dedupedPrices = prices
            .GroupBy(p => p.MunicipalityNis)
            .Select(g => g.First())
            .ToList();

        logger.LogInformation(
            "Parsed {RowCount} rows, filtered {FilteredCount}, result: {MunicipalityCount} municipalities",
            rowsProcessed, rowsFiltered, dedupedPrices.Count);

        if (dedupedPrices.Count > 0)
        {
            var minPrice = dedupedPrices.Min(p => p.MedianHousePrice);
            var maxPrice = dedupedPrices.Max(p => p.MedianHousePrice);
            logger.LogInformation("Price range: {MinPrice:N0} - {MaxPrice:N0} EUR", minPrice, maxPrice);
        }

        return (dedupedPrices, detectedYear);
    }

    /// <summary>
    /// Detect the latest year available in the Excel file.
    /// </summary>
    public int DetectLatestYear(string filePath)
    {
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read);
        using var reader = ExcelReaderFactory.CreateReader(stream);

        var dataSet = reader.AsDataSet();

        var numericSheets = dataSet.Tables.Cast<DataTable>()
            .Where(t => int.TryParse(t.TableName, out _))
            .Select(t => int.Parse(t.TableName))
            .OrderByDescending(y => y)
            .ToList();

        if (numericSheets.Count == 0)
            throw new InvalidOperationException("No year sheets found in Excel file");

        return numericSheets.First();
    }

    private DataTable? FindTargetSheet(DataSet dataSet, int? year)
    {
        if (year.HasValue)
        {
            var exactMatch = dataSet.Tables.Cast<DataTable>()
                .FirstOrDefault(t => t.TableName == year.Value.ToString());

            if (exactMatch != null)
                return exactMatch;
        }

        // Find latest numeric sheet
        return dataSet.Tables.Cast<DataTable>()
            .Where(t => int.TryParse(t.TableName, out _))
            .OrderByDescending(t => int.Parse(t.TableName))
            .FirstOrDefault();
    }

    private record ColumnIndices(int NisCodeIndex, int TypeIndex, int Q50Index, int LevelIndex);

    private ColumnIndices FindColumns(DataTable table)
    {
        var columnNames = table.Columns.Cast<DataColumn>()
            .Select((c, i) => (Name: c.ColumnName.ToUpperInvariant(), Index: i))
            .ToList();

        // Find NIS code column
        var nisCodeIndex = columnNames
            .FirstOrDefault(c => c.Name.Contains("REFNIS") || c.Name.Contains("NIS")).Index;

        // Find property type column
        var typeIndex = columnNames
            .FirstOrDefault(c => c.Name.Contains("TYPE")).Index;

        // Find Q50/median column
        var q50Index = columnNames
            .FirstOrDefault(c => c.Name.Contains("Q50") || c.Name.Contains("P_50") || c.Name.Contains("MEDIAN")).Index;

        // Find geographic level column
        var levelIndex = columnNames
            .FirstOrDefault(c => c.Name.Contains("LEVEL") || c.Name.Contains("NIVEAU")).Index;

        if (nisCodeIndex < 0 || q50Index < 0)
        {
            throw new InvalidOperationException(
                $"Could not find required columns. Available: {string.Join(", ", table.Columns.Cast<DataColumn>().Select(c => c.ColumnName))}");
        }

        logger.LogDebug(
            "Column indices: NIS={NisIndex}, Type={TypeIndex}, Q50={Q50Index}, Level={LevelIndex}",
            nisCodeIndex, typeIndex, q50Index, levelIndex);

        return new ColumnIndices(nisCodeIndex, typeIndex, q50Index, levelIndex);
    }

    private static bool IsValidLevel(string? levelStr)
    {
        if (string.IsNullOrWhiteSpace(levelStr))
            return false;

        // Municipality level is typically 5 or "5" or "Gemeente"
        if (levelStr == "5" || levelStr.Equals("gemeente", StringComparison.OrdinalIgnoreCase))
            return true;

        if (int.TryParse(levelStr, out var level))
            return level == 5;

        return false;
    }

    private static bool IsRegularHouse(string propertyType)
    {
        // Include: maison, huis, woning, house
        // Exclude: appartement, apartment, flat
        var isHouse = propertyType.Contains("maison") ||
                      propertyType.Contains("huis") ||
                      propertyType.Contains("woning") ||
                      propertyType.Contains("house");

        var isApartment = propertyType.Contains("appartement") ||
                          propertyType.Contains("apartment") ||
                          propertyType.Contains("flat");

        return isHouse && !isApartment;
    }

    private static bool TryParsePrice(string? priceStr, out int price)
    {
        price = 0;

        if (string.IsNullOrWhiteSpace(priceStr))
            return false;

        // Try direct parse
        if (decimal.TryParse(priceStr, out var decimalPrice))
        {
            price = (int)Math.Round(decimalPrice);
            return price > 0;
        }

        // Try with culture variations
        var normalized = priceStr.Replace(",", ".").Replace(" ", "");
        if (decimal.TryParse(normalized, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out decimalPrice))
        {
            price = (int)Math.Round(decimalPrice);
            return price > 0;
        }

        return false;
    }
}
