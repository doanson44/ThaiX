using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Resilience;

namespace ThaiX.Infrastructure.ExternalApis.Transport;

/// <summary>
/// Sends requests directly over HTTP using HttpClient. Timeout and retry are applied by ExternalApiResilienceHandler.
/// </summary>
public sealed class HttpTransport : IApiTransport
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly HttpClient _httpClient;

    public HttpTransport(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T?> SendAsync<T>(ApiRequest request, CancellationToken cancellationToken)
    {
        using var message = BuildRequestMessage(request);

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

    private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36";

    private static HttpRequestMessage BuildRequestMessage(ApiRequest request)
    {
        var uri = BuildUri(request);
        var message = new HttpRequestMessage(request.Method, uri);

        var timeout = TimeSpan.FromSeconds(ResolveTimeout(request));
        message.Options.Set(ExternalApiResilienceHandler.RequestTimeoutKey, timeout);

        if (request.Body != null)
        {
            message.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
        }

        var hasExplicitUserAgent = false;

        if (request.Headers != null)
        {
            foreach (var header in request.Headers)
            {
                if (string.Equals(header.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
                {
                    if (message.Content != null)
                    {
                        message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(header.Value);
                    }

                    continue;
                }

                if (string.Equals(header.Key, "User-Agent", StringComparison.OrdinalIgnoreCase))
                {
                    hasExplicitUserAgent = true;
                }

                message.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        if (!hasExplicitUserAgent)
        {
            message.Headers.TryAddWithoutValidation("User-Agent", DefaultUserAgent);
        }

        return message;
    }

    private static Uri BuildUri(ApiRequest request)
    {
        var url = request.Url ?? string.Empty;
        var withQuery = QueryStringBuilder.AppendQueryString(url, request.QueryParameters);
        return new Uri(withQuery, UriKind.RelativeOrAbsolute);
    }

    private static int ResolveTimeout(ApiRequest request)
    {
        return request.TimeoutSeconds > 0 ? request.TimeoutSeconds : 120;
    }
}
