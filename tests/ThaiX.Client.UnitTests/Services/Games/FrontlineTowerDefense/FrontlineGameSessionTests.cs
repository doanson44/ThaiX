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
    public void FireAtEnemy_WhenInfantryIsHit_SpendsOneFundAndAppliesSniperDamage()
    {
        var level = FrontlineLevelMapCatalog.Find("bonus-01")!;
        var progress = FrontlinePlayerProgress.CreateNew();
        progress.UnlockLevel(level.UnlockIndex);
        var session = new FrontlineSniperMissionSession(
            level,
            FrontlineWaveCatalog.Find(level.Id)!,
            progress,
            new Random(42));
        session.Simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 500, 0.1, 10));
        var enemy = session.Simulation.Enemies.Single();
        session.Advance(500);
        session.Advance(500);

        var fired = session.FireAtEnemy(enemy.Id);

        fired.Should().BeTrue();
        session.ShotsFired.Should().Be(1);
        session.Hits.Should().Be(1);
        session.Simulation.Funds.Should().Be(499);
        enemy.Health.Should().Be(200);
        enemy.CoreTemperature.Should().Be(37);
    }

    [Fact]
    public void FireAtEnemy_WhenMetalTargetIsHit_AppliesReducedSniperDamage()
    {
        var level = FrontlineLevelMapCatalog.Find("bonus-01")!;
        var progress = FrontlinePlayerProgress.CreateNew();
        progress.UnlockLevel(level.UnlockIndex);
        var session = new FrontlineSniperMissionSession(
            level,
            FrontlineWaveCatalog.Find(level.Id)!,
            progress,
            new Random(42));
        session.Simulation.SpawnEnemy(
            new FrontlineEnemyDefinition("vehicle", 500, 0.1, 20, IsMetal: true));
        var enemy = session.Simulation.Enemies.Single();
        session.Advance(500);
        session.Advance(500);

        session.FireAtEnemy(enemy.Id);

        enemy.Health.Should().Be(350);
        enemy.CoreTemperature.Should().Be(205);
    }

    [Fact]
    public void FireAt_WhenNoEnemyIsAtAimPoint_SpendsOneFundAndRecordsMiss()
    {
        var level = FrontlineLevelMapCatalog.Find("bonus-01")!;
        var progress = FrontlinePlayerProgress.CreateNew();
        progress.UnlockLevel(level.UnlockIndex);
        var session = new FrontlineSniperMissionSession(
            level,
            FrontlineWaveCatalog.Find(level.Id)!,
            progress,
            new Random(42));
        session.Advance(500);
        session.Advance(500);

        var fired = session.FireAt(new FrontlinePoint(500, 400));

        fired.Should().BeTrue();
        session.ShotsFired.Should().Be(1);
        session.Hits.Should().Be(0);
        session.Simulation.Funds.Should().Be(499);
    }

    [Fact]
    public void FireAt_WhenNoFundsRemain_DoesNotFire()
    {
        var level = FrontlineLevelMapCatalog.Find("bonus-01")!;
        var progress = FrontlinePlayerProgress.CreateNew();
        progress.UnlockLevel(level.UnlockIndex);
        var session = new FrontlineSniperMissionSession(
            level,
            FrontlineWaveCatalog.Find(level.Id)!,
            progress,
            new Random(42));
        session.Simulation.TrySpendFunds(session.Simulation.Funds).Should().BeTrue();
        session.Advance(500);
        session.Advance(500);

        var fired = session.FireAt(new FrontlinePoint(100, 100));

        fired.Should().BeFalse();
        session.ShotsFired.Should().Be(0);
        session.Simulation.Funds.Should().Be(0);
    }

    [Fact]
    public void Advance_WhenAllWavesAreCleared_AwardsLegacyMissionScoreAndCredits()
    {
        var level = FrontlineLevelMapCatalog.Find("level-01")!;
        var waveSet = new FrontlineWaveSet(
            level.Id,
            level.LevelNumber,
            level.IsBonus,
            level.GameFrame,
            [new FrontlineWaveGroup("enemy1", 1, 1, 0)]);
        var progress = FrontlinePlayerProgress.CreateNew();
        var session = new FrontlineGameSession(level, waveSet, progress, new Random(42));

        session.TryStartNextWave().Should().BeTrue();
        session.Advance(100);
        var enemy = session.Simulation.Enemies.Single();
        session.Simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(DirectDamage: enemy.Health))
            .Should().BeTrue();

        session.Advance(100);

        session.State.Should().Be(FrontlineGameSessionState.Victory);
        session.TotalScore.Should().Be(7570);
        session.CreditsEarned.Should().Be(1892);
        progress.Credits.Should().Be(2892);
        progress.LevelScores[level.ProgressIndex].Should().Be(7570);
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
