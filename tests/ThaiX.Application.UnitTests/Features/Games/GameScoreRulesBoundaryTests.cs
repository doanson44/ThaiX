using ThaiX.Application.Features.Games;

namespace ThaiX.Application.UnitTests.Features.Games;

public sealed class GameScoreRulesBoundaryTests
{
    [Theory]
    [InlineData("2048", 0)]
    [InlineData("2048", 1_000_000)]
    [InlineData("minesweeper", 0)]
    [InlineData("minesweeper", 86_400)]
    [InlineData("sudoku", 0)]
    [InlineData("sudoku", 86_400)]
    [InlineData("memory-cards", 6)]
    [InlineData("memory-cards", 10_000)]
    [InlineData("15-puzzle", 0)]
    [InlineData("15-puzzle", 100_000)]
    [InlineData("lights-out", 0)]
    [InlineData("lights-out", 100_000)]
    [InlineData("snake", 0)]
    [InlineData("snake", 3_000)]
    [InlineData("tetris", 0)]
    [InlineData("tetris", 1_000_000)]
    [InlineData("flappy", 0)]
    [InlineData("flappy", 100_000)]
    [InlineData("breakout", 0)]
    [InlineData("breakout", 100_000)]
    [InlineData("pong", 0)]
    [InlineData("pong", 1)]
    [InlineData("wordle", 1)]
    [InlineData("wordle", 7)]
    [InlineData("hangman", 0)]
    [InlineData("hangman", 6)]
    [InlineData("typing-speed", 0)]
    [InlineData("typing-speed", 1_000)]
    [InlineData("tic-tac-toe", 0)]
    [InlineData("tic-tac-toe", 1)]
    [InlineData("connect-four", 0)]
    [InlineData("connect-four", 1)]
    [InlineData("tower-defense", 0)]
    [InlineData("tower-defense", 100_000)]
    public void IsValidScore_ShouldAcceptInclusiveBoundaries_ForEveryGame(string gameId, decimal score)
    {
        // Arrange

        // Act
        var result = GameScoreRules.IsValidScore(gameId, score);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData("2048", -1)]
    [InlineData("2048", 1_000_001)]
    [InlineData("minesweeper", -1)]
    [InlineData("minesweeper", 86_401)]
    [InlineData("sudoku", -1)]
    [InlineData("sudoku", 86_401)]
    [InlineData("memory-cards", 5)]
    [InlineData("memory-cards", 10_001)]
    [InlineData("15-puzzle", -1)]
    [InlineData("15-puzzle", 100_001)]
    [InlineData("lights-out", -1)]
    [InlineData("lights-out", 100_001)]
    [InlineData("snake", -10)]
    [InlineData("snake", 3_010)]
    [InlineData("tetris", -1)]
    [InlineData("tetris", 1_000_001)]
    [InlineData("flappy", -1)]
    [InlineData("flappy", 100_001)]
    [InlineData("breakout", -1)]
    [InlineData("breakout", 100_001)]
    [InlineData("pong", -1)]
    [InlineData("pong", 2)]
    [InlineData("wordle", 0)]
    [InlineData("wordle", 8)]
    [InlineData("hangman", -1)]
    [InlineData("hangman", 7)]
    [InlineData("typing-speed", -1)]
    [InlineData("typing-speed", 1_001)]
    [InlineData("tic-tac-toe", -1)]
    [InlineData("tic-tac-toe", 2)]
    [InlineData("connect-four", -1)]
    [InlineData("connect-four", 2)]
    [InlineData("tower-defense", -10)]
    [InlineData("tower-defense", 100_010)]
    public void IsValidScore_ShouldRejectOutOfRangeValues_ForEveryGame(string gameId, decimal score)
    {
        // Arrange

        // Act
        var result = GameScoreRules.IsValidScore(gameId, score);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("unknown-game")]
    public void IsSupported_ShouldRejectUnknownOrEmptyIdentifiers(string? gameId)
    {
        // Arrange

        // Act
        var result = GameScoreRules.IsSupported(gameId!);

        // Assert
        Assert.False(result);
    }

    [Theory]
    [InlineData("2048")]
    [InlineData("MINESWEEPER")]
    [InlineData("Connect-Four")]
    public void IsSupported_ShouldIgnoreIdentifierCasing(string gameId)
    {
        // Arrange

        // Act
        var result = GameScoreRules.IsSupported(gameId);

        // Assert
        Assert.True(result);
    }
}
