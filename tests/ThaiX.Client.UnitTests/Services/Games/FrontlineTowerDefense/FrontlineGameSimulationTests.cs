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
        var simulation = new FrontlineGameSimulation(Path, 500, 20);
        var definition = new FrontlineTowerDefinition("rifle", 100, 50, 10, 5);

        var result = simulation.TryBuildTower(definition, new FrontlinePoint(25, 25));

        result.Should().BeTrue();
        simulation.Funds.Should().Be(400);
        simulation.Towers.Should().ContainSingle();
    }

    [Fact]
    public void TryBuildTower_WithInsufficientFunds_LeavesStateUnchanged()
    {
        var simulation = new FrontlineGameSimulation(Path, 50, 20);
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
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 20, 1, 15, 1));

        simulation.Advance();

        simulation.Enemies.Should().BeEmpty();
        simulation.Funds.Should().Be(415);
        simulation.Score.Should().Be(15);
    }

    [Fact]
    public void Advance_WhenEnemyReachesEndOfPath_AppliesLifeDamage()
    {
        var simulation = new FrontlineGameSimulation(Path, 0, 3);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("fast", 100, 100, 5, 2));

        simulation.Advance();

        simulation.Lives.Should().Be(1);
        simulation.Status.Should().Be(FrontlineGameStatus.Planning);
    }

    [Fact]
    public void Advance_WhenLivesReachZero_SetsDefeat()
    {
        var simulation = new FrontlineGameSimulation(Path, 0, 1);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("fast", 100, 100, 5, 1));

        simulation.Advance();

        simulation.Status.Should().Be(FrontlineGameStatus.Defeat);
    }
}