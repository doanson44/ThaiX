using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace ThaiX.Client.VisualTests;

public sealed class GamesHubCoverageTests
{
    private static readonly string[] ExpectedGameIds =
    [
        "2048", "minesweeper", "sudoku", "memory-cards", "15-puzzle", "lights-out",
        "snake", "tetris", "flappy", "breakout", "pong", "wordle", "hangman",
        "typing-speed", "tic-tac-toe", "connect-four"
    ];

    [Fact]
    public void GamesHub_ShouldExposeAllSixteenGames()
    {
        // Arrange
        var root = RepoRoot.Resolve();
        var catalogPath = Path.Combine(root, "src", "ThaiX.Client", "Services", "Games", "GameCatalog.cs");

        // Act
        var source = File.ReadAllText(catalogPath);
        var actual = Regex.Matches(source, @"new\(""([^""]+)""")
            .Select(match => match.Groups[1].Value)
            .OrderBy(value => value)
            .ToArray();

        // Assert
        Assert.Equal(ExpectedGameIds.OrderBy(value => value), actual);
    }

    [Fact]
    public void GameEngine_ShouldHaveAnImplementationForEveryCatalogEntry()
    {
        // Arrange
        var root = RepoRoot.Resolve();
        var enginePath = Path.Combine(root, "src", "ThaiX.Client", "wwwroot", "js", "games.js");

        // Act
        var source = File.ReadAllText(enginePath);
        var actual = Regex.Matches(source, @"case ""([^""]+)""")
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value)
            .ToArray();

        // Assert
        Assert.Equal(ExpectedGameIds.OrderBy(value => value), actual);
    }

    [Fact]
    public void GameRenderer_ShouldHaveAnImplementationForEveryCatalogEntry()
    {
        // Arrange
        var root = RepoRoot.Resolve();
        var rendererPath = Path.Combine(root, "src", "ThaiX.Client", "wwwroot", "js", "game-renderers.js");

        // Act
        var source = File.ReadAllText(rendererPath);
        var actual = Regex.Matches(source, @"case ""([^""]+)""")
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(value => value)
            .ToArray();

        // Assert
        Assert.Equal(ExpectedGameIds.OrderBy(value => value), actual);
    }

    [Fact]
    public void GamesNavigation_ShouldBePublicAndHaveAHubRoute()
    {
        // Arrange
        var root = RepoRoot.Resolve();
        var navPath = Path.Combine(root, "src", "ThaiX.Client", "Layout", "NavMenu.razor");
        var hubPath = Path.Combine(root, "src", "ThaiX.Client", "Pages", "Games", "GamesHub.razor");
        var playerPath = Path.Combine(root, "src", "ThaiX.Client", "Pages", "Games", "GamePlayer.razor");

        // Act
        var nav = File.ReadAllText(navPath);
        var hub = File.ReadAllText(hubPath);
        var player = File.ReadAllText(playerPath);

        // Assert
        Assert.Contains("Path=\"games\"", nav, StringComparison.Ordinal);
        Assert.Contains("@page \"/games\"", hub, StringComparison.Ordinal);
        Assert.Contains("@page \"/games/{GameId}\"", player, StringComparison.Ordinal);
    }

    [Fact]
    public void GameResources_ShouldBeValidAndHaveMatchingEnglishAndVietnameseKeys()
    {
        // Arrange
        var root = RepoRoot.Resolve();
        var localization = Path.Combine(root, "src", "ThaiX.Client", "Localization");
        var englishPath = Path.Combine(localization, "SharedResource.resx");
        var vietnamesePath = Path.Combine(localization, "SharedResource.vi.resx");
        var resourceKeysPath = Path.Combine(root, "src", "ThaiX.Client", "Constants", "ResourceKeys.cs");

        // Act
        var english = XDocument.Load(englishPath).Descendants("data")
            .Select(element => (string?)element.Attribute("name"))
            .Where(name => name?.StartsWith("Games.", StringComparison.Ordinal) == true)
            .OrderBy(name => name)
            .ToArray();
        var resourceKeys = Regex.Matches(File.ReadAllText(resourceKeysPath), @"""(Games\.[^""]+)""")
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name)
            .ToArray();
        var vietnamese = XDocument.Load(vietnamesePath).Descendants("data")
            .Select(element => (string?)element.Attribute("name"))
            .Where(name => name?.StartsWith("Games.", StringComparison.Ordinal) == true)
            .OrderBy(name => name)
            .ToArray();

        // Assert
        Assert.NotEmpty(english);
        Assert.Equal(english, vietnamese);
        Assert.All(resourceKeys, key => Assert.Contains(key, english));
    }
}
