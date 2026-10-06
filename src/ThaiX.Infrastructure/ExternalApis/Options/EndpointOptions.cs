namespace ThaiX.Infrastructure.ExternalApis.Options;

/// <summary>
/// Configuration for a single external API endpoint (policy: cache, proxy, timeout).
/// </summary>
public sealed class EndpointOptions
{
    /// <summary>Full URL of the endpoint. Query parameters must be included here; do not reconstruct in code.</summary>
    public string Url { get; set; } = string.Empty;

    public bool UseCache { get; set; }

    public int CacheDurationSeconds { get; set; }

    public bool UseProxy { get; set; }

    public int TimeoutSeconds { get; set; } = 30;
}
