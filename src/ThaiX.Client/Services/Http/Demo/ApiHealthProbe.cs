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
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            return (false, $"Health returned {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
