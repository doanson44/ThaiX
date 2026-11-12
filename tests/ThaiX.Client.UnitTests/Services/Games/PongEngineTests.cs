using ThaiX.Client.Services.Games;

namespace ThaiX.Client.UnitTests.Services.Games;

public sealed class PongEngineTests
{
    [Fact]
    public void Reset_ShouldInitializeCenteredBoardAndBaseSpeed()
    {
        // Arrange
        var engine = new PongEngine(new Random(1));

        // Act
        engine.Reset();

        // Assert
        engine.PlayerPaddleY.Should().Be((PongEngine.BoardHeight - PongEngine.PaddleHeight) / 2.0);
        engine.AiPaddleY.Should().Be(engine.PlayerPaddleY);
        engine.BallX.Should().Be(PongEngine.BoardWidth / 2.0);
        engine.BallY.Should().Be(PongEngine.BoardHeight / 2.0);
        engine.BallSpeed.Should().Be(320);
        engine.PlayerScore.Should().Be(0);
        engine.AiScore.Should().Be(0);
        engine.RallyHits.Should().Be(0);
        engine.IsFinished.Should().BeFalse();
    }

    [Fact]
    public void Advance_OneFixedStep_ShouldMoveBallUsingVelocityPerSecond()
    {
        // Arrange
        var engine = new PongEngine(new Random(1));
        var expectedX = engine.BallX + engine.BallVelocityX / 60;

        // Act
        engine.Advance(1.0 / 60, 0);

        // Assert
        engine.BallX.Should().BeApproximately(expectedX, 0.0001);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(1, PongEngine.BoardHeight - PongEngine.PaddleHeight)]
    public void Advance_PlayerPaddle_ShouldStayWithinBoard(int direction, double expectedY)
    {
        // Arrange
        var engine = new PongEngine(new Random(1));

        // Act
        for (var step = 0; step < 100; step++)
            engine.Advance(1.0 / 60, direction);

        // Assert
        engine.PlayerPaddleY.Should().Be(expectedY);
    }

    [Fact]
    public void Advance_LargeElapsedTime_ShouldLimitCatchUpSteps()
    {
        // Arrange
        var engine = new PongEngine(new Random(1));
        var initialX = engine.BallX;

        // Act
        engine.Advance(1, 0);

        // Assert
        (engine.BallX - initialX).Should().BeLessThan(320 * 5.0 / 60);
    }

    [Fact]
    public void Advance_InvalidElapsedTime_ShouldNotChangeState()
    {
        // Arrange
        var engine = new PongEngine(new Random(1));
        var initialX = engine.BallX;
        var initialY = engine.BallY;

        // Act
        engine.Advance(double.NaN, 1);
        engine.Advance(double.PositiveInfinity, -1);
        engine.Advance(-1, 0);

        // Assert
        engine.BallX.Should().Be(initialX);
        engine.BallY.Should().Be(initialY);
        engine.PlayerPaddleY.Should().Be((PongEngine.BoardHeight - PongEngine.PaddleHeight) / 2.0);
    }

    [Fact]
    public void PaddleHit_ShouldIncreaseBallSpeedBySixPercent()
    {
        // Arrange
        var engine = new PongEngine(new Random(1));
        var initialSpeed = engine.BallSpeed;

        // Act
        for (var step = 0; step < 600 && engine.RallyHits < 2; step++)
        {
            var paddleCenter = engine.PlayerPaddleY + PongEngine.PaddleHeight / 2.0;
            var direction = engine.BallY < paddleCenter ? -1 : 1;
            engine.Advance(1.0 / 60, direction);
        }

        // Assert
        engine.RallyHits.Should().Be(2);
        engine.BallSpeed.Should().BeApproximately(initialSpeed * 1.06 * 1.06, 0.001);
    }

    [Fact]
    public void Advance_WhenFinished_ShouldKeepStateUnchanged()
    {
        // Arrange
        var engine = new PongEngine(new Random(1));
        var initialX = engine.BallX;

        // Act
        for (var step = 0; step < 10_000 && !engine.IsFinished; step++)
            engine.Advance(1.0 / 60, 0);

        var finishedX = engine.BallX;
        engine.Advance(1, 1);

        // Assert
        engine.IsFinished.Should().BeTrue();
        engine.BallX.Should().Be(finishedX);
        initialX.Should().Be(PongEngine.BoardWidth / 2.0);
    }
}
