namespace ThaiX.Infrastructure.ExternalApis.Options;

/// <summary>
/// Configuration for the Travis Proxy API.
/// </summary>
public sealed class ProxyOptions
{
    public const string SectionName = "ExternalApis:Proxy";

    public string BaseUrl { get; set; } = string.Empty;
    public string BinanceBaseUrl { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
