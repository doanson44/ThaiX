using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// MEXC external API provider. Reads endpoint policies from configuration (ExternalApis:Providers:Mexc).
/// </summary>
public sealed class MexcApiProvider
{
    private const string ProviderName = "Mexc";
    private const string ContractTickerEndpoint = "ContractTicker";
    private const string ContractTickerBySymbolEndpoint = "ContractTickerBySymbol";
    private const string ContractKlineEndpoint = "ContractKline";
    private const string ContractFundingRateEndpoint = "ContractFundingRate";
    private const string ContractDepthEndpoint = "ContractDepth";
    private const string SpotTicker24HrEndpoint = "SpotTicker24Hr";
    private const string SpotTicker24HrBySymbolEndpoint = "SpotTicker24HrBySymbol";
    private const string SpotKlinesEndpoint = "SpotKlines";
    private const string SpotDepthEndpoint = "SpotDepth";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public MexcApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    private EndpointOptions GetContractTickerEndpoint()
    { return _options.GetEndpoint(ProviderName, ContractTickerEndpoint); }

    private EndpointOptions GetContractTickerBySymbolEndpoint()
    { return _options.GetEndpoint(ProviderName, ContractTickerBySymbolEndpoint); }

    private EndpointOptions GetContractKlineEndpoint()
    { return _options.GetEndpoint(ProviderName, ContractKlineEndpoint); }

    private EndpointOptions GetContractFundingRateEndpoint()
    { return _options.GetEndpoint(ProviderName, ContractFundingRateEndpoint); }

    private EndpointOptions GetContractDepthEndpoint()
    { return _options.GetEndpoint(ProviderName, ContractDepthEndpoint); }

    private EndpointOptions GetSpotTicker24HrEndpoint()
    { return _options.GetEndpoint(ProviderName, SpotTicker24HrEndpoint); }

    private EndpointOptions GetSpotTicker24HrBySymbolEndpoint()
    { return _options.GetEndpoint(ProviderName, SpotTicker24HrBySymbolEndpoint); }

    private EndpointOptions GetSpotKlinesEndpoint()
    { return _options.GetEndpoint(ProviderName, SpotKlinesEndpoint); }

    private EndpointOptions GetSpotDepthEndpoint()
    { return _options.GetEndpoint(ProviderName, SpotDepthEndpoint); }

    /// <summary>
    /// Gets MEXC contract ticker for all market symbols.
    /// </summary>
    public Task<T?> GetContractTickerAsync<T>(CancellationToken cancellationToken = default)
        => GetContractTickerAsync<T>(bypassCache: false, cancellationToken);

    /// <summary>
    /// Gets MEXC contract ticker for all market symbols.
    /// When bypassCache is true, skips the external API cache.
    /// </summary>
    public Task<T?> GetContractTickerAsync<T>(bool bypassCache, CancellationToken cancellationToken = default)
    {
        var endpoint = GetContractTickerEndpoint();
        var request = new ApiRequest
        {
            Url = endpoint.Url,
            Method = HttpMethod.Get,
            UseCache = !bypassCache && endpoint.UseCache,
            CacheDuration = endpoint.CacheDurationSeconds > 0
                ? TimeSpan.FromSeconds(endpoint.CacheDurationSeconds)
                : null,
            UseProxy = endpoint.UseProxy,
            TimeoutSeconds = endpoint.TimeoutSeconds > 0 ? endpoint.TimeoutSeconds : 30
        };

        return _apiService.ExecuteAsync<T>(request, cancellationToken);
    }

    /// <summary>
    /// Gets MEXC contract ticker by symbol (example: BTC_USDT).
    /// </summary>
    public Task<T?> GetContractTickerBySymbolAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));
        }

        var endpoint = GetContractTickerBySymbolEndpoint();
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
    /// Gets MEXC contract kline data by symbol with optional interval.
    /// </summary>
    public Task<T?> GetContractKlineAsync<T>(string symbol, string? interval = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));
        }

        var endpoint = GetContractKlineEndpoint();
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
            TimeoutSeconds = endpoint.TimeoutSeconds > 0 ? endpoint.TimeoutSeconds : 30,
            QueryParameters = string.IsNullOrWhiteSpace(interval)
                ? null
                : new Dictionary<string, string>
                {
                    ["interval"] = interval.Trim()
                }
        };

        return _apiService.ExecuteAsync<T>(request, cancellationToken);
    }

    /// <summary>
    /// Gets MEXC contract funding rates for all symbols.
    /// </summary>
    public Task<T?> GetContractFundingRatesAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetContractFundingRateEndpoint();

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
    /// Gets MEXC contract depth by symbol.
    /// </summary>
    public Task<T?> GetContractDepthAsync<T>(string symbol, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));
        }

        var endpoint = GetContractDepthEndpoint();
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
    /// Gets MEXC spot 24h ticker statistics for all symbols.
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
    /// Gets MEXC spot 24h ticker statistics by symbol.
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

    /// <summary>
    /// Gets MEXC spot klines by symbol and interval.
    /// </summary>
    public Task<T?> GetSpotKlinesAsync<T>(string symbol, string interval, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));
        }

        if (string.IsNullOrWhiteSpace(interval))
        {
            throw new ArgumentException("Interval cannot be null or empty.", nameof(interval));
        }

        var endpoint = GetSpotKlinesEndpoint();
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();

        var request = new ApiRequest
        {
            Url = endpoint.Url,
            Method = HttpMethod.Get,
            UseCache = endpoint.UseCache,
            CacheDuration = endpoint.CacheDurationSeconds > 0
                ? TimeSpan.FromSeconds(endpoint.CacheDurationSeconds)
                : null,
            UseProxy = endpoint.UseProxy,
            TimeoutSeconds = endpoint.TimeoutSeconds > 0 ? endpoint.TimeoutSeconds : 30,
            QueryParameters = new Dictionary<string, string>
            {
                ["symbol"] = normalizedSymbol,
                ["interval"] = interval.Trim()
            }
        };

        return _apiService.ExecuteAsync<T>(request, cancellationToken);
    }

    /// <summary>
    /// Gets MEXC spot depth by symbol.
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
}