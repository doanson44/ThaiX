using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineSniperMissionSessionTests
{
    private static FrontlineSniperMissionSession CreateSession()
    {
        var level = FrontlineLevelMapCatalog.Find("bonus-01")!;
        var progress = FrontlinePlayerProgress.CreateNew();
        progress.UnlockLevel(level.UnlockIndex);

        return new FrontlineSniperMissionSession(
            level,
            FrontlineWaveCatalog.Find(level.Id)!,
            progress,
            new Random(42));
    }

    [Fact]
    public void FireAtEnemy_WhenReloadingAfterShot_RejectsSecondShotUntilReloadCompletes()
    {
        var session = CreateSession();
        session.Simulation.SpawnEnemy(
            new FrontlineEnemyDefinition("infantry", 500, 0.1, 10));
        var enemy = session.Simulation.Enemies.Single();

        session.Advance(500);
        session.Advance(500);
        session.CanFire.Should().BeTrue();

        session.FireAtEnemy(enemy.Id).Should().BeTrue();

        session.Advance(100);

        session.CanFire.Should().BeFalse();
        session.FireAtEnemy(enemy.Id).Should().BeFalse();
        session.ShotsFired.Should().Be(1);
        session.Simulation.Funds.Should().Be(499);

        session.Advance(56.25);

        session.CanFire.Should().BeTrue();
    }

    [Fact]
    public void Advance_WhenPaused_DoesNotProgressSniperReload()
    {
        var session = CreateSession();
        session.Simulation.SpawnEnemy(
            new FrontlineEnemyDefinition("infantry", 500, 0.1, 10));
        var enemy = session.Simulation.Enemies.Single();

        session.Advance(500);
        session.Advance(500);
        session.FireAtEnemy(enemy.Id).Should().BeTrue();

        session.Pause();
        session.Advance(500);

        session.CanFire.Should().BeFalse();

        session.Resume();
        session.Advance(100);

        session.CanFire.Should().BeFalse();

        session.Advance(56.25);

        session.CanFire.Should().BeTrue();
    }

    [Fact]
    public void FireAt_WhenEnemyIsHitAtTheAimSample_EndsTracerAtTheSamplePoint()
    {
        var session = CreateSession();
        session.Simulation.SpawnEnemy(
            new FrontlineEnemyDefinition("infantry", 500, 0.0001, 10),
            new FrontlinePath([new FrontlinePoint(400, 240), new FrontlinePoint(401, 240)]));
        session.Advance(500);
        session.Advance(500);
        var aim = new FrontlinePoint(402, 240);

        session.FireAt(aim).Should().BeTrue();

        session.Hits.Should().Be(1);
        session.Simulation.CombatTraces.Should().ContainSingle()
            .Which.End.Should().Be(aim);
        session.Simulation.Particles.Should().Contain(particle =>
            particle.Strip == FrontlineSpriteStrip.MuzzleFlash);
    }

    [Fact]
    public void FireAt_WhenNoEnemyIsSampled_EmitsThreeSparksAtTheAimPoint()
    {
        var session = CreateSession();
        session.Advance(500);
        session.Advance(500);

        session.FireAt(new FrontlinePoint(400, 240)).Should().BeTrue();

        session.Hits.Should().Be(0);
        session.Simulation.Sparks.Should().HaveCount(3);
        session.Simulation.Sparks.Should().OnlyContain(spark =>
            spark.X == 400 && spark.Y == 240);
    }
}
