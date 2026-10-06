using Microsoft.Playwright;
using Xunit;

namespace ThaiX.Client.VisualTests;

/// <summary>Smoke: header theme select switches data-bs-theme and persists.</summary>
[Collection("ThaiX Playwright")]
public sealed class ThemeToggleSmokeTests
{
    private readonly AuthFixture _auth;

    public ThemeToggleSmokeTests(AuthFixture auth)
    {
        _auth = auth;
    }

    [Fact]
    public async Task ThemeSelect_SwitchesToLight_AndPersists()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/auth/login", 1280, 800);

            var themeSelect = page.Locator("select[aria-label='Theme'], select[title='Theme']").First;
            await themeSelect.WaitForAsync(Visible());

            var initial = await page.EvaluateAsync<string>(
                "() => document.documentElement.getAttribute('data-bs-theme')");
            Assert.Equal("dark", initial);

            await themeSelect.SelectOptionAsync("light");
            await page.WaitForFunctionAsync(
                "() => document.documentElement.getAttribute('data-bs-theme') === 'light'",
                null,
                new PageWaitForFunctionOptions { Timeout = 10000 });

            var stored = await page.EvaluateAsync<string?>(
                "() => localStorage.getItem('ThaiX-theme')");
            Assert.Equal("light", stored);

            await page.ReloadAsync();
            await page.WaitForFunctionAsync(
                "() => document.documentElement.getAttribute('data-bs-theme') === 'light'",
                null,
                new PageWaitForFunctionOptions { Timeout = 15000 });

            themeSelect = page.Locator("select[aria-label='Theme'], select[title='Theme']").First;
            await themeSelect.WaitForAsync(Visible());
            await themeSelect.SelectOptionAsync("dark");
            await page.WaitForFunctionAsync(
                "() => document.documentElement.getAttribute('data-bs-theme') === 'dark'",
                null,
                new PageWaitForFunctionOptions { Timeout = 10000 });
        }
    }

    private static LocatorWaitForOptions Visible(float timeout = 30000) => new()
    {
        State = WaitForSelectorState.Visible,
        Timeout = timeout
    };
}
