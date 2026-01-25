using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Pipeline.Core.Services;
using Xunit;

namespace Pipeline.Core.Tests.Services;

public class OverpassClientTests
{
    private readonly ILogger<OverpassClient> _logger;
    private readonly IOptions<OverpassOptions> _options;

    public OverpassClientTests()
    {
        _logger = Substitute.For<ILogger<OverpassClient>>();
        _options = Options.Create(new OverpassOptions
        {
            BaseUrl = "https://overpass-api.de/api/interpreter",
            Bbox = "50.68,2.54,51.51,5.92",
            TimeoutSeconds = 30,
            DelayBetweenRequestsMs = 0 // No delay for tests
        });
    }

    [Fact]
    public async Task FetchDomainAsync_ParsesNodeElements()
    {
        // Arrange: Overpass response with node elements
        var json = """
            {
                "elements": [
                    {
                        "type": "node",
                        "id": 123456,
                        "lat": 51.0543,
                        "lon": 3.7174,
                        "tags": {
                            "name": "Dierenkliniek Test",
                            "amenity": "veterinary",
                            "phone": "+32 9 123 45 67"
                        }
                    },
                    {
                        "type": "node",
                        "id": 789012,
                        "lat": 51.0500,
                        "lon": 3.7200,
                        "tags": {
                            "name": "Pet Shop Test",
                            "shop": "pet"
                        }
                    }
                ]
            }
            """;

        var httpClient = CreateMockHttpClient(json);
        var client = new OverpassClient(httpClient, _options, _logger);

        // Act
        var elements = await client.FetchDomainAsync("pets");

        // Assert
        elements.Should().HaveCount(2);

        elements[0].Type.Should().Be("node");
        elements[0].Id.Should().Be(123456);
        elements[0].Lat.Should().Be(51.0543);
        elements[0].Lon.Should().Be(3.7174);
        elements[0].Tags.Should().ContainKey("name");
        elements[0].Tags!["name"].Should().Be("Dierenkliniek Test");

        elements[1].Id.Should().Be(789012);
        elements[1].Tags!["shop"].Should().Be("pet");
    }

    [Fact]
    public async Task FetchDomainAsync_ParsesWayElementsWithCenter()
    {
        // Arrange: Overpass response with way element (has center from "out center")
        var json = """
            {
                "elements": [
                    {
                        "type": "way",
                        "id": 456789,
                        "center": {
                            "lat": 50.8503,
                            "lon": 4.3517
                        },
                        "tags": {
                            "name": "Carrefour",
                            "shop": "supermarket"
                        }
                    }
                ]
            }
            """;

        var httpClient = CreateMockHttpClient(json);
        var client = new OverpassClient(httpClient, _options, _logger);

        // Act
        var elements = await client.FetchDomainAsync("shopping");

        // Assert
        elements.Should().HaveCount(1);
        elements[0].Type.Should().Be("way");
        elements[0].Center.Should().NotBeNull();
        elements[0].Center!.Lat.Should().Be(50.8503);
        elements[0].Center!.Lon.Should().Be(4.3517);
    }

    [Fact]
    public async Task FetchDomainAsync_ParsesRelationElements()
    {
        // Arrange: Overpass response with relation element
        var json = """
            {
                "elements": [
                    {
                        "type": "relation",
                        "id": 987654,
                        "center": {
                            "lat": 51.2194,
                            "lon": 4.4025
                        },
                        "tags": {
                            "name": "Stadspark",
                            "leisure": "park"
                        }
                    }
                ]
            }
            """;

        var httpClient = CreateMockHttpClient(json);
        var client = new OverpassClient(httpClient, _options, _logger);

        // Act
        var elements = await client.FetchDomainAsync("green");

        // Assert
        elements.Should().HaveCount(1);
        elements[0].Type.Should().Be("relation");
        elements[0].Tags!["leisure"].Should().Be("park");
    }

    [Fact]
    public async Task FetchDomainAsync_HandlesElementsWithoutTags()
    {
        // Arrange: Element without tags (happens with some OSM data)
        var json = """
            {
                "elements": [
                    {
                        "type": "node",
                        "id": 111,
                        "lat": 51.0,
                        "lon": 3.0
                    }
                ]
            }
            """;

        var httpClient = CreateMockHttpClient(json);
        var client = new OverpassClient(httpClient, _options, _logger);

        // Act
        var elements = await client.FetchDomainAsync("pets");

        // Assert
        elements.Should().HaveCount(1);
        elements[0].Tags.Should().BeNull();
    }

    [Fact]
    public async Task FetchDomainAsync_HandlesEmptyResponse()
    {
        // Arrange: Empty response
        var json = """{"elements": []}""";

        var httpClient = CreateMockHttpClient(json);
        var client = new OverpassClient(httpClient, _options, _logger);

        // Act
        var elements = await client.FetchDomainAsync("pets");

        // Assert
        elements.Should().BeEmpty();
    }

    [Fact]
    public async Task FetchDomainAsync_ThrowsOnUnknownDomain()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("{}");
        var client = new OverpassClient(httpClient, _options, _logger);

        // Act & Assert
        await client.Invoking(c => c.FetchDomainAsync("unknown"))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Unknown domain*");
    }

    [Fact]
    public void AvailableDomains_ReturnsAllDomains()
    {
        // Arrange
        var httpClient = CreateMockHttpClient("{}");
        var client = new OverpassClient(httpClient, _options, _logger);

        // Act
        var domains = client.AvailableDomains;

        // Assert
        domains.Should().Contain("pets");
        domains.Should().Contain("shopping");
        domains.Should().Contain("healthcare");
        domains.Should().Contain("education");
        domains.Should().Contain("transport");
        domains.Should().Contain("green");
        domains.Should().HaveCount(6);
    }

    [Fact]
    public async Task FetchDomainsAsync_FetchesAllDomainsWhenNull()
    {
        // Arrange: Simple response for all domains
        var json = """{"elements": [{"type": "node", "id": 1, "lat": 51.0, "lon": 3.0}]}""";
        var httpClient = CreateMockHttpClient(json);
        var client = new OverpassClient(httpClient, _options, _logger);

        // Act
        var results = await client.FetchDomainsAsync(domains: null);

        // Assert: Should fetch all 6 domains
        results.Should().HaveCount(6);
        results.Should().ContainKey("pets");
        results.Should().ContainKey("shopping");
        results.Should().ContainKey("healthcare");
        results.Should().ContainKey("education");
        results.Should().ContainKey("transport");
        results.Should().ContainKey("green");
    }

    [Fact]
    public async Task FetchDomainsAsync_FetchesOnlySpecifiedDomains()
    {
        // Arrange
        var json = """{"elements": [{"type": "node", "id": 1, "lat": 51.0, "lon": 3.0}]}""";
        var httpClient = CreateMockHttpClient(json);
        var client = new OverpassClient(httpClient, _options, _logger);

        // Act
        var results = await client.FetchDomainsAsync(domains: ["pets", "green"]);

        // Assert
        results.Should().HaveCount(2);
        results.Should().ContainKey("pets");
        results.Should().ContainKey("green");
        results.Should().NotContainKey("shopping");
    }

    private static HttpClient CreateMockHttpClient(string responseJson)
    {
        var handler = new MockHttpMessageHandler(responseJson);
        return new HttpClient(handler);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _responseJson;

        public MockHttpMessageHandler(string responseJson)
        {
            _responseJson = responseJson;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_responseJson, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(response);
        }
    }
}
