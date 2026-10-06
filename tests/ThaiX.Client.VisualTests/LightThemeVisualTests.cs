using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ThaiX.Client.VisualTests;

/// <summary>
/// Light-theme sample screenshots + sidebar check. Does not replace dark
/// <see cref="VisualRegressionTests"/> / <see cref="StyleConsistencyTests"/>.
/// </summary>
[Collection("ThaiX Playwright")]
public sealed class LightThemeVisualTests
{
    private readonly AuthFixture _auth;
    private readonly ITestOutputHelper _output;
    private static readonly string BaselineDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Baselines");
    private static readonly string DiffDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TestResults", "Diffs");
    private static readonly bool UpdateBaselines = Environment.GetEnvironmentVariable("UPDATE_BASELINES") == "true";

    private static readonly (string Name, int Width, int Height)[] LightViewports =
    [
        ("Desktop", 1280, 800),
        ("Mobile", 375, 812)
    ];

    public LightThemeVisualTests(AuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _output = output;
    }

    private sealed record LightCase(string Route, bool RequiresAuth, string ViewportName, int Width, int Height);

    [Fact]
    public async Task LightTheme_SampleRoutes_MatchBaselines_AndSidebarVisible()
    {
        var cases = ScreenshotCases().ToList();
        _output.WriteLine(
            $"Running {cases.Count} light-theme screenshot checks with parallelism={AuthFixture.MaxDegreeOfParallelism}");

        await _auth.RunParallelAsync(
            cases,
            async (item, page) =>
            {
                await _auth.GotoAsync(page, item.Route, item.Width, item.Height);
                await EnsureLightThemeAsync(page);

                if (item.Route == "/" && item.ViewportName == "Desktop")
                {
                    await OpenSidebarIfNeededAsync(page);
                    var menu = page.Locator(".rz-panel-menu").First;
                    await menu.WaitForAsync(Visible());
                    Assert.True(await menu.IsVisibleAsync(), "PanelMenu should be visible under light theme.");
                }

                var isLight = await page.EvaluateAsync<bool>(
                    "() => document.documentElement.getAttribute('data-bs-theme') === 'light'");
                Assert.True(isLight, $"data-bs-theme=light missing for {item.Route} @ {item.ViewportName}.");

                var safeName = item.Route == "/" ? "_home" : item.Route.TrimStart('/').Replace('/', '_');
                var fileName = $"{safeName}_Light_{item.ViewportName}.png";
                var baselinePath = Path.Combine(BaselineDir, fileName);

                if (UpdateBaselines)
                {
                    Directory.CreateDirectory(BaselineDir);
                    await page.ScreenshotAsync(new PageScreenshotOptions { Path = baselinePath, FullPage = true });
                    return;
                }

                var screenshot = await page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true });

                if (!File.Exists(baselinePath))
                {
                    Directory.CreateDirectory(BaselineDir);
                    await File.WriteAllBytesAsync(baselinePath, screenshot);
                    Assert.Fail($"Baseline created for {fileName}. Re-run tests to verify.");
                }

                var baseline = await File.ReadAllBytesAsync(baselinePath);
                if (screenshot.Length == baseline.Length && screenshot.AsSpan().SequenceEqual(baseline.AsSpan()))
                {
                    return;
                }

                Directory.CreateDirectory(DiffDir);
                var diffPath = Path.Combine(DiffDir, fileName);
                await File.WriteAllBytesAsync(diffPath, screenshot);
                Assert.Fail(
                    $"Light screenshot mismatch for {item.Route} ({item.ViewportName}). " +
                    $"Diff: {diffPath}. Baseline: {baselinePath}.");
            },
            item => item.RequiresAuth,
            item => (item.Width, item.Height),
            item => $"{item.Route} @ Light/{item.ViewportName}");
    }

    private static IEnumerable<LightCase> ScreenshotCases()
    {
        foreach (var (route, requiresAuth) in RouteCatalog.DesignTokenSampleRoutes)
        {
            foreach (var (name, w, h) in LightViewports)
            {
                yield return new LightCase(route, requiresAuth, name, w, h);
            }
        }
    }

    private static async Task EnsureLightThemeAsync(IPage page)
    {
        await page.EvaluateAsync("() => window.ThaiXTheme && window.ThaiXTheme.set('light')");
        await page.WaitForFunctionAsync(
            "() => document.documentElement.getAttribute('data-bs-theme') === 'light'",
            null,
            new PageWaitForFunctionOptions { Timeout = 10000 });
        // Let Radzen theme stylesheet swap settle before capture.
        await page.WaitForTimeoutAsync(400);
    }

    private static async Task OpenSidebarIfNeededAsync(IPage page)
    {
        var menu = page.Locator(".rz-panel-menu").First;
        if (await menu.IsVisibleAsync())
        {
            return;
        }

        var toggle = page.Locator(".rz-sidebar-toggle, button:has(.rzi-menu), button:has(.rzi:text-is('menu'))").First;
        if (await toggle.CountAsync() > 0)
        {
            await toggle.ClickAsync(new LocatorClickOptions { Timeout = 10000 });
            await menu.WaitForAsync(Visible(15000));
        }
    }

    private static LocatorWaitForOptions Visible(float timeout = 30000) => new()
    {
        State = WaitForSelectorState.Visible,
        Timeout = timeout
    };
}
