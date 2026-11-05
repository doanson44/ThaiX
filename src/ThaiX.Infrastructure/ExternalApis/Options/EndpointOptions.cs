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

    /// <summary>When true, the request is rendered by the headless browser (bot-challenge pages).</summary>
    public bool UseBrowser { get; set; }

    public int TimeoutSeconds { get; set; } = 30;
}
