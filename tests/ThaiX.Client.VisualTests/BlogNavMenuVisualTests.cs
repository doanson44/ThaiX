using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace ThaiX.Client.VisualTests;

/// <summary>
/// Verifies the public Blog list is reachable from the sidebar menu (guest + authenticated).
/// </summary>
[Collection("ThaiX Playwright")]
public sealed class BlogNavMenuVisualTests
{
    private readonly AuthFixture _auth;
    private readonly ITestOutputHelper _output;

    public BlogNavMenuVisualTests(AuthFixture auth, ITestOutputHelper output)
    {
        _auth = auth;
        _output = output;
    }

    [Fact]
    public async Task PublicBlogMenu_NavigatesToBlogList_AsGuest()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/", 1280, 800, settleMs: 700);
            await OpenSidebarIfNeededAsync(page);
            await ClickPublicBlogMenuAsync(page);

            await AuthFixture.WaitForPageReadyAsync(page, "/blog");
            Assert.Contains("/blog", page.Url, StringComparison.OrdinalIgnoreCase);

            await page.Locator("h1, .rz-text-h3, .rz-datalist").First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });

            _output.WriteLine($"Guest navigated to {page.Url}");
        }
    }

    [Fact]
    public async Task PublicBlogMenu_NavigatesToBlogList_WhenAuthenticated()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: true, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/", 1280, 800, settleMs: 700);
            await OpenSidebarIfNeededAsync(page);
            await ClickPublicBlogMenuAsync(page);

            await AuthFixture.WaitForPageReadyAsync(page, "/blog");
            Assert.Contains("/blog", page.Url, StringComparison.OrdinalIgnoreCase);

            await page.Locator(".rz-datalist, h1, .rz-text-h3").First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });

            _output.WriteLine($"Authenticated navigated to {page.Url}");
        }
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
            await menu.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
        }
    }

    private static async Task ClickPublicBlogMenuAsync(IPage page)
    {
        // Top-level public Blog entry (Path="blog") — prefer exact href, then menu text.
        var byHref = page.Locator(".rz-panel-menu a[href='/blog'], .rz-panel-menu a[href='blog'], .rz-navigation-item a[href='/blog'], .rz-navigation-item a[href='blog']");
        if (await byHref.CountAsync() > 0)
        {
            await byHref.First.ClickAsync(new LocatorClickOptions { Timeout = 15000 });
            return;
        }

        var byText = page.Locator(".rz-panel-menu").GetByRole(AriaRole.Link, new LocatorGetByRoleOptions
        {
            Name = "Blog",
            Exact = true
        });

        await byText.First.ClickAsync(new LocatorClickOptions { Timeout = 15000 });
    }
}
