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
        var definition = new FrontlineTowerDefinition("rifle", 100, 50, 10, 100);

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
    public void UpgradeDamage_WithEnoughFunds_ChargesQuarterOfPurchaseCostAndAddsTwentyPercent()
    {
        var simulation = new FrontlineGameSimulation(Path, 500);
        simulation.TryBuildTower(
            new FrontlineTowerDefinition("rifle", 100, 50, 10, 5),
            new FrontlinePoint(25, 25));
        var tower = simulation.Towers.Single();
        var funds = 25;

        var upgraded = tower.UpgradeDamage(ref funds);

        upgraded.Should().BeTrue();
        funds.Should().Be(0);
        tower.Damage.Should().Be(12);
    }

    [Fact]
    public void Advance_WhenEnemyEntersTowerRange_ResolvesEnemyAndAwardsReward()
    {
        var simulation = new FrontlineGameSimulation(Path, 500);
        simulation.TryBuildTower(
            new FrontlineTowerDefinition("rifle", 100, 60, 25, 100),
            new FrontlinePoint(50, 10));
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 20, 1, 15));

        simulation.Advance();
        simulation.Advance();

        simulation.Enemies.Should().BeEmpty();
        simulation.Funds.Should().Be(415);
        simulation.Score.Should().Be(30);
    }

    [Fact]
    public void Advance_WhenNapalmSupportStrikeFires_AppliesAreaDamageAndExpiresAfterSixShots()
    {
        var simulation = new FrontlineGameSimulation(Path, 0, random: new Random(1));
        var support = FrontlineTowerCatalog.Find("suptower1")!;
        simulation.TryBuildTower(support.CreateDefinition(), new FrontlinePoint(50, 0));
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 1000, 0.1, 10));
        var enemy = simulation.Enemies.Single();

        for (var index = 0; index < 6; index++)
            simulation.Advance(34);

        enemy.Health.Should().BeLessThan(1000);
        simulation.Towers.Should().BeEmpty();
    }

    [Fact]
    public void TryMoveTower_WhenSupportStrikeSelected_IsRejected()
    {
        var simulation = new FrontlineGameSimulation(Path, 10);
        var support = FrontlineTowerCatalog.Find("suptower3")!;
        simulation.TryBuildTower(support.CreateDefinition(), new FrontlinePoint(50, 0));
        var tower = simulation.Towers.Single();

        var moved = simulation.TryMoveTower(tower.Id, new FrontlinePoint(100, 0));

        moved.Should().BeFalse();
        simulation.Funds.Should().Be(10);
        tower.Cell.Should().Be(new FrontlineGridPoint(1, 0));
    }

    [Fact]
    public void Advance_WhenNapalmShellImpacts_CreatesPersistentBurnAreaEffect()
    {
        var simulation = new FrontlineGameSimulation(Path, 0, random: new Random(1));
        var support = FrontlineTowerCatalog.Find("suptower1")!;
        simulation.TryBuildTower(
            support.CreateDefinition() with { ShotLimit = 1 },
            new FrontlinePoint(50, 0));
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 1000, 0.01, 10));
        var enemy = simulation.Enemies.Single();

        for (var tick = 0; tick < 34; tick++)
            simulation.Advance();

        simulation.ActiveSupportProjectileCount.Should().Be(1);
        enemy.Health.Should().Be(1000);

        for (var tick = 0; tick < 120; tick++)
            simulation.Advance();

        simulation.ActiveSupportProjectileCount.Should().Be(0);
        simulation.ActiveAreaEffectCount.Should().Be(1);
        enemy.Health.Should().BeLessThan(950);
    }

    [Fact]
    public void TrySellTower_WhenSupportStrikeSelected_DoesNotRefundOrRemoveIt()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        var support = FrontlineTowerCatalog.Find("suptower3")!;
        simulation.TryBuildTower(support.CreateDefinition(), new FrontlinePoint(50, 0));
        var tower = simulation.Towers.Single();

        var sold = simulation.TrySellTower(tower.Id);

        sold.Should().BeFalse();
        simulation.Towers.Should().ContainSingle();
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
    public void Advance_WhenDamageOverTimeKillsEnemy_AwardsCreditsAndKillScore()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 0.1, 1, 10));
        var enemy = simulation.Enemies.Single();
        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(Burn: 1000));

        simulation.Advance();

        simulation.Enemies.Should().BeEmpty();
        simulation.Funds.Should().Be(10);
        simulation.Score.Should().Be(20);
    }

    [Fact]
    public void ApplyDamage_WithPoisonAgainstMetalEnemy_DoesNotApplyPoison()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
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
    public void Advance_WhenLevelHealthReachesZero_SetsDefeat()
    {
        var simulation = new FrontlineGameSimulation(Path, 0, 100);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("fast", 100, 100, 5));

        simulation.Advance();

        simulation.Status.Should().Be(FrontlineGameStatus.Defeat);
    }
}