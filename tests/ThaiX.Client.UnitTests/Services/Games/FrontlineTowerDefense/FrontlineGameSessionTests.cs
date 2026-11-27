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
    public void Advance_AfterManuallyStartingWave_SpawnsFirstEnemyImmediately()
    {
        var session = CreateSession();

        session.TryStartNextWave().Should().BeTrue();

        session.CurrentWave.Should().Be(1);
        session.Simulation.Enemies.Should().ContainSingle()
            .Which.Definition.Id.Should().Be("enemy3");
    }

    [Fact]
    public void Advance_WithZeroElapsedTime_StillProcessesLegacyFrame()
    {
        var session = CreateSession();

        session.Advance(0);

        session.Simulation.Tick.Should().Be(1);
        session.CurrentWave.Should().Be(0);
    }

    [Fact]
    public void Advance_WithZeroElapsedTime_StillAppliesStatusDamage()
    {
        var session = CreateSession();
        session.Simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 100, 1, 10));
        var enemy = session.Simulation.Enemies.Single();
        session.Simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(Burn: 200));

        session.Advance(0);

        enemy.Health.Should().BeApproximately(99.5, 0.001);
        enemy.BurnDuration.Should().Be(234);
        enemy.Distance.Should().Be(0);
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
    public void TryStartNextWave_WhenPaused_DoesNotStartNextGroup()
    {
        var session = CreateSession();
        session.Pause();

        session.CanStartNextWave.Should().BeFalse();
        session.TryStartNextWave().Should().BeFalse();
        session.CurrentWave.Should().Be(0);
    }

    [Fact]
    public void TryStartNextWave_AfterResumingFromPause_StartsNextGroup()
    {
        var session = CreateSession();
        session.Pause();
        session.TryStartNextWave().Should().BeFalse();

        session.Resume();

        session.CanStartNextWave.Should().BeTrue();
        session.TryStartNextWave().Should().BeTrue();
        session.CurrentWave.Should().Be(1);
    }

    [Fact]
    public void Advance_WhenPausedAndTowerIsLoaded_StillFiresLikeLegacyRuntime()
    {
        var level = new FrontlineLevelMapDefinition(
            "pause-test",
            1,
            false,
            10,
            [new FrontlineGridPoint(1, -1)],
            [new FrontlineGridPoint(1, 0)],
            [new FrontlineGridPoint(1, 0)],
            []);
        var waveSet = new FrontlineWaveSet("pause-test", 1, false, 10, []);
        var session = new FrontlineGameSession(
            level,
            waveSet,
            FrontlinePlayerProgress.CreateNew(),
            new Random(42));
        var tower = new FrontlineTowerDefinition("pause-test", 0, 1000, 10, 100);
        session.Simulation.TryBuildTower(tower, new FrontlinePoint(80, 80)).Should().BeTrue();
        var path = new FrontlinePath(
        [
            new FrontlinePoint(100, 80),
            new FrontlinePoint(300, 80)
        ]);
        session.Simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 100, 0.0001, 10), path);
        var enemy = session.Simulation.Enemies.Single();

        session.Advance(500);
        session.Pause();
        session.Advance(41.6667);

        enemy.Health.Should().Be(90);
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
        session.CanFire.Should().BeFalse();

        session.Advance(500);

        session.CanFire.Should().BeFalse();

        session.Advance(500);

        session.CanFire.Should().BeTrue();
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
        session.Simulation.CombatTraces.Should().ContainSingle()
            .Which.Kind.Should().Be(FrontlineCombatTraceKind.Sniper);
        session.Simulation.CombatTraces.Single().End.Should().Be(new FrontlinePoint(500, 400));
    }

    [Fact]
    public void FireAt_WhenEnemyIsWithinSourceAimSamplingWindow_HitsEnemy()
    {
        var level = FrontlineLevelMapCatalog.Find("bonus-01")!;
        var progress = FrontlinePlayerProgress.CreateNew();
        progress.UnlockLevel(level.UnlockIndex);
        var session = new FrontlineSniperMissionSession(
            level,
            FrontlineWaveCatalog.Find(level.Id)!,
            progress,
            new Random(42));
        var path = new FrontlinePath(
        [
            new FrontlinePoint(521, 240),
            new FrontlinePoint(721, 240)
        ]);
        session.Simulation.SpawnEnemy(
            new FrontlineEnemyDefinition("infantry", 500, 0.1, 10),
            path);
        var enemy = session.Simulation.Enemies.Single();
        session.Advance(500);
        session.Advance(500);

        var fired = session.FireAt(new FrontlinePoint(510, 240));

        fired.Should().BeTrue();
        session.Hits.Should().Be(1);
        enemy.Health.Should().Be(200);
    }

    [Fact]
    public void GetShooterPosition_BonusMissionThree_ReturnsAuthoredManualTowerPosition()
    {
        var level = FrontlineLevelMapCatalog.Find("bonus-03")!;

        FrontlineSniperMissionSession.GetShooterPosition(level)
            .Should().Be(new FrontlinePoint(302, 102));
    }

    [Fact]
    public void FireAt_WhenFundsReachZero_AllowsOneFinalShot()
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

        fired.Should().BeTrue();
        session.ShotsFired.Should().Be(1);
        session.Simulation.Funds.Should().Be(-1);

        session.Advance(500);
        session.CanFire.Should().BeFalse();
        session.FireAt(new FrontlinePoint(100, 100)).Should().BeFalse();
        session.ShotsFired.Should().Be(1);
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
    public void Forfeit_ZeroesLevelHealthAndEndsTheMissionAsDefeat()
    {
        var session = CreateSession();
        session.Pause();

        session.Simulation.Forfeit();

        session.Simulation.LevelHealth.Should().Be(0);
        session.Simulation.Status.Should().Be(FrontlineGameStatus.Defeat);

        session.Advance(0);

        session.State.Should().Be(FrontlineGameSessionState.Defeat);
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
