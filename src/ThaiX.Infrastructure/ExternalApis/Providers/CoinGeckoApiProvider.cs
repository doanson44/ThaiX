using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// CoinGecko external API provider. Reads endpoint policies from configuration (ExternalApis:Providers:CoinGecko).
/// </summary>
public sealed class CoinGeckoApiProvider
{
    private const string ProviderName = "CoinGecko";
    private const string CoinsListEndpoint = "CoinsList";
    private const string CoinMarketByIdEndpoint = "CoinMarketById";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public CoinGeckoApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    /// <summary>
    /// Gets the CoinsList endpoint config. Throws if not configured.
    /// </summary>
    private EndpointOptions GetCoinsListEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(CoinsListEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{CoinsListEndpoint}' is not configured.");
    }

    /// <summary>
    /// Gets the CoinMarketById endpoint config. Throws if not configured.
    /// </summary>
    private EndpointOptions GetCoinMarketByIdEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(CoinMarketByIdEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{CoinMarketByIdEndpoint}' is not configured.");
    }

    /// <summary>
    /// Calls CoinGecko coins list endpoint. Policy (cache, proxy, timeout) from config.
    /// </summary>
    public Task<T?> GetCoinsListAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetCoinsListEndpoint();
        var request = new ApiRequest
        {
            Url = endpoint.Url,
            Method = HttpMethod.Get,
            Headers = new Dictionary<string, string>
            {
                { "User-Agent", "ThaiX/1.0" },
                { "Accept", "application/json" }
            },
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
    /// Calls CoinGecko markets endpoint by coin id (e.g. bitcoin, ethereum).
    /// Policy (cache, proxy, timeout) from config.
    /// </summary>
    public Task<T?> GetCoinMarketByIdAsync<T>(string coinId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(coinId))
        {
            throw new ArgumentException("Coin id cannot be null or empty.", nameof(coinId));
        }

        var endpoint = GetCoinMarketByIdEndpoint();
        var normalizedCoinId = coinId.Trim().ToLowerInvariant();
        var url = endpoint.Url.Replace("{coinId}", Uri.EscapeDataString(normalizedCoinId));

        var request = new ApiRequest
        {
            Url = url,
            Method = HttpMethod.Get,
            Headers = new Dictionary<string, string>
            {
                { "User-Agent", "ThaiX/1.0" },
                { "Accept", "application/json" }
            },
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