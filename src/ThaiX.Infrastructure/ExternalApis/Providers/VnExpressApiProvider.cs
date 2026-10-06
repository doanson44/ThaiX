using Microsoft.Extensions.Options;
using ThaiX.Application.Common.Enums;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// VnExpress external API provider. Reads endpoint policies from configuration (ExternalApis:Providers:VnExpress).
/// </summary>
public sealed class VnExpressApiProvider
{
    private const string ProviderName = "VnExpress";
    private const string BankRateOnlineEndpoint = "BankRateOnline";
    private const string BankRateOfflineEndpoint = "BankRateOffline";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public VnExpressApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    /// <summary>
    /// Calls the VnExpress bank deposit rates endpoint matching the requested channel
    /// (types=bank_rate_online for online, types=bank_rate_offline for counter).
    /// Policy (cache, proxy, timeout) from config.
    /// </summary>
    public Task<T?> GetBankRateAsync<T>(BankRateChannel channel, CancellationToken cancellationToken = default)
    {
        return ExecuteAsync<T>(ResolveEndpointName(channel), cancellationToken);
    }

    private static string ResolveEndpointName(BankRateChannel channel) => channel switch
    {
        BankRateChannel.Offline => BankRateOfflineEndpoint,
        BankRateChannel.Online => BankRateOnlineEndpoint,
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unsupported bank rate channel.")
    };

    private Task<T?> ExecuteAsync<T>(string endpointName, CancellationToken cancellationToken)
    {
        if (!_options.Providers.TryGetValue(ProviderName, out var endpoints) ||
            !endpoints.TryGetValue(endpointName, out var endpoint))
        {
            throw new InvalidOperationException(
                $"External API endpoint '{ProviderName}:{endpointName}' is not configured.");
        }

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
