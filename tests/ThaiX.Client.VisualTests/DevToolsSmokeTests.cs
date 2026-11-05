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
    public async Task ToolHost_JsonFormatter_ClearDraft_ShouldRemainEmptyAfterReload()
    {
        const string storageKey = "ThaiX.tools.draft.json-formatter";

        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/tools/json-formatter", 1280, 800);

            var input = page.Locator(".ThaiX-mono-editor__input").First;
            await input.WaitForAsync(Visible(15000));
            await input.FillAsync("{\"clearMe\":true}");
            await page.WaitForTimeoutAsync(600);

            await page.GetByRole(AriaRole.Button, new() { Name = "Clear draft" }).ClickAsync();
            await page.WaitForFunctionAsync(
                "(key) => { const raw = localStorage.getItem(key); if (!raw) return false; const envelope = JSON.parse(raw); const state = JSON.parse(envelope.stateJson); return state.input === ''; }",
                storageKey);

            await page.ReloadAsync();
            await page.GetByText("JSON Formatter").First.WaitForAsync(Visible(15000));
            await input.WaitForAsync(Visible(15000));

            Assert.Equal(string.Empty, await input.InputValueAsync());
        }
    }

    [Theory]
    [InlineData(375, 812)]
    [InlineData(768, 1024)]
    [InlineData(1280, 800)]
    public async Task ToolHost_JsonFormatter_ShouldNotOverflowViewport(int width, int height)
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, width, height);
        await using (context)
        {
            await _auth.GotoAsync(page, "/tools/json-formatter", width, height);

            var horizontalOverflow = await page.EvaluateAsync<int>(
                "() => document.documentElement.scrollWidth - document.documentElement.clientWidth");

            Assert.True(horizontalOverflow <= 0, $"Horizontal overflow detected at {width}px: {horizontalOverflow}px.");
        }
    }

    [Fact]
    public async Task ToolHost_JsonFormatter_CtrlEnter_ShouldRunPrimaryAction()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/tools/json-formatter", 1280, 800);

            var editors = page.Locator(".ThaiX-mono-editor__input");
            await editors.First.WaitForAsync(Visible(15000));
            await editors.First.FillAsync("{\"ok\":true}");
            await editors.First.PressAsync("Control+Enter");

            await page.WaitForTimeoutAsync(300);
            Assert.Contains("\"ok\": true", await editors.Nth(1).InputValueAsync(), StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task ToolHost_JsonFormatter_InvalidJson_ShouldShowLocalizedPosition()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/tools/json-formatter", 1280, 800);

            var input = page.Locator(".ThaiX-mono-editor__input").First;
            await input.WaitForAsync(Visible(15000));
            await input.FillAsync("{\\n  \\"value\\": }");
            await page.GetByRole(AriaRole.Button, new() { Name = "Pretty" }).ClickAsync();

            await page.GetByText("Invalid JSON at line 2, byte column", new() { Exact = false })
                .WaitForAsync(Visible(5000));
        }
    }

    [Fact]
    public async Task ToolHost_Fullscreen_ShouldExitWithEscape()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/tools/json-formatter", 1280, 800);

            await page.GetByRole(AriaRole.Button, new() { Name = "Fullscreen" }).WaitForAsync(Visible(15000));
            await page.GetByRole(AriaRole.Button, new() { Name = "Fullscreen" }).ClickAsync();
            await page.Locator(".ThaiX-tools--fullscreen").WaitForAsync(Visible(5000));

            await page.Keyboard.PressAsync("Escape");
            await page.Locator(".ThaiX-tools--fullscreen").WaitForAsync(
                new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
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
