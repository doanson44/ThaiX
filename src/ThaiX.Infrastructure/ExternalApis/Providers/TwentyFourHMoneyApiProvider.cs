using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// 24HMoney external API provider.
/// </summary>
public sealed class TwentyFourHMoneyApiProvider
{
    private const string ProviderName = "TwentyFourHMoney";
    private const string TransactionListSsiEndpoint = "TransactionListSsi";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public TwentyFourHMoneyApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    public Task<T?> GetTransactionListSsiAsync<T>(
        string symbol,
        int page = 1,
        int perPage = 1000,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(symbol))
        {
            throw new ArgumentException("Symbol cannot be null or empty.", nameof(symbol));
        }

        var endpoint = GetTransactionListSsiEndpoint();
        var normalizedSymbol = symbol.Trim().ToUpperInvariant();
        var url = endpoint.Url
            .Replace("{symbol}", Uri.EscapeDataString(normalizedSymbol))
            .Replace("{page}", page.ToString(System.Globalization.CultureInfo.InvariantCulture))
            .Replace("{perPage}", perPage.ToString(System.Globalization.CultureInfo.InvariantCulture));

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

    private EndpointOptions GetTransactionListSsiEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(TransactionListSsiEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{TransactionListSsiEndpoint}' is not configured.");
    }
}
