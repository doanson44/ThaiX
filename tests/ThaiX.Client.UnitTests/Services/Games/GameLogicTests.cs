using ThaiX.Client.Pages.Games;
using ThaiX.Client.Services.Games;

namespace ThaiX.Client.UnitTests.Services.Games;

public sealed class GameLogicTests
{
    [Theory]
    [InlineData(new[] { 2, 2, 2, 2 }, new[] { 4, 4, 0, 0 }, 8)]
    [InlineData(new[] { 2, 0, 2, 4 }, new[] { 4, 4, 0, 0 }, 4)]
    [InlineData(new[] { 4, 2, 2, 0 }, new[] { 4, 4, 0, 0 }, 4)]
    [InlineData(new[] { 2, 4, 8, 16 }, new[] { 2, 4, 8, 16 }, 0)]
    public void MergeLine_ShouldMergeEachPairOnce(int[] line, int[] expected, int expectedGain)
    {
        // Arrange

        // Act
        var (merged, gained) = GameLogic.MergeLine(line);

        // Assert
        merged.Should().Equal(expected);
        gained.Should().Be(expectedGain);
    }

    [Theory]
    [InlineData("CRANE", "CRANE", "ccccc")]
    [InlineData("LEVEL", "APPLE", "ppaaa")]
    [InlineData("SPEED", "ERASE", "pappa")]
    public void WordleStatuses_ShouldMarkRepeatedLettersOnlyWhileUnmatched(string guess, string word, string expected)
    {
        // Arrange

        // Act
        var statuses = GameLogic.WordleStatuses(guess, word);

        // Assert
        statuses.Should().Be(expected);
    }

    [Fact]
    public void RotateClockwise_ShouldTurnPieceAQuarter()
    {
        // Arrange
        int[][] piece = [[1, 0, 0], [1, 1, 1]];

        // Act
        var rotated = GameLogic.RotateClockwise(piece);

        // Assert
        rotated.Should().BeEquivalentTo(new[] { new[] { 1, 1 }, new[] { 1, 0 }, new[] { 1, 0 } },
            options => options.WithStrictOrdering());
    }

    [Fact]
    public void BestTicTacToeMove_ShouldBlockAnImmediateWin()
    {
        // Arrange
        var cells = new char[9];
        cells[0] = 'X';
        cells[1] = 'X';
        cells[4] = 'O';

        // Act
        var move = GameLogic.BestTicTacToeMove(cells);

        // Assert
        move.Should().Be(2);
        cells.Count(cell => cell != '\0').Should().Be(3);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(1, -1)]
    public void ConnectFourWon_ShouldDetectFourInEveryDirection(int rowStep, int columnStep)
    {
        // Arrange
        var board = new int[6, 7];
        for (var step = 0; step < 4; step++)
            board[1 + rowStep * step, 3 + columnStep * step] = 1;

        // Act
        var won = GameLogic.ConnectFourWon(board, 1);

        // Assert
        won.Should().BeTrue();
        GameLogic.ConnectFourWon(board, 2).Should().BeFalse();
    }

    [Theory]
    [InlineData(-5, 0, 0)]
    [InlineData(4, 4, 0)]
    [InlineData(10, 10, 0)]
    [InlineData(16, 10, 6)]
    [InlineData(99, 10, 20)]
    public void PointOnPath_ShouldFollowEachSegmentAndClampToTheEnds(double distance, double expectedX, double expectedY)
    {
        // Arrange
        (double X, double Y)[] path = [(0, 0), (10, 0), (10, 20)];

        // Act
        var point = GameLogic.PointOnPath(path, distance);

        // Assert
        point.Should().Be((expectedX, expectedY));
    }

    [Fact]
    public void Catalog_ShouldMapEveryGameToItsOwnBoardComponent()
    {
        // Arrange

        // Act
        var components = GameCatalog.All.Select(game => game.Component).ToArray();

        // Assert
        components.Should().OnlyHaveUniqueItems();
        components.Should().OnlyContain(component => component.IsSubclassOf(typeof(GameBase)));
    }
}
