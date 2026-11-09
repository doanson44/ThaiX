using Xunit;
using FluentAssertions;
using ThaiX.Application.Features.Games;

namespace ThaiX.Application.UnitTests.Features.Games;

public sealed class GameScoreRulesTests
{
    [Fact]
    public void IsSupported_ShouldReturnTrue_ForAllSixteenGames()
    {
        // Arrange
        var gameIds = new[]
        {
            "2048", "minesweeper", "sudoku", "memory-cards", "15-puzzle", "lights-out",
            "snake", "tetris", "flappy", "breakout", "pong", "wordle", "hangman",
            "typing-speed", "tic-tac-toe", "connect-four"
        };

        // Act
        var results = gameIds.Select(GameScoreRules.IsSupported);

        // Assert
        results.Should().OnlyContain(result => result);
    }

    [Theory]
    [InlineData("minesweeper", 45, 60, true)]
    [InlineData("sudoku", 60, 45, false)]
    [InlineData("memory-cards", 12, 14, true)]
    [InlineData("wordle", 4, 5, true)]
    public void IsBetter_ShouldUseAscendingOrder_ForTimeAndMoveGames(
        string gameId,
        decimal candidate,
        decimal current,
        bool expected)
    {
        // Arrange

        // Act
        var result = GameScoreRules.IsBetter(gameId, candidate, current);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("2048", 4096, 2048, true)]
    [InlineData("snake", 100, 120, false)]
    [InlineData("tetris", 500, 400, true)]
    [InlineData("typing-speed", 75, 70, true)]
    public void IsBetter_ShouldUseDescendingOrder_ForHighScoreGames(
        string gameId,
        decimal candidate,
        decimal current,
        bool expected)
    {
        // Arrange

        // Act
        var result = GameScoreRules.IsBetter(gameId, candidate, current);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("2048", 500)]
    [InlineData("minesweeper", 86_400)]
    [InlineData("sudoku", 120)]
    [InlineData("memory-cards", 6)]
    [InlineData("15-puzzle", 40)]
    [InlineData("lights-out", 20)]
    [InlineData("snake", 120)]
    [InlineData("tetris", 1_000)]
    [InlineData("flappy", 5)]
    [InlineData("breakout", 500)]
    [InlineData("pong", 1)]
    [InlineData("wordle", 5)]
    [InlineData("hangman", 6)]
    [InlineData("typing-speed", 75)]
    [InlineData("tic-tac-toe", 0)]
    [InlineData("connect-four", 1)]
    public void IsValidScore_ShouldAcceptPlausibleScores(string gameId, decimal score)
    {
        // Arrange

        // Act
        var result = GameScoreRules.IsValidScore(gameId, score);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("snake", 125)]
    [InlineData("snake", 3_010)]
    [InlineData("wordle", 8)]
    [InlineData("hangman", 7)]
    [InlineData("connect-four", 2)]
    [InlineData("memory-cards", 5)]
    public void IsValidScore_ShouldRejectImpossibleScores(string gameId, decimal score)
    {
        // Arrange

        // Act
        var result = GameScoreRules.IsValidScore(gameId, score);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("2048", 2048.5)]
    [InlineData("minesweeper", 12.5)]
    [InlineData("memory-cards", 8.5)]
    [InlineData("typing-speed", 75.5)]
    public void IsValidScore_ShouldRejectFractionalMetrics(string gameId, decimal score)
    {
        // Arrange

        // Act
        var result = GameScoreRules.IsValidScore(gameId, score);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("pong")]
    [InlineData("TIC-TAC-TOE")]
    [InlineData("Connect-Four")]
    public void IsCumulativeWinGame_ShouldMatchVersusGames_CaseInsensitively(string gameId)
    {
        // Arrange

        // Act
        var result = GameScoreRules.IsCumulativeWinGame(gameId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void BuildBinCode_ShouldNormalizeGameId()
    {
        // Arrange

        // Act
        var result = GameScoreRules.BuildBinCode("connect-four");

        // Assert
        result.Should().Be("GAMES_LEADERBOARD_CONNECT_FOUR");
    }
}
