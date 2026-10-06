using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// Yahoo Finance external API provider. Reads endpoint policies from configuration (ExternalApis:Providers:YahooFinance).
/// </summary>
public sealed class YahooFinanceApiProvider
{
    private const string ProviderName = "YahooFinance";
    private const string VixChartEndpoint = "VixChart";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public YahooFinanceApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    /// <summary>
    /// Gets the VixChart endpoint config. Throws if not configured.
    /// </summary>
    private EndpointOptions GetVixChartEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(VixChartEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{VixChartEndpoint}' is not configured.");
    }

    /// <summary>
    /// Calls Yahoo Finance VIX chart endpoint. Period2 is automatically set to UTC now.
    /// Policy (cache, proxy, timeout) from config.
    /// </summary>
    public Task<T?> GetVixChartAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetVixChartEndpoint();

        var period2 = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var url = endpoint.Url.Replace("{period2}", period2.ToString());

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
            Headers = new Dictionary<string, string>
            {
                { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36" }
            }
        };
        return _apiService.ExecuteAsync<T>(request, cancellationToken);
    }
}
