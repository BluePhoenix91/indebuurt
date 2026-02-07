using FluentAssertions;
using Pipeline.Core.Services.Boundaries;
using Xunit;

namespace Pipeline.Core.Tests.Services.Boundaries;

public class SlugGeneratorTests
{
    private readonly SlugGenerator _slugGenerator = new();

    #region Slugify Tests

    [Theory]
    [InlineData("Gent", "gent")]
    [InlineData("Sint-Niklaas", "sint-niklaas")]
    [InlineData("Binnenstad", "binnenstad")]
    [InlineData("AARTSELAAR", "aartselaar")]
    public void Slugify_SimpleName_ReturnsLowercaseSlug(string input, string expected)
    {
        _slugGenerator.Slugify(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("Bruxelles-Ville", "bruxelles-ville")]
    [InlineData("Saint-Gilles", "saint-gilles")]
    [InlineData("Étterbeek", "etterbeek")]
    [InlineData("Hérent", "herent")]
    [InlineData("Knokke-Heist", "knokke-heist")]
    public void Slugify_AccentedChars_TransliteratesCorrectly(string input, string expected)
    {
        _slugGenerator.Slugify(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("'s Hertogendijk", "s-hertogendijk")]
    [InlineData("A. Buylstraat", "a-buylstraat")]
    [InlineData("Centrum (Noord)", "centrum-noord")]
    public void Slugify_SpecialChars_RemovesCorrectly(string input, string expected)
    {
        _slugGenerator.Slugify(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("  Gent  ", "gent")]
    [InlineData("Multiple   Spaces", "multiple-spaces")]
    [InlineData("Trailing-  -Hyphens-", "trailing-hyphens")]
    public void Slugify_WhitespaceAndHyphens_CollapsesCorrectly(string input, string expected)
    {
        _slugGenerator.Slugify(input).Should().Be(expected);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData(null, "")]
    [InlineData("   ", "")]
    public void Slugify_EmptyOrWhitespace_ReturnsEmpty(string? input, string expected)
    {
        _slugGenerator.Slugify(input!).Should().Be(expected);
    }

    #endregion

    #region GenerateNeighborhoodSlugs Tests

    [Fact]
    public void GenerateNeighborhoodSlugs_UniqueNames_ReturnsBaseSlug()
    {
        var neighborhoods = new[]
        {
            new NeighborhoodMetadata("44021A0", "Binnenstad", "Gent", "Provincie Oost-Vlaanderen", 3),
            new NeighborhoodMetadata("11002A0", "Centrum", "Antwerpen", "Provincie Antwerpen", 5),
        };

        var result = _slugGenerator.GenerateNeighborhoodSlugs(neighborhoods);

        result["44021A0"].Should().Be("gent-binnenstad");
        result["11002A0"].Should().Be("antwerpen-centrum");
    }

    [Fact]
    public void GenerateNeighborhoodSlugs_DuplicateNames_AppendNisCode()
    {
        // Two neighborhoods in the same city with the same name (different NIS codes)
        var neighborhoods = new[]
        {
            new NeighborhoodMetadata("44021A0", "Centrum", "Gent", "Provincie Oost-Vlaanderen", 2),
            new NeighborhoodMetadata("44021B0", "Centrum", "Gent", "Provincie Oost-Vlaanderen", 3),
        };

        var result = _slugGenerator.GenerateNeighborhoodSlugs(neighborhoods);

        result["44021A0"].Should().Be("gent-centrum-44021a0");
        result["44021B0"].Should().Be("gent-centrum-44021b0");
    }

    [Fact]
    public void GenerateNeighborhoodSlugs_EmptyInput_ReturnsEmptyDictionary()
    {
        var result = _slugGenerator.GenerateNeighborhoodSlugs(Array.Empty<NeighborhoodMetadata>());

        result.Should().BeEmpty();
    }

    #endregion

    #region GenerateSectorSlugs Tests

    [Fact]
    public void GenerateSectorSlugs_UniqueSectors_ReturnsBaseSlug()
    {
        var sectors = new[]
        {
            ("44021A001", "Gent", "Binnenstad"),
            ("11002A001", "Antwerpen", "Centrum"),
        };

        var result = _slugGenerator.GenerateSectorSlugs(sectors);

        result["44021A001"].Should().Be("gent-binnenstad");
        result["11002A001"].Should().Be("antwerpen-centrum");
    }

    [Fact]
    public void GenerateSectorSlugs_DuplicateNames_AppendSectorCode()
    {
        var sectors = new[]
        {
            ("44021A001", "Gent", "Binnenstad"),
            ("44021A002", "Gent", "Binnenstad"),
        };

        var result = _slugGenerator.GenerateSectorSlugs(sectors);

        result["44021A001"].Should().Be("gent-binnenstad-44021a001");
        result["44021A002"].Should().Be("gent-binnenstad-44021a002");
    }

    #endregion
}
