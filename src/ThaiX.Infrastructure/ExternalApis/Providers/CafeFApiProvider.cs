using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// CafeF external API provider. Reads endpoint policies from configuration (ExternalApis:Providers:CafeF).
/// </summary>
public sealed class CafeFApiProvider
{
    private const string ProviderName = "CafeF";
    private const string BankInterestRatesEndpoint = "BankInterestRates";
    private const string CommoditiesEndpoint = "Commodities";
    private const string CurrenciesEndpoint = "Currencies";
    private const string CryptocurrenciesEndpoint = "Cryptocurrencies";
    private const string StockPriceHistoryEndpoint = "StockPriceHistory";
    private const string StockWatchlistPriceEndpoint = "StockWatchlistPrice";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public CafeFApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    /// <summary>
    /// Gets the BankInterestRates endpoint config. Throws if not configured.
    /// </summary>
    private EndpointOptions GetBankInterestRatesEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(BankInterestRatesEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{BankInterestRatesEndpoint}' is not configured.");
    }

    /// <summary>
    /// Gets the Commodities endpoint config. Throws if not configured.
    /// </summary>
    private EndpointOptions GetCommoditiesEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(CommoditiesEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{CommoditiesEndpoint}' is not configured.");
    }

    /// <summary>
    /// Gets the Currencies endpoint config. Throws if not configured.
    /// </summary>
    private EndpointOptions GetCurrenciesEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(CurrenciesEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{CurrenciesEndpoint}' is not configured.");
    }

    /// <summary>
    /// Gets the Cryptocurrencies endpoint config. Throws if not configured.
    /// </summary>
    private EndpointOptions GetCryptocurrenciesEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(CryptocurrenciesEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{CryptocurrenciesEndpoint}' is not configured.");
    }

    /// <summary>
    /// Gets the StockPriceHistory endpoint config. Throws if not configured.
    /// </summary>
    private EndpointOptions GetStockPriceHistoryEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(StockPriceHistoryEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{StockPriceHistoryEndpoint}' is not configured.");
    }

    /// <summary>
    /// Gets the StockWatchlistPrice endpoint config. Throws if not configured.
    /// </summary>
    private EndpointOptions GetStockWatchlistPriceEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(StockWatchlistPriceEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{StockWatchlistPriceEndpoint}' is not configured.");
    }

    /// <summary>
    /// Calls CafeF bank interest rates endpoint. Policy (cache, proxy, timeout) from config.
    /// </summary>
    public Task<T?> GetBankInterestRatesAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetBankInterestRatesEndpoint();
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
    /// Calls CafeF commodities endpoint (type=1). Returns gold, silver, oil, metals, agricultural products.
    /// </summary>
    public Task<T?> GetCommoditiesAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetCommoditiesEndpoint();
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
    /// Calls CafeF currencies endpoint (type=2). Returns currency exchange rates (USD, EUR, GBP, etc.).
    /// </summary>
    public Task<T?> GetCurrenciesAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetCurrenciesEndpoint();
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
    /// Calls CafeF cryptocurrencies endpoint (type=3). Returns cryptocurrency prices (Bitcoin, Ethereum, etc.).
    /// </summary>
    public Task<T?> GetCryptocurrenciesAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetCryptocurrenciesEndpoint();
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
    /// Calls CafeF stock price history endpoint by stock symbol (e.g. VNM, VIC, NKG).
    /// Policy (cache, proxy, timeout) from config.
    /// </summary>
    public Task<T?> GetStockPriceHistoryAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Stock symbol cannot be null or empty.", nameof(symbol));
        }

        var endpoint = GetStockPriceHistoryEndpoint();
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var url = endpoint.Url.Replace("{symbol}", normalizedSymbol);

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
    /// Calls CafeF watchlist price endpoint by stock symbol (e.g. VNM, VIC, NKG).
    /// Policy (cache, proxy, timeout) from config.
    /// </summary>
    public Task<T?> GetStockWatchlistPriceAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Stock symbol cannot be null or empty.", nameof(symbol));
        }

        var endpoint = GetStockWatchlistPriceEndpoint();
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var url = endpoint.Url.Replace("{symbol}", normalizedSymbol);

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
