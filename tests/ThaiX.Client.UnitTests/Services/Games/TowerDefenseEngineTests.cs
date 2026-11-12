using ThaiX.Client.Pages.Games.TowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games;

public sealed class TowerDefenseEngineTests
{
    [Fact]
    public void Definitions_ShouldExposeFullTowerEnemyAndCampaignCatalogs()
    {
        TowerDefenseEngine.TowerDefinitions.Should().HaveCount(10);
        TowerDefenseEngine.EnemyDefinitions.Should().HaveCount(8);
        TowerDefenseEngine.Maps.Should().HaveCount(5);
        TowerDefenseEngine.Missions.Should().HaveCount(14);
        TowerDefenseEngine.TowerDefinitions.Select(item => item.Id).Should().OnlyHaveUniqueItems();
        TowerDefenseEngine.EnemyDefinitions.Select(item => item.Id).Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void StartCampaign_ShouldRejectLockedMission()
    {
        var engine = new TowerDefenseEngine();

        engine.StartCampaign(2);

        engine.Mode.Should().BeNull();
        engine.StartCampaign(1);
        engine.Mode.Should().Be(TowerDefenseMode.Campaign);
        engine.MissionNumber.Should().Be(1);
        engine.TotalWaves.Should().Be(5);
    }

    [Fact]
    public void Build_ShouldChargeGoldAndRejectBlockedOrOccupiedCells()
    {
        var engine = new TowerDefenseEngine();
        engine.StartCampaign(1);
        var cell = Enumerable.Range(0, TowerDefenseEngine.Columns * TowerDefenseEngine.Rows)
            .First(candidate => !engine.BlockedCells.Contains(candidate));
        var startingGold = engine.Gold;

        engine.Build(cell, 0).Should().BeTrue();
        engine.Gold.Should().Be(startingGold - TowerDefenseEngine.TowerDefinitions[0].Cost);
        engine.Build(cell, 1).Should().BeFalse();
        engine.Build(engine.BlockedCells.First(), 0).Should().BeFalse();
        engine.Towers.Should().ContainSingle(tower => tower.Cell == cell);
    }

    [Fact]
    public void UpgradeAndSell_ShouldUpdateTowerAndRefundGold()
    {
        var engine = new TowerDefenseEngine();
        engine.StartCampaign(1);
        var cell = Enumerable.Range(0, TowerDefenseEngine.Columns * TowerDefenseEngine.Rows)
            .First(candidate => !engine.BlockedCells.Contains(candidate));
        engine.Build(cell, 0);
        var tower = engine.Towers.Single();
        var baseDamage = tower.Damage;
        var goldBeforeUpgrade = engine.Gold;

        engine.Upgrade(cell, "damage").Should().BeTrue();
        tower.Damage.Should().BeGreaterThan(baseDamage);
        engine.Gold.Should().BeLessThan(goldBeforeUpgrade);

        engine.SetTargeting(cell, TowerTargeting.Strongest).Should().BeTrue();
        tower.Targeting.Should().Be(TowerTargeting.Strongest);
        engine.Sell(cell).Should().BeTrue();
        engine.Towers.Should().BeEmpty();
    }

    [Fact]
    public void StartEndless_ShouldUseIndependentEndlessMode()
    {
        var engine = new TowerDefenseEngine();

        engine.StartEndless();

        engine.Mode.Should().Be(TowerDefenseMode.Endless);
        engine.MissionNumber.Should().Be(0);
        engine.Lives.Should().Be(20);
        engine.Gold.Should().Be(200);
        engine.TotalWaves.Should().Be(0);
    }

    [Fact]
    public void Tick_ShouldSpawnEnemiesAfterInitialDeploymentDelay()
    {
        var engine = new TowerDefenseEngine();
        engine.StartCampaign(1);

        for (var tick = 0; tick < 150; tick++)
            engine.Tick();

        engine.Wave.Should().Be(1);
        engine.Enemies.Should().NotBeEmpty();
    }
}