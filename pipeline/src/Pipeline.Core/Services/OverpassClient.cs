using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pipeline.Core.Dtos.Overpass;

namespace Pipeline.Core.Services;

/// <summary>
/// HTTP client for querying the Overpass API with rate limiting.
/// </summary>
public interface IOverpassClient
{
    /// <summary>
    /// Gets the list of available domain names.
    /// </summary>
    IReadOnlyList<string> AvailableDomains { get; }

    /// <summary>
    /// Fetches POI domains from Overpass API.
    /// </summary>
    /// <param name="domains">Domains to fetch, or null for all domains.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>All elements grouped by domain.</returns>
    Task<Dictionary<string, OverpassElement[]>> FetchDomainsAsync(
        IEnumerable<string>? domains = null,
        IProgress<(string domain, int count)>? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches a single domain from Overpass API.
    /// </summary>
    Task<OverpassElement[]> FetchDomainAsync(
        string domain,
        CancellationToken cancellationToken = default);
}

public class OverpassClient(
    HttpClient httpClient,
    IOptions<OverpassOptions> optionsAccessor,
    ILogger<OverpassClient> logger) : IOverpassClient
{
    private readonly OverpassOptions _options = optionsAccessor.Value;
    private DateTime _lastRequestTime = DateTime.MinValue;

    // Domain-specific Overpass queries
    private static readonly Dictionary<string, string[]> DomainFilters = new()
    {
        ["pets"] = [@"nwr[""amenity""=""veterinary""]", @"nwr[""shop""=""pet""]", @"nwr[""leisure""=""dog_park""]"],
        ["shopping"] = [@"nwr[""shop""=""supermarket""]"],
        ["healthcare"] = [@"nwr[""amenity""=""pharmacy""]"],
        ["education"] = [@"nwr[""amenity""=""school""]"],
        ["transport"] = [@"nwr[""highway""=""bus_stop""]", @"nwr[""public_transport""=""platform""]", @"nwr[""railway""=""station""]", @"nwr[""railway""=""halt""]"],
        ["green"] = [@"nwr[""leisure""=""park""]"]
    };

    public IReadOnlyList<string> AvailableDomains => DomainFilters.Keys.ToList();

    public async Task<Dictionary<string, OverpassElement[]>> FetchDomainsAsync(
        IEnumerable<string>? domains = null,
        IProgress<(string domain, int count)>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, OverpassElement[]>();
        var domainsToFetch = domains?.ToList() ?? DomainFilters.Keys.ToList();

        foreach (var domain in domainsToFetch)
        {
            var elements = await FetchDomainAsync(domain, cancellationToken);
            results[domain] = elements;
            progress?.Report((domain, elements.Length));
        }

        return results;
    }

    public async Task<OverpassElement[]> FetchDomainAsync(
        string domain,
        CancellationToken cancellationToken = default)
    {
        if (!DomainFilters.TryGetValue(domain, out var filters))
        {
            throw new ArgumentException($"Unknown domain: {domain}", nameof(domain));
        }

        await EnforceRateLimitAsync(cancellationToken);

        var query = BuildQuery(filters);
        logger.LogDebug("Fetching {Domain} from Overpass API...", domain);

        var response = await ExecuteWithRetryAsync(query, cancellationToken);
        return response.Elements;
    }

    private string BuildQuery(string[] filters)
    {
        var filterBlock = string.Join(";", filters);
        return $"[out:json][bbox:{_options.Bbox}];({filterBlock};);out center tags;";
    }

    private async Task EnforceRateLimitAsync(CancellationToken cancellationToken)
    {
        var elapsed = DateTime.UtcNow - _lastRequestTime;
        var delay = TimeSpan.FromMilliseconds(_options.DelayBetweenRequestsMs) - elapsed;

        if (delay > TimeSpan.Zero)
        {
            logger.LogDebug("Rate limiting: waiting {Delay}ms before next request", delay.TotalMilliseconds);
            await Task.Delay(delay, cancellationToken);
        }

        _lastRequestTime = DateTime.UtcNow;
    }

    private async Task<OverpassResponse> ExecuteWithRetryAsync(
        string query,
        CancellationToken cancellationToken,
        int maxAttempts = 3)
    {
        var attempt = 0;
        var baseDelay = TimeSpan.FromSeconds(5);

        while (true)
        {
            attempt++;

            try
            {
                var content = new FormUrlEncodedContent([new("data", query)]);
                var response = await httpClient.PostAsync(_options.BaseUrl, content, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                    response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    if (attempt >= maxAttempts)
                    {
                        throw new HttpRequestException($"Overpass API returned {response.StatusCode} after {maxAttempts} attempts");
                    }

                    var retryDelay = baseDelay * Math.Pow(2, attempt - 1);
                    logger.LogWarning(
                        "Overpass API returned {StatusCode}, retrying in {Delay}s (attempt {Attempt}/{Max})",
                        response.StatusCode, retryDelay.TotalSeconds, attempt, maxAttempts);

                    await Task.Delay(retryDelay, cancellationToken);
                    continue;
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<OverpassResponse>(json);

                return result ?? throw new JsonException("Failed to deserialize Overpass response");
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // HTTP timeout
                if (attempt >= maxAttempts)
                {
                    throw new TimeoutException($"Overpass API timed out after {maxAttempts} attempts");
                }

                var retryDelay = baseDelay * Math.Pow(2, attempt - 1);
                logger.LogWarning(
                    "Overpass API timed out, retrying in {Delay}s (attempt {Attempt}/{Max})",
                    retryDelay.TotalSeconds, attempt, maxAttempts);

                await Task.Delay(retryDelay, cancellationToken);
            }
        }
    }
}
