namespace ThaiX.Infrastructure.ExternalApis.Options;

/// <summary>
/// Fully resolved configuration for one external API endpoint.
/// </summary>
public sealed class EndpointOptions
{
    public string Url { get; set; } = string.Empty;
    public bool UseCache { get; set; }
    public int CacheDurationSeconds { get; set; }
    public bool UseProxy { get; set; }
    public int TimeoutSeconds { get; set; } = 30;
}
