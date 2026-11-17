using Xunit;

namespace ThaiX.Client.VisualTests;

public sealed class TowerDefenseResponsiveLayoutTests
{
    [Fact]
    public void BattleLayout_SeparatesHudAndPreservesBoardViewBox()
    {
        // Arrange
        var root = RepoRoot.Resolve();
        var gamePath = Path.Combine(root, "src", "ThaiX.Client", "Pages", "Games", "TowerDefenseGame.razor");
        var source = File.ReadAllText(gamePath);

        // Act
        var hasBoard = source.Contains("<div class=\"td-board\">", StringComparison.Ordinal);
        var hasHud = source.Contains("<div class=\"td-hud\" role=\"group\"", StringComparison.Ordinal);
        var preservesViewBox = source.Contains("viewBox=\"0 0 480 376\"", StringComparison.Ordinal);

        // Assert
        Assert.True(hasBoard);
        Assert.True(hasHud);
        Assert.True(preservesViewBox);
    }

    [Fact]
    public void PortraitArsenal_ShowsTowerNamesAndSupportsHorizontalTouchScroll()
    {
        // Arrange
        var root = RepoRoot.Resolve();
        var cssPath = Path.Combine(root, "src", "ThaiX.Client", "Pages", "Games", "TowerDefenseGame.razor.css");
        var css = File.ReadAllText(cssPath).Replace("\r\n", "\n");

        // Act
        var hasPortraitBreakpoint = css.Contains(
            "@media (max-width: 767.98px) and (orientation: portrait)", StringComparison.Ordinal);
        var hasHorizontalScroll = css.Contains("overflow-x: auto;", StringComparison.Ordinal);
        var supportsTouchScroll = css.Contains("touch-action: pan-x;", StringComparison.Ordinal);
        var rendersTowerLabels = css.Contains("display: -webkit-box;", StringComparison.Ordinal);
        var hasLargeTouchTarget = css.Contains("min-height: 5.25rem;", StringComparison.Ordinal);

        // Assert
        Assert.True(hasPortraitBreakpoint);
        Assert.True(hasHorizontalScroll);
        Assert.True(supportsTouchScroll);
        Assert.True(rendersTowerLabels);
        Assert.True(hasLargeTouchTarget);
    }

    [Fact]
    public void LandscapeLayout_UsesBoardAndControlColumns()
    {
        // Arrange
        var root = RepoRoot.Resolve();
        var cssPath = Path.Combine(root, "src", "ThaiX.Client", "Pages", "Games", "TowerDefenseGame.razor.css");
        var css = File.ReadAllText(cssPath).Replace("\r\n", "\n");

        // Act
        var hasLandscapeBreakpoint = css.Contains(
            "@media (max-height: 520px) and (orientation: landscape)", StringComparison.Ordinal);
        var hasAdaptiveAreas = css.Contains(
            "\"board actions\"\n            \"towers info\"", StringComparison.Ordinal);

        // Assert
        Assert.True(hasLandscapeBreakpoint);
        Assert.True(hasAdaptiveAreas);
    }

    [Fact]
    public void TowerDefenseCanvas_AllowsArsenalTouchGesturesWithoutChangingOtherGames()
    {
        // Arrange
        var root = RepoRoot.Resolve();
        var gamePlayerPath = Path.Combine(root, "src", "ThaiX.Client", "Pages", "Games", "GamePlayer.razor");
        var gamePlayerCssPath = Path.Combine(root, "src", "ThaiX.Client", "Pages", "Games", "GamePlayer.razor.css");
        var towerDefenseCssPath = Path.Combine(root, "src", "ThaiX.Client", "Pages", "Games", "TowerDefenseGame.razor.css");
        var gamePlayer = File.ReadAllText(gamePlayerPath).Replace("\r\n", "\n");
        var gamePlayerCss = File.ReadAllText(gamePlayerCssPath).Replace("\r\n", "\n");
        var towerDefenseCss = File.ReadAllText(towerDefenseCssPath).Replace("\r\n", "\n");

        // Act
        var hasScopedCanvasClass = gamePlayer.Contains("td-game-canvas", StringComparison.Ordinal);
        var allowsCanvasGestures = gamePlayerCss.Contains(
            ".game-canvas.td-game-canvas {\n    touch-action: auto;", StringComparison.Ordinal);
        var keepsBoardGestures = towerDefenseCss.Contains("touch-action: none;", StringComparison.Ordinal);
        var enablesArsenalScrolling = towerDefenseCss.Contains("touch-action: pan-x;", StringComparison.Ordinal);

        // Assert
        Assert.True(hasScopedCanvasClass);
        Assert.True(allowsCanvasGestures);
        Assert.True(keepsBoardGestures);
        Assert.True(enablesArsenalScrolling);
    }
}
