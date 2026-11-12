using ThaiX.Client.Services.Games;

namespace ThaiX.Client.UnitTests.Services.Games;

public sealed class FlappyEngineTests
{
    [Fact]
    public void Reset_ShouldInitializeBirdAndFirstPipe()
    {
        // Arrange
        var engine = new FlappyEngine(new Random(1));

        // Act
        engine.Reset();

        // Assert
        engine.BirdY.Should().Be(230);
        engine.BirdVelocity.Should().Be(0);
        engine.Pipes.Should().ContainSingle();
        engine.Pipes[0].X.Should().Be(560);
        engine.Pipes[0].GapCenter.Should().Be(235);
        engine.Score.Should().Be(0);
        engine.HasStarted.Should().BeFalse();
        engine.IsFinished.Should().BeFalse();
    }

    [Fact]
    public void Advance_BeforeFirstFlap_ShouldKeepGameIdle()
    {
        // Arrange
        var engine = new FlappyEngine(new Random(1));

        // Act
        engine.Advance(1);

        // Assert
        engine.BirdY.Should().Be(230);
        engine.Pipes[0].X.Should().Be(560);
        engine.HasStarted.Should().BeFalse();
    }

    [Fact]
    public void Flap_ShouldStartGameAndApplyUpwardImpulse()
    {
        // Arrange
        var engine = new FlappyEngine(new Random(1));

        // Act
        engine.Flap();

        // Assert
        engine.HasStarted.Should().BeTrue();
        engine.BirdVelocity.Should().BeApproximately(-216.66666666666666, 0.0001);
    }

    [Fact]
    public void Advance_OneFixedStep_ShouldApplyGravityAndMovePipeByElapsedTime()
    {
        // Arrange
        var engine = new FlappyEngine(new Random(1));
        engine.Flap();

        // Act
        engine.Advance(1.0 / 60);

        // Assert
        engine.BirdVelocity.Should().BeApproximately(-208.88888888888886, 0.0001);
        engine.BirdY.Should().BeApproximately(226.51851851851853, 0.0001);
        engine.Pipes[0].X.Should().BeApproximately(558.3333333333334, 0.0001);
    }

    [Fact]
    public void Advance_EquivalentElapsedTime_ShouldProduceSameSimulationState()
    {
        // Arrange
        var singleAdvance = new FlappyEngine(new Random(1));
        var splitAdvance = new FlappyEngine(new Random(1));
        singleAdvance.Flap();
        splitAdvance.Flap();

        // Act
        singleAdvance.Advance(1.0 / 30);
        splitAdvance.Advance(1.0 / 60);
        splitAdvance.Advance(1.0 / 60);

        // Assert
        singleAdvance.BirdY.Should().BeApproximately(splitAdvance.BirdY, 0.0001);
        singleAdvance.BirdVelocity.Should().BeApproximately(splitAdvance.BirdVelocity, 0.0001);
        singleAdvance.Pipes[0].X.Should().BeApproximately(splitAdvance.Pipes[0].X, 0.0001);
    }

    [Fact]
    public void Advance_LargeElapsedTime_ShouldLimitCatchUpSteps()
    {
        // Arrange
        var engine = new FlappyEngine(new Random(1));
        engine.Flap();

        // Act
        engine.Advance(1);

        // Assert
        engine.Pipes[0].X.Should().BeApproximately(551.6666666666666, 0.0001);
    }

    [Fact]
    public void Advance_InvalidElapsedTime_ShouldNotChangeState()
    {
        // Arrange
        var engine = new FlappyEngine(new Random(1));
        engine.Flap();
        var initialY = engine.BirdY;
        var initialX = engine.Pipes[0].X;

        // Act
        engine.Advance(double.NaN);
        engine.Advance(double.PositiveInfinity);
        engine.Advance(-1);

        // Assert
        engine.BirdY.Should().Be(initialY);
        engine.Pipes[0].X.Should().Be(initialX);
    }

    [Fact]
    public void Advance_BirdReachingFloor_ShouldFinishRoundAndClampPosition()
    {
        // Arrange
        var engine = new FlappyEngine(new Random(1));
        engine.Flap();

        // Act
        for (var step = 0; step < 240 && !engine.IsFinished; step++)
            engine.Advance(1.0 / 60);

        // Assert
        engine.IsFinished.Should().BeTrue();
        engine.BirdY.Should().Be(FlappyEngine.FloorY - FlappyEngine.BirdRadius);
    }

    [Fact]
    public void Advance_PassingPipe_ShouldAwardOnePoint()
    {
        // Arrange
        var engine = new FlappyEngine(new FixedRandom(0.55));
        engine.Flap();

        // Act
        for (var step = 0; step < 400 && !engine.IsFinished; step++)
        {
            if (step > 0 && step % 56 == 0)
                engine.Flap();

            engine.Advance(1.0 / 60);
        }

        // Assert
        engine.Score.Should().Be(1);
        engine.Pipes.Should().Contain(pipe => pipe.Passed);
    }

    [Fact]
    public void Flap_AfterGameOver_ShouldNotRestartRound()
    {
        // Arrange
        var engine = new FlappyEngine(new Random(1));
        engine.Flap();

        for (var step = 0; step < 240 && !engine.IsFinished; step++)
            engine.Advance(1.0 / 60);

        var finalVelocity = engine.BirdVelocity;

        // Act
        engine.Flap();

        // Assert
        engine.IsFinished.Should().BeTrue();
        engine.BirdVelocity.Should().Be(finalVelocity);
    }

    private sealed class FixedRandom(double value) : Random
    {
        public override double NextDouble() => value;
    }
}
