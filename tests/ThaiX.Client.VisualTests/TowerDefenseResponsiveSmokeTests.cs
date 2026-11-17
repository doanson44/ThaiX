using Microsoft.Playwright;
using Xunit;

namespace ThaiX.Client.VisualTests;

[Collection("ThaiX Playwright")]
public sealed class TowerDefenseResponsiveSmokeTests
{
    private readonly AuthFixture _auth;

    public TowerDefenseResponsiveSmokeTests(AuthFixture auth)
    {
        _auth = auth;
    }

    [Fact]
    public async Task TowerDefense_PortraitViewport_KeepsArsenalScrollableAndBoardUsable()
    {
        // Arrange
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 375, 812);
        await using (context)
        {
            await _auth.GotoAsync(page, "/games/tower-defense", 375, 812, networkIdleTimeoutMs: 0);
            await page.GetByRole(AriaRole.Button, new() { Name = "Deploy" }).ClickAsync();
            var board = page.Locator(".td-board");
            var arsenal = page.Locator(".td-towers");
            await board.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            // Act
            var arsenalClientWidth = await arsenal.EvaluateAsync<int>("element => element.clientWidth");
            var arsenalScrollWidth = await arsenal.EvaluateAsync<int>("element => element.scrollWidth");
            var arsenalTouchAction = await arsenal.EvaluateAsync<string>(
                "element => getComputedStyle(element).touchAction");
            var canvasTouchAction = await page.Locator(".game-canvas").EvaluateAsync<string>(
                "element => getComputedStyle(element).touchAction");
            var firstTowerName = await arsenal.Locator(".td-name").First.EvaluateAsync<string>(
                "element => getComputedStyle(element).display");

            // Assert
            Assert.True(arsenalScrollWidth > arsenalClientWidth,
                "The portrait arsenal should overflow horizontally so every tower remains reachable.");
            Assert.Equal("pan-x", arsenalTouchAction);
            Assert.Equal("auto", canvasTouchAction);
            Assert.NotEqual("none", firstTowerName);
        }
    }

    [Fact]
    public async Task TowerDefense_HudOverlay_AlignsWithBoard()
    {
        // Arrange
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/games/tower-defense", 1280, 800, networkIdleTimeoutMs: 0);
            await page.GetByRole(AriaRole.Button, new() { Name = "Deploy" }).ClickAsync();
            var board = page.Locator(".td-board");
            var hud = page.Locator(".td-hud");
            await hud.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            // Act
            var boardBox = await board.BoundingBoxAsync();
            var hudBox = await hud.BoundingBoxAsync();

            // Assert
            Assert.NotNull(boardBox);
            Assert.NotNull(hudBox);
            Assert.InRange(Math.Abs(hudBox.X - boardBox.X), 0, 1);
            Assert.InRange(Math.Abs(hudBox.Y - boardBox.Y), 0, 1);
            Assert.InRange(Math.Abs(hudBox.Width - boardBox.Width), 0, 1);
            Assert.InRange(Math.Abs(hudBox.Height - boardBox.Width / 12), 0, 2);
        }
    }

    [Fact]
    public async Task TowerDefense_LandscapeViewport_UsesTwoColumnBattleLayout()
    {
        // Arrange
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 844, 390);
        await using (context)
        {
            await _auth.GotoAsync(page, "/games/tower-defense", 844, 390, networkIdleTimeoutMs: 0);
            await page.GetByRole(AriaRole.Button, new() { Name = "Deploy" }).ClickAsync();
            var layout = page.Locator(".td-match-layout");
            await layout.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            // Act
            var gridAreas = await layout.EvaluateAsync<string>(
                "element => getComputedStyle(element).gridTemplateAreas");
            var canvasBox = await page.Locator(".game-canvas").BoundingBoxAsync();

            // Assert
            Assert.Contains("board actions", gridAreas, StringComparison.Ordinal);
            Assert.Contains("towers info", gridAreas, StringComparison.Ordinal);
            Assert.NotNull(canvasBox);
            Assert.True(canvasBox.Width <= 844);
        }
    }
}
