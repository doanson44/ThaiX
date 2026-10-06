using Microsoft.Playwright;
using Xunit;

namespace ThaiX.Client.VisualTests;

/// <summary>Smoke: Dev Tools hub and host pages load anonymously.</summary>
[Collection("ThaiX Playwright")]
public sealed class DevToolsSmokeTests
{
    private readonly AuthFixture _auth;

    public DevToolsSmokeTests(AuthFixture auth)
    {
        _auth = auth;
    }

    [Fact]
    public async Task ToolsHub_Loads_WhenAnonymous()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/tools", 1280, 800);

            await page.GetByRole(AriaRole.Heading, new() { Name = "Tools" }).First.WaitForAsync(Visible());
            await page.GetByText("Developer").First.WaitForAsync(Visible(15000));
            await page.GetByText("JSON Formatter").First.WaitForAsync(Visible(15000));
            await page.GetByText("Position Size").First.WaitForAsync(Visible(15000));
        }
    }

    [Fact]
    public async Task ToolHost_PositionSize_LoadsCalculator_WhenAnonymous()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/tools/position-size", 1280, 800);

            await page.GetByText("Position Size").First.WaitForAsync(Visible());
            await page.GetByText("Calculate").First.WaitForAsync(Visible(15000));
            await page.GetByText("Results").First.WaitForAsync(Visible(15000));
            await page.GetByText("Clear draft").First.WaitForAsync(Visible(15000));
        }
    }

    [Fact]
    public async Task ToolHost_JsonFormatter_Loads_WhenAnonymous()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/tools/json-formatter", 1280, 800);

            await page.GetByText("JSON Formatter").First.WaitForAsync(Visible());
            await page.Locator("textarea, .rz-textarea").First.WaitForAsync(Visible(15000));
            await page.GetByText("Clear draft").First.WaitForAsync(Visible(15000));
            await page.GetByText("Pretty").First.WaitForAsync(Visible(15000));
        }
    }

    [Fact]
    public async Task ToolHost_JsonFormatter_PersistsAnonymousDraft_AcrossReload()
    {
        const string marker = "ThaiX-draft-smoke-42";
        const string storageKey = "ThaiX.tools.draft.json-formatter";

        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/tools/json-formatter", 1280, 800);

            var input = page.Locator(".ThaiX-mono-editor__input").First;
            await input.WaitForAsync(Visible(15000));
            await input.FillAsync(marker);
            await page.WaitForTimeoutAsync(600);

            var stored = await page.EvaluateAsync<string?>(
                $"(key) => localStorage.getItem(key)",
                storageKey);
            Assert.Contains(marker, stored ?? string.Empty, StringComparison.Ordinal);

            await page.ReloadAsync();
            await page.GetByText("JSON Formatter").First.WaitForAsync(Visible(15000));
            await input.WaitForAsync(Visible(15000));

            var restored = await input.InputValueAsync();
            Assert.Contains(marker, restored, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task ToolHost_MarkdownPreview_ShowsPreviewPane()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/tools/markdown-preview", 1280, 800);
            await page.GetByText("Markdown Preview").First.WaitForAsync(Visible());
            await page.Locator(".ThaiX-md-editor").First.WaitForAsync(Visible(15000));
            await page.Locator(".ThaiX-md-editor__toolbar").First.WaitForAsync(Visible());
            await page.Locator(".ThaiX-md-editor__page").First.WaitForAsync(Visible());
            await page.Locator(".ThaiX-md-preview").First.WaitForAsync(Visible(15000));

            await page.GetByRole(AriaRole.Button, new() { Name = "Preview" }).First.ClickAsync();
            await page.Locator(".ThaiX-md-preview--focus").First.WaitForAsync(Visible(15000));
            await page.GetByRole(AriaRole.Button, new() { Name = "Edit" }).First.WaitForAsync(Visible());

            await page.GetByRole(AriaRole.Button, new() { Name = "Preview" }).First.ClickAsync();
            await page.Locator(".ThaiX-md-editor").First.WaitForAsync(Visible(15000));
            await page.Locator(".ThaiX-md-preview").First.WaitForAsync(Visible(15000));
            await page.Locator(".ThaiX-md-preview--focus").WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });

            await page.GetByRole(AriaRole.Button, new() { Name = "Edit" }).First.ClickAsync();
            await page.Locator(".ThaiX-md-editor").First.WaitForAsync(Visible(15000));
            await page.Locator(".ThaiX-md-preview").First.WaitForAsync(Visible(15000));
        }
    }

    private static LocatorWaitForOptions Visible(float timeout = 30000) => new()
    {
        State = WaitForSelectorState.Visible,
        Timeout = timeout
    };
}
