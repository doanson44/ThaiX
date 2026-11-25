using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineSoundCueTests
{
    private static readonly FrontlinePath Path = new(
    [
        new FrontlinePoint(80, 48),
        new FrontlinePoint(300, 48)
    ]);

    [Fact]
    public void Advance_WhenRifleTowerFires_QueuesRifleCue()
    {
        var simulation = new FrontlineGameSimulation(Path, 50);
        var rifle = FrontlineTowerCatalog.Find("tower2")!;
        simulation.TryBuildTower(
            rifle.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50)).Should().BeTrue();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 100, 0.0001, 10));

        simulation.Advance(1);
        simulation.Advance(0);

        simulation.Sounds.Should().ContainSingle().Which.Should().Be(FrontlineSound.Rifle);
    }

    [Fact]
    public void Advance_WhenFlameThrowerFires_QueuesFlameCue()
    {
        var simulation = new FrontlineGameSimulation(Path, 250, random: new Random(1));
        var flame = FrontlineTowerCatalog.Find("tower9")!;
        simulation.TryBuildTower(
            flame.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50)).Should().BeTrue();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 100, 0.0001, 10));

        simulation.Advance(1);
        simulation.Advance(8);

        simulation.Sounds.Should().Contain(FrontlineSound.Flame);
    }

    [Fact]
    public void ApplyDamage_WhenInfantryDies_QueuesSplatCue()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("enemy1", 50, 0.0001, 10));
        var enemy = simulation.Enemies.Single();

        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(DirectDamage: 100)).Should().BeTrue();

        simulation.Sounds.Should().ContainSingle().Which.Should().Be(FrontlineSound.Splat);
    }

    [Fact]
    public void ApplyDamage_WhenVehicleDies_QueuesExplodeCue()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("Tank1", 500, 0.0001, 20, IsMetal: true));
        var enemy = simulation.Enemies.Single();

        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(DirectDamage: 1000)).Should().BeTrue();

        simulation.Sounds.Should().ContainSingle().Which.Should().Be(FrontlineSound.Explode);
    }

    [Fact]
    public void DrainSounds_ReturnsQueuedCuesAndClearsTheQueue()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("enemy1", 50, 0.0001, 10));
        var enemy = simulation.Enemies.Single();
        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(DirectDamage: 100)).Should().BeTrue();

        var drained = simulation.DrainSounds();

        drained.Should().ContainSingle();
        simulation.DrainSounds().Should().BeEmpty();
    }
}
