using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pipeline.Core.Services.Boundaries;
using Xunit;

namespace Pipeline.Core.Tests.Services.Boundaries;

public class GeoJsonSectorReaderTests
{
    private readonly GeoJsonSectorReader _reader;

    public GeoJsonSectorReaderTests()
    {
        var logger = Substitute.For<ILogger<GeoJsonSectorReader>>();
        _reader = new GeoJsonSectorReader(logger);
    }

    [Fact]
    public async Task ReadAsync_WithValidGeoJson_ParsesFeaturesCorrectly()
    {
        // Arrange
        var geoJson = CreateTestGeoJson(
            CreateFeature("44021A001", "Binnenstad", "Gent", "Provincie Oost-Vlaanderen", null),
            CreateFeature("11002A001", "Centrum", "Antwerpen", "Provincie Antwerpen", null));

        var filePath = WriteTempFile(geoJson);

        try
        {
            // Act
            var (totalFeatures, sectors) = await _reader.ReadAsync(filePath, null);

            // Assert
            totalFeatures.Should().Be(2);
            sectors.Should().HaveCount(2);

            sectors[0].CdSector.Should().Be("44021A001");
            sectors[0].SectorNameNl.Should().Be("Binnenstad");
            sectors[0].CityNameNl.Should().Be("Gent");
            sectors[0].ProvinceNl.Should().Be("Provincie Oost-Vlaanderen");
            sectors[0].GeometryWkb.Should().NotBeEmpty();

            sectors[1].CdSector.Should().Be("11002A001");
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ReadAsync_FiltersWallonianProvinces()
    {
        // Arrange
        var geoJson = CreateTestGeoJson(
            CreateFeature("44021A001", "Binnenstad", "Gent", "Provincie Oost-Vlaanderen", null),
            CreateFeature("62063A001", "Centre", "Liège", "Province de Liège", null),
            CreateFeature("52011A001", "Centre", "Charleroi", "Province de Hainaut", null));

        var filePath = WriteTempFile(geoJson);

        try
        {
            // Act
            var (totalFeatures, sectors) = await _reader.ReadAsync(filePath, null);

            // Assert
            totalFeatures.Should().Be(3);
            sectors.Should().HaveCount(1);
            sectors[0].CdSector.Should().Be("44021A001");
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ReadAsync_IncludesBrusselsRegion()
    {
        // Arrange
        var geoJson = CreateTestGeoJson(
            CreateFeature("21004A001", "Centrum", "Brussel", null, "Brussels Hoofdstedelijk Gewest"));

        var filePath = WriteTempFile(geoJson);

        try
        {
            // Act
            var (_, sectors) = await _reader.ReadAsync(filePath, null);

            // Assert
            sectors.Should().HaveCount(1);
            sectors[0].CdSector.Should().Be("21004A001");
            sectors[0].RegionNl.Should().Be("Brussels Hoofdstedelijk Gewest");
            sectors[0].ProvinceNl.Should().BeNull();
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ReadAsync_AllFlemishProvinces_AreIncluded()
    {
        // Arrange
        var geoJson = CreateTestGeoJson(
            CreateFeature("11001A001", "Test", "Aartselaar", "Provincie Antwerpen", null),
            CreateFeature("71001A001", "Test", "Alken", "Provincie Limburg", null),
            CreateFeature("44001A001", "Test", "Aalst", "Provincie Oost-Vlaanderen", null),
            CreateFeature("31001A001", "Test", "Brugge", "Provincie West-Vlaanderen", null),
            CreateFeature("24001A001", "Test", "Aarschot", "Provincie Vlaams-Brabant", null));

        var filePath = WriteTempFile(geoJson);

        try
        {
            // Act
            var (_, sectors) = await _reader.ReadAsync(filePath, null);

            // Assert
            sectors.Should().HaveCount(5);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ReadAsync_FeatureWithoutSectorCode_IsSkipped()
    {
        // Arrange - feature with missing cd_sector
        var geoJson = """
            {
                "type": "FeatureCollection",
                "features": [
                    {
                        "type": "Feature",
                        "properties": {
                            "tx_munty_descr_nl": "Gent",
                            "tx_prov_descr_nl": "Provincie Oost-Vlaanderen"
                        },
                        "geometry": {
                            "type": "Polygon",
                            "coordinates": [[[100000, 200000], [100100, 200000], [100100, 200100], [100000, 200100], [100000, 200000]]]
                        }
                    }
                ]
            }
            """;

        var filePath = WriteTempFile(geoJson);

        try
        {
            // Act
            var (totalFeatures, sectors) = await _reader.ReadAsync(filePath, null);

            // Assert
            totalFeatures.Should().Be(1);
            sectors.Should().BeEmpty();
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public async Task ReadAsync_EmptyFeatureCollection_ReturnsEmpty()
    {
        var geoJson = """{ "type": "FeatureCollection", "features": [] }""";
        var filePath = WriteTempFile(geoJson);

        try
        {
            var (totalFeatures, sectors) = await _reader.ReadAsync(filePath, null);

            totalFeatures.Should().Be(0);
            sectors.Should().BeEmpty();
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    #region Helpers

    private static string CreateTestGeoJson(params string[] features)
    {
        return $$"""
            {
                "type": "FeatureCollection",
                "features": [{{string.Join(",\n", features)}}]
            }
            """;
    }

    private static string CreateFeature(string cdSector, string sectorName, string city, string? province, string? region)
    {
        var properties = new List<string>
        {
            $"\"cd_sector\": \"{cdSector}\"",
            $"\"tx_sector_descr_nl\": \"{sectorName}\"",
            $"\"tx_munty_descr_nl\": \"{city}\""
        };

        if (province != null)
            properties.Add($"\"tx_prov_descr_nl\": \"{province}\"");
        if (region != null)
            properties.Add($"\"tx_rgn_descr_nl\": \"{region}\"");

        // Simple polygon in EPSG:31370 coordinate space (Belgian Lambert 72)
        return $$"""
            {
                "type": "Feature",
                "properties": { {{string.Join(", ", properties)}} },
                "geometry": {
                    "type": "Polygon",
                    "coordinates": [[[100000, 200000], [100100, 200000], [100100, 200100], [100000, 200100], [100000, 200000]]]
                }
            }
            """;
    }

    private static string WriteTempFile(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.geojson");
        File.WriteAllText(path, content);
        return path;
    }

    #endregion
}
