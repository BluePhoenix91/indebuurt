using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pipeline.Core.Services.Statbel;
using Xunit;

namespace Pipeline.Core.Tests.Services.Statbel;

public class StatbelImportServiceTests
{
    private readonly IStatbelDownloader _downloader;
    private readonly IPopulationDataParser _populationParser;
    private readonly IHousePriceDataParser _housePriceParser;
    private readonly IStatbelStagingRepository _repository;
    private readonly ILogger<StatbelImportService> _logger;
    private readonly StatbelImportService _service;

    public StatbelImportServiceTests()
    {
        _downloader = Substitute.For<IStatbelDownloader>();
        _populationParser = Substitute.For<IPopulationDataParser>();
        _housePriceParser = Substitute.For<IHousePriceDataParser>();
        _repository = Substitute.For<IStatbelStagingRepository>();
        _logger = Substitute.For<ILogger<StatbelImportService>>();

        _service = new StatbelImportService(
            _downloader,
            _populationParser,
            _housePriceParser,
            _repository,
            _logger);
    }

    [Fact]
    public void AvailableDatasets_ContainsExpectedValues()
    {
        _service.AvailableDatasets.Should().Contain("population");
        _service.AvailableDatasets.Should().Contain("house-prices");
        _service.AvailableDatasets.Should().HaveCount(2);
    }

    [Fact]
    public async Task ImportAsync_WithSpecifiedYear_UsesProvidedYear()
    {
        // Arrange
        _downloader.DownloadPopulationAsync(2024, Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns("/tmp/pop.txt");
        _downloader.DownloadHousePricesAsync(Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns("/tmp/prices.xlsx");

        _populationParser.Parse("/tmp/pop.txt")
            .Returns(new List<NeighborhoodPopulation>
            {
                new("44021A0", 1000, 10m, 100m)
            });
        _housePriceParser.Parse("/tmp/prices.xlsx", 2024)
            .Returns((new List<MunicipalityHousePrice>
            {
                new("44021", 350000)
            }, 2024));

        _repository.GetCurrentCountsAsync(2024, Arg.Any<CancellationToken>())
            .Returns((0, 0));
        _repository.MergePopulationAsync(2024, Arg.Any<List<NeighborhoodPopulation>>(), Arg.Any<CancellationToken>())
            .Returns(new DatasetImportResult(100, 95, 5, []));
        _repository.MergeHousePricesAsync(2024, Arg.Any<List<MunicipalityHousePrice>>(), Arg.Any<CancellationToken>())
            .Returns(new DatasetImportResult(50, 45, 5, []));

        // Act
        var result = await _service.ImportAsync(year: 2024);

        // Assert
        result.Year.Should().Be(2024);
        await _downloader.DidNotReceive().DetectLatestPopulationYearAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportAsync_WithoutYear_DetectsLatestYear()
    {
        // Arrange
        _downloader.DetectLatestPopulationYearAsync(Arg.Any<CancellationToken>())
            .Returns(2025);
        _downloader.DownloadPopulationAsync(2025, Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns("/tmp/pop.txt");
        _downloader.DownloadHousePricesAsync(Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns("/tmp/prices.xlsx");

        _populationParser.Parse("/tmp/pop.txt")
            .Returns(new List<NeighborhoodPopulation>());
        _housePriceParser.Parse("/tmp/prices.xlsx", 2025)
            .Returns((new List<MunicipalityHousePrice>(), 2025));

        _repository.GetCurrentCountsAsync(2025, Arg.Any<CancellationToken>())
            .Returns((0, 0));
        _repository.MergePopulationAsync(2025, Arg.Any<List<NeighborhoodPopulation>>(), Arg.Any<CancellationToken>())
            .Returns(new DatasetImportResult(100, 95, 5, []));
        _repository.MergeHousePricesAsync(2025, Arg.Any<List<MunicipalityHousePrice>>(), Arg.Any<CancellationToken>())
            .Returns(new DatasetImportResult(50, 45, 5, []));

        // Act
        var result = await _service.ImportAsync(year: null);

        // Assert
        result.Year.Should().Be(2025);
        await _downloader.Received(1).DetectLatestPopulationYearAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportAsync_WithDatasetPopulation_OnlyImportsPopulation()
    {
        // Arrange
        _downloader.DownloadPopulationAsync(2024, Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns("/tmp/pop.txt");

        _populationParser.Parse("/tmp/pop.txt")
            .Returns(new List<NeighborhoodPopulation>
            {
                new("44021A0", 1000, 10m, 100m)
            });

        _repository.GetCurrentCountsAsync(2024, Arg.Any<CancellationToken>())
            .Returns((0, 0));
        _repository.MergePopulationAsync(2024, Arg.Any<List<NeighborhoodPopulation>>(), Arg.Any<CancellationToken>())
            .Returns(new DatasetImportResult(100, 95, 5, []));

        // Act
        var result = await _service.ImportAsync(year: 2024, dataset: "population");

        // Assert
        result.PopulationResult.Should().NotBeNull();
        result.HousePriceResult.Should().BeNull();

        await _downloader.DidNotReceive().DownloadHousePricesAsync(Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().MergeHousePricesAsync(Arg.Any<int>(), Arg.Any<List<MunicipalityHousePrice>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportAsync_WithDatasetHousePrices_OnlyImportsHousePrices()
    {
        // Arrange
        _downloader.DownloadHousePricesAsync(Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns("/tmp/prices.xlsx");

        _housePriceParser.Parse("/tmp/prices.xlsx", 2024)
            .Returns((new List<MunicipalityHousePrice>
            {
                new("44021", 350000)
            }, 2024));

        _repository.GetCurrentCountsAsync(2024, Arg.Any<CancellationToken>())
            .Returns((0, 0));
        _repository.MergeHousePricesAsync(2024, Arg.Any<List<MunicipalityHousePrice>>(), Arg.Any<CancellationToken>())
            .Returns(new DatasetImportResult(50, 45, 5, []));

        // Act
        var result = await _service.ImportAsync(year: 2024, dataset: "house-prices");

        // Assert
        result.PopulationResult.Should().BeNull();
        result.HousePriceResult.Should().NotBeNull();

        await _downloader.DidNotReceive().DownloadPopulationAsync(Arg.Any<int>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().MergePopulationAsync(Arg.Any<int>(), Arg.Any<List<NeighborhoodPopulation>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportAsync_WithDryRun_DoesNotWriteToDatabase()
    {
        // Arrange
        _downloader.DownloadPopulationAsync(2024, Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns("/tmp/pop.txt");
        _downloader.DownloadHousePricesAsync(Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns("/tmp/prices.xlsx");

        _populationParser.Parse("/tmp/pop.txt")
            .Returns(new List<NeighborhoodPopulation>
            {
                new("44021A0", 1000, 10m, 100m)
            });
        _housePriceParser.Parse("/tmp/prices.xlsx", 2024)
            .Returns((new List<MunicipalityHousePrice>
            {
                new("44021", 350000)
            }, 2024));

        _repository.GetCurrentCountsAsync(2024, Arg.Any<CancellationToken>())
            .Returns((0, 0));

        // Act
        var result = await _service.ImportAsync(year: 2024, dryRun: true);

        // Assert
        await _repository.DidNotReceive().MergePopulationAsync(Arg.Any<int>(), Arg.Any<List<NeighborhoodPopulation>>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().MergeHousePricesAsync(Arg.Any<int>(), Arg.Any<List<MunicipalityHousePrice>>(), Arg.Any<CancellationToken>());

        // Dry run should still return results with row counts but 0 updates
        result.PopulationResult.Should().NotBeNull();
        result.PopulationResult!.NeighborhoodsUpdated.Should().Be(0);
        result.HousePriceResult.Should().NotBeNull();
        result.HousePriceResult!.NeighborhoodsUpdated.Should().Be(0);
    }

    [Fact]
    public async Task ImportAsync_ReportsProgress()
    {
        // Arrange
        var progressMessages = new List<string>();
        var progress = new Progress<string>(msg => progressMessages.Add(msg));

        _downloader.DownloadPopulationAsync(2024, Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns("/tmp/pop.txt");
        _downloader.DownloadHousePricesAsync(Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns("/tmp/prices.xlsx");

        _populationParser.Parse("/tmp/pop.txt")
            .Returns(new List<NeighborhoodPopulation>
            {
                new("44021A0", 1000, 10m, 100m)
            });
        _housePriceParser.Parse("/tmp/prices.xlsx", 2024)
            .Returns((new List<MunicipalityHousePrice>
            {
                new("44021", 350000)
            }, 2024));

        _repository.GetCurrentCountsAsync(2024, Arg.Any<CancellationToken>())
            .Returns((0, 0));
        _repository.MergePopulationAsync(2024, Arg.Any<List<NeighborhoodPopulation>>(), Arg.Any<CancellationToken>())
            .Returns(new DatasetImportResult(100, 95, 5, []));
        _repository.MergeHousePricesAsync(2024, Arg.Any<List<MunicipalityHousePrice>>(), Arg.Any<CancellationToken>())
            .Returns(new DatasetImportResult(50, 45, 5, []));

        // Act
        await _service.ImportAsync(year: 2024, progress: progress);

        // Assert: Should have received progress messages
        // Give progress reports time to process (they run on SynchronizationContext)
        await Task.Delay(50);
        progressMessages.Should().NotBeEmpty();
    }
}
