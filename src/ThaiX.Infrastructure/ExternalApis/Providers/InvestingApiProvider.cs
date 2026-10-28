using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// Investing.com market data provider. Endpoint and request policies are configuration-driven.
/// </summary>
public sealed class InvestingApiProvider
{
    private const string ProviderName = "Investing";
    private const string VnIndexCandlesEndpoint = "VnIndexCandles";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public InvestingApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    public Task<T?> GetVnIndexCandlesAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = _options.GetEndpoint(ProviderName, VnIndexCandlesEndpoint);
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
            Headers = new Dictionary<string, string>
            {
                { "Accept", "application/json" },
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 Chrome/131.0.0.0 Safari/537.36" }
            }
        };

        return _apiService.ExecuteAsync<T>(request, cancellationToken);
    }
}
