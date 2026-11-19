using System.Collections;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Timeout;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;
using ThaiX.Infrastructure.ExternalApis.Resilience;

namespace ThaiX.Infrastructure.ExternalApis.Transport;

/// <summary>
/// Routes API requests through the appropriate transport with per-host rate limiting.
/// request.UseProxy overrides global setting. request.UseBrowser bypasses the HTTP transports.
/// </summary>
public sealed class ApiTransportRouter : IApiTransport
{
    private readonly ExternalApisOptions _options;
    private readonly HttpTransport _httpTransport;
    private readonly ProxyTransport _proxyTransport;
    private readonly BrowserTransport _browserTransport;
    private readonly ExternalApiRateLimiter _rateLimiter;
    private readonly ILogger<ApiTransportRouter> _logger;

    public ApiTransportRouter(
        IOptions<ExternalApisOptions> options,
        HttpTransport httpTransport,
        ProxyTransport proxyTransport,
        BrowserTransport browserTransport,
        ExternalApiRateLimiter rateLimiter,
        ILogger<ApiTransportRouter> logger)
    {
        _options = options.Value;
        _httpTransport = httpTransport;
        _proxyTransport = proxyTransport;
        _browserTransport = browserTransport;
        _rateLimiter = rateLimiter;
        _logger = logger;
    }

    public async Task<T?> SendAsync<T>(ApiRequest request, CancellationToken cancellationToken)
    {
        if (request.UseBrowser)
        {
            return await _browserTransport.SendAsync<T>(request, cancellationToken).ConfigureAwait(false);
        }

        var useProxy = request.UseProxy ?? _options.UseProxy;
        if (useProxy)
        {
            return await _rateLimiter.ExecuteAsync(
                request,
                (r, ct) => _proxyTransport.SendAsync<T>(r, ct),
                cancellationToken).ConfigureAwait(false);
        }

        T? response;
        try
        {
            response = await _rateLimiter.ExecuteAsync(
                request,
                (r, ct) => _httpTransport.SendAsync<T>(r, ct),
                cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException exception) when (CanFallback(request, useProxy, exception))
        {
            _logger.LogWarning(
                exception,
                "External API direct request failed; switching to proxy. Host={Host}",
                GetHost(request.Url));
            return await SendProxyFallbackAsync<T>(request, cancellationToken).ConfigureAwait(false);
        }
        catch (TimeoutRejectedException exception) when (CanFallback(request, useProxy))
        {
            _logger.LogWarning(
                exception,
                "External API direct request timed out; switching to proxy. Host={Host}",
                GetHost(request.Url));
            return await SendProxyFallbackAsync<T>(request, cancellationToken).ConfigureAwait(false);
        }

        if (request.EnableProxyFallback && IsEmptyResponse(response))
        {
            _logger.LogWarning(
                "External API direct request returned no usable data; switching to proxy. Host={Host}",
                GetHost(request.Url));
            return await SendProxyFallbackAsync<T>(request, cancellationToken).ConfigureAwait(false);
        }

        return response;
    }

    private Task<T?> SendProxyFallbackAsync<T>(ApiRequest request, CancellationToken cancellationToken)
    {
        return _rateLimiter.ExecuteAsync(
            request,
            (r, ct) => _proxyTransport.SendAsync<T>(r, ct),
            cancellationToken);
    }

    private static bool CanFallback(ApiRequest request, bool useProxy)
    {
        return request.EnableProxyFallback
            && !useProxy
            && !request.UseBrowser
            && request.Method == HttpMethod.Get;
    }

    private static bool CanFallback(ApiRequest request, bool useProxy, HttpRequestException exception)
    {
        if (!CanFallback(request, useProxy))
        {
            return false;
        }

        if (exception.StatusCode is not { } statusCode)
        {
            return true;
        }

        var numericStatusCode = (int)statusCode;
        return statusCode is System.Net.HttpStatusCode.Forbidden
                or System.Net.HttpStatusCode.RequestTimeout
            || numericStatusCode == 451
            || numericStatusCode >= 500;
    }

    private static bool IsEmptyResponse<T>(T? response)
    {
        if (response is null || EqualityComparer<T?>.Default.Equals(response, default))
        {
            return true;
        }

        if (response is string text)
        {
            return string.IsNullOrWhiteSpace(text);
        }

        if (response is JsonElement jsonElement)
        {
            return jsonElement.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined
                || jsonElement.ValueKind == JsonValueKind.Array && jsonElement.GetArrayLength() == 0;
        }

        if (response is IEnumerable enumerable && IsEmptyEnumerable(enumerable))
        {
            return true;
        }

        var responseType = response.GetType();
        var dataProperty = responseType.GetProperty("Data");
        if (dataProperty is not null)
        {
            var data = dataProperty.GetValue(response);
            if (data is null || data is IEnumerable dataEnumerable && IsEmptyEnumerable(dataEnumerable))
            {
                return true;
            }
        }

        var bidsProperty = responseType.GetProperty("Bids");
        var asksProperty = responseType.GetProperty("Asks");
        if (bidsProperty is not null && asksProperty is not null)
        {
            var bids = bidsProperty.GetValue(response) as IEnumerable;
            var asks = asksProperty.GetValue(response) as IEnumerable;
            return (bids is null || IsEmptyEnumerable(bids))
                && (asks is null || IsEmptyEnumerable(asks));
        }

        return false;
    }

    private static bool IsEmptyEnumerable(IEnumerable enumerable)
    {
        var enumerator = enumerable.GetEnumerator();
        try
        {
            return !enumerator.MoveNext();
        }
        finally
        {
            (enumerator as IDisposable)?.Dispose();
        }
    }

    private static string GetHost(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri.Host : "unknown";
    }
}
