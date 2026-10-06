using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// Dragon Capital external API provider. Reads endpoint policies from configuration (ExternalApis:Providers:DragonCapital).
/// Supports fund codes: VF1, VF4, VFMVN30, VFMVND, VFMMID.
/// </summary>
public sealed class DragonCapitalApiProvider
{
    private const string ProviderName = "DragonCapital";
    private const string FundPortfolioEndpoint = "FundPortfolio";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public DragonCapitalApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    /// <summary>
    /// Gets the FundPortfolio endpoint config. Throws if not configured.
    /// </summary>
    private EndpointOptions GetFundPortfolioEndpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(FundPortfolioEndpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{FundPortfolioEndpoint}' is not configured.");
    }

    /// <summary>
    /// Calls Dragon Capital fund portfolio endpoint with specified fund code.
    /// Supported fund codes: VF1, VF4, VFMVN30, VFMVND, VFMMID.
    /// Policy (cache, proxy, timeout) from config.
    /// </summary>
    public Task<T?> GetFundPortfolioAsync<T>(string fundCode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(fundCode))
        {
            throw new ArgumentException("Fund code cannot be null or empty.", nameof(fundCode));
        }

        var endpoint = GetFundPortfolioEndpoint();

        var url = endpoint.Url.Replace("{fundCode}", fundCode);

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
