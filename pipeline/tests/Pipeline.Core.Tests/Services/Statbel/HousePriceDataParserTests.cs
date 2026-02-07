using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pipeline.Core.Services.Statbel;
using Xunit;

namespace Pipeline.Core.Tests.Services.Statbel;

public class HousePriceDataParserTests : IDisposable
{
    private readonly HousePriceDataParser _parser;
    private readonly string _tempDir;

    public HousePriceDataParserTests()
    {
        var logger = Substitute.For<ILogger<HousePriceDataParser>>();
        _parser = new HousePriceDataParser(logger);
        _tempDir = Path.Combine(Path.GetTempPath(), $"statbel_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Parse_WithValidExcel_ExtractsHousePrices()
    {
        // Arrange: Create test Excel file
        var filePath = CreateTestExcelFile(new[]
        {
            new TestHousePriceRow("44021", "5", "Maisons d'habitation ordinaires", 350000m),
            new TestHousePriceRow("44022", "5", "Maisons d'habitation ordinaires", 280000m),
        });

        // Act
        var (prices, year) = _parser.Parse(filePath, 2024);

        // Assert
        prices.Should().HaveCount(2);
        prices.Should().Contain(p => p.MunicipalityNis == "44021" && p.MedianHousePrice == 350000);
        prices.Should().Contain(p => p.MunicipalityNis == "44022" && p.MedianHousePrice == 280000);
        year.Should().Be(2024);
    }

    [Fact]
    public void Parse_FiltersOutApartments()
    {
        // Arrange
        var filePath = CreateTestExcelFile(new[]
        {
            new TestHousePriceRow("44021", "5", "Maisons d'habitation ordinaires", 350000m),
            new TestHousePriceRow("44021", "5", "Appartements", 200000m), // Should be filtered
        });

        // Act
        var (prices, _) = _parser.Parse(filePath, 2024);

        // Assert
        prices.Should().HaveCount(1);
        prices[0].MedianHousePrice.Should().Be(350000);
    }

    [Fact]
    public void Parse_FiltersNonMunicipalityLevel()
    {
        // Arrange
        var filePath = CreateTestExcelFile(new[]
        {
            new TestHousePriceRow("44021", "5", "Maisons", 350000m), // Municipality
            new TestHousePriceRow("4", "2", "Maisons", 300000m), // Province level - should filter
        });

        // Act
        var (prices, _) = _parser.Parse(filePath, 2024);

        // Assert
        prices.Should().HaveCount(1);
        prices[0].MunicipalityNis.Should().Be("44021");
    }

    [Fact]
    public void Parse_PadsNisCodeToFiveChars()
    {
        // Arrange: NIS code with 4 chars
        var filePath = CreateTestExcelFile(new[]
        {
            new TestHousePriceRow("1001", "5", "Maisons", 400000m),
        });

        // Act
        var (prices, _) = _parser.Parse(filePath, 2024);

        // Assert
        prices.Should().HaveCount(1);
        prices[0].MunicipalityNis.Should().Be("01001");
    }

    [Fact]
    public void Parse_DeduplicatesMunicipalities()
    {
        // Arrange: Same municipality appears twice
        var filePath = CreateTestExcelFile(new[]
        {
            new TestHousePriceRow("44021", "5", "Maisons villa", 400000m),
            new TestHousePriceRow("44021", "5", "Maisons ordinaires", 350000m),
        });

        // Act
        var (prices, _) = _parser.Parse(filePath, 2024);

        // Assert
        prices.Should().HaveCount(1);
        // Takes first match
        prices[0].MedianHousePrice.Should().Be(400000);
    }

    [Fact]
    public void Parse_SkipsRowsWithMissingPrice()
    {
        // Arrange
        var filePath = CreateTestExcelFile(new[]
        {
            new TestHousePriceRow("44021", "5", "Maisons", 350000m),
            new TestHousePriceRow("44022", "5", "Maisons", null), // No price
        });

        // Act
        var (prices, _) = _parser.Parse(filePath, 2024);

        // Assert
        prices.Should().HaveCount(1);
        prices[0].MunicipalityNis.Should().Be("44021");
    }

    [Fact]
    public void Parse_UsesLatestSheetWhenYearNotSpecified()
    {
        // Arrange: Excel with multiple year sheets
        var filePath = CreateTestExcelFileMultipleYears();

        // Act
        var (prices, year) = _parser.Parse(filePath, year: null);

        // Assert
        year.Should().Be(2025); // Latest sheet
    }

    [Fact]
    public void DetectLatestYear_ReturnsLatestSheet()
    {
        // Arrange
        var filePath = CreateTestExcelFileMultipleYears();

        // Act
        var latestYear = _parser.DetectLatestYear(filePath);

        // Assert
        latestYear.Should().Be(2025);
    }

    private record TestHousePriceRow(string NisCode, string Level, string Type, decimal? Q50);

    private string CreateTestExcelFile(TestHousePriceRow[] rows)
    {
        // Since we can't easily create Excel files in tests without additional libraries,
        // we'll skip this test for now and rely on integration tests with real files.
        // In a real scenario, you'd use a library like ClosedXML to create test files.

        // For now, create a minimal valid xlsx file structure
        // This is a placeholder - in reality, you'd copy a test fixture file
        var filePath = Path.Combine(_tempDir, "test_houseprices.xlsx");

        // Create a minimal XLSX that ExcelDataReader can parse
        CreateMinimalExcel(filePath, "2024", rows);

        return filePath;
    }

    private string CreateTestExcelFileMultipleYears()
    {
        var filePath = Path.Combine(_tempDir, "test_multiyear.xlsx");

        // This would create sheets for 2023, 2024, 2025
        CreateMinimalExcelMultiSheet(filePath);

        return filePath;
    }

    private void CreateMinimalExcel(string filePath, string sheetName, TestHousePriceRow[] rows)
    {
        // Use System.IO.Compression to create a minimal XLSX file
        // XLSX is a ZIP file with XML contents

        using var archive = System.IO.Compression.ZipFile.Open(filePath, System.IO.Compression.ZipArchiveMode.Create);

        // [Content_Types].xml
        var contentTypesEntry = archive.CreateEntry("[Content_Types].xml");
        using (var stream = contentTypesEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write("""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                    <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                    <Default Extension="xml" ContentType="application/xml"/>
                    <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                    <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                </Types>
                """);
        }

        // _rels/.rels
        var relsEntry = archive.CreateEntry("_rels/.rels");
        using (var stream = relsEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write("""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
        }

        // xl/_rels/workbook.xml.rels
        var wbRelsEntry = archive.CreateEntry("xl/_rels/workbook.xml.rels");
        using (var stream = wbRelsEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write("""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                </Relationships>
                """);
        }

        // xl/workbook.xml
        var wbEntry = archive.CreateEntry("xl/workbook.xml");
        using (var stream = wbEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write($"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                    <sheets>
                        <sheet name="{sheetName}" sheetId="1" r:id="rId1"/>
                    </sheets>
                </workbook>
                """);
        }

        // xl/worksheets/sheet1.xml
        var sheetEntry = archive.CreateEntry("xl/worksheets/sheet1.xml");
        using (var stream = sheetEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            var rowsXml = new System.Text.StringBuilder();

            // Header row
            rowsXml.AppendLine("""<row r="1"><c r="A1" t="inlineStr"><is><t>CD_REFNIS</t></is></c><c r="B1" t="inlineStr"><is><t>CD_NIVEAU_REFNIS</t></is></c><c r="C1" t="inlineStr"><is><t>CD_TYPE_NL</t></is></c><c r="D1" t="inlineStr"><is><t>Q50</t></is></c></row>""");

            // Data rows
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                var rowNum = i + 2;
                var q50Value = row.Q50.HasValue ? $"<v>{row.Q50.Value}</v>" : "";
                rowsXml.AppendLine($"""<row r="{rowNum}"><c r="A{rowNum}" t="inlineStr"><is><t>{row.NisCode}</t></is></c><c r="B{rowNum}" t="inlineStr"><is><t>{row.Level}</t></is></c><c r="C{rowNum}" t="inlineStr"><is><t>{row.Type}</t></is></c><c r="D{rowNum}">{q50Value}</c></row>""");
            }

            writer.Write($"""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                    <sheetData>
                        {rowsXml}
                    </sheetData>
                </worksheet>
                """);
        }
    }

    private void CreateMinimalExcelMultiSheet(string filePath)
    {
        using var archive = System.IO.Compression.ZipFile.Open(filePath, System.IO.Compression.ZipArchiveMode.Create);

        // [Content_Types].xml
        var contentTypesEntry = archive.CreateEntry("[Content_Types].xml");
        using (var stream = contentTypesEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write("""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                    <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                    <Default Extension="xml" ContentType="application/xml"/>
                    <Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>
                    <Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                    <Override PartName="/xl/worksheets/sheet2.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                    <Override PartName="/xl/worksheets/sheet3.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/>
                </Types>
                """);
        }

        // _rels/.rels
        var relsEntry = archive.CreateEntry("_rels/.rels");
        using (var stream = relsEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write("""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/>
                </Relationships>
                """);
        }

        // xl/_rels/workbook.xml.rels
        var wbRelsEntry = archive.CreateEntry("xl/_rels/workbook.xml.rels");
        using (var stream = wbRelsEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write("""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                    <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/>
                    <Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet2.xml"/>
                    <Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet3.xml"/>
                </Relationships>
                """);
        }

        // xl/workbook.xml - sheets for 2023, 2024, 2025
        var wbEntry = archive.CreateEntry("xl/workbook.xml");
        using (var stream = wbEntry.Open())
        using (var writer = new StreamWriter(stream))
        {
            writer.Write("""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships">
                    <sheets>
                        <sheet name="2023" sheetId="1" r:id="rId1"/>
                        <sheet name="2024" sheetId="2" r:id="rId2"/>
                        <sheet name="2025" sheetId="3" r:id="rId3"/>
                    </sheets>
                </workbook>
                """);
        }

        // Create empty sheets
        var emptySheetContent = """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
                <sheetData>
                    <row r="1"><c r="A1" t="inlineStr"><is><t>CD_REFNIS</t></is></c><c r="B1" t="inlineStr"><is><t>CD_NIVEAU_REFNIS</t></is></c><c r="C1" t="inlineStr"><is><t>CD_TYPE_NL</t></is></c><c r="D1" t="inlineStr"><is><t>Q50</t></is></c></row>
                    <row r="2"><c r="A2" t="inlineStr"><is><t>44021</t></is></c><c r="B2" t="inlineStr"><is><t>5</t></is></c><c r="C2" t="inlineStr"><is><t>Maisons</t></is></c><c r="D2"><v>300000</v></c></row>
                </sheetData>
            </worksheet>
            """;

        foreach (var sheetNum in new[] { 1, 2, 3 })
        {
            var sheetEntry = archive.CreateEntry($"xl/worksheets/sheet{sheetNum}.xml");
            using var stream = sheetEntry.Open();
            using var writer = new StreamWriter(stream);
            writer.Write(emptySheetContent);
        }
    }
}
