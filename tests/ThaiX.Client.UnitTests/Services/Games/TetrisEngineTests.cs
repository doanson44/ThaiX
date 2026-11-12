using ThaiX.Client.Services.Games;

namespace ThaiX.Client.UnitTests.Services.Games;

public sealed class TetrisEngineTests
{
    [Fact]
    public void Reset_ShouldStartWithAnEmptyBoardAndDeterministicFirstPiece()
    {
        // Arrange
        var engine = new TetrisEngine(new FixedRandom());

        // Act

        // Assert
        engine.CurrentKind.Should().Be(TetrisPieceKind.I);
        engine.PieceY.Should().Be(-1);
        engine.Score.Should().Be(0);
        engine.Lines.Should().Be(0);
        engine.IsFinished.Should().BeFalse();
        engine.Board.Cast<int>().Should().OnlyContain(value => value == 0);
    }

    [Fact]
    public void SevenBag_ShouldDealEveryPieceOnceBeforeRepeating()
    {
        // Arrange
        var engine = new TetrisEngine(new FixedRandom());
        var dealt = new List<TetrisPieceKind>();

        // Act
        for (var i = 0; i < 7; i++)
        {
            dealt.Add(engine.CurrentKind);
            engine.HardDrop();
        }

        // Assert
        dealt.Should().HaveCount(7);
        dealt.Should().OnlyHaveUniqueItems();
        dealt.Should().BeEquivalentTo(Enum.GetValues<TetrisPieceKind>());
    }

    [Fact]
    public void GhostY_ShouldFindTheLowestValidPosition()
    {
        // Arrange
        var engine = new TetrisEngine(new FixedRandom());

        // Act
        var ghostY = engine.GhostY;

        // Assert
        ghostY.Should().Be(18);
        engine.PieceY.Should().Be(-1);
    }

    [Fact]
    public void HardDrop_ShouldMoveToGhostPositionAndAwardTwoPointsPerCell()
    {
        // Arrange
        var engine = new TetrisEngine(new FixedRandom());
        var expectedDistance = engine.GhostY - engine.PieceY;

        // Act
        engine.HardDrop();

        // Assert
        engine.Score.Should().Be(expectedDistance * 2);
        engine.Board[19, 3].Should().Be((int)TetrisPieceKind.I);
        engine.Board[19, 6].Should().Be((int)TetrisPieceKind.I);
        engine.PieceY.Should().Be(-1);
    }

    [Fact]
    public void RotateClockwise_FourTimes_ShouldRestoreTheOriginalOrientation()
    {
        // Arrange
        var engine = new TetrisEngine(new FixedRandom());
        var original = engine.CurrentShape.Select(row => row.ToArray()).ToArray();

        // Act
        for (var i = 0; i < 4; i++)
            engine.RotateClockwise();

        // Assert
        engine.Rotation.Should().Be(0);
        engine.CurrentShape.Should().BeEquivalentTo(original, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Hold_ShouldAllowOnlyOneSwapUntilTheCurrentPieceLocks()
    {
        // Arrange
        var engine = new TetrisEngine(new FixedRandom());
        var first = engine.CurrentKind;

        // Act
        engine.Hold();
        var heldAfterFirstUse = engine.HeldPiece;
        var currentAfterFirstUse = engine.CurrentKind;
        engine.Hold();

        // Assert
        heldAfterFirstUse.Should().Be(first);
        engine.CurrentKind.Should().Be(currentAfterFirstUse);
        engine.HeldPiece.Should().Be(first);

        // Act
        engine.HardDrop();
        engine.Hold();

        // Assert
        engine.CurrentKind.Should().Be(first);
        engine.HeldPiece.Should().Be(currentAfterFirstUse);
    }

    [Fact]
    public void Advance_GravityInterval_ShouldMovePieceDownOneCell()
    {
        // Arrange
        var engine = new TetrisEngine(new FixedRandom());
        var initialY = engine.PieceY;

        // Act
        engine.Advance(TimeSpan.FromMilliseconds(800));

        // Assert
        engine.PieceY.Should().Be(initialY + 1);
    }

    [Fact]
    public void Advance_NegativeElapsedTime_ShouldNotChangeThePiece()
    {
        // Arrange
        var engine = new TetrisEngine(new FixedRandom());
        var initialY = engine.PieceY;

        // Act
        engine.Advance(TimeSpan.FromMilliseconds(-100));

        // Assert
        engine.PieceY.Should().Be(initialY);
        engine.IsFinished.Should().BeFalse();
    }

    private sealed class FixedRandom : Random
    {
        public override int Next(int maxValue) => 0;
    }
}
