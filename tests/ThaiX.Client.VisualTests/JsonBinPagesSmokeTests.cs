using Microsoft.Playwright;
using Xunit;

namespace ThaiX.Client.VisualTests;

/// <summary>
/// Smoke: JsonBins admin + public share pages load under demo/dummy data.
/// </summary>
[Collection("ThaiX Playwright")]
public sealed class JsonBinPagesSmokeTests
{
    private readonly AuthFixture _auth;

    public JsonBinPagesSmokeTests(AuthFixture auth)
    {
        _auth = auth;
    }

    [Fact]
    public async Task JsonBinList_ShowsDemoRows_InDemoMode()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: true, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/admin/json-bins", 1280, 800, settleMs: 1200);

            await ExpectDemoBannerAsync(page);
            await page.GetByText("JSON Bins").First.WaitForAsync(Visible());
            await page.GetByText("Demo JsonBin").First.WaitForAsync(Visible());
            await page.GetByText("DEMO_").First.WaitForAsync(Visible(15000));
        }
    }

    [Fact]
    public async Task JsonBinCreate_LoadsForm_InDemoMode()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: true, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/admin/json-bins/create", 1280, 800);

            await ExpectDemoBannerAsync(page);
            await page.GetByText("Create JSON Bin").First.WaitForAsync(Visible());
            await page.Locator("textarea, .rz-textarea").First.WaitForAsync(Visible(15000));
        }
    }

    [Fact]
    public async Task JsonBinEdit_LoadsDemoDetail_InDemoMode()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: true, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, $"/admin/json-bins/{RouteCatalog.SmokeEntityId}", 1280, 800);

            await ExpectDemoBannerAsync(page);
            await page.GetByText("Edit JSON Bin").First.WaitForAsync(Visible());
            await page.GetByText("Share link").First.WaitForAsync(Visible(15000));
        }
    }

    [Fact]
    public async Task JsonBinPublicShare_ShowsSharedDemoContent()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/public/json-bins/share/smoke-token", 1280, 800);

            await page.GetByText("Shared demo bin").First.WaitForAsync(Visible());
            await page.GetByText("DEMO_SHARED").First.WaitForAsync(Visible(15000));
        }
    }

    private static async Task ExpectDemoBannerAsync(IPage page)
    {
        await page.Locator("text=/Demo mode/i").First.WaitForAsync(Visible());
    }

    private static LocatorWaitForOptions Visible(float timeout = 30000) => new()
    {
        State = WaitForSelectorState.Visible,
        Timeout = timeout
    };
}
