namespace ThaiX.Infrastructure.ExternalApis.Options;

/// <summary>
/// Configuration for external proxy APIs.
/// </summary>
public sealed class ProxyOptions
{
    public const string SectionName = "ExternalApis:Proxy";

    public string BaseUrl { get; set; } = string.Empty;
    public string InvestingBaseUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}