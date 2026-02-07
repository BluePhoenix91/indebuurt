using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Pipeline.Core.Services.Statbel;
using Xunit;

namespace Pipeline.Core.Tests.Services.Statbel;

public class StatbelDownloaderTests
{
    private readonly ILogger<StatbelDownloader> _logger;
    private readonly IOptions<StatbelOptions> _options;

    public StatbelDownloaderTests()
    {
        _logger = Substitute.For<ILogger<StatbelDownloader>>();
        _options = Options.Create(new StatbelOptions
        {
            PopulationUrlTemplate = "https://statbel.fgov.be/files/OPENDATA_SECTOREN_{year}.zip",
            HousePricesUrl = "https://statbel.fgov.be/files/vastgoed.xlsx",
            TimeoutSeconds = 30
        });
    }

    [Fact]
    public async Task DetectLatestPopulationYearAsync_FindsFirstSuccessfulYear()
    {
        // Arrange: 2026 returns 404, 2025 returns 200
        var handler = new MockHttpMessageHandler(request =>
        {
            if (request.RequestUri!.ToString().Contains("2026"))
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            if (request.RequestUri.ToString().Contains("2025"))
                return new HttpResponseMessage(HttpStatusCode.OK);
            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var httpClient = new HttpClient(handler);
        var downloader = new StatbelDownloader(httpClient, _options, _logger);

        // Act
        var year = await downloader.DetectLatestPopulationYearAsync();

        // Assert
        year.Should().Be(2025);
    }

    [Fact]
    public async Task DetectLatestPopulationYearAsync_ThrowsWhenNoYearFound()
    {
        // Arrange: All years return 404
        var handler = new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.NotFound));
        var httpClient = new HttpClient(handler);
        var downloader = new StatbelDownloader(httpClient, _options, _logger);

        // Act & Assert
        await downloader.Invoking(d => d.DetectLatestPopulationYearAsync())
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Could not detect latest population year*");
    }

    [Fact]
    public void Constructor_CreatesCacheDirectory()
    {
        // Arrange & Act
        var handler = new MockHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK));
        var httpClient = new HttpClient(handler);
        var downloader = new StatbelDownloader(httpClient, _options, _logger);

        // Assert: Cache directory should exist
        var cacheDir = Path.Combine(Path.GetTempPath(), "statbel");
        Directory.Exists(cacheDir).Should().BeTrue();
    }

    [Fact]
    public async Task GetPopulationUrl_SubstitutesYear()
    {
        // This tests the URL template substitution
        // We verify by checking the request URL in DetectLatestPopulationYearAsync

        // Arrange
        string? capturedUrl = null;
        var handler = new MockHttpMessageHandler(request =>
        {
            capturedUrl = request.RequestUri!.ToString();
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        var httpClient = new HttpClient(handler);
        var customOptions = Options.Create(new StatbelOptions
        {
            PopulationUrlTemplate = "https://example.com/data_{year}.zip"
        });
        var downloader = new StatbelDownloader(httpClient, customOptions, _logger);

        // Act
        await downloader.DetectLatestPopulationYearAsync();

        // Assert: URL should contain a year (current or previous)
        var currentYear = DateTime.Now.Year.ToString();
        var prevYear = (DateTime.Now.Year - 1).ToString();
        var containsYear = capturedUrl!.Contains(currentYear) || capturedUrl.Contains(prevYear);
        containsYear.Should().BeTrue($"URL should contain {currentYear} or {prevYear}");
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}
