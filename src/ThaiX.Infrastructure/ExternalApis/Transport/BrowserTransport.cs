using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Playwright;

namespace ThaiX.Infrastructure.ExternalApis.Transport;

/// <summary>
/// Transport that renders the request URL in a headless browser and returns the HTML.
/// Used for endpoints that cannot be reached with a plain HTTP client (bot challenge pages).
/// </summary>
public sealed class BrowserTransport : IApiTransport
{
    private readonly BrowserSession _session;

    public BrowserTransport(BrowserSession session)
    {
        _session = session;
    }

    public async Task<T?> SendAsync<T>(ApiRequest request, CancellationToken cancellationToken)
    {
        if (typeof(T) != typeof(string))
        {
            throw new NotSupportedException(
                $"BrowserTransport only supports string responses. Requested type: {typeof(T).Name}.");
        }

        var html = await _session
            .GetHtmlAsync(request.Url, request.TimeoutSeconds, request.WaitForSelector, cancellationToken)
            .ConfigureAwait(false);

        return (T?)(object)html;
    }
}
