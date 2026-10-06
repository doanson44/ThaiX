using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// Sacombank external API provider. Reads endpoint policies from configuration (ExternalApis:Providers:Sacombank).
/// </summary>
public sealed class SacombankApiProvider
{
    private const string ProviderName = "Sacombank";
    private const string ExchangeRateEndpoint = "ExchangeRate";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public SacombankApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    /// <summary>
    /// Gets the ExchangeRate endpoint config. Throws if not configured.
    /// </summary>
    private EndpointOptions GetExchangeRateEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(ExchangeRateEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{ExchangeRateEndpoint}' is not configured.");
    }

    /// <summary>
    /// Calls Sacombank exchange rate endpoint. Returns latest exchange rates for currencies and gold.
    /// Policy (cache, proxy, timeout) from config.
    /// </summary>
    public Task<T?> GetExchangeRatesAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetExchangeRateEndpoint();
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
