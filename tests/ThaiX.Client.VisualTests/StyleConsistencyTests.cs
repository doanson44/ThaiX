using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ThaiX.Client.VisualTests;

/// <summary>
/// Full-system style-consistency sweep (Layer 2). Exercises every routed page in
/// ThaiX.Client at desktop/tablet/mobile in parallel Playwright workers.
/// </summary>
[Collection("ThaiX Playwright")]
public class StyleConsistencyTests
{
    private readonly AuthFixture _auth;
    private readonly ITestOutputHelper _output;

    public StyleConsistencyTests(AuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _output = output;
    }

    private sealed record RouteCase(string Route, bool RequiresAuth, string ViewportName, int Width, int Height);

    [Fact]
    public async Task Routes_NoHorizontalScroll_DarkThemeActive_NoUncaughtErrors()
    {
        var cases = RouteCatalog.AllRoutesAllViewports()
            .Select(row => new RouteCase(
                (string)row[0],
                (bool)row[1],
                (string)row[2],
                (int)row[3],
                (int)row[4]))
            .ToList();

        _output.WriteLine(
            $"Running {cases.Count} style checks with parallelism={AuthFixture.MaxDegreeOfParallelism}");

        await _auth.RunParallelAsync(
            cases,
            async (item, page) =>
            {
                var uncaughtErrors = new List<string>();
                var consoleErrors = new List<string>();

                void OnPageError(object? _, string error) => uncaughtErrors.Add(error);
                void OnConsole(object? _, IConsoleMessage msg)
                {
                    if (msg.Type == "error")
                    {
                        consoleErrors.Add(msg.Text);
                    }
                }

                page.PageError += OnPageError;
                page.Console += OnConsole;

                try
                {
                    await _auth.GotoAsync(page, item.Route, item.Width, item.Height);

                    var hasHorizontalScroll = await page.EvaluateAsync<bool>(
                        "() => document.documentElement.scrollWidth > document.documentElement.clientWidth + 1");
                    var isDarkTheme = await page.EvaluateAsync<bool>(
                        "() => document.documentElement.getAttribute('data-bs-theme') === 'dark'");
                    var hasBlazorErrorUi = await page.EvaluateAsync<bool>(
                        "() => { const el = document.querySelector('#blazor-error-ui'); return !!el && getComputedStyle(el).display !== 'none'; }");

                    if (consoleErrors.Count > 0)
                    {
                        _output.WriteLine(
                            $"[console.error] {item.Route} @ {item.ViewportName}: {string.Join(" | ", consoleErrors)}");
                    }

                    Assert.False(
                        hasHorizontalScroll,
                        $"Horizontal scroll detected on {item.Route} @ {item.ViewportName} ({item.Width}x{item.Height}).");
                    Assert.True(
                        isDarkTheme,
                        $"data-bs-theme=\"dark\" missing on <html> for {item.Route} @ {item.ViewportName}.");
                    Assert.False(
                        hasBlazorErrorUi,
                        $"Blazor error UI banner visible on {item.Route} @ {item.ViewportName} — page threw during render.");
                    Assert.Empty(uncaughtErrors);
                }
                finally
                {
                    page.PageError -= OnPageError;
                    page.Console -= OnConsole;
                }
            },
            item => item.RequiresAuth,
            item => (item.Width, item.Height),
            item => $"{item.Route} @ {item.ViewportName}");
    }

    [Fact]
    public async Task PortfolioDetail_ConsistentStyle_AllViewports()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: true, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/portfolio/list", 1280, 800, settleMs: 900);

            var href = await page.EvaluateAsync<string?>(@"
                () => {
                    const links = Array.from(document.querySelectorAll(""a[href^='/portfolio/']""));
                    const detail = links.find(a => {
                        const href = a.getAttribute('href') ?? '';
                        return href !== '/portfolio/list'
                            && !href.includes('price-alerts')
                            && !href.includes('market-scanner');
                    });
                    return detail ? detail.getAttribute('href') : null;
                }");

            if (href is null)
            {
                _output.WriteLine("No portfolios exist in this environment — skipping PortfolioDetail live check.");
                return;
            }

            foreach (var (name, width, height) in RouteCatalog.Viewports)
            {
                await _auth.GotoAsync(page, href, width, height);

                var hasHorizontalScroll = await page.EvaluateAsync<bool>(
                    "() => document.documentElement.scrollWidth > document.documentElement.clientWidth + 1");
                var isDarkTheme = await page.EvaluateAsync<bool>(
                    "() => document.documentElement.getAttribute('data-bs-theme') === 'dark'");

                Assert.False(hasHorizontalScroll, $"Horizontal scroll on portfolio detail @ {name}.");
                Assert.True(isDarkTheme, $"Dark theme missing on portfolio detail @ {name}.");
            }
        }
    }

    [Fact]
    public async Task Pages_DesignTokens_MatchDarkFinancePalette()
    {
        var cases = RouteCatalog.DesignTokenSampleRoutes.ToList();

        await _auth.RunParallelAsync(
            cases,
            async (item, page) =>
            {
                await _auth.GotoAsync(page, item.Route, 1280, 800, settleMs: 500);

                var accent = await page.EvaluateAsync<string>(
                    "() => getComputedStyle(document.documentElement).getPropertyValue('--tx-accent').trim().toLowerCase()");
                var positive = await page.EvaluateAsync<string>(
                    "() => getComputedStyle(document.documentElement).getPropertyValue('--tx-positive').trim().toLowerCase()");
                var negative = await page.EvaluateAsync<string>(
                    "() => getComputedStyle(document.documentElement).getPropertyValue('--tx-negative').trim().toLowerCase()");

                Assert.Equal("#f59e0b", accent);
                Assert.Equal("#10b981", positive);
                Assert.Equal("#f43f5e", negative);
            },
            item => item.RequiresAuth,
            _ => (1280, 800),
            item => item.Route);
    }
}
