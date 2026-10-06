using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ThaiX.Client.VisualTests;

/// <summary>
/// Full-page screenshot baselines (Layer 2) for every route in <see cref="RouteCatalog"/>,
/// captured at desktop and mobile in parallel Playwright workers.
/// </summary>
[Collection("ThaiX Playwright")]
public class VisualRegressionTests
{
    private readonly AuthFixture _auth;
    private readonly ITestOutputHelper _output;
    private static readonly string BaselineDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Baselines");
    private static readonly string DiffDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "TestResults", "Diffs");
    private static readonly bool UpdateBaselines = Environment.GetEnvironmentVariable("UPDATE_BASELINES") == "true";

    public VisualRegressionTests(AuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _output = output;
    }

    private sealed record ScreenshotCase(string Route, bool RequiresAuth, string ViewportName, int Width, int Height);

    [Fact]
    public async Task Screenshots_MatchBaselines()
    {
        var cases = ScreenshotRoutes().ToList();
        _output.WriteLine(
            $"Running {cases.Count} screenshot checks with parallelism={AuthFixture.MaxDegreeOfParallelism}");

        await _auth.RunParallelAsync(
            cases,
            async (item, page) =>
            {
                await _auth.GotoAsync(page, item.Route, item.Width, item.Height);

                var safeName = item.Route == "/" ? "_home" : item.Route.TrimStart('/').Replace('/', '_');
                var fileName = $"{safeName}_{item.ViewportName}.png";
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
                    $"Screenshot mismatch for {item.Route} ({item.ViewportName}). " +
                    $"Diff saved to {diffPath}. Baseline: {baselinePath}. " +
                    $"To accept this change, delete the baseline and re-run with UPDATE_BASELINES=true.");
            },
            item => item.RequiresAuth,
            item => (item.Width, item.Height),
            item => $"{item.Route} @ {item.ViewportName}");
    }

    private static IEnumerable<ScreenshotCase> ScreenshotRoutes()
    {
        var filter = Environment.GetEnvironmentVariable("VISUAL_ROUTE_FILTER");

        foreach (var route in RouteCatalog.PublicRoutes)
        {
            if (!RouteMatchesFilter(route, filter))
            {
                continue;
            }

            foreach (var (name, w, h) in new[] { ("Desktop", 1280, 800), ("Mobile", 375, 812) })
            {
                yield return new ScreenshotCase(route, false, name, w, h);
            }
        }

        foreach (var route in RouteCatalog.AuthenticatedRoutes)
        {
            if (!RouteMatchesFilter(route, filter))
            {
                continue;
            }

            foreach (var (name, w, h) in new[] { ("Desktop", 1280, 800), ("Mobile", 375, 812) })
            {
                yield return new ScreenshotCase(route, true, name, w, h);
            }
        }
    }

    private static bool RouteMatchesFilter(string route, string? filter) =>
        string.IsNullOrWhiteSpace(filter) ||
        route.Contains(filter, StringComparison.OrdinalIgnoreCase);
}
