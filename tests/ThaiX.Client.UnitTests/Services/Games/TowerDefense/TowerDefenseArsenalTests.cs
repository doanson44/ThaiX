using ThaiX.Client.Services.Games.TowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.TowerDefense;

public sealed class TowerDefenseArsenalTests
{
    [Fact]
    public void TowerCatalog_ContainsElevenUniqueKinds()
    {
        // Arrange
        var towers = TowerDefenseData.Towers;

        // Act
        var kinds = towers.Select(tower => tower.Kind).ToArray();

        // Assert
        towers.Should().HaveCount(11);
        kinds.Should().OnlyHaveUniqueItems();
        Enum.GetValues<TowerKind>().Should().BeEquivalentTo(kinds);
    }

    [Fact]
    public void TowerUnlockCost_AdvancedTowers_UsesProgressivePrices()
    {
        // Arrange
        var unlocks = new[]
        {
            TowerDefenseData.TowerUnlockCost(TowerKind.Radar),
            TowerDefenseData.TowerUnlockCost(TowerKind.PowerAmplifier),
            TowerDefenseData.TowerUnlockCost(TowerKind.ParticleBeam),
            TowerDefenseData.TowerUnlockCost(TowerKind.PlasmaCannon),
            TowerDefenseData.TowerUnlockCost(TowerKind.FlakCannon)
        };

        // Act
        var startingTowerUnlocks = Enum.GetValues<TowerKind>()
            .Where(kind => TowerDefenseData.TowerUnlockCost(kind) == 0)
            .ToArray();

        // Assert
        unlocks.Order().Should().Equal(new[] { 250, 350, 500, 600, 750 });
        startingTowerUnlocks.Should().HaveCount(6);
    }

    [Fact]
    public void StartingFundsUpgradeCost_IncreasesForEachPermanentPurchase()
    {
        // Arrange
        var costs = new[]
        {
            TowerDefenseData.StartingFundsUpgradeCost(0),
            TowerDefenseData.StartingFundsUpgradeCost(200),
            TowerDefenseData.StartingFundsUpgradeCost(400)
        };

        // Act
        var increment = TowerDefenseData.StartingFundsIncrement;

        // Assert
        costs.Should().Equal(new[] { 250, 500, 750 });
        increment.Should().Be(200);
    }

    [Fact]
    public void Match_StartsWithPurchasedStartingFundsBonus()
    {
        // Arrange
        var difficulty = TowerDefenseData.Difficulties[1];
        const int bonus = 400;

        // Act
        var match = new TowerDefenseMatch(TowerDefenseData.Maps[0], difficulty, endless: false, startingGoldBonus: bonus);

        // Assert
        match.Gold.Should().Be(difficulty.StartGold + bonus);
    }

    [Fact]
    public void MissionReward_IncludesUnspentStartingGold()
    {
        // Arrange
        var match = new TowerDefenseMatch(
            TowerDefenseData.Maps[0],
            TowerDefenseData.Difficulties[1],
            endless: false);

        // Act
        var reward = match.MissionReward;

        // Assert
        reward.Should().Be(match.Gold + 100 + match.Wave * 10 + match.Lives * 5);
    }

    [Fact]
    public void RadarTower_ProvidesRangeSupport()
    {
        // Arrange
        var match = new TowerDefenseMatch(
            TowerDefenseData.Maps[0],
            TowerDefenseData.Difficulties[1],
            endless: false,
            startingGoldBonus: 500);
        var gunner = match.Build(0, TowerKind.Gunner);
        var radar = match.Build(1, TowerKind.Radar);

        // Act
        var range = match.Range(gunner!);

        // Assert
        radar.Should().NotBeNull();
        range.Should().Be(TowerDefenseData.Tower(TowerKind.Gunner).Range + 20);
    }

    [Fact]
    public void RangeAt_AttackTower_IncludesNearbyRadarSupport()
    {
        // Arrange
        var match = new TowerDefenseMatch(
            TowerDefenseData.Maps[0],
            TowerDefenseData.Difficulties[1],
            endless: false,
            startingGoldBonus: 1000);
        var radarCell = match.FirstFreeCell(0);
        var previewCell = Enumerable.Range(0, TowerDefenseMatch.Columns * TowerDefenseMatch.Rows)
            .First(cell => cell != radarCell && !match.IsBlocked(cell));
        match.Build(radarCell, TowerKind.Radar);

        // Act
        var previewRange = match.RangeAt(previewCell, TowerKind.Gunner);
        var gunner = match.Build(previewCell, TowerKind.Gunner);

        // Assert
        previewRange.Should().Be(TowerDefenseData.Tower(TowerKind.Gunner).Range + 20);
        match.Range(gunner!).Should().Be(previewRange);
    }

    [Fact]
    public void RangeAt_RadarTower_DoesNotApplyItsOwnSupportBonus()
    {
        // Arrange
        var match = new TowerDefenseMatch(
            TowerDefenseData.Maps[0],
            TowerDefenseData.Difficulties[1],
            endless: false,
            startingGoldBonus: 1000);
        var cell = match.FirstFreeCell(0);

        // Act
        var previewRange = match.RangeAt(cell, TowerKind.Radar);

        // Assert
        previewRange.Should().Be(TowerDefenseData.Tower(TowerKind.Radar).Range);
    }

    [Fact]
    public void CanBuild_UsesSameRulesAsBuild()
    {
        // Arrange
        var match = new TowerDefenseMatch(TowerDefenseData.Maps[0], TowerDefenseData.Difficulties[1], endless: false);
        var cell = match.FirstFreeCell(0);

        // Act
        var canBuild = match.CanBuild(cell, TowerKind.Gunner);
        var built = match.Build(cell, TowerKind.Gunner);

        // Assert
        canBuild.Should().BeTrue();
        built.Should().NotBeNull();
        match.CanBuild(cell, TowerKind.Gunner).Should().BeFalse();
        match.CanBuild(10, TowerKind.Gunner).Should().BeFalse();
        match.CanBuild(match.FirstFreeCell(1), TowerKind.Sniper).Should().BeFalse();
    }

    [Fact]
    public void PowerAmplifier_IncreasesNearbyTowerDamage()
    {
        // Arrange
        var match = new TowerDefenseMatch(
            TowerDefenseData.Maps[0],
            TowerDefenseData.Difficulties[1],
            endless: false,
            startingGoldBonus: 500);
        var gunner = match.Build(0, TowerKind.Gunner);
        var amplifier = match.Build(1, TowerKind.PowerAmplifier);

        // Act
        var damage = match.Damage(gunner!);

        // Assert
        amplifier.Should().NotBeNull();
        damage.Should().Be(TowerDefenseData.Tower(TowerKind.Gunner).Damage + 5);
    }

    [Fact]
    public void EasyDifficulty_DoublesDamageForEveryAttackTower()
    {
        // Arrange
        var match = new TowerDefenseMatch(
            TowerDefenseData.Maps[0],
            TowerDefenseData.Difficulties.Single(difficulty => difficulty.Id == "easy"),
            endless: false,
            startingGoldBonus: 5000);
        var attackTowers = TowerDefenseData.Towers.Where(tower => !tower.IsSupport).ToArray();
        var freeCells = Enumerable.Range(0, TowerDefenseMatch.Columns * TowerDefenseMatch.Rows)
            .Where(cell => !match.IsBlocked(cell))
            .Take(attackTowers.Length)
            .ToArray();

        // Act
        var builtTowers = attackTowers.Select((spec, index) => match.Build(freeCells[index], spec.Kind)).ToArray();

        // Assert
        builtTowers.Should().OnlyContain(tower => tower != null);
        foreach (var tower in builtTowers)
            match.Damage(tower!).Should().Be(tower!.Spec.Damage * 2);
    }

    [Fact]
    public void EasyDifficulty_KeepsTowerAndUpgradePricesUnchanged()
    {
        // Arrange
        var easy = new TowerDefenseMatch(
            TowerDefenseData.Maps[0],
            TowerDefenseData.Difficulties.Single(difficulty => difficulty.Id == "easy"),
            endless: false,
            startingGoldBonus: 5000);
        var normal = new TowerDefenseMatch(
            TowerDefenseData.Maps[0],
            TowerDefenseData.Difficulties.Single(difficulty => difficulty.Id == "normal"),
            endless: false,
            startingGoldBonus: 5000);
        var spec = TowerDefenseData.Tower(TowerKind.Gunner);
        var easyTower = easy.Build(easy.FirstFreeCell(0), spec.Kind);
        var normalTower = normal.Build(normal.FirstFreeCell(0), spec.Kind);

        // Act
        var easyUpgradeCost = easy.UpgradeCost(easyTower!, UpgradeTrack.Damage);
        var normalUpgradeCost = normal.UpgradeCost(normalTower!, UpgradeTrack.Damage);

        // Assert
        easyTower!.Invested.Should().Be(spec.Cost);
        easyUpgradeCost.Should().Be(normalUpgradeCost);
        easyUpgradeCost.Should().Be((int)(Math.Ceiling(spec.Cost * 0.5 / 5) * 5));
    }

    [Fact]
    public void Maps_ContainFourteenCampaignAndThreeBonusMissions_KeepObstaclesOffRoads()
    {
        // Arrange
        var maps = TowerDefenseData.Maps;

        // Act
        var collisions = maps.SelectMany(map =>
        {
            var roadCells = GetRoadCells(map);
            return map.Trees.Concat(map.Rocks).Intersect(roadCells).Select(cell => (map.Id, Cell: cell));
        }).ToArray();

        // Assert
        maps.Where(map => !map.IsBonus).Should().HaveCount(14);
        maps.Where(map => map.IsBonus).Should().HaveCount(3);
        maps.Select(map => map.Id).Should().OnlyHaveUniqueItems();
        collisions.Should().BeEmpty();
    }

    [Fact]
    public void BonusSniperMission_RequiresTheSniperTower()
    {
        // Arrange
        var mission = TowerDefenseData.Maps.Single(map => map.Id == "bonus-sniper");

        // Act
        var requiredTower = mission.RequiredTower;

        // Assert
        mission.IsBonus.Should().BeTrue();
        requiredTower.Should().Be(TowerKind.Sniper);
    }

    [Fact]
    public void FlakCannon_IsAirOnlyAndUsesTheProposedStats()
    {
        // Arrange
        var flak = TowerDefenseData.Tower(TowerKind.FlakCannon);

        // Act
        var canHitAir = flak.CanHit(EnemyClass.Air);
        var canHitGround = flak.CanHit(EnemyClass.Infantry);

        // Assert
        flak.Cost.Should().Be(350);
        flak.Range.Should().Be(190);
        flak.Damage.Should().Be(32);
        flak.Cooldown.Should().Be(18);
        flak.VsAir.Should().Be(2);
        canHitAir.Should().BeTrue();
        canHitGround.Should().BeFalse();
        TowerDefenseData.TowerUnlockCost(TowerKind.FlakCannon).Should().Be(600);
    }

    [Fact]
    public void BonusAirDefenseMission_RequiresFlakAndSpawnsOnlyHelicopters()
    {
        // Arrange
        var mission = TowerDefenseData.Maps.Single(map => map.Id == "bonus-air-defense");

        // Act
        var groups = TowerDefenseData.WaveGroups(20, mission);
        var campaignGroups = Enumerable.Range(1, TowerDefenseData.CampaignWaves)
            .SelectMany(wave => TowerDefenseData.WaveGroups(wave, mission));

        // Assert
        campaignGroups.Should().OnlyContain(group => group.Kind == EnemyKind.Helicopter);
        mission.IsBonus.Should().BeTrue();
        mission.RequiredTower.Should().Be(TowerKind.FlakCannon);
        groups.Should().ContainSingle();
        groups[0].Kind.Should().Be(EnemyKind.Helicopter);
        groups[0].Count.Should().Be(32);
        TowerDefenseData.WaveGroups(21, mission).Single().Kind.Should().Be(EnemyKind.Helicopter);
    }

    [Fact]
    public void Build_AirDefenseBonusMission_RejectsNonFlakTowers()
    {
        // Arrange
        var mission = TowerDefenseData.Maps.Single(map => map.Id == "bonus-air-defense");
        var match = new TowerDefenseMatch(mission, TowerDefenseData.Difficulties[1], endless: false);
        var cell = match.FirstFreeCell(0);

        // Act
        var rejected = match.Build(cell, TowerKind.Gunner);
        var accepted = match.Build(cell, TowerKind.FlakCannon);

        // Assert
        rejected.Should().BeNull();
        accepted.Should().NotBeNull();
    }

    [Fact]
    public void Build_SniperBonusMission_RejectsOtherTowerKinds()
    {
        // Arrange
        var mission = TowerDefenseData.Maps.Single(map => map.Id == "bonus-sniper");
        var match = new TowerDefenseMatch(mission, TowerDefenseData.Difficulties[1], endless: false);
        var cell = match.FirstFreeCell(0);

        // Act
        var rejected = match.Build(cell, TowerKind.Gunner);
        var accepted = match.Build(cell, TowerKind.Sniper);

        // Assert
        rejected.Should().BeNull();
        accepted.Should().NotBeNull();
    }

    [Fact]
    public void Move_TowerToFreeTile_UpdatesItsPosition()
    {
        // Arrange
        var match = new TowerDefenseMatch(TowerDefenseData.Maps[0], TowerDefenseData.Difficulties[1], endless: false);
        var origin = match.FirstFreeCell(0);
        var destination = Enumerable.Range(0, TowerDefenseMatch.Columns * TowerDefenseMatch.Rows)
            .First(cell => cell != origin && !match.IsBlocked(cell));

        var tower = match.Build(origin, TowerKind.Gunner);

        // Act
        var moved = match.Move(tower!, destination);

        // Assert
        moved.Should().BeTrue();
        tower!.Cell.Should().Be(destination);
        match.TowerAt(origin).Should().BeNull();
        match.TowerAt(destination).Should().BeSameAs(tower);
    }

    [Fact]
    public void Move_TowerOntoRoad_IsRejected()
    {
        // Arrange
        var match = new TowerDefenseMatch(TowerDefenseData.Maps[0], TowerDefenseData.Difficulties[1], endless: false);
        var tower = match.Build(match.FirstFreeCell(0), TowerKind.Gunner);

        // Act
        var moved = match.Move(tower!, 10);

        // Assert
        moved.Should().BeFalse();
    }

    [Fact]
    public void InterWaveDelay_MissionBreak_IsApproximatelyTenSeconds()
    {
        // Arrange
        var delayMilliseconds = TowerDefenseMatch.InterWaveDelayTicks * TowerDefenseMatch.TickMilliseconds;

        // Act
        var delaySeconds = delayMilliseconds / 1000.0;

        // Assert
        delaySeconds.Should().BeInRange(9, 11);
    }

    [Fact]
    public void Tower_WhenKillingEnemy_TracksKillsAndDamageDealt()
    {
        // Arrange
        var match = new TowerDefenseMatch(
            TowerDefenseData.Maps[0],
            TowerDefenseData.Difficulties[0],
            endless: false,
            startingGoldBonus: 1000);
        var towerCell = match.FirstFreeCell(0);
        var sniper = match.Build(towerCell, TowerKind.Sniper);
        match.StartWave();

        // Act
        var maxTicks = 1000;
        while (match.State == MatchState.Running && sniper!.Kills == 0 && maxTicks-- > 0)
        {
            match.Advance();
        }

        // Assert
        sniper!.DamageDealt.Should().BeGreaterThan(0);
        sniper.Kills.Should().BeGreaterThanOrEqualTo(1);
    }

    private static HashSet<int> GetRoadCells(MapSpec map)
    {
        var cells = new HashSet<int>();
        foreach (var route in map.Routes)
        {
            for (var i = 1; i < route.Length; i++)
            {
                var (column, row) = route[i - 1];
                var (endColumn, endRow) = route[i];
                while (true)
                {
                    if (column is >= 0 and < TowerDefenseMatch.Columns && row is >= 0 and < TowerDefenseMatch.Rows)
                        cells.Add(row * TowerDefenseMatch.Columns + column);

                    if ((column, row) == (endColumn, endRow))
                        break;

                    column += Math.Sign(endColumn - column);
                    row += Math.Sign(endRow - row);
                }
            }
        }

        return cells;
    }
}
