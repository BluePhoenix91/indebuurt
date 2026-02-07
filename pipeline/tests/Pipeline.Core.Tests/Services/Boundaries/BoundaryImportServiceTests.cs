using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pipeline.Core.Services.Boundaries;
using Xunit;

namespace Pipeline.Core.Tests.Services.Boundaries;

public class BoundaryImportServiceTests
{
    private readonly IBoundaryDownloader _downloader;
    private readonly IGeoJsonSectorReader _reader;
    private readonly ISlugGenerator _slugGenerator;
    private readonly IBoundaryStagingRepository _repository;
    private readonly BoundaryImportService _service;

    public BoundaryImportServiceTests()
    {
        _downloader = Substitute.For<IBoundaryDownloader>();
        _reader = Substitute.For<IGeoJsonSectorReader>();
        _slugGenerator = Substitute.For<ISlugGenerator>();
        _repository = Substitute.For<IBoundaryStagingRepository>();
        var logger = Substitute.For<ILogger<BoundaryImportService>>();

        _service = new BoundaryImportService(
            _downloader,
            _reader,
            _slugGenerator,
            _repository,
            logger);
    }

    [Fact]
    public async Task ImportAsync_DryRun_DoesNotWriteToDatabase()
    {
        // Arrange
        _repository.GetCurrentCountsAsync(Arg.Any<CancellationToken>())
            .Returns((0, 0));
        _repository.GetStatisticsCountAsync(Arg.Any<CancellationToken>())
            .Returns(0);

        _downloader.ResolveGeoJsonPathAsync(null, Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns("/tmp/test.geojson");

        var sectors = new List<SectorFeature>
        {
            new("44021A001", "Binnenstad", "Gent", "Provincie Oost-Vlaanderen", null, new byte[] { 1, 2, 3 }),
            new("44021A002", "Centrum", "Gent", "Provincie Oost-Vlaanderen", null, new byte[] { 4, 5, 6 }),
        };
        _reader.ReadAsync(Arg.Any<string>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns((19000, sectors));

        // Act
        var result = await _service.ImportAsync(dryRun: true);

        // Assert
        result.TotalFeaturesInFile.Should().Be(19000);
        result.SectorsAfterFilter.Should().Be(2);
        result.SectorsImported.Should().Be(0);
        result.NeighborhoodsCreated.Should().Be(1); // 2 sectors share prefix 44021A0 → 1 expected neighborhood

        await _repository.DidNotReceive().CreateStagingTableAsync(Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().BulkInsertSectorsAsync(
            Arg.Any<List<SectorFeature>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportAsync_FullImport_CallsAllRepositoryMethods()
    {
        // Arrange
        SetupSuccessfulImport();

        // Act
        await _service.ImportAsync();

        // Assert — verify all steps were called in order
        await _repository.Received(1).GetCurrentCountsAsync(Arg.Any<CancellationToken>());
        await _repository.Received(1).CreateStagingTableAsync(Arg.Any<CancellationToken>());
        await _repository.Received(1).BulkInsertSectorsAsync(
            Arg.Any<List<SectorFeature>>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).GetNeighborhoodMetadataAsync(Arg.Any<CancellationToken>());
        _slugGenerator.Received(1).GenerateNeighborhoodSlugs(Arg.Any<IEnumerable<NeighborhoodMetadata>>());
        await _repository.Received(1).CreateAndPopulateNeighborhoodsAsync(
            Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).ApplyMunicipalityMappingAsync(Arg.Any<CancellationToken>());
        await _repository.Received(1).GetSectorMetadataAsync(Arg.Any<CancellationToken>());
        _slugGenerator.Received(1).GenerateSectorSlugs(
            Arg.Any<IEnumerable<(string, string, string)>>());
        await _repository.Received(1).CreateAndPopulateStatisticalSectorsAsync(
            Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).CreateNeighborhoodStatisticsTableAsync(Arg.Any<CancellationToken>());
        await _repository.Received(1).DropStagingTableAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportAsync_WithExistingStatistics_AddsWarning()
    {
        // Arrange
        SetupSuccessfulImport();
        _repository.GetCurrentCountsAsync(Arg.Any<CancellationToken>())
            .Returns((2800, 9900));
        _repository.GetStatisticsCountAsync(Arg.Any<CancellationToken>())
            .Returns(5600);

        // Act
        var result = await _service.ImportAsync();

        // Assert
        result.HasWarnings.Should().BeTrue();
        result.Warnings.Should().ContainSingle(w => w.Contains("neighborhood_statistics"));
        result.PreviousNeighborhoodCount.Should().Be(2800);
        result.PreviousSectorCount.Should().Be(9900);
    }

    [Fact]
    public async Task ImportAsync_WithExplicitFilePath_PassesPathToDownloader()
    {
        // Arrange
        SetupSuccessfulImport();

        // Act
        await _service.ImportAsync(filePath: "/custom/path.geojson");

        // Assert
        await _downloader.Received(1).ResolveGeoJsonPathAsync(
            "/custom/path.geojson",
            Arg.Any<IProgress<string>>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ImportAsync_NoSectorsAfterFilter_ThrowsInvalidOperation()
    {
        // Arrange
        _repository.GetCurrentCountsAsync(Arg.Any<CancellationToken>())
            .Returns((0, 0));
        _repository.GetStatisticsCountAsync(Arg.Any<CancellationToken>())
            .Returns(0);

        _downloader.ResolveGeoJsonPathAsync(Arg.Any<string?>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns("/tmp/test.geojson");

        _reader.ReadAsync(Arg.Any<string>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns((19000, new List<SectorFeature>()));

        // Act & Assert
        await _service.Invoking(s => s.ImportAsync())
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No sectors found*");
    }

    [Fact]
    public async Task ImportAsync_ReportsProgress()
    {
        // Arrange
        SetupSuccessfulImport();
        var progressMessages = new List<string>();
        var progress = new Progress<string>(msg => progressMessages.Add(msg));

        // Act
        await _service.ImportAsync(progress: progress);

        // Assert — give progress callbacks time to propagate
        await Task.Delay(50);
        progressMessages.Should().NotBeEmpty();
        progressMessages.Should().Contain(m => m.Contains("staging"));
    }

    [Fact]
    public async Task ImportAsync_ReturnsCorrectCounts()
    {
        // Arrange
        SetupSuccessfulImport();
        _repository.CreateAndPopulateNeighborhoodsAsync(
                Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(2800);
        _repository.CreateAndPopulateStatisticalSectorsAsync(
                Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(9900);

        // Act
        var result = await _service.ImportAsync();

        // Assert
        result.TotalFeaturesInFile.Should().Be(19000);
        result.SectorsAfterFilter.Should().Be(2);
        result.NeighborhoodsCreated.Should().Be(2800);
        result.SectorsImported.Should().Be(9900);
    }

    private void SetupSuccessfulImport()
    {
        _repository.GetCurrentCountsAsync(Arg.Any<CancellationToken>())
            .Returns((0, 0));
        _repository.GetStatisticsCountAsync(Arg.Any<CancellationToken>())
            .Returns(0);

        _downloader.ResolveGeoJsonPathAsync(Arg.Any<string?>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns("/tmp/test.geojson");

        var sectors = new List<SectorFeature>
        {
            new("44021A001", "Binnenstad", "Gent", "Provincie Oost-Vlaanderen", null, new byte[] { 1, 2, 3 }),
            new("44021A002", "Centrum", "Gent", "Provincie Oost-Vlaanderen", null, new byte[] { 4, 5, 6 }),
        };
        _reader.ReadAsync(Arg.Any<string>(), Arg.Any<IProgress<string>>(), Arg.Any<CancellationToken>())
            .Returns((19000, sectors));

        _repository.GetNeighborhoodMetadataAsync(Arg.Any<CancellationToken>())
            .Returns(new List<NeighborhoodMetadata>
            {
                new("44021A0", "Binnenstad", "Gent", "Provincie Oost-Vlaanderen", 2)
            });

        _slugGenerator.GenerateNeighborhoodSlugs(Arg.Any<IEnumerable<NeighborhoodMetadata>>())
            .Returns(new Dictionary<string, string> { ["44021A0"] = "gent-binnenstad" });

        _repository.CreateAndPopulateNeighborhoodsAsync(
                Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(1);

        _repository.GetSectorMetadataAsync(Arg.Any<CancellationToken>())
            .Returns(new List<(string, string, string)>
            {
                ("44021A001", "Gent", "Binnenstad"),
                ("44021A002", "Gent", "Centrum"),
            });

        _slugGenerator.GenerateSectorSlugs(Arg.Any<IEnumerable<(string, string, string)>>())
            .Returns(new Dictionary<string, string>
            {
                ["44021A001"] = "gent-binnenstad",
                ["44021A002"] = "gent-centrum",
            });

        _repository.CreateAndPopulateStatisticalSectorsAsync(
                Arg.Any<Dictionary<string, string>>(), Arg.Any<CancellationToken>())
            .Returns(2);
    }
}
