using ThaiX.Client.Services.Games.TowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.TowerDefense;

public sealed class TowerDefenseArsenalTests
{
    [Fact]
    public void TowerCatalog_ContainsTenUniqueKinds()
    {
        // Arrange
        var towers = TowerDefenseData.Towers;

        // Act
        var kinds = towers.Select(tower => tower.Kind).ToArray();

        // Assert
        towers.Should().HaveCount(10);
        kinds.Should().OnlyHaveUniqueItems();
        Enum.GetValues<TowerKind>().Should().BeEquivalentTo(kinds);
    }

    [Fact]
    public void TowerUnlockCost_AdvancedTowers_UsesProgressivePrices()
    {
        // Arrange
        var unlocks = new[]
        {
            TowerDefenseData.TowerUnlockCost(TowerKind.Shotgun),
            TowerDefenseData.TowerUnlockCost(TowerKind.MachineGun),
            TowerDefenseData.TowerUnlockCost(TowerKind.BioChemical),
            TowerDefenseData.TowerUnlockCost(TowerKind.HeatRay)
        };

        // Act
        var startingTowerUnlocks = Enum.GetValues<TowerKind>()
            .Where(kind => TowerDefenseData.TowerUnlockCost(kind) == 0)
            .ToArray();

        // Assert
        unlocks.Should().BeInAscendingOrder();
        unlocks.Should().Equal(new[] { 250, 500, 750, 1000 });
        startingTowerUnlocks.Should().HaveCount(6);
    }

    [Fact]
    public void BioChemicalTower_UsesPoisonEffect()
    {
        // Arrange
        var tower = TowerDefenseData.Tower(TowerKind.BioChemical);

        // Act
        var effect = tower.Effect;

        // Assert
        effect.Should().Be(TowerEffect.Poison);
    }

    [Fact]
    public void Maps_ContainFourteenMissions_KeepObstaclesOffRoads()
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
        maps.Where(map => map.IsBonus).Should().HaveCount(2);
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
