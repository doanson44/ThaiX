namespace ThaiX.Client.Services.Http.Demo;

internal static class ApiHealthProbe
{
    public static async Task<(bool IsOnline, string? FailureReason)> ProbeAsync(
        string apiBaseUrl,
        int timeoutSeconds,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            return (false, "Api:BaseUrl is empty.");
        }

        try
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri(apiBaseUrl.TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(Math.Clamp(timeoutSeconds, 1, 30))
            };

            using var response = await client.GetAsync("health", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return (false, $"Health returned {(int)response.StatusCode}.");
            }

            // A static host (SPA fallback / DevServer) answers every path with the app shell,
            // so an HTML 200 must not be mistaken for a live API.
            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (string.Equals(mediaType, "text/html", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Health endpoint returned HTML instead of an API response.");
            }

            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
