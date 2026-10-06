namespace ThaiX.Infrastructure.ExternalApis.Core;

/// <summary>
/// Represents an external HTTP API request.
/// </summary>
public sealed class ApiRequest
{
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public string Url { get; set; } = string.Empty;
    public Dictionary<string, string>? Headers { get; set; }
    public Dictionary<string, string>? QueryParameters { get; set; }
    public string? Body { get; set; }
    public bool UseCache { get; set; }
    public TimeSpan? CacheDuration { get; set; }
    public string? CacheGroup { get; set; }

    /// <summary>When set, overrides global proxy setting. When null, router uses global UseProxy.</summary>
    public bool? UseProxy { get; set; }

    public int TimeoutSeconds { get; set; } = 30;
}
