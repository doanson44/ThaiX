using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// ChainBroker external API provider. Reads endpoint policies from configuration (ExternalApis:Providers:ChainBroker).
/// </summary>
public sealed class ChainBrokerApiProvider
{
    private const string ProviderName = "ChainBroker";
    private const string UnlocksListEndpoint = "UnlocksList";
    private const string ProjectsListEndpoint = "ProjectsList";
    private const string FundsListEndpoint = "FundsList";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;

    public ChainBrokerApiProvider(ExternalApiService apiService, IOptions<ExternalApisOptions> options)
    {
        _apiService = apiService;
        _options = options.Value;
    }

    private EndpointOptions GetEndpoint(string endpointName)
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(endpointName, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{endpointName}' is not configured.");
    }

    private ApiRequest BuildRequest(EndpointOptions endpoint, int? page)
    {
        var url = endpoint.Url;
        if (page.HasValue)
        {
            url = BuildPagedUrl(endpoint, page.Value);
        }

        return new ApiRequest
        {
            Url = url,
            Method = HttpMethod.Get,
            Headers = new Dictionary<string, string>
            {
                { "User-Agent", "ThaiX/1.0" },
                { "Accept", "application/json" }
            },
            UseCache = endpoint.UseCache,
            CacheDuration = endpoint.CacheDurationSeconds > 0
                ? TimeSpan.FromSeconds(endpoint.CacheDurationSeconds)
                : null,
            UseProxy = endpoint.UseProxy,
            TimeoutSeconds = endpoint.TimeoutSeconds > 0 ? endpoint.TimeoutSeconds : 30
        };
    }

    private ApiRequest BuildSyncRequest(EndpointOptions endpoint, int page) => new()
    {
        Url = BuildPagedUrl(endpoint, page),
        Method = HttpMethod.Get,
        Headers = new Dictionary<string, string>
        {
            { "User-Agent", "ThaiX/1.0" },
            { "Accept", "application/json" }
        },
        UseCache = false,
        UseProxy = endpoint.UseProxy,
        TimeoutSeconds = endpoint.TimeoutSeconds > 0 ? endpoint.TimeoutSeconds : 60
    };

    private static string BuildPagedUrl(EndpointOptions endpoint, int page)
    {
        return endpoint.Url.TrimEnd('/') + $"/?page={page}";
    }

    private static string BuildPagedFundUrl(EndpointOptions endpoint, int page, string fundSlug)
    {
        return endpoint.Url.TrimEnd('/') + $"/?page={page}&funds={Uri.EscapeDataString(fundSlug)}";
    }

    private ApiRequest BuildSyncFundRequest(EndpointOptions endpoint, int page, string fundSlug) => new()
    {
        Url = BuildPagedFundUrl(endpoint, page, fundSlug),
        Method = HttpMethod.Get,
        Headers = new Dictionary<string, string>
        {
            { "User-Agent", "ThaiX/1.0" },
            { "Accept", "application/json" }
        },
        UseCache = false,
        UseProxy = endpoint.UseProxy,
        TimeoutSeconds = endpoint.TimeoutSeconds > 0 ? endpoint.TimeoutSeconds : 60
    };

    /// <summary>Gets ChainBroker token unlocks list. Optional page parameter for pagination.</summary>
    public Task<T?> GetUnlocksListAsync<T>(int? page = null, CancellationToken cancellationToken = default)
    {
        var endpoint = GetEndpoint(UnlocksListEndpoint);
        var request = BuildRequest(endpoint, page);
        return _apiService.ExecuteAsync<T>(request, cancellationToken);
    }

    /// <summary>Gets ChainBroker unlocks list (page for background sync, cache bypassed).</summary>
    public Task<T?> GetUnlocksListSyncAsync<T>(int page, CancellationToken cancellationToken = default)
    {
        var endpoint = GetEndpoint(UnlocksListEndpoint);
        return _apiService.ExecuteAsync<T>(BuildSyncRequest(endpoint, page), cancellationToken);
    }

    public string GetUnlocksListSyncUrl(int page)
    {
        return BuildPagedUrl(GetEndpoint(UnlocksListEndpoint), page);
    }

    /// <summary>Gets ChainBroker projects list. Optional page parameter for pagination.</summary>
    public Task<T?> GetProjectsListAsync<T>(int? page = null, CancellationToken cancellationToken = default)
    {
        var endpoint = GetEndpoint(ProjectsListEndpoint);
        var request = BuildRequest(endpoint, page);
        return _apiService.ExecuteAsync<T>(request, cancellationToken);
    }

    /// <summary>Gets ChainBroker projects list (page for background sync, cache bypassed).</summary>
    public Task<T?> GetProjectsListSyncAsync<T>(int page, CancellationToken cancellationToken = default)
    {
        var endpoint = GetEndpoint(ProjectsListEndpoint);
        return _apiService.ExecuteAsync<T>(BuildSyncRequest(endpoint, page), cancellationToken);
    }

    public string GetProjectsListSyncUrl(int page)
    {
        return BuildPagedUrl(GetEndpoint(ProjectsListEndpoint), page);
    }

    /// <summary>Gets ChainBroker projects list filtered by fund slug (cache bypassed).</summary>
    public Task<T?> GetProjectsByFundSyncAsync<T>(int page, string fundSlug, CancellationToken cancellationToken = default)
    {
        var endpoint = GetEndpoint(ProjectsListEndpoint);
        return _apiService.ExecuteAsync<T>(BuildSyncFundRequest(endpoint, page, fundSlug), cancellationToken);
    }

    public string GetProjectsByFundSyncUrl(int page, string fundSlug)
    {
        return BuildPagedFundUrl(GetEndpoint(ProjectsListEndpoint), page, fundSlug);
    }

    /// <summary>Gets ChainBroker funds list. Optional page parameter for pagination.</summary>
    public Task<T?> GetFundsListAsync<T>(int? page = null, CancellationToken cancellationToken = default)
    {
        var endpoint = GetEndpoint(FundsListEndpoint);
        var request = BuildRequest(endpoint, page);
        return _apiService.ExecuteAsync<T>(request, cancellationToken);
    }

    /// <summary>Gets ChainBroker funds list (page for background sync, cache bypassed).</summary>
    public Task<T?> GetFundsListSyncAsync<T>(int page, CancellationToken cancellationToken = default)
    {
        var endpoint = GetEndpoint(FundsListEndpoint);
        return _apiService.ExecuteAsync<T>(BuildSyncRequest(endpoint, page), cancellationToken);
    }

    public string GetFundsListSyncUrl(int page)
    {
        return BuildPagedUrl(GetEndpoint(FundsListEndpoint), page);
    }
}
