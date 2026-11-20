using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineTowerPlacementTests
{
    private static readonly FrontlinePath Path = new(
    [
        new FrontlinePoint(48, -16),
        new FrontlinePoint(48, 16),
        new FrontlinePoint(528, 16)
    ]);

    [Fact]
    public void TryBuildTower_OnPathOrBlockedCell_IsRejected()
    {
        var level = FrontlineLevelMapCatalog.Find("level-01")!;
        var simulation = new FrontlineGameSimulation(Path, 500, levelMap: level);
        var definition = new FrontlineTowerDefinition("rifle", 50, 60, 10, 5);

        var onPath = simulation.TryBuildTower(definition, new FrontlinePoint(48, 16));
        var onBlockedCell = simulation.TryBuildTower(definition, new FrontlinePoint(16, 16));

        onPath.Should().BeFalse();
        onBlockedCell.Should().BeFalse();
        simulation.Funds.Should().Be(500);
        simulation.Towers.Should().BeEmpty();
    }

    [Fact]
    public void TryBuildTower_OnEmptyCell_SnapsToCellCenterAndDeductsBuildCost()
    {
        var level = FrontlineLevelMapCatalog.Find("level-01")!;
        var simulation = new FrontlineGameSimulation(Path, 500, levelMap: level);
        var definition = new FrontlineTowerDefinition("rifle", 50, 60, 10, 5);

        var built = simulation.TryBuildTower(definition, new FrontlinePoint(81, 82));

        built.Should().BeTrue();
        simulation.Funds.Should().Be(450);
        simulation.Towers.Single().Cell.Should().Be(new FrontlineGridPoint(2, 2));
        simulation.Towers.Single().Position.Should().Be(new FrontlinePoint(80, 80));
    }

    [Fact]
    public void TryMoveTower_ToEmptyCell_ChargesRelocationFeeAndSnapsPosition()
    {
        var level = FrontlineLevelMapCatalog.Find("level-01")!;
        var simulation = new FrontlineGameSimulation(Path, 500, levelMap: level);
        simulation.TryBuildTower(
            new FrontlineTowerDefinition("rifle", 50, 60, 10, 5),
            new FrontlinePoint(80, 80));
        var tower = simulation.Towers.Single();

        var moved = simulation.TryMoveTower(tower.Id, new FrontlinePoint(112, 112));

        moved.Should().BeTrue();
        simulation.Funds.Should().Be(440);
        tower.Cell.Should().Be(new FrontlineGridPoint(3, 3));
        tower.Position.Should().Be(new FrontlinePoint(112, 112));
    }

    [Fact]
    public void TrySellTower_ReturnsHalfBuildCostAndRemovesTower()
    {
        var simulation = new FrontlineGameSimulation(Path, 500);
        simulation.TryBuildTower(
            new FrontlineTowerDefinition("rifle", 50, 60, 10, 5),
            new FrontlinePoint(80, 80));
        var tower = simulation.Towers.Single();

        var sold = simulation.TrySellTower(tower.Id);

        sold.Should().BeTrue();
        simulation.Funds.Should().Be(475);
        simulation.Towers.Should().BeEmpty();
    }

    [Fact]
    public void TryUpgradeTower_DeductsQuarterBuildCostAndIncreasesDamageByTwentyPercent()
    {
        var simulation = new FrontlineGameSimulation(Path, 500);
        simulation.TryBuildTower(
            new FrontlineTowerDefinition("rifle", 50, 60, 10, 5),
            new FrontlinePoint(80, 80));
        var tower = simulation.Towers.Single();

        var upgraded = simulation.TryUpgradeTower(tower.Id, FrontlineTowerUpgradeType.Damage);

        upgraded.Should().BeTrue();
        simulation.Funds.Should().Be(438);
        tower.Damage.Should().Be(12);
    }
}
