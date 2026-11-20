using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineWaveSchedulerTests
{
    [Fact]
    public void CampaignLevelOne_ContainsTwentyFourAuthoredGroups()
    {
        FrontlineWaveCatalog.Find("level-01")!.ToSchedule().Groups.Should().HaveCount(24);
    }

    [Fact]
    public void All_ContainsFourteenCampaignAndFourBonusWaveSchedules()
    {
        FrontlineWaveCatalog.All.Should().HaveCount(18);
        FrontlineWaveCatalog.Campaign.Should().HaveCount(14);
        FrontlineWaveCatalog.Bonus.Should().HaveCount(4);
        FrontlineWaveCatalog.All.Sum(level => level.Groups.Count).Should().Be(963);
    }

    [Fact]
    public void TryStartNextGroup_WhenCooldownIsReady_StartsWaveImmediately()
    {
        var scheduler = new FrontlineWaveScheduler(FrontlineWaveCatalog.Find("level-01")!.ToSchedule());

        scheduler.CanManuallyStart.Should().BeTrue();
        scheduler.TryStartNextGroup().Should().BeTrue();
        scheduler.CurrentGroup.Should().Be(1);
        scheduler.CanManuallyStart.Should().BeFalse();

        for (var tick = 0; tick < 199; tick++)
            scheduler.Advance(0, enemiesRemain: false);

        scheduler.CanManuallyStart.Should().BeFalse();
        scheduler.Advance(0, enemiesRemain: false);
        scheduler.CanManuallyStart.Should().BeTrue();
        scheduler.TryStartNextGroup().Should().BeTrue();
        scheduler.CurrentGroup.Should().Be(2);
        scheduler.ActiveSpawnerCount.Should().Be(2);
    }

    [Fact]
    public void Advance_BeforeInitialCountdownExpires_DoesNotStartWave()
    {
        var scheduler = new FrontlineWaveScheduler(FrontlineWaveCatalog.Find("level-01")!.ToSchedule());

        var spawned = scheduler.Advance(999, enemiesRemain: false);

        spawned.Should().BeEmpty();
        scheduler.CurrentGroup.Should().Be(0);
    }

    [Fact]
    public void Advance_WhenGroupDelayIsShorterThanSpawnDuration_KeepsBothSpawnersActive()
    {
        var scheduler = new FrontlineWaveScheduler(new FrontlineWaveSchedule(
        [
            new FrontlineWaveGroup("enemy1", 3, 100, 10),
            new FrontlineWaveGroup("enemy2", 2, 100, 10)
        ]));

        scheduler.Advance(1000, enemiesRemain: false);
        scheduler.Advance(20, enemiesRemain: false);

        scheduler.ActiveSpawnerCount.Should().Be(2);
        var spawned = scheduler.Advance(100, enemiesRemain: false);

        spawned.Select(request => request.EnemyType).Should().BeEquivalentTo("enemy1", "enemy2");
        spawned.Single(request => request.EnemyType == "enemy1").WaveNumber.Should().Be(1);
        spawned.Single(request => request.EnemyType == "enemy2").WaveNumber.Should().Be(2);
    }

    [Fact]
    public void Advance_AfterInitialCountdown_StartsFirstGroupWithoutImmediateSpawn()
    {
        var scheduler = new FrontlineWaveScheduler(FrontlineWaveCatalog.Find("level-01")!.ToSchedule());

        var spawned = scheduler.Advance(1000, enemiesRemain: false);

        spawned.Should().BeEmpty();
        scheduler.CurrentGroup.Should().Be(1);
    }

    [Fact]
    public void Advance_WhenSpawnIntervalIsReached_SpawnsOneEnemy()
    {
        var scheduler = new FrontlineWaveScheduler(FrontlineWaveCatalog.Find("level-01")!.ToSchedule());
        scheduler.Advance(1000, enemiesRemain: false);

        var spawned = scheduler.Advance(15, enemiesRemain: false);

        spawned.Should().ContainSingle().Which.EnemyType.Should().Be("enemy3");
    }

    [Fact]
    public void VerifiedMetalEnemy_UsesLegacyMetalRewardAndTemperature()
    {
        var enemy = FrontlineEnemyCatalog.Verified.Single(item => item.Id == "enemy5");
        var definition = enemy.CreateDefinition(1);
        var simulation = new FrontlineGameSimulation(
            new FrontlinePath([new FrontlinePoint(0, 0), new FrontlinePoint(100, 0)]),
            0);
        simulation.SpawnEnemy(definition);

        simulation.Enemies.Single().CoreTemperature.Should().Be(200);
        definition.Reward.Should().Be(20);
    }

    [Fact]
    public void HealthForWave_AppliesTenPercentPerWaveScaling()
    {
        var enemy = FrontlineEnemyCatalog.Verified.Single(item => item.Id == "enemy1");

        enemy.HealthForWave(3).Should().Be(65);
    }
}
