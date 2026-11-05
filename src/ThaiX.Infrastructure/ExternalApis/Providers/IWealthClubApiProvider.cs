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
    private const string PotentialStocksEndpoint = "PotentialStocks";

    // Marker that proves the potential-stock list rendered (each item links to its research PDF).
    private const string PotentialStocksAnchorSelector = "a[href*='oneclick']";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public IWealthClubApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    private EndpointOptions GetStreamEndpoint()
    { return _options.GetEndpoint(ProviderName, StreamEndpoint); }

    private EndpointOptions GetPotentialStocksEndpoint()
    { return _options.GetEndpoint(ProviderName, PotentialStocksEndpoint); }

    /// <summary>
    /// Content source configured at ExternalApis:Providers:IWealthClub:Source.
    /// Callers use it to decide which sync flow to run.
    /// </summary>
    public TcbsSyncSource SyncSource => _options.GetProviderSource(ProviderName);

    /// <summary>True when the configured source is <see cref="TcbsSyncSource.PotentialStocks"/>.</summary>
    public bool IsPotentialStocksSource => SyncSource == TcbsSyncSource.PotentialStocks;

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

        return _apiService.ExecuteAsync<T>(BuildRequest(endpoint, url), cancellationToken);
    }

    /// <summary>
    /// Gets the TCBS potential stock page using the configured PotentialStocks endpoint.
    /// The endpoint must set "UseBrowser": true because the page is protected by a bot challenge.
    /// </summary>
    public Task<T?> GetPotentialStocksAsync<T>(CancellationToken cancellationToken = default)
    {
        var endpoint = GetPotentialStocksEndpoint();
        return _apiService.ExecuteAsync<T>(
            BuildRequest(endpoint, endpoint.Url, PotentialStocksAnchorSelector), cancellationToken);
    }

    /// <summary>
    /// Gets content from whichever source is configured (<see cref="SyncSource"/>).
    /// The cursor is only used by the Stream source.
    /// </summary>
    public Task<T?> GetContentAsync<T>(string? lastContentId, CancellationToken cancellationToken = default)
    {
        return IsPotentialStocksSource
            ? GetPotentialStocksAsync<T>(cancellationToken)
            : GetStreamAsync<T>(lastContentId, cancellationToken);
    }

    private static ApiRequest BuildRequest(EndpointOptions endpoint, string url, string? waitForSelector = null)
    {
        return new ApiRequest
        {
            Url = url,
            Method = HttpMethod.Get,
            UseCache = endpoint.UseCache,
            CacheDuration = endpoint.CacheDurationSeconds > 0
                ? TimeSpan.FromSeconds(endpoint.CacheDurationSeconds)
                : null,
            UseProxy = endpoint.UseProxy,
            UseBrowser = endpoint.UseBrowser,
            WaitForSelector = waitForSelector,
            TimeoutSeconds = endpoint.TimeoutSeconds > 0 ? endpoint.TimeoutSeconds : 30
        };
    }
}
