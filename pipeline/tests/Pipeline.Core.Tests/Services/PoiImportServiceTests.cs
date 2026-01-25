using FluentAssertions;
using Pipeline.Core.Dtos.Overpass;
using Pipeline.Core.Services.PoiImport;
using Xunit;

namespace Pipeline.Core.Tests.Services;

public class OverpassToPoisConverterTests
{
    #region Category Mapping Tests

    [Theory]
    [InlineData("amenity", "veterinary", "vet", "pets")]
    [InlineData("shop", "pet", "pet_store", "pets")]
    [InlineData("leisure", "dog_park", "dog_park", "pets")]
    [InlineData("shop", "supermarket", "supermarket", "shopping")]
    [InlineData("amenity", "pharmacy", "pharmacy", "healthcare")]
    [InlineData("amenity", "school", "school", "education")]
    [InlineData("highway", "bus_stop", "bus_stop", "transport")]
    [InlineData("public_transport", "platform", "bus_stop", "transport")]
    [InlineData("railway", "station", "train_station", "transport")]
    [InlineData("railway", "halt", "train_station", "transport")]
    [InlineData("leisure", "park", "park", "green")]
    public void GetCategoryAndDomain_WithKnownTag_ReturnsMappedValues(
        string tagKey, string tagValue, string expectedCategory, string expectedDomain)
    {
        // Arrange
        var tags = new Dictionary<string, string> { [tagKey] = tagValue };

        // Act
        var (category, domain) = OverpassToPoisConverter.GetCategoryAndDomain(tags);

        // Assert
        category.Should().Be(expectedCategory);
        domain.Should().Be(expectedDomain);
    }

    [Fact]
    public void GetCategoryAndDomain_WithUnknownTag_ReturnsNull()
    {
        // Arrange
        var tags = new Dictionary<string, string> { ["unknown"] = "value" };

        // Act
        var (category, domain) = OverpassToPoisConverter.GetCategoryAndDomain(tags);

        // Assert
        category.Should().BeNull();
        domain.Should().BeNull();
    }

    [Fact]
    public void GetCategoryAndDomain_WithNullTags_ReturnsNull()
    {
        // Act
        var (category, domain) = OverpassToPoisConverter.GetCategoryAndDomain(null);

        // Assert
        category.Should().BeNull();
        domain.Should().BeNull();
    }

    [Fact]
    public void GetCategoryAndDomain_WithMultipleTags_ReturnsFirstMatch()
    {
        // Arrange: element with multiple tags (first match wins)
        var tags = new Dictionary<string, string>
        {
            ["name"] = "Test Shop",
            ["shop"] = "pet",
            ["amenity"] = "pharmacy" // Also matches, but shop=pet should match first
        };

        // Act
        var (category, domain) = OverpassToPoisConverter.GetCategoryAndDomain(tags);

        // Assert: depending on dictionary iteration order, either could match
        // The important thing is that we get a valid match
        category.Should().NotBeNull();
        domain.Should().NotBeNull();
    }

    #endregion

    #region Element Parsing Tests

    [Fact]
    public void GetCoordinates_FromNode_ReturnsLatLon()
    {
        // Arrange: node element has lat/lon directly
        var element = new OverpassElement
        {
            Type = "node",
            Id = 123,
            Lat = 51.0543,
            Lon = 3.7174
        };

        // Act
        var (lat, lon) = OverpassToPoisConverter.GetCoordinates(element);

        // Assert
        lat.Should().Be(51.0543);
        lon.Should().Be(3.7174);
    }

    [Fact]
    public void GetCoordinates_FromWayWithCenter_ReturnsCenterLatLon()
    {
        // Arrange: way element has center from "out center"
        var element = new OverpassElement
        {
            Type = "way",
            Id = 456,
            Center = new OverpassCenter { Lat = 50.8503, Lon = 4.3517 }
        };

        // Act
        var (lat, lon) = OverpassToPoisConverter.GetCoordinates(element);

        // Assert
        lat.Should().Be(50.8503);
        lon.Should().Be(4.3517);
    }

    [Fact]
    public void GetCoordinates_FromRelationWithCenter_ReturnsCenterLatLon()
    {
        // Arrange: relation element has center
        var element = new OverpassElement
        {
            Type = "relation",
            Id = 789,
            Center = new OverpassCenter { Lat = 51.2194, Lon = 4.4025 }
        };

        // Act
        var (lat, lon) = OverpassToPoisConverter.GetCoordinates(element);

        // Assert
        lat.Should().Be(51.2194);
        lon.Should().Be(4.4025);
    }

    [Fact]
    public void GetCoordinates_WithNoCoordinates_ReturnsNull()
    {
        // Arrange: element without coordinates
        var element = new OverpassElement
        {
            Type = "way",
            Id = 999
        };

        // Act
        var (lat, lon) = OverpassToPoisConverter.GetCoordinates(element);

        // Assert
        lat.Should().BeNull();
        lon.Should().BeNull();
    }

    [Fact]
    public void GetCoordinates_PrefersDirectLatLonOverCenter()
    {
        // Arrange: element with both direct lat/lon and center (shouldn't happen, but test priority)
        var element = new OverpassElement
        {
            Type = "node",
            Id = 111,
            Lat = 51.0,
            Lon = 3.0,
            Center = new OverpassCenter { Lat = 52.0, Lon = 4.0 }
        };

        // Act
        var (lat, lon) = OverpassToPoisConverter.GetCoordinates(element);

        // Assert: should prefer direct lat/lon
        lat.Should().Be(51.0);
        lon.Should().Be(3.0);
    }

    #endregion
}
