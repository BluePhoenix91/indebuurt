using FluentAssertions;
using NSubstitute;
using Pipeline.Core.Entities.Content;
using Pipeline.Core.Enums;
using Pipeline.Core.Repositories;
using Pipeline.Core.Services;
using Xunit;

namespace Pipeline.Core.Tests.Services;

public class ValueCardBuilderTests
{
    private readonly IGisRepository _gisRepository;
    private readonly ValueCardBuilder _builder;

    public ValueCardBuilderTests()
    {
        _gisRepository = Substitute.For<IGisRepository>();
        _builder = new ValueCardBuilder(_gisRepository);
    }

    #region POI Cards - In Neighborhood

    [Fact]
    public async Task BuildAsync_WithPoisInNeighborhood_ReturnsCountAndNearestWalking()
    {
        // Arrange: 20 parks, nearest at 160m (2 min walk)
        var template = CreateTemplate(CardType.Parks);
        _gisRepository.GetPoiCountAsync("44021A0", "park", default)
            .Returns(new PoiCount(20, 160));
        _gisRepository.GetNearestPoiAsync("44021A0", "park", default)
            .Returns(new NearestPoi(123, "Appelbrugparkje", 160, IsInside: true));

        // Act
        var result = await _builder.BuildAsync("44021A0", template);

        // Assert
        result.Title.Should().Be("Parken");
        result.Description.Should().Be("20 parken in de wijk");
        result.Detail.Should().Be("Dichtstbijzijnde op 2 min lopen"); // Parks don't show name
        result.Distance.Should().Be("2 mins");
        result.DistanceIcon.Should().Be("fa-solid fa-person-walking");
        result.Icon.Should().Be("fa-solid fa-tree");
    }

    [Fact]
    public async Task BuildAsync_WithNamedPoi_IncludesNameInDetail()
    {
        // Arrange: Vet at 400m (5 min walk)
        var template = CreateTemplate(CardType.Vets);
        _gisRepository.GetPoiCountAsync("44021A0", "veterinary", default)
            .Returns(new PoiCount(3, 400));
        _gisRepository.GetNearestPoiAsync("44021A0", "veterinary", default)
            .Returns(new NearestPoi(456, "Dierenkliniek Gent", 400, IsInside: true));

        // Act
        var result = await _builder.BuildAsync("44021A0", template);

        // Assert
        result.Description.Should().Be("3 dierenartsen in de wijk");
        result.Detail.Should().Be("Dierenkliniek Gent op 5 min lopen"); // Vets show name
    }

    [Fact]
    public async Task BuildAsync_SinglePoi_UsesSingularForm()
    {
        // Arrange: 1 supermarket
        var template = CreateTemplate(CardType.Supermarkets);
        _gisRepository.GetPoiCountAsync("44021A0", "supermarket", default)
            .Returns(new PoiCount(1, 200));
        _gisRepository.GetNearestPoiAsync("44021A0", "supermarket", default)
            .Returns(new NearestPoi(789, "Colruyt", 200, IsInside: true));

        // Act
        var result = await _builder.BuildAsync("44021A0", template);

        // Assert
        result.Description.Should().Be("1 supermarkt in de wijk");
    }

    #endregion

    #region POI Cards - Outside Neighborhood

    [Fact]
    public async Task BuildAsync_WithNearbyPoiOutsideNeighborhood_ShowsNeighborhoodName()
    {
        // Arrange: No dog parks in neighborhood, nearest in Dampoort at 800m
        var template = CreateTemplate(CardType.DogParks);
        _gisRepository.GetPoiCountAsync("44021A0", "dog_park", default)
            .Returns(new PoiCount(0, null));
        _gisRepository.GetNearestPoiAsync("44021A0", "dog_park", default)
            .Returns(new NearestPoi(456, "Hondenspeelweide Dampoort", 800, IsInside: false));
        _gisRepository.GetContainingNeighborhoodNameAsync(456, default)
            .Returns("Dampoort");

        // Act
        var result = await _builder.BuildAsync("44021A0", template);

        // Assert
        result.Description.Should().Be("Hondenspeelweide in naburig Dampoort");
        result.Detail.Should().Be("Dichtstbijzijnde op 10 min lopen");
        result.Distance.Should().Be("10 mins");
    }

    [Fact]
    public async Task BuildAsync_WithNearbyPoiOutside_NoNeighborhoodName_ShowsGenericPhrase()
    {
        // Arrange: No parks in neighborhood, nearest outside but no neighborhood name
        var template = CreateTemplate(CardType.Parks);
        _gisRepository.GetPoiCountAsync("44021A0", "park", default)
            .Returns(new PoiCount(0, null));
        _gisRepository.GetNearestPoiAsync("44021A0", "park", default)
            .Returns(new NearestPoi(789, "Testpark", 500, IsInside: false));
        _gisRepository.GetContainingNeighborhoodNameAsync(789, default)
            .Returns((string?)null);

        // Act
        var result = await _builder.BuildAsync("44021A0", template);

        // Assert
        result.Description.Should().Be("Park in de buurt");
    }

    #endregion

    #region POI Cards - No POIs Found

    [Fact]
    public async Task BuildAsync_WithNoPoisAtAll_ReturnsNotFoundText()
    {
        // Arrange: No vets at all
        var template = CreateTemplate(CardType.Vets);
        _gisRepository.GetPoiCountAsync("44021A0", "veterinary", default)
            .Returns((PoiCount?)null);
        _gisRepository.GetNearestPoiAsync("44021A0", "veterinary", default)
            .Returns((NearestPoi?)null);

        // Act
        var result = await _builder.BuildAsync("44021A0", template);

        // Assert
        result.Description.Should().Be("Geen dierenarts gevonden");
        result.Detail.Should().Be("Niet binnen 30 minuten bereikbaar");
        result.Distance.Should().BeEmpty();
        result.DistanceIcon.Should().BeNull();
    }

    #endregion

    #region Travel Mode Tests

    [Theory]
    [InlineData(50, "50m", "fa-solid fa-person-walking")]     // Very close = meters
    [InlineData(80, "80m", "fa-solid fa-person-walking")]     // Under 100m = meters
    [InlineData(160, "2 mins", "fa-solid fa-person-walking")] // 2 min walk
    [InlineData(800, "10 mins", "fa-solid fa-person-walking")] // 10 min walk
    public async Task BuildAsync_WalkingDistance_FormatsCorrectly(double meters, string expectedDistance, string expectedIcon)
    {
        // Arrange: Park at various distances (all within walking range)
        var template = CreateTemplate(CardType.Parks);
        _gisRepository.GetPoiCountAsync("44021A0", "park", default)
            .Returns(new PoiCount(5, meters));
        _gisRepository.GetNearestPoiAsync("44021A0", "park", default)
            .Returns(new NearestPoi(123, "Testpark", meters, IsInside: true));

        // Act
        var result = await _builder.BuildAsync("44021A0", template);

        // Assert
        result.Distance.Should().Be(expectedDistance);
        result.DistanceIcon.Should().Be(expectedIcon);
    }

    [Fact]
    public async Task BuildAsync_BikingDistance_ShowsBikeIcon()
    {
        // Arrange: Park at 1600m (20 min walk = biking range)
        var template = CreateTemplate(CardType.Parks);
        _gisRepository.GetPoiCountAsync("44021A0", "park", default)
            .Returns(new PoiCount(2, 1600));
        _gisRepository.GetNearestPoiAsync("44021A0", "park", default)
            .Returns(new NearestPoi(123, "Testpark", 1600, IsInside: true));

        // Act
        var result = await _builder.BuildAsync("44021A0", template);

        // Assert
        result.DistanceIcon.Should().Be("fa-solid fa-bicycle");
        result.Detail.Should().Contain("fietsen");
    }

    [Fact]
    public async Task BuildAsync_CarDistance_ShowsCarIcon()
    {
        // Arrange: Park at 3200m (40 min walk = car range)
        var template = CreateTemplate(CardType.Parks);
        _gisRepository.GetPoiCountAsync("44021A0", "park", default)
            .Returns(new PoiCount(1, 3200));
        _gisRepository.GetNearestPoiAsync("44021A0", "park", default)
            .Returns(new NearestPoi(123, "Ver Park", 3200, IsInside: true));

        // Act
        var result = await _builder.BuildAsync("44021A0", template);

        // Assert
        result.DistanceIcon.Should().Be("fa-solid fa-car");
        result.Detail.Should().Contain("rijden");
    }

    #endregion

    #region Transit Cards

    [Fact]
    public async Task BuildAsync_ForTransit_ShowsCountAndQualityDetail()
    {
        // Arrange: 41 transit stops
        var template = CreateTemplate(CardType.Transit);
        _gisRepository.GetTransitCountAsync("44021A0", default)
            .Returns(41);

        // Act
        var result = await _builder.BuildAsync("44021A0", template);

        // Assert
        result.Description.Should().Be("41 haltes in de wijk");
        result.Detail.Should().Be("Uitstekende verbindingen");
        result.Icon.Should().Be("fa-solid fa-bus");
        result.Distance.Should().BeEmpty();
        result.DistanceIcon.Should().BeNull();
    }

    [Theory]
    [InlineData(0, "Geen haltes in de wijk", "Beperkt openbaar vervoer")]
    [InlineData(1, "1 halte in de wijk", "1 halte beschikbaar")]
    [InlineData(5, "5 haltes in de wijk", "Redelijke bereikbaarheid")]
    [InlineData(15, "15 haltes in de wijk", "Goede bereikbaarheid")]
    [InlineData(25, "25 haltes in de wijk", "Uitstekende verbindingen")]
    public async Task BuildAsync_ForTransit_GeneratesAppropriateText(int count, string expectedDescription, string expectedDetail)
    {
        // Arrange
        var template = CreateTemplate(CardType.Transit);
        _gisRepository.GetTransitCountAsync("44021A0", default)
            .Returns(count);

        // Act
        var result = await _builder.BuildAsync("44021A0", template);

        // Assert
        result.Description.Should().Be(expectedDescription);
        result.Detail.Should().Be(expectedDetail);
    }

    #endregion

    #region BuildAllAsync

    [Fact]
    public async Task BuildAllAsync_ReturnsCardsInSortOrder()
    {
        // Arrange
        var templates = new[]
        {
            CreateTemplate(CardType.Transit, sortOrder: 6),
            CreateTemplate(CardType.Parks, sortOrder: 2),
            CreateTemplate(CardType.DogParks, sortOrder: 1)
        };

        _gisRepository.GetPoiCountAsync(Arg.Any<string>(), Arg.Any<string>(), default)
            .Returns(new PoiCount(5, 100));
        _gisRepository.GetNearestPoiAsync(Arg.Any<string>(), Arg.Any<string>(), default)
            .Returns(new NearestPoi(1, "Test", 100, true));
        _gisRepository.GetTransitCountAsync(Arg.Any<string>(), default)
            .Returns(10);

        // Act
        var results = await _builder.BuildAllAsync("44021A0", templates);

        // Assert
        results.Should().HaveCount(3);
        results[0].Title.Should().Be("Hondenparken");
        results[1].Title.Should().Be("Parken");
        results[2].Title.Should().Be("Openbaar vervoer");
    }

    #endregion

    private static ValueCardTemplate CreateTemplate(CardType cardType, int sortOrder = 1)
    {
        var title = cardType switch
        {
            CardType.DogParks => "Hondenparken",
            CardType.Parks => "Parken",
            CardType.Vets => "Dierenartsen",
            CardType.PetStores => "Dierenwinkels",
            CardType.Supermarkets => "Supermarkten",
            CardType.Transit => "Openbaar vervoer",
            _ => "Unknown"
        };

        return new ValueCardTemplate
        {
            CardType = cardType,
            Title = title,
            SortOrder = sortOrder
        };
    }
}
