using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Playwright;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Playwright;

/// <summary>
/// Owns one headless browser process used for pages protected by bot challenges (for example Cloudflare).
/// One browser instance per application process; every call uses a fresh browser context.
/// </summary>
public sealed class BrowserSession : IAsyncDisposable
{
    private static readonly string[] ChallengeMarkers =
    [
        "Just a moment",
        "Chờ một chút",
        "cf_chl_opt"
    ];

    private readonly BrowserOptions _options;
    private readonly ILogger<BrowserSession> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private CancellationTokenSource? _idleCts;

    public BrowserSession(IOptions<BrowserOptions> options, ILogger<BrowserSession> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Loads <paramref name="url"/> in a fresh browser context and returns the rendered HTML.
    /// Waits (bounded) for <paramref name="waitForSelector"/> when provided, so a challenge page has time to resolve.
    /// </summary>
    public async Task<string> GetHtmlAsync(
        string url,
        int timeoutSeconds,
        string? waitForSelector,
        CancellationToken cancellationToken = default)
    {
        CancelIdleRelease();

        await _gate.WaitAsync(cancellationToken);

        try
        {
            const int maxAttempts = 2;

            for (var attempt = 1; ; attempt++)
            {
                var isLastAttempt = attempt >= maxAttempts;

                var browser = await EnsureBrowserAsync(cancellationToken);
                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    Locale = _options.Locale,
                    UserAgent = string.IsNullOrWhiteSpace(_options.UserAgent) ? null : _options.UserAgent,
                    ExtraHTTPHeaders = string.IsNullOrWhiteSpace(_options.AcceptLanguage)
                        ? null
                        : new Dictionary<string, string> { ["Accept-Language"] = _options.AcceptLanguage }
                });

                try
                {
                    var page = await context.NewPageAsync();

                    var response = await page.GotoAsync(url, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.DOMContentLoaded,
                        Timeout = ResolveNavigationTimeoutMs(timeoutSeconds)
                    });

                    if (response is null || !response.Ok)
                    {
                        throw new InvalidOperationException(
                            $"Browser navigation failed for '{url}'. Status={response?.Status.ToString() ?? "none"}. " +
                            "A 403 here means the bot challenge was not passed (check ExternalApis:Browser:UserAgent/Channel).");
                    }

                    var selectorTimedOut = false;

                    if (!string.IsNullOrWhiteSpace(waitForSelector))
                    {
                        try
                        {
                            await page.WaitForSelectorAsync(waitForSelector, new PageWaitForSelectorOptions
                            {
                                State = WaitForSelectorState.Attached,
                                Timeout = Math.Max(1, _options.WaitForSelectorTimeoutSeconds) * 1000
                            });
                        }
                        catch (Exception ex) when (ex is System.TimeoutException or PlaywrightException)
                        {
                            selectorTimedOut = true;
                            _logger.LogWarning(
                                "Browser wait for selector '{Selector}' did not complete (attempt {Attempt}/{MaxAttempts}). Url={Url}, Reason={Reason}",
                                waitForSelector, attempt, maxAttempts, url, ex.Message);
                        }
                    }

                    var html = await page.ContentAsync();

                    if (IsChallengePage(html))
                    {
                        if (!isLastAttempt)
                        {
                            _logger.LogWarning(
                                "Browser received a bot challenge page (attempt {Attempt}/{MaxAttempts}). Url={Url}, Title='{Title}', Length={Length}",
                                attempt, maxAttempts, url, ExtractTitle(html), html.Length);
                        }
                        else
                        {
                            throw new InvalidOperationException(
                                $"Browser received a bot challenge page for '{url}'. Status={response.Status}, Title='{ExtractTitle(html)}', Length={html.Length}. " +
                                "Configure ExternalApis:Browser:UserAgent or Channel (for example 'msedge').");
                        }
                    }
                    else if (!selectorTimedOut)
                    {
                        return html;
                    }
                    else if (isLastAttempt)
                    {
                        _logger.LogWarning(
                            "Browser selector '{Selector}' never appeared, returning the rendered HTML. Url={Url}, Title='{Title}', Length={Length}",
                            waitForSelector, url, ExtractTitle(html), html.Length);
                        return html;
                    }

                    _logger.LogInformation(
                        "Retrying browser fetch. Url={Url}, NextAttempt={Attempt}/{MaxAttempts}", url, attempt + 1, maxAttempts);
                }
                finally
                {
                    await context.CloseAsync();
                }

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
            }
        }
        finally
        {
            _gate.Release();
            ScheduleIdleRelease();
        }
    }

    private void CancelIdleRelease()
    {
        var cts = _idleCts;
        _idleCts = null;
        cts?.Cancel();
        cts?.Dispose();
    }

    private void ScheduleIdleRelease()
    {
        if (_options.IdleTimeoutSeconds <= 0 || _browser is null)
        {
            return;
        }

        CancelIdleRelease();

        var cts = new CancellationTokenSource();
        _idleCts = cts;
        _ = ReleaseAfterIdleAsync(cts);
    }

    private async Task ReleaseAfterIdleAsync(CancellationTokenSource cts)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, _options.IdleTimeoutSeconds)), cts.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        await _gate.WaitAsync().ConfigureAwait(false);

        try
        {
            if (cts.IsCancellationRequested)
            {
                return;
            }

            if (_browser is not null)
            {
                await _browser.DisposeAsync().ConfigureAwait(false);
                _browser = null;
            }

            _playwright?.Dispose();
            _playwright = null;

            _logger.LogInformation(
                "Browser released after {Seconds}s idle", _options.IdleTimeoutSeconds);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to release the idle browser");
        }
        finally
        {
            _gate.Release();
        }
    }

    private static bool IsChallengePage(string html)
    {
        return ChallengeMarkers.Any(marker => html.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static string ExtractTitle(string html)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            html, "<title[^>]*>(?<title>[^<]*)</title>",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return match.Success ? match.Groups["title"].Value.Trim() : "none";
    }

    private int ResolveNavigationTimeoutMs(int timeoutSeconds)
    {
        var configured = timeoutSeconds > 0 ? timeoutSeconds : _options.NavigationTimeoutSeconds;
        return Math.Max(5, Math.Min(configured, _options.NavigationTimeoutSeconds)) * 1000;
    }

    private async Task<IBrowser> EnsureBrowserAsync(CancellationToken cancellationToken)
    {
        if (_browser is { IsConnected: true })
        {
            return _browser;
        }

        if (_browser is not null)
        {
            await _browser.DisposeAsync();
            _browser = null;
        }

        cancellationToken.ThrowIfCancellationRequested();

        _playwright ??= await Microsoft.Playwright.Playwright.CreateAsync();

        var channel = string.IsNullOrWhiteSpace(_options.Channel) ? null : _options.Channel;

        try
        {
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = _options.Headless,
                Channel = channel,
                // Containers run as root on a small /dev/shm, both flags are required there.
                Args =
                [
                    "--disable-blink-features=AutomationControlled",
                    "--no-sandbox",
                    "--disable-dev-shm-usage"
                ]
            });
        }
        catch (PlaywrightException ex)
        {
            _logger.LogError(ex,
                "Failed to launch the browser (channel={Channel}). If this is a fresh deployment run " +
                "'playwright install chromium' or set ExternalApis:Browser:Channel to an installed browser.",
                channel ?? "bundled");
            throw;
        }

        _logger.LogInformation("Browser launched. Channel={Channel} Headless={Headless}", channel ?? "bundled", _options.Headless);
        return _browser;
    }

    public async ValueTask DisposeAsync()
    {
        CancelIdleRelease();

        if (_browser is not null)
        {
            await _browser.DisposeAsync();
            _browser = null;
        }

        _playwright?.Dispose();
        _playwright = null;
        _gate.Dispose();
    }
}
