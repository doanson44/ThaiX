using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;
using ThaiX.Infrastructure.ExternalApis.Resilience;

namespace ThaiX.Infrastructure.ExternalApis.Transport;

/// <summary>
/// Routes API requests through the appropriate transport with per-host rate limiting.
/// request.UseProxy overrides global setting when set.
/// </summary>
public sealed class ApiTransportRouter : IApiTransport
{
    private readonly ExternalApisOptions _options;
    private readonly HttpTransport _httpTransport;
    private readonly ProxyTransport _proxyTransport;
    private readonly ExternalApiRateLimiter _rateLimiter;

    public ApiTransportRouter(
        IOptions<ExternalApisOptions> options,
        HttpTransport httpTransport,
        ProxyTransport proxyTransport,
        ExternalApiRateLimiter rateLimiter)
    {
        _options = options.Value;
        _httpTransport = httpTransport;
        _proxyTransport = proxyTransport;
        _rateLimiter = rateLimiter;
    }

    public Task<T?> SendAsync<T>(ApiRequest request, CancellationToken cancellationToken)
    {
        var useProxy = request.UseProxy ?? _options.UseProxy;
        return useProxy
            ? _rateLimiter.ExecuteAsync(request, (r, ct) => _proxyTransport.SendAsync<T>(r, ct), cancellationToken)
            : _rateLimiter.ExecuteAsync(request, (r, ct) => _httpTransport.SendAsync<T>(r, ct), cancellationToken);
    }
}
