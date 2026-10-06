using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// VnDirect external API provider. Reads endpoint policies from configuration (ExternalApis:Providers:VnDirect).
/// </summary>
public sealed class VnDirectApiProvider
{
    private const string ProviderName = "VnDirect";
    private const string TopStocksEndpoint = "TopStocks";
    private const string RatiosLatestEndpoint = "RatiosLatest";
    private const string RatiosLatestByItemCodeEndpoint = "RatiosLatestByItemCode";
    private const string EventsEndpoint = "Events";
    private const string StockPricesEndpoint = "StockPrices";
    private const string StockPricesAllEndpoint = "StockPricesAll";
    private const string RecommendationsEndpoint = "Recommendations";
    private const string TechnicalSignalsEndpoint = "TechnicalSignals";
    private const string ChangePricesEndpoint = "ChangePrices";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public VnDirectApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    private EndpointOptions GetTopStocksEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(TopStocksEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{TopStocksEndpoint}' is not configured.");
    }

    private EndpointOptions GetRatiosLatestEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(RatiosLatestEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{RatiosLatestEndpoint}' is not configured.");
    }

    /// <summary>
    /// Gets VnDirect top stocks list.
    /// This endpoint should be called through proxy.
    /// </summary>
    public Task<T?> GetTopStocksAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetTopStocksEndpoint();

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
    /// Gets VnDirect latest ratios by stock code.
    /// This endpoint should be called through proxy.
    /// </summary>
    public Task<T?> GetRatiosLatestAsync<T>(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));
        }

        var endpoint = GetRatiosLatestEndpoint();
        var normalizedCode = code.Trim().ToUpperInvariant();
        var url = endpoint.Url.Replace("{Code}", Uri.EscapeDataString(normalizedCode));

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

    private EndpointOptions GetEventsEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(EventsEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{EventsEndpoint}' is not configured.");
    }

    private EndpointOptions GetRatiosLatestByItemCodeEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(RatiosLatestByItemCodeEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{RatiosLatestByItemCodeEndpoint}' is not configured.");
    }

    /// <summary>
    /// Gets VnDirect stock events list (nomargin, alert, halt, control, suspend, noticed).
    /// This endpoint should be called through proxy.
    /// </summary>
    public Task<T?> GetEventsAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetEventsEndpoint();

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

    private EndpointOptions GetStockPricesEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(StockPricesEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{StockPricesEndpoint}' is not configured.");
    }

    private EndpointOptions GetStockPricesAllEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(StockPricesAllEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{StockPricesAllEndpoint}' is not configured.");
    }

    private EndpointOptions GetRecommendationsEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(RecommendationsEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{RecommendationsEndpoint}' is not configured.");
    }

    private EndpointOptions GetTechnicalSignalsEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(TechnicalSignalsEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{TechnicalSignalsEndpoint}' is not configured.");
    }

    /// <summary>
    /// Gets VnDirect stock price history by stock code.
    /// This endpoint should be called through proxy.
    /// </summary>
    public Task<T?> GetStockPricesAsync<T>(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Code cannot be null or empty.", nameof(code));
        }

        var endpoint = GetStockPricesEndpoint();
        var normalizedCode = code.Trim().ToUpperInvariant();
        var url = endpoint.Url.Replace("{Code}", Uri.EscapeDataString(normalizedCode));

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
    /// Gets VnDirect stock prices for all listed stocks (no code filter).
    /// Returns the most recent page of price records sorted by date descending.
    /// This endpoint requires proxy routing.
    /// </summary>
    public Task<T?> GetStockPricesAllAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetStockPricesAllEndpoint();

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
    /// Gets VnDirect latest market ratios by predefined item codes.
    /// This endpoint should be called through proxy.
    /// </summary>
    public Task<T?> GetRatiosLatestByItemCodeAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetRatiosLatestByItemCodeEndpoint();

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
    /// Gets VnDirect recommendations list.
    /// This endpoint should be called through proxy.
    /// </summary>
    public Task<T?> GetRecommendationsAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetRecommendationsEndpoint();

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

    private EndpointOptions GetChangePricesEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(ChangePricesEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{ChangePricesEndpoint}' is not configured.");
    }

    /// <summary>
    /// Gets VnDirect change prices for market indices (VNINDEX, HNX, UPCOM, VN30, VN30F1M) for period 1D.
    /// This endpoint should be called through proxy.
    /// </summary>
    public Task<T?> GetChangePricesAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetChangePricesEndpoint();

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
    /// Gets VnDirect technical signals list for a strategy.
    /// This endpoint should be called through proxy.
    /// </summary>
    public Task<T?> GetTechnicalSignalsAsync<T>(string strategy, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(strategy))
        {
            throw new ArgumentException("Strategy cannot be null or empty.", nameof(strategy));
        }

        var endpoint = GetTechnicalSignalsEndpoint();
        var normalizedStrategy = strategy.Trim();
        var encodedStrategy = Uri.EscapeDataString(normalizedStrategy);
        var url = endpoint.Url.Replace("{strategy}", encodedStrategy);

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
