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

    [Theory]
    [InlineData(375, 812)]
    [InlineData(768, 1024)]
    [InlineData(1280, 800)]
    public async Task LoginPage_EmailOtpMode_ShouldRemainUsableWithoutHorizontalOverflow(int width, int height)
    {
        // Arrange
        var (context, page) = await _auth.CreatePageAsync(false, width, height, locale: "en-US");

        try
        {
            await _auth.GotoAsync(page, "/auth/login", width, height);

            // Act
            await page.Locator("button:has-text('Email OTP')").ClickAsync();
            await page.Locator("button:has-text('Send verification code')").WaitForAsync(
                new LocatorWaitForOptions { State = WaitForSelectorState.Visible });

            // Assert
            Assert.Equal(0, await page.Locator("input[type='password']").CountAsync());
            Assert.True(await page.Locator("input[type='text']").CountAsync() >= 1);

            var hasHorizontalOverflow = await page.EvaluateAsync<bool>(
                "() => document.documentElement.scrollWidth > document.documentElement.clientWidth + 1");
            Assert.False(hasHorizontalOverflow);

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
