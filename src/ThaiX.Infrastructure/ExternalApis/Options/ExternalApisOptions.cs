namespace ThaiX.Infrastructure.ExternalApis.Options;

/// <summary>
/// Root configuration for external APIs: global defaults and per-provider endpoint policies.
/// </summary>
public sealed class ExternalApisOptions
{
    public const string SectionName = "ExternalApis";

    /// <summary>
    /// When false, integration tests can replace real outbound HTTP with deterministic fakes.
    /// Defaults to true so production and development environments keep real transport behavior.
    /// </summary>
    public bool UseRealApi { get; set; } = true;

    /// <summary>Global default: use proxy when request does not specify UseProxy.</summary>
    public bool UseProxy { get; set; }

    /// <summary>Maximum concurrent requests per host. Default 10 if not configured.</summary>
    public int MaxConcurrentPerHost { get; set; } = 10;

    /// <summary>Per-provider endpoint policies. Key: provider name (e.g. CafeF). Value: endpoint name -> policy.</summary>
    public Dictionary<string, Dictionary<string, EndpointOptions>> Providers { get; set; } = new();
}
