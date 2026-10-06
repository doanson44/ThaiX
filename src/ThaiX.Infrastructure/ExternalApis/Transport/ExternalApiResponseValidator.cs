namespace ThaiX.Infrastructure.ExternalApis.Transport;

internal static class ExternalApiResponseValidator
{
    private const int MaxBodyLength = 1_000;

    public static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = response.Content is null
            ? null
            : await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var message = BuildMessage(response, body);
        throw new HttpRequestException(message, null, response.StatusCode);
    }

    private static string BuildMessage(HttpResponseMessage response, string? body)
    {
        var url = response.RequestMessage?.RequestUri?.ToString() ?? "unknown";
        var trimmedBody = TrimBody(body);

        return string.IsNullOrWhiteSpace(trimmedBody)
            ? $"External API request failed. StatusCode={(int)response.StatusCode} ({response.StatusCode}), ReasonPhrase={response.ReasonPhrase}, Url={url}"
            : $"External API request failed. StatusCode={(int)response.StatusCode} ({response.StatusCode}), ReasonPhrase={response.ReasonPhrase}, Url={url}, Body={trimmedBody}";
    }

    private static string? TrimBody(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        var normalized = body.ReplaceLineEndings(" ").Trim();
        return normalized.Length <= MaxBodyLength
            ? normalized
            : normalized[..MaxBodyLength] + "...";
    }
}
