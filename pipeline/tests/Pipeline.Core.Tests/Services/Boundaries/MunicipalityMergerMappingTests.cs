using FluentAssertions;
using Pipeline.Core.Services.Boundaries;
using Xunit;

namespace Pipeline.Core.Tests.Services.Boundaries;

public class MunicipalityMergerMappingTests
{
    [Theory]
    [InlineData("44040A0", "44088")] // Melle → Merelbeke-Melle
    [InlineData("44043B0", "44088")] // Merelbeke → Merelbeke-Melle
    [InlineData("44034A0", "44087")] // Lochristi → Lochristi (absorbed Wachtebeke)
    [InlineData("44073A0", "44087")] // Wachtebeke → Lochristi
    [InlineData("11007A0", "11002")] // Borsbeek → Antwerpen
    [InlineData("23023A0", "23106")] // Galmaarden → Pajottegem
    [InlineData("23024A0", "23106")] // Gooik → Pajottegem
    [InlineData("23032A0", "23106")] // Herne → Pajottegem
    [InlineData("71022A0", "71072")] // Hasselt → Hasselt (absorbed Kortessem)
    [InlineData("73040A0", "71072")] // Kortessem → Hasselt
    public void GetStatbelMunicipalityNis_MergedMunicipality_ReturnsNewCode(
        string nisCode, string expectedMunicipalityNis)
    {
        MunicipalityMergerMapping.GetStatbelMunicipalityNis(nisCode)
            .Should().Be(expectedMunicipalityNis);
    }

    [Theory]
    [InlineData("44021A0", "44021")] // Gent (unchanged)
    [InlineData("11002A0", "11002")] // Antwerpen (unchanged)
    [InlineData("12025A0", "12025")] // Mechelen (unchanged)
    public void GetStatbelMunicipalityNis_UnchangedMunicipality_ReturnsFirstFiveChars(
        string nisCode, string expectedMunicipalityNis)
    {
        MunicipalityMergerMapping.GetStatbelMunicipalityNis(nisCode)
            .Should().Be(expectedMunicipalityNis);
    }

    [Fact]
    public void GetMergerGroups_ReturnsAllMergerGroups()
    {
        var groups = MunicipalityMergerMapping.GetMergerGroups();

        // 13 distinct new NIS codes
        groups.Should().HaveCount(13);

        // Pajottegem should have 3 old codes
        groups["23106"].Should().HaveCount(3);
        groups["23106"].Should().Contain("23023"); // Galmaarden
        groups["23106"].Should().Contain("23024"); // Gooik
        groups["23106"].Should().Contain("23032"); // Herne

        // Beveren-Kruibeke-Zwijndrecht should have 3 old codes
        groups["46030"].Should().HaveCount(3);
    }
}
