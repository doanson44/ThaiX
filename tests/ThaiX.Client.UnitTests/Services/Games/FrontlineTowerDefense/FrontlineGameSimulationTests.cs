using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineGameSimulationTests
{
    private static readonly FrontlinePath Path = new(
    [
        new FrontlinePoint(0, 0),
        new FrontlinePoint(100, 0)
    ]);

    [Fact]
    public void TryBuildTower_WithSufficientFunds_DeductsCostAndAddsTower()
    {
        var simulation = new FrontlineGameSimulation(Path, 500);
        var definition = new FrontlineTowerDefinition("rifle", 100, 50, 10, 5);

        var result = simulation.TryBuildTower(definition, new FrontlinePoint(25, 25));

        result.Should().BeTrue();
        simulation.Funds.Should().Be(400);
        simulation.Towers.Should().ContainSingle();
    }

    [Fact]
    public void TryBuildTower_WithInsufficientFunds_LeavesStateUnchanged()
    {
        var simulation = new FrontlineGameSimulation(Path, 50);
        var definition = new FrontlineTowerDefinition("rifle", 100, 50, 10, 5);

        var result = simulation.TryBuildTower(definition, new FrontlinePoint(25, 25));

        result.Should().BeFalse();
        simulation.Funds.Should().Be(50);
        simulation.Towers.Should().BeEmpty();
    }

    [Fact]
    public void Advance_WhenEnemyEntersTowerRange_ResolvesEnemyAndAwardsReward()
    {
        var simulation = new FrontlineGameSimulation(Path, 500, 20);
        simulation.TryBuildTower(
            new FrontlineTowerDefinition("rifle", 100, 60, 25, 1),
            new FrontlinePoint(50, 10));
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 20, 1, 15));

        simulation.Advance();

        simulation.Enemies.Should().BeEmpty();
        simulation.Funds.Should().Be(415);
        simulation.Score.Should().Be(15);
    }

    [Fact]
    public void ApplyDamage_WithBurnProfile_AppliesTemperatureAndDamageOverTime()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 100, 1, 5));
        var enemy = simulation.Enemies.Single();

        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(Burn: 170));
        simulation.Advance();

        enemy.Health.Should().Be(99.5);
        enemy.BurnDuration.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ApplyDamage_WithPoisonAgainstMetalEnemy_DoesNotApplyPoison()
    {
        var simulation = new FrontlineGameSimulation(Path, 0, 20);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("tank", 100, 1, 5, IsMetal: true));
        var enemy = simulation.Enemies.Single();

        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(Poison: 100));
        simulation.Advance();

        enemy.PoisonDuration.Should().Be(0);
        enemy.Health.Should().Be(100);
    }

    [Fact]
    public void Advance_WhenEnemyReachesEndOfPath_AppliesLifeDamage()
    {
        var simulation = new FrontlineGameSimulation(Path, 0, 1000);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("fast", 100, 100, 5));

        simulation.Advance();

        simulation.LevelHealth.Should().Be(900);
        simulation.Status.Should().Be(FrontlineGameStatus.Planning);
    }

    [Fact]
    public void Advance_WhenLivesReachZero_SetsDefeat()
    {
        var simulation = new FrontlineGameSimulation(Path, 0, 1000);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("fast", 100, 100, 5));

        simulation.Advance();

        simulation.Status.Should().Be(FrontlineGameStatus.Defeat);
    }
}