using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;
using ThaiX.Infrastructure.ExternalApis.Resilience;

namespace ThaiX.Infrastructure.ExternalApis.Transport;

/// <summary>
/// Sends requests through configured proxy endpoints. Timeout and retry are applied by ExternalApiResilienceHandler.
/// </summary>
public sealed class ProxyTransport : IApiTransport
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static readonly JsonSerializerOptions PayloadOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _httpClient;
    private readonly ProxyOptions _options;

    public ProxyTransport(HttpClient httpClient, IOptions<ProxyOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<T?> SendAsync<T>(ApiRequest request, CancellationToken cancellationToken)
    {
        var payload = new ProxyRequest
        {
            TargetUrl = request.Url,
            Method = request.Method.Method,
            Headers = request.Headers ?? new Dictionary<string, string>(),
            QueryParameters = request.QueryParameters ?? new Dictionary<string, string>(),
            Body = request.Body,
            TimeoutSeconds = ResolveTimeout(request),
            UseCache = request.UseCache,
            CacheDurationSeconds = ResolveCacheDurationSeconds(request.CacheDuration)
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, BuildProxyUri(request.Url));
        var authorization = BuildAuthorization();
        if (authorization is not null)
        {
            message.Headers.Authorization = authorization;
        }
        message.Content = new StringContent(JsonSerializer.Serialize(payload, PayloadOptions), Encoding.UTF8, "application/json");

        var timeout = TimeSpan.FromSeconds(ResolveTimeout(request));
        message.Options.Set(ExternalApiResilienceHandler.RequestTimeoutKey, timeout);

        using var response = await _httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken).ConfigureAwait(false);

        await ExternalApiResponseValidator.EnsureSuccessAsync(response, cancellationToken).ConfigureAwait(false);

        if (response.Content == null)
        {
            return default;
        }

        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (typeof(T) == typeof(string))
        {
            return (T?)(object)content;
        }

        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    private Uri BuildProxyUri(string targetUrl)
    {
        var selectedBaseUrl = ResolveProxyBaseUrl(targetUrl);

        if (string.IsNullOrWhiteSpace(selectedBaseUrl))
        {
            throw new InvalidOperationException("Proxy base URL is not configured.");
        }

        if (_httpClient.BaseAddress != null)
        {
            var clientBaseAddress = _httpClient.BaseAddress.ToString();
            if (string.Equals(clientBaseAddress.TrimEnd('/'), selectedBaseUrl.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
            {
                return new Uri(_httpClient.BaseAddress, "api/proxy");
            }
        }

        var baseUri = new Uri(EnsureTrailingSlash(selectedBaseUrl));
        return new Uri(baseUri, "api/proxy");
    }

    private string ResolveProxyBaseUrl(string targetUrl)
    {
        if (IsBinanceRequest(targetUrl) && !string.IsNullOrWhiteSpace(_options.BinanceBaseUrl))
        {
            return _options.BinanceBaseUrl;
        }

        return _options.BaseUrl;
    }

    private static bool IsBinanceRequest(string targetUrl)
    {
        if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var targetUri))
        {
            return false;
        }

        return targetUri.Host.EndsWith("binance.com", StringComparison.OrdinalIgnoreCase);
    }

    private AuthenticationHeaderValue? BuildAuthorization()
    {
        if (string.IsNullOrWhiteSpace(_options.Token))
        {
            return null;
        }

        return new AuthenticationHeaderValue("Bearer", _options.Token);
    }

    private static string EnsureTrailingSlash(string value)
    {
        return value.EndsWith("/", StringComparison.Ordinal) ? value : value + "/";
    }

    private static int ResolveTimeout(ApiRequest request)
    {
        var timeout = request.TimeoutSeconds > 0 ? request.TimeoutSeconds : 120;
        return Math.Clamp(timeout, 1, 300);
    }

    private static int? ResolveCacheDurationSeconds(TimeSpan? duration)
    {
        if (!duration.HasValue)
        {
            return null;
        }

        var seconds = (int)Math.Ceiling(duration.Value.TotalSeconds);
        return Math.Clamp(seconds, 10, 3600);
    }

    private sealed class ProxyRequest
    {
        public string TargetUrl { get; init; } = string.Empty;
        public string Method { get; init; } = "GET";
        public Dictionary<string, string>? Headers { get; init; }
        public Dictionary<string, string>? QueryParameters { get; init; }
        public string? Body { get; init; }
        public int TimeoutSeconds { get; init; }
        public bool UseCache { get; init; }
        public int? CacheDurationSeconds { get; init; }
    }
}
