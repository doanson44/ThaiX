using Microsoft.Playwright;
using Xunit;

namespace ThaiX.Client.VisualTests;

[Collection("ThaiX Playwright")]
public sealed class GamesSmokeTests
{
    private static readonly string[] GameIds =
    [
        "2048", "minesweeper", "sudoku", "memory-cards", "15-puzzle", "lights-out",
        "snake", "tetris", "flappy", "breakout", "pong", "wordle", "hangman",
        "typing-speed", "tic-tac-toe", "connect-four"
    ];

    private readonly AuthFixture _auth;

    public GamesSmokeTests(AuthFixture auth)
    {
        _auth = auth;
    }

    [Fact]
    public async Task GamesHub_ShowsAllSixteenGames_AsGuest()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/games", 1280, 800);
            await page.GetByRole(AriaRole.Heading, new() { Name = "Games" }).First.WaitForAsync(Visible());

            var cards = page.Locator(".game-card");
            await cards.First.WaitForAsync(Visible());
            Assert.Equal(16, await cards.CountAsync());
        }
    }

    [Theory]
    [MemberData(nameof(GameRouteData))]
    public async Task GameRoute_LoadsCanvasAndInstructions_AsGuest(string gameId)
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, $"/games/{gameId}", 1280, 800);
            await page.Locator(".game-canvas").WaitForAsync(Visible(30000));
            await page.GetByText("How to play").First.WaitForAsync(Visible(30000));
            var canvas = page.Locator(".game-canvas");
            var box = await canvas.BoundingBoxAsync();
            Assert.NotNull(box);
            Assert.True(box.Width > 100, $"Game canvas for '{gameId}' did not render.");
        }
    }

    [Theory]
    [InlineData(375, 812)]
    [InlineData(768, 1024)]
    [InlineData(1280, 800)]
    public async Task GamePlayer_FitsResponsiveViewport(int viewportWidth, int viewportHeight)
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, viewportWidth, viewportHeight);
        await using (context)
        {
            await _auth.GotoAsync(page, "/games/2048", viewportWidth, viewportHeight);
            var canvas = page.Locator(".game-canvas");
            await canvas.WaitForAsync(Visible(30000));
            var box = await canvas.BoundingBoxAsync();
            Assert.NotNull(box);
            Assert.True(box.Width <= viewportWidth, $"Canvas width {box.Width}px exceeds viewport {viewportWidth}px.");
            Assert.True(await page.EvaluateAsync<int>("document.documentElement.scrollWidth") <= viewportWidth,
                $"Page content exceeds viewport {viewportWidth}px.");
        }
    }

    public static IEnumerable<object[]> GameRouteData() =>
        GameIds.Select(gameId => new object[] { gameId });

    private static LocatorWaitForOptions Visible(float timeout = 30000) => new()
    {
        State = WaitForSelectorState.Visible,
        Timeout = timeout
    };
}
