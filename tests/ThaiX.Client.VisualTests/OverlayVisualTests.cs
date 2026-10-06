using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ThaiX.Client.VisualTests;

/// <summary>
/// Opens Radzen form dialogs, confirm prompts, and toast notifications, then asserts
/// visibility + dark theme (Layer 2 overlays — complements route-only sweeps).
/// </summary>
[Collection("ThaiX Playwright")]
public sealed class OverlayVisualTests
{
    private readonly AuthFixture _auth;
    private readonly ITestOutputHelper _output;

    public OverlayVisualTests(AuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _output = output;
    }

    [Fact]
    public async Task Overlays_Open_AndKeepDarkTheme()
    {
        var cases = OverlayCatalog.Cases.ToList();
        _output.WriteLine(
            $"Running {cases.Count} overlay checks with parallelism={AuthFixture.MaxDegreeOfParallelism}");

        await _auth.RunParallelAsync(
            cases,
            async (item, page) =>
            {
                await OverlayOpenHelper.ExecuteAsync(page, _auth, item);

                var isDarkTheme = await page.EvaluateAsync<bool>(
                    "() => document.documentElement.getAttribute('data-bs-theme') === 'dark'");
                Assert.True(isDarkTheme, $"data-bs-theme=\"dark\" missing after opening overlay '{item.Id}'.");

                if (item.Kind == OverlayCatalog.OverlayKind.FormDialog)
                {
                    var dialogBox = page.Locator(item.AssertSelector).First;
                    await dialogBox.WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 60000
                    });
                    var box = await dialogBox.BoundingBoxAsync();
                    Assert.NotNull(box);
                    Assert.True(box.Width > 80, $"Dialog '{item.Id}' rendered too narrow ({box.Width}px).");
                    Assert.True(box.Height > 40, $"Dialog '{item.Id}' rendered too short ({box.Height}px).");
                }
            },
            item => item.RequiresAuth,
            item => (item.Width, item.Height),
            item => $"{item.Kind}/{item.Id} ({item.ComponentName})");
    }
}
