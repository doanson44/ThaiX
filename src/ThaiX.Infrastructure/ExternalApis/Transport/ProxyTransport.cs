using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;
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
        var usesCmsxProxy = UsesCmsxProxy(request.Url);
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
        AddProxyAuthentication(message, usesCmsxProxy);
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
        if (string.IsNullOrWhiteSpace(content))
        {
            return default;
        }

        if (usesCmsxProxy)
        {
            var proxyResponse = JsonSerializer.Deserialize<CmsxProxyResponse>(content, JsonOptions)
                ?? throw new JsonException("CMSX proxy returned an empty response envelope.");

            if (proxyResponse.StatusCode is < 100 or > 599)
            {
                throw new JsonException("CMSX proxy returned an invalid target status code.");
            }

            if (proxyResponse.StatusCode is < 200 or >= 300)
            {
                throw new HttpRequestException(
                    $"Proxied external API request failed. StatusCode={proxyResponse.StatusCode}, ReasonPhrase={proxyResponse.ReasonPhrase}",
                    null,
                    (HttpStatusCode)proxyResponse.StatusCode);
            }

            content = proxyResponse.Body ?? string.Empty;
            if (string.IsNullOrWhiteSpace(content))
            {
                return default;
            }
        }

        if (typeof(T) == typeof(string))
        {
            return (T?)(object)content;
        }

        return JsonSerializer.Deserialize<T>(content, JsonOptions);
    }

    public async Task<CmsxProxyTestResponse> SendDiagnosticAsync(
        CmsxProxyTestRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Url)
            || request.Url.Length > 4096
            || !Uri.TryCreate(request.Url, UriKind.Absolute, out var targetUri)
            || (targetUri.Scheme != Uri.UriSchemeHttp && targetUri.Scheme != Uri.UriSchemeHttps)
            || !string.IsNullOrEmpty(targetUri.UserInfo))
        {
            throw new ArgumentException("Target URL must be an absolute HTTP or HTTPS URL without embedded credentials.");
        }

        await ValidatePublicTargetAsync(targetUri, cancellationToken).ConfigureAwait(false);

        var methodName = request.Method?.Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(methodName)
            || !new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS" }
                .Contains(methodName, StringComparer.Ordinal))
        {
            throw new ArgumentException("Unsupported HTTP method.");
        }

        var method = new HttpMethod(methodName);

        if (request.Body?.Length > 65536)
        {
            throw new ArgumentException("Request body must not exceed 64 KB.");
        }

        if (request.QueryParameters?.Count > 50)
        {
            throw new ArgumentException("No more than 50 query parameters are allowed.");
        }

        if (string.IsNullOrWhiteSpace(_options.BaseUrl)
            || !Uri.TryCreate(_options.BaseUrl, UriKind.Absolute, out var proxyBaseUri)
            || (proxyBaseUri.Scheme != Uri.UriSchemeHttp && proxyBaseUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException("CMSX proxy base URL is not configured correctly.");
        }

        var headers = (request.Headers ?? [])
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Key)
                && pair.Value is not null
                && pair.Value.Length <= 8192
                && !new[] { "Host", "Connection", "Content-Length", "Transfer-Encoding", "Proxy-Authorization", "X-Content-Management-Key" }
                    .Contains(pair.Key, StringComparer.OrdinalIgnoreCase))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.OrdinalIgnoreCase);

        var payload = new
        {
            targetUrl = request.Url,
            method = method.Method,
            headers,
            queryParameters = request.QueryParameters ?? [],
            body = request.Body,
            timeoutSeconds = Math.Clamp(request.TimeoutSeconds, 1, 60),
            useCache = false
        };

        var proxyUri = new Uri(new Uri(EnsureTrailingSlash(_options.BaseUrl)), "api/proxy");
        using var message = new HttpRequestMessage(HttpMethod.Post, proxyUri);
        AddProxyAuthentication(message, usesCmsxProxy: true);
        message.Content = new StringContent(
            JsonSerializer.Serialize(payload, PayloadOptions),
            Encoding.UTF8,
            "application/json");

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(request.TimeoutSeconds, 1, 60)));

        using var response = await _httpClient.SendAsync(
            message,
            HttpCompletionOption.ResponseHeadersRead,
            timeoutSource.Token).ConfigureAwait(false);

        var proxyBody = await ReadLimitedContentAsync(response.Content, 1_048_576, timeoutSource.Token)
            .ConfigureAwait(false);
        stopwatch.Stop();

        if (!response.IsSuccessStatusCode)
        {
            return new CmsxProxyTestResponse(
                (int)response.StatusCode,
                response.ReasonPhrase,
                null,
                null,
                proxyBody,
                stopwatch.ElapsedMilliseconds);
        }

        try
        {
            var envelope = JsonSerializer.Deserialize<CmsxProxyResponse>(proxyBody, JsonOptions);
            if (envelope is null || envelope.StatusCode is < 100 or > 599)
            {
                return new CmsxProxyTestResponse(
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    null,
                    null,
                    proxyBody,
                    stopwatch.ElapsedMilliseconds);
            }

            return new CmsxProxyTestResponse(
                (int)response.StatusCode,
                response.ReasonPhrase,
                envelope.StatusCode,
                envelope.ReasonPhrase,
                envelope.Body ?? string.Empty,
                stopwatch.ElapsedMilliseconds);
        }
        catch (JsonException)
        {
            return new CmsxProxyTestResponse(
                (int)response.StatusCode,
                response.ReasonPhrase,
                null,
                null,
                proxyBody,
                stopwatch.ElapsedMilliseconds);
        }
    }

    private static async Task ValidatePublicTargetAsync(Uri targetUri, CancellationToken cancellationToken)
    {
        var host = targetUri.DnsSafeHost.Trim('[', ']');
        if (string.Equals(host, "localhost", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase)
            || host.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Local and private network targets are not allowed.");
        }

        IPAddress[] addresses;
        if (IPAddress.TryParse(host, out var literalAddress))
        {
            addresses = [literalAddress];
        }
        else
        {
            try
            {
                addresses = await Dns.GetHostAddressesAsync(host, cancellationToken).ConfigureAwait(false);
            }
            catch (SocketException)
            {
                throw new ArgumentException("Target host could not be resolved.");
            }
        }

        if (addresses.Length == 0 || addresses.Any(address => !IsPublicAddress(address)))
        {
            throw new ArgumentException("Target host must resolve exclusively to public IP addresses.");
        }
    }

    private static bool IsPublicAddress(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        var bytes = address.GetAddressBytes();
        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            var a = bytes[0];
            var b = bytes[1];
            return a != 0 && a != 10 && a != 127 && a < 224
                && !(a == 100 && b is >= 64 and <= 127)
                && !(a == 169 && b == 254)
                && !(a == 172 && b is >= 16 and <= 31)
                && !(a == 192 && b == 168)
                && !(a == 192 && b == 0)
                && !(a == 192 && b == 2)
                && !(a == 198 && b is 18 or 19)
                && !(a == 198 && b == 51 && bytes[2] == 100)
                && !(a == 203 && b == 0 && bytes[2] == 113)
                && !(a == 192 && b == 88 && bytes[2] == 99);
        }

        if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            return !address.Equals(IPAddress.IPv6Any)
                && !address.Equals(IPAddress.IPv6Loopback)
                && !address.IsIPv6LinkLocal
                && !address.IsIPv6SiteLocal
                && !address.IsIPv6Multicast
                && (bytes[0] & 0xE0) == 0x20
                && !(bytes[0] == 0x20 && bytes[1] == 0x01 && bytes[2] == 0x0D && bytes[3] == 0xB8);
        }

        return false;
    }

    private static async Task<string> ReadLimitedContentAsync(
        HttpContent? content,
        int maximumBytes,
        CancellationToken cancellationToken)
    {
        if (content is null)
        {
            return string.Empty;
        }

        await using var input = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var output = new MemoryStream();
        var buffer = new byte[8192];
        int read;
        while ((read = await input.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            if (output.Length + read > maximumBytes)
            {
                throw new InvalidOperationException("CMSX proxy response exceeded the 1 MB diagnostic limit.");
            }

            await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
        }

        return Encoding.UTF8.GetString(output.ToArray());
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

        if (IsInvestingRequest(targetUrl) && !string.IsNullOrWhiteSpace(_options.InvestingBaseUrl))
        {
            return _options.InvestingBaseUrl;
        }

        return _options.BaseUrl;
    }

    private bool UsesCmsxProxy(string targetUrl)
    {
        return !(IsBinanceRequest(targetUrl) && !string.IsNullOrWhiteSpace(_options.BinanceBaseUrl))
            && !(IsInvestingRequest(targetUrl) && !string.IsNullOrWhiteSpace(_options.InvestingBaseUrl));
    }

    private void AddProxyAuthentication(HttpRequestMessage message, bool usesCmsxProxy)
    {
        if (string.IsNullOrWhiteSpace(_options.Token))
        {
            return;
        }

        if (usesCmsxProxy)
        {
            message.Headers.TryAddWithoutValidation("X-Content-Management-Key", _options.Token);
            return;
        }

        message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.Token);
    }

    private static bool IsBinanceRequest(string targetUrl)
    {
        return IsHostRequest(targetUrl, "binance.com");
    }

    private static bool IsInvestingRequest(string targetUrl)
    {
        return IsHostRequest(targetUrl, "investing.com");
    }

    private static bool IsHostRequest(string targetUrl, string suffix)
    {
        if (!Uri.TryCreate(targetUrl, UriKind.Absolute, out var targetUri))
        {
            return false;
        }

        return targetUri.Host.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
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

    private sealed class CmsxProxyResponse
    {
        public int StatusCode { get; init; }
        public string? ReasonPhrase { get; init; }
        public string? Body { get; init; }
    }
}
