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
            await page.Locator(".td-deploy").ClickAsync();

            var board = page.Locator(".td-board");
            var canvas = page.Locator(".game-canvas");
            var arsenal = page.Locator(".td-towers");
            await board.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            // Act
            var arsenalClientWidth = await arsenal.EvaluateAsync<int>("element => element.clientWidth");
            var arsenalScrollWidth = await arsenal.EvaluateAsync<int>("element => element.scrollWidth");
            var arsenalTouchAction = await arsenal.EvaluateAsync<string>(
                "element => getComputedStyle(element).touchAction");
            var boardTouchAction = await board.EvaluateAsync<string>(
                "element => getComputedStyle(element).touchAction");
            var canvasTouchAction = await canvas.EvaluateAsync<string>(
                "element => getComputedStyle(element).touchAction");
            var towerNameCount = await arsenal.Locator(".td-name").CountAsync();
            var firstTowerNameDisplay = await arsenal.Locator(".td-name").First.EvaluateAsync<string>(
                "element => getComputedStyle(element).display");

            // Assert
            Assert.True(arsenalScrollWidth > arsenalClientWidth,
                "The portrait arsenal should overflow horizontally so every tower remains reachable.");
            Assert.Equal("pan-x", arsenalTouchAction);
            Assert.Equal("none", boardTouchAction);
            Assert.Equal("auto", canvasTouchAction);
            Assert.Equal(10, towerNameCount);
            Assert.NotEqual("none", firstTowerNameDisplay);
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
            await page.Locator(".td-deploy").ClickAsync();

            var board = page.Locator(".td-board");
            var hud = page.Locator(".td-hud");
            await hud.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            // Act
            var boardBox = await board.BoundingBoxAsync();
            var hudBox = await hud.BoundingBoxAsync();

            // Assert
            Assert.NotNull(boardBox);
            Assert.NotNull(hudBox);
            Assert.InRange(Math.Abs(hudBox.X - boardBox.X), 0d, 1d);
            Assert.InRange(Math.Abs(hudBox.Y - boardBox.Y), 0d, 1d);
            Assert.InRange(Math.Abs(hudBox.Width - boardBox.Width), 0d, 1d);
            Assert.InRange(Math.Abs(hudBox.Height - boardBox.Width / 12), 0d, 2d);
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
            await page.Locator(".td-deploy").ClickAsync();

            var layout = page.Locator(".td-match-layout");
            await layout.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            // Act
            var gridAreas = await layout.EvaluateAsync<string>(
                "element => getComputedStyle(element).gridTemplateAreas");
            var canvasBox = await page.Locator(".game-canvas").BoundingBoxAsync();

            // Assert
            Assert.True(gridAreas.Contains("board actions", StringComparison.Ordinal));
            Assert.True(gridAreas.Contains("towers info", StringComparison.Ordinal));
            Assert.NotNull(canvasBox);
            Assert.True(canvasBox.Width <= 844);
        }
    }

    [Fact]
    public async Task TowerDefense_TabletViewport_DoesNotOverflowHorizontally()
    {
        // Arrange
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 768, 1024);
        await using (context)
        {
            await _auth.GotoAsync(page, "/games/tower-defense", 768, 1024, networkIdleTimeoutMs: 0);
            await page.Locator(".td-deploy").ClickAsync();

            // Act
            var canvasBox = await page.Locator(".game-canvas").BoundingBoxAsync();
            var documentWidth = await page.EvaluateAsync<int>(
                "document.documentElement.scrollWidth");
            var boardWidth = await page.Locator(".td-board").EvaluateAsync<double>(
                "element => element.getBoundingClientRect().width");

            // Assert
            Assert.NotNull(canvasBox);
            Assert.True(canvasBox.Width <= 768);
            Assert.True(boardWidth <= 768);
            Assert.True(documentWidth <= 768,
                $"The tablet layout overflows the viewport: document width is {documentWidth}px.");
        }
    }

    [Fact]
    public async Task TowerDefense_KeyboardPlacement_CanConfirmAndSelectTower()
    {
        // Arrange
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/games/tower-defense", 1280, 800, networkIdleTimeoutMs: 0);
            await page.Locator(".td-deploy").ClickAsync();

            var canvas = page.Locator(".game-canvas");
            await canvas.FocusAsync();

            // Act
            await page.Keyboard.PressAsync("1");
            await page.Keyboard.PressAsync("Enter");

            var popover = page.Locator(".td-placement-popover");
            await popover.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            var confirmButton = page.Locator(".td-popover-confirm");
            var placementDisabled = await confirmButton.EvaluateAsync<bool>("el => el.classList.contains('td-btn-disabled')");

            await confirmButton.ClickAsync();
            var upgradeActions = page.Locator(".td-upgrades");
            var selectedTowerName = page.Locator(".td-info-head strong");

            // Assert
            Assert.False(placementDisabled);
            await upgradeActions.WaitForAsync(new() { State = WaitForSelectorState.Visible });
            Assert.False(string.IsNullOrWhiteSpace(await selectedTowerName.InnerTextAsync()));
        }
    }
    [Fact]
    public async Task TowerDefense_KeyboardEscape_CancelsPendingPlacement()
    {
        // Arrange
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/games/tower-defense", 1280, 800, networkIdleTimeoutMs: 0);
            await page.Locator(".td-deploy").ClickAsync();

            var canvas = page.Locator(".game-canvas");
            await canvas.FocusAsync();
            await page.Keyboard.PressAsync("1");
            await page.Keyboard.PressAsync("Enter");

            var popover = page.Locator(".td-placement-popover");
            await popover.WaitForAsync(new() { State = WaitForSelectorState.Visible });

            // Act
            await page.Keyboard.PressAsync("Escape");

            // Assert
            await popover.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
            Assert.Equal(0, await popover.CountAsync());
        }
    }

}