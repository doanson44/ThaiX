namespace ThaiX.Infrastructure.ExternalApis.Options;

/// <summary>
/// Root configuration for external APIs: global defaults and per-provider endpoint policies.
/// </summary>
public sealed class ExternalApisOptions
{
    public const string SectionName = "ExternalApis";

    public bool UseRealApi { get; set; } = true;
    public bool UseProxy { get; set; }
    public int MaxConcurrentPerHost { get; set; } = 10;
    public Dictionary<string, ProviderOptions> Providers { get; set; } = new();

    public EndpointOptions GetEndpoint(string providerName, string endpointName)
    {
        if (!Providers.TryGetValue(providerName, out var provider) ||
            !provider.Endpoints.TryGetValue(endpointName, out var endpoint))
        {
            throw new InvalidOperationException(
                $"External API endpoint '{providerName}:{endpointName}' is not configured.");
        }

        var defaults = provider.Defaults;
        return new EndpointOptions
        {
            Url = endpoint.Url,
            UseCache = endpoint.UseCache ?? defaults.UseCache ?? false,
            CacheDurationSeconds = endpoint.CacheDurationSeconds ?? defaults.CacheDurationSeconds ?? 0,
            UseProxy = endpoint.UseProxy ?? defaults.UseProxy ?? UseProxy,
            UseBrowser = endpoint.UseBrowser ?? defaults.UseBrowser ?? false,
            TimeoutSeconds = endpoint.TimeoutSeconds ?? defaults.TimeoutSeconds ?? 30
        };
    }

    /// <summary>Resolves the configured content source for a provider. Defaults to <see cref="TcbsSyncSource.Stream"/>.</summary>
    public TcbsSyncSource GetProviderSource(string providerName)
    {
        return Providers.TryGetValue(providerName, out var provider) && provider.Source.HasValue
            ? provider.Source.Value
            : TcbsSyncSource.Stream;
    }
}

public sealed class ProviderOptions
{
    /// <summary>Which content source to read. Null falls back to <see cref="TcbsSyncSource.Stream"/>.</summary>
    public TcbsSyncSource? Source { get; set; }

    public EndpointPolicyOptions Defaults { get; set; } = new();
    public Dictionary<string, EndpointDefinitionOptions> Endpoints { get; set; } = new();
}

public class EndpointPolicyOptions
{
    public bool? UseCache { get; set; }
    public int? CacheDurationSeconds { get; set; }
    public bool? UseProxy { get; set; }

    /// <summary>Render the endpoint in the headless browser instead of a plain HTTP client.</summary>
    public bool? UseBrowser { get; set; }

    public int? TimeoutSeconds { get; set; }
}

public sealed class EndpointDefinitionOptions : EndpointPolicyOptions
{
    public string Url { get; set; } = string.Empty;
}
