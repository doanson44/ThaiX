using Microsoft.Playwright;
using Xunit;

namespace ThaiX.Client.VisualTests;

[Collection("ThaiX Playwright")]
public sealed class LoginEmailOtpVisualTests
{
    private readonly AuthFixture _auth;

    public LoginEmailOtpVisualTests(AuthFixture auth)
    {
        _auth = auth;
    }

    [Fact]
    public async Task LoginPage_EmailOtpMode_ShouldRemainUsableAtAllBreakpoints()
    {
        // Arrange
        var (context, page) = await _auth.CreatePageAsync(false, 375, 812, locale: "en-US");

        try
        {
            await _auth.GotoAsync(page, "/auth/login", 375, 812);

            // Act
            await page.Locator("button:has-text('Email OTP')").ClickAsync();
            await page.Locator("button:has-text('Send verification code')").WaitForAsync(
                new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            // Assert
            Assert.Equal(0, await page.Locator("input[type='password']").CountAsync());
            Assert.True(await page.Locator("input.rz-textbox").CountAsync() >= 1);

            foreach (var viewport in new[] { (Width: 375, Height: 812), (Width: 768, Height: 1024), (Width: 1280, Height: 800) })
            {
                await page.SetViewportSizeAsync(viewport.Width, viewport.Height);
                await page.WaitForTimeoutAsync(150);

                var hasHorizontalOverflow = await page.EvaluateAsync<bool>(
                    "() => document.documentElement.scrollWidth > document.documentElement.clientWidth + 1");
                Assert.False(
                    hasHorizontalOverflow,
                    $"Email OTP login overflows horizontally at {viewport.Width}px.");
                Assert.True(await page.Locator("button:has-text('Send verification code')").IsVisibleAsync());
            }

            // Switching back must leave the original password login available.
            await page.Locator("button:has-text('Password')").ClickAsync();
            Assert.Equal(1, await page.Locator("input[type='password']").CountAsync());
        }
        finally
        {
            await context.CloseAsync();
        }
    }
}
