using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// Binance external API provider. Reads endpoint policies from configuration (ExternalApis:Providers:Binance).
/// </summary>
public sealed class BinanceApiProvider
{
    private const string ProviderName = "Binance";
    private const string FundingRatesEndpoint = "FundingRates";
    private const string FuturesTicker24HrEndpoint = "FuturesTicker24Hr";
    private const string FuturesTicker24HrBySymbolEndpoint = "FuturesTicker24HrBySymbol";
    private const string SpotDepthEndpoint = "SpotDepth";
    private const string FuturesDepthEndpoint = "FuturesDepth";
    private const string SpotTicker24HrEndpoint = "SpotTicker24Hr";
    private const string SpotTicker24HrBySymbolEndpoint = "SpotTicker24HrBySymbol";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public BinanceApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    private EndpointOptions GetFundingRatesEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(FundingRatesEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{FundingRatesEndpoint}' is not configured.");
    }

    private EndpointOptions GetFuturesTicker24HrEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(FuturesTicker24HrEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{FuturesTicker24HrEndpoint}' is not configured.");
    }

    private EndpointOptions GetFuturesTicker24HrBySymbolEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(FuturesTicker24HrBySymbolEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{FuturesTicker24HrBySymbolEndpoint}' is not configured.");
    }

    private EndpointOptions GetSpotDepthEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(SpotDepthEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{SpotDepthEndpoint}' is not configured.");
    }

    private EndpointOptions GetFuturesDepthEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(FuturesDepthEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{FuturesDepthEndpoint}' is not configured.");
    }

    private EndpointOptions GetSpotTicker24HrEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(SpotTicker24HrEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{SpotTicker24HrEndpoint}' is not configured.");
    }

    private EndpointOptions GetSpotTicker24HrBySymbolEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(SpotTicker24HrBySymbolEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{SpotTicker24HrBySymbolEndpoint}' is not configured.");
    }

    /// <summary>
    /// Calls Binance funding rate history endpoint. Policy (cache, proxy, timeout) from config.
    /// </summary>
    public Task<T?> GetFundingRatesAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetFundingRatesEndpoint();
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
    /// Calls Binance futures 24hr ticker endpoint for all symbols.
    /// </summary>
    public Task<T?> GetFuturesTicker24HrAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetFuturesTicker24HrEndpoint();
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
    /// Calls Binance futures 24hr ticker endpoint by symbol.
    /// </summary>
    public Task<T?> GetFuturesTicker24HrBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));
        }

        var endpoint = GetFuturesTicker24HrBySymbolEndpoint();
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var url = endpoint.Url.Replace("{symbol}", Uri.EscapeDataString(normalizedSymbol));

        var request = new ApiRequest
        {
            Url = url,
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
    /// Calls Binance spot depth endpoint by symbol (limit is fixed in configuration).
    /// </summary>
    public Task<T?> GetSpotDepthAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));
        }

        var endpoint = GetSpotDepthEndpoint();
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var url = endpoint.Url.Replace("{symbol}", Uri.EscapeDataString(normalizedSymbol));

        var request = new ApiRequest
        {
            Url = url,
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
    /// Calls Binance futures depth endpoint by symbol (limit is fixed in configuration).
    /// </summary>
    public Task<T?> GetFuturesDepthAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));
        }

        var endpoint = GetFuturesDepthEndpoint();
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var url = endpoint.Url.Replace("{symbol}", Uri.EscapeDataString(normalizedSymbol));

        var request = new ApiRequest
        {
            Url = url,
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
    /// Calls Binance spot 24hr ticker endpoint for all symbols.
    /// </summary>
    public Task<T?> GetSpotTicker24HrAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetSpotTicker24HrEndpoint();
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
    /// Calls Binance spot 24hr ticker endpoint by symbol.
    /// </summary>
    public Task<T?> GetSpotTicker24HrBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));
        }

        var endpoint = GetSpotTicker24HrBySymbolEndpoint();
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var url = endpoint.Url.Replace("{symbol}", Uri.EscapeDataString(normalizedSymbol));

        var request = new ApiRequest
        {
            Url = url,
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
