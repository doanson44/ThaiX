using ThaiX.Client.Services.Games;

namespace ThaiX.Client.UnitTests.Services.Games;

public sealed class GameCatalogTests
{
    [Fact]
    public void All_ShouldContainEveryGameOnce()
    {
        // Arrange
        var expectedIds = new[]
        {
            "2048", "minesweeper", "sudoku", "memory-cards", "15-puzzle", "lights-out",
            "snake", "tetris", "flappy", "breakout", "pong", "wordle", "hangman",
            "typing-speed", "tic-tac-toe", "connect-four", "tower-defense"
        };

        // Act
        var actualIds = GameCatalog.All.Select(game => game.Id).ToArray();

        // Assert
        actualIds.Should().OnlyHaveUniqueItems();
        actualIds.Should().BeEquivalentTo(expectedIds);
    }

    [Theory]
    [InlineData("2048", "2048")]
    [InlineData("MINESWEEPER", "minesweeper")]
    [InlineData("Connect-Four", "connect-four")]
    public void Find_ShouldMatchGameIdCaseInsensitively(string input, string expectedId)
    {
        // Arrange

        // Act
        var result = GameCatalog.Find(input);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(expectedId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unknown-game")]
    public void Find_ShouldReturnNull_WhenGameIdIsUnknown(string? gameId)
    {
        // Arrange

        // Act
        var result = GameCatalog.Find(gameId);

        // Assert
        result.Should().BeNull();
    }
}
