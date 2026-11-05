using Microsoft.Extensions.Options;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;
using ThaiX.Infrastructure.ExternalApis.Resilience;

namespace ThaiX.Infrastructure.ExternalApis.Transport;

/// <summary>
/// Routes API requests through the appropriate transport with per-host rate limiting.
/// request.UseProxy overrides global setting when set. request.UseBrowser bypasses the HTTP transports.
/// </summary>
public sealed class ApiTransportRouter : IApiTransport
{
    private readonly ExternalApisOptions _options;
    private readonly HttpTransport _httpTransport;
    private readonly ProxyTransport _proxyTransport;
    private readonly BrowserTransport _browserTransport;
    private readonly ExternalApiRateLimiter _rateLimiter;

    public ApiTransportRouter(
        IOptions<ExternalApisOptions> options,
        HttpTransport httpTransport,
        ProxyTransport proxyTransport,
        BrowserTransport browserTransport,
        ExternalApiRateLimiter rateLimiter)
    {
        _options = options.Value;
        _httpTransport = httpTransport;
        _proxyTransport = proxyTransport;
        _browserTransport = browserTransport;
        _rateLimiter = rateLimiter;
    }

    public Task<T?> SendAsync<T>(ApiRequest request, CancellationToken cancellationToken)
    {
        if (request.UseBrowser)
        {
            return _browserTransport.SendAsync<T>(request, cancellationToken);
        }

        var useProxy = request.UseProxy ?? _options.UseProxy;
        return useProxy
            ? _rateLimiter.ExecuteAsync(request, (r, ct) => _proxyTransport.SendAsync<T>(r, ct), cancellationToken)
            : _rateLimiter.ExecuteAsync(request, (r, ct) => _httpTransport.SendAsync<T>(r, ct), cancellationToken);
    }
}
