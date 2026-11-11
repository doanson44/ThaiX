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
        "typing-speed", "tic-tac-toe", "connect-four", "tower-defense"
    ];

    private readonly AuthFixture _auth;

    public GamesSmokeTests(AuthFixture auth)
    {
        _auth = auth;
    }

    [Fact]
    public async Task GamesHub_ShowsEveryGame_AsGuest()
    {
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            await _auth.GotoAsync(page, "/games", 1280, 800);
            await page.GetByRole(AriaRole.Heading, new() { Name = "Games" }).First.WaitForAsync(Visible());

            var cards = page.Locator(".game-card");
            await cards.First.WaitForAsync(Visible());
            Assert.Equal(GameIds.Length, await cards.CountAsync());
        }
    }

    private static readonly (int Width, int Height)[] Viewports =
    [
        (375, 812),
        (768, 1024),
        (1280, 800)
    ];

    [Theory]
    [MemberData(nameof(GameRouteData))]
    public async Task GamePlayer_LoadsCanvasAndFitsEveryViewport_AsGuest(string gameId)
    {
        // One navigation per game: every full page load re-boots the WASM app in Docker,
        // so the viewport checks resize the already loaded page instead of reloading it.
        var (context, page) = await _auth.CreatePageAsync(authenticated: false, 1280, 800);
        await using (context)
        {
            var pageErrors = new List<string>();
            page.PageError += (_, error) => pageErrors.Add(error);

            await _auth.GotoAsync(page, $"/games/{gameId}", 1280, 800, networkIdleTimeoutMs: 0);
            var canvas = page.Locator(".game-canvas");
            await canvas.WaitForAsync(Visible());
            await page.GetByText("How to play").First.WaitForAsync(Visible());

            var desktopBox = await canvas.BoundingBoxAsync();
            Assert.NotNull(desktopBox);
            Assert.True(desktopBox.Width > 100, $"Game canvas for '{gameId}' did not render.");

            await canvas.FocusAsync();
            await page.Keyboard.PressAsync("ArrowLeft");
            await page.Keyboard.PressAsync("ArrowRight");
            await page.Keyboard.PressAsync("Enter");

            foreach (var (width, height) in Viewports)
            {
                await page.SetViewportSizeAsync(width, height);
                await page.WaitForTimeoutAsync(150);

                var box = await canvas.BoundingBoxAsync();
                Assert.NotNull(box);
                Assert.True(box.Width > 100, $"Game canvas for '{gameId}' is too narrow at {width}px.");
                Assert.True(box.Width <= width, $"Canvas width {box.Width}px exceeds viewport {width}px.");
                Assert.True(await page.EvaluateAsync<int>("document.documentElement.scrollWidth") <= width,
                    $"Page content exceeds viewport {width}px for '{gameId}'.");
            }

            Assert.Empty(pageErrors);
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
