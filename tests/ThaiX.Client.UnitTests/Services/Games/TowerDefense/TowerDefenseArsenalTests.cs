using ThaiX.Client.Services.Games.TowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.TowerDefense;

public sealed class TowerDefenseArsenalTests
{
    [Fact]
    public void Towers_ShouldExposeTenUniqueKinds()
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
    public void BioChemicalTower_ShouldUsePoisonEffect()
    {
        // Arrange
        var tower = TowerDefenseData.Tower(TowerKind.BioChemical);

        // Act
        var effect = tower.Effect;

        // Assert
        effect.Should().Be(TowerEffect.Poison);
    }

    [Fact]
    public void Maps_ShouldContainFourteenMissionsWithNoObstaclesOnRoads()
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
        maps.Should().HaveCount(14);
        maps.Select(map => map.Id).Should().OnlyHaveUniqueItems();
        collisions.Should().BeEmpty();
    }

    [Fact]
    public void InterWaveDelay_ShouldBeApproximatelyTenSeconds()
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
