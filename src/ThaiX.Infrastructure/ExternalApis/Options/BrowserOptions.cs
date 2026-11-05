namespace ThaiX.Infrastructure.ExternalApis.Options;

/// <summary>
/// Configuration for the headless browser used by endpoints protected by bot challenges.
/// Bound from ExternalApis:Browser.
/// </summary>
public sealed class BrowserOptions
{
    public const string SectionName = "ExternalApis:Browser";

    /// <summary>Run the browser headless. Disable only for local diagnostics.</summary>
    public bool Headless { get; set; } = true;

    /// <summary>
    /// Browser channel to launch. Empty/null uses the Playwright-bundled Chromium;
    /// "msedge" or "chrome" uses the system browser (no browser download needed).
    /// </summary>
    public string? Channel { get; set; }

    /// <summary>Accept-language locale applied to each browser context.</summary>
    public string Locale { get; set; } = "vi-VN";

    /// <summary>
    /// User agent sent by the browser context. Leaves the Playwright default (HeadlessChrome) when empty,
    /// which Cloudflare rejects, so a real desktop UA is used by default.
    /// </summary>
    public string UserAgent { get; set; } =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/156.0.0.0 Safari/537.36 Edg/156.0.0.0";

    /// <summary>Accept-Language header sent by the browser context.</summary>
    public string AcceptLanguage { get; set; } = "vi-VN,vi;q=0.9,en;q=0.8";

    /// <summary>Timeout for a full page load.</summary>
    public int NavigationTimeoutSeconds { get; set; } = 45;

    /// <summary>Timeout for waiting on the marker selector that proves the page rendered.</summary>
    public int WaitForSelectorTimeoutSeconds { get; set; } = 25;

    /// <summary>
    /// Release the browser (and its driver process) after this many seconds without a fetch.
    /// The next fetch relaunches it (~2s). Zero keeps the browser alive for the process lifetime.
    /// </summary>
    public int IdleTimeoutSeconds { get; set; } = 60;
}
