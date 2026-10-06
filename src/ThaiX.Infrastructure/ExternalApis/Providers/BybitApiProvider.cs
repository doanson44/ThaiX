using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// Bybit external API provider. Reads endpoint policies from configuration (ExternalApis:Providers:Bybit).
/// </summary>
public sealed class BybitApiProvider
{
    private const string ProviderName = "Bybit";
    private const string LinearTickersEndpoint = "LinearTickers";
    private const string SpotTickersEndpoint = "SpotTickers";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public BybitApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    /// <summary>
    /// Gets the LinearTickers endpoint config. Throws if not configured.
    /// </summary>
    private EndpointOptions GetLinearTickersEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(LinearTickersEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{LinearTickersEndpoint}' is not configured.");
    }

    /// <summary>
    /// Gets the SpotTickers endpoint config. Throws if not configured.
    /// </summary>
    private EndpointOptions GetSpotTickersEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(SpotTickersEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{SpotTickersEndpoint}' is not configured.");
    }

    /// <summary>
    /// Calls Bybit linear/perpetual tickers endpoint. Policy (cache, proxy, timeout) from config.
    /// </summary>
    public Task<T?> GetLinearTickersAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetLinearTickersEndpoint();
        var request = new ApiRequest
        {
            Url = endpoint.Url,
            Method = HttpMethod.Get,
            UseCache = endpoint.UseCache,
            CacheDuration = endpoint.CacheDurationSeconds > 0
                ? TimeSpan.FromSeconds(endpoint.CacheDurationSeconds)
                : null,
            UseProxy = endpoint.UseProxy,
            TimeoutSeconds = endpoint.TimeoutSeconds > 0 ? endpoint.TimeoutSeconds : 30
        };
        return _apiService.ExecuteAsync<T>(request, cancellationToken);
    }

    /// <summary>
    /// Calls Bybit spot tickers endpoint. Policy (cache, proxy, timeout) from config.
    /// </summary>
    public Task<T?> GetSpotTickersAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetSpotTickersEndpoint();
        var request = new ApiRequest
        {
            Url = endpoint.Url,
            Method = HttpMethod.Get,
            UseCache = endpoint.UseCache,
            CacheDuration = endpoint.CacheDurationSeconds > 0
                ? TimeSpan.FromSeconds(endpoint.CacheDurationSeconds)
                : null,
            UseProxy = endpoint.UseProxy,
            TimeoutSeconds = endpoint.TimeoutSeconds > 0 ? endpoint.TimeoutSeconds : 30
        };
        return _apiService.ExecuteAsync<T>(request, cancellationToken);
    }
}
