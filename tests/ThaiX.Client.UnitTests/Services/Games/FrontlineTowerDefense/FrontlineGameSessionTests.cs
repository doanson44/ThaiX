using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineGameSessionTests
{
    private static FrontlineGameSession CreateSession(FrontlinePlayerProgress? progress = null) =>
        new(
            FrontlineLevelMapCatalog.Find("level-01")!,
            FrontlineWaveCatalog.Find("level-01")!,
            progress ?? FrontlinePlayerProgress.CreateNew(),
            new Random(42));

    [Fact]
    public void Constructor_WithMatchingLevelData_StartsPlayingWithLegacyFunds()
    {
        var session = CreateSession();

        session.State.Should().Be(FrontlineGameSessionState.Playing);
        session.Simulation.Funds.Should().Be(500);
        session.Simulation.LevelHealth.Should().Be(1000);
        session.TotalWaves.Should().Be(24);
        session.CurrentWave.Should().Be(0);
    }

    [Fact]
    public void TryStartNextWave_WhenManualCooldownIsReady_StartsFirstWave()
    {
        var session = CreateSession();

        session.CanStartNextWave.Should().BeTrue();
        session.TryStartNextWave().Should().BeTrue();
        session.CurrentWave.Should().Be(1);
        session.CanStartNextWave.Should().BeFalse();
    }

    [Fact]
    public void Advance_WithZeroElapsedTime_DoesNotAdvanceSimulation()
    {
        var session = CreateSession();

        session.Advance(0);

        session.Simulation.Tick.Should().Be(0);
        session.CurrentWave.Should().Be(0);
    }

    [Fact]
    public void Pause_StopsMovementButKeepsSimulationTicking()
    {
        var session = CreateSession();
        session.Simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 100, 1, 10));
        var enemy = session.Simulation.Enemies.Single();
        session.Pause();

        session.Advance(41.6667);

        session.State.Should().Be(FrontlineGameSessionState.Paused);
        session.Simulation.Tick.Should().Be(1);
        enemy.Distance.Should().Be(0);
    }

    [Fact]
    public void Advance_UsesLegacyElapsedTimeScaleAndEightUnitCap()
    {
        var session = CreateSession();
        session.SetSpeedMultiplier(2);
        session.Simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 100, 1, 10));
        var enemy = session.Simulation.Enemies.Single();

        session.Advance(1000);

        enemy.Distance.Should().Be(8);
    }

    [Fact]
    public void SetSpeedMultiplier_OutsideSupportedRange_IsRejected()
    {
        var session = CreateSession();

        session.SetSpeedMultiplier(9).Should().BeFalse();
        session.SetSpeedMultiplier(4).Should().BeFalse();
        session.SetSpeedMultiplier(0).Should().BeFalse();
        session.SetSpeedMultiplier(2).Should().BeTrue();
        session.SpeedMultiplier.Should().Be(2);
    }

    [Fact]
    public void Constructor_WhenCampaignLevelIsLocked_Throws()
    {
        var level = FrontlineLevelMapCatalog.Find("level-02")!;
        var waveSet = FrontlineWaveCatalog.Find("level-02")!;

        var act = () => new FrontlineGameSession(
            level,
            waveSet,
            FrontlinePlayerProgress.CreateNew(),
            new Random(42));

        act.Should().Throw<FrontlineLevelLockedException>();
    }

    [Fact]
    public void Constructor_WhenMapHasNoValidAuthoredRoute_ThrowsRouteUnavailable()
    {
        var level = FrontlineLevelMapCatalog.Find("level-08")!;
        var waveSet = FrontlineWaveCatalog.Find("level-08")!;
        var progress = FrontlinePlayerProgress.CreateNew();
        progress.UnlockLevel(level.UnlockIndex);

        var act = () => new FrontlineGameSession(level, waveSet, progress, new Random(42));

        act.Should().Throw<FrontlineRouteUnavailableException>();
    }

    [Fact]
    public void Constructor_WhenMapAndWaveSetDoNotMatch_Throws()
    {
        var level = FrontlineLevelMapCatalog.Find("level-01")!;
        var waveSet = FrontlineWaveCatalog.Find("level-02")!;

        var act = () => new FrontlineGameSession(
            level,
            waveSet,
            FrontlinePlayerProgress.CreateNew(),
            new Random(42));

        act.Should().Throw<ArgumentException>();
    }
}
