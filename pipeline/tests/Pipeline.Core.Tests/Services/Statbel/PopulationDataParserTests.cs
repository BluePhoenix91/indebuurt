using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pipeline.Core.Services.Statbel;
using Xunit;

namespace Pipeline.Core.Tests.Services.Statbel;

public class PopulationDataParserTests : IDisposable
{
    private readonly PopulationDataParser _parser;
    private readonly string _tempDir;

    public PopulationDataParserTests()
    {
        var logger = Substitute.For<ILogger<PopulationDataParser>>();
        _parser = new PopulationDataParser(logger);
        _tempDir = Path.Combine(Path.GetTempPath(), $"statbel_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Parse_WithValidData_AggregatesCorrectly()
    {
        // Arrange: Two sectors in same neighborhood (44021A0)
        // Note: area is in hectares (hm²), 1 km² = 100 hectares
        // 50 hm² = 0.5 km², 25 hm² = 0.25 km²
        var content = "CD_REFNIS|CD_SECTOR|TOTAL|OPPERVLAKKTE IN HM²\n" +
                      "44021|44021A001|1000|50\n" +
                      "44021|44021A002|500|25\n" +
                      "44022|44022B001|2000|100";
        var filePath = WriteTestFile("population.txt", content);

        // Act
        var result = _parser.Parse(filePath);

        // Assert
        result.Should().HaveCount(2);

        var neighborhood1 = result.First(n => n.NisCode == "44021A0");
        neighborhood1.Population.Should().Be(1500); // 1000 + 500
        neighborhood1.AreaKm2.Should().Be(0.75m); // (50 + 25) / 100
        neighborhood1.PopulationDensity.Should().Be(2000m); // 1500 / 0.75

        var neighborhood2 = result.First(n => n.NisCode == "44022B0");
        neighborhood2.Population.Should().Be(2000);
        neighborhood2.AreaKm2.Should().Be(1m);
        neighborhood2.PopulationDensity.Should().Be(2000m);
    }

    [Fact]
    public void Parse_WithBomEncoding_HandlesCorrectly()
    {
        // Arrange: File with UTF-8 BOM
        var content = "\uFEFFCD_REFNIS|CD_SECTOR|TOTAL|OPPERVLAKKTE IN HM²\n44021|44021A001|1000|50";
        var filePath = WriteTestFile("population_bom.txt", content);

        // Act
        var result = _parser.Parse(filePath);

        // Assert
        result.Should().HaveCount(1);
        result[0].NisCode.Should().Be("44021A0");
    }

    [Fact]
    public void Parse_WithMissingPopulation_TreatsAsZero()
    {
        // Arrange
        var content = "CD_REFNIS|CD_SECTOR|TOTAL|OPPERVLAKKTE IN HM²\n44021|44021A001||50";
        var filePath = WriteTestFile("population_empty.txt", content);

        // Act
        var result = _parser.Parse(filePath);

        // Assert
        result.Should().HaveCount(1);
        result[0].Population.Should().Be(0);
    }

    [Fact]
    public void Parse_WithCommaDecimalSeparator_ParsesCorrectly()
    {
        // Arrange: Belgian format uses comma as decimal separator
        var content = "CD_REFNIS|CD_SECTOR|TOTAL|OPPERVLAKKTE IN HM²\n44021|44021A001|1000|50,50";
        var filePath = WriteTestFile("population_comma.txt", content);

        // Act
        var result = _parser.Parse(filePath);

        // Assert
        result.Should().HaveCount(1);
        result[0].AreaKm2.Should().Be(0.505m);
    }

    [Fact]
    public void Parse_WithPeriodDecimalSeparator_ParsesCorrectly()
    {
        // Arrange: Some files might use period instead of comma
        var content = "CD_REFNIS|CD_SECTOR|TOTAL|OPPERVLAKKTE IN HM²\n44021|44021A001|1000|50.00";
        var filePath = WriteTestFile("population_period.txt", content);

        // Act
        var result = _parser.Parse(filePath);

        // Assert
        result.Should().HaveCount(1);
        result[0].AreaKm2.Should().Be(0.5m);
    }

    [Fact]
    public void Parse_WithZeroArea_ReturnsZeroDensity()
    {
        // Arrange
        var content = "CD_REFNIS|CD_SECTOR|TOTAL|OPPERVLAKKTE IN HM²\n44021|44021A001|1000|0";
        var filePath = WriteTestFile("population_zero_area.txt", content);

        // Act
        var result = _parser.Parse(filePath);

        // Assert
        result.Should().HaveCount(1);
        result[0].PopulationDensity.Should().Be(0m);
    }

    [Fact]
    public void Parse_WithAlternativeAreaColumnName_FindsColumn()
    {
        // Arrange: Different column name that still contains "HM"
        var content = "CD_REFNIS|CD_SECTOR|TOTAL|SUPERFICIE HM2\n44021|44021A001|1000|50";
        var filePath = WriteTestFile("population_alt_column.txt", content);

        // Act
        var result = _parser.Parse(filePath);

        // Assert
        result.Should().HaveCount(1);
        result[0].AreaKm2.Should().Be(0.5m);
    }

    [Fact]
    public void Parse_WithEmptySectorCode_SkipsRow()
    {
        // Arrange
        var content = "CD_REFNIS|CD_SECTOR|TOTAL|OPPERVLAKKTE IN HM²\n44021|44021A001|1000|50\n44021||500|25";
        var filePath = WriteTestFile("population_empty_sector.txt", content);

        // Act
        var result = _parser.Parse(filePath);

        // Assert
        result.Should().HaveCount(1);
        result[0].Population.Should().Be(1000); // Only first row counted
    }

    [Fact]
    public void Parse_ThrowsOnMissingAreaColumn()
    {
        // Arrange: No area column
        var content = "CD_REFNIS|CD_SECTOR|TOTAL|OTHER_COLUMN\n44021|44021A001|1000|50";
        var filePath = WriteTestFile("population_no_area.txt", content);

        // Act & Assert
        var act = () => _parser.Parse(filePath);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Could not find area column*");
    }

    private string WriteTestFile(string fileName, string content)
    {
        var filePath = Path.Combine(_tempDir, fileName);
        File.WriteAllText(filePath, content, System.Text.Encoding.UTF8);
        return filePath;
    }
}
