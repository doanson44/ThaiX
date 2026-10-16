using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// iWealth Club external API provider. Reads endpoint policies from configuration (ExternalApis:Providers:IWealthClub).
/// </summary>
public sealed class IWealthClubApiProvider
{
    private const string ProviderName = "IWealthClub";
    private const string StreamEndpoint = "Stream";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public IWealthClubApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    private EndpointOptions GetStreamEndpoint()
    { return _options.GetEndpoint(ProviderName, StreamEndpoint); }

    /// <summary>
    /// Gets iWealth Club stream content by optional cursor (lastContentId).
    /// First call uses literal "null" placeholder.
    /// </summary>
    public Task<T?> GetStreamAsync<T>(string? lastContentId, CancellationToken cancellationToken = default)
    {
        var endpoint = GetStreamEndpoint();
        var url = string.IsNullOrWhiteSpace(lastContentId)
            ? endpoint.Url
            : $"{endpoint.Url}&StreamQuery[from]={Uri.EscapeDataString(lastContentId.Trim())}";

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
