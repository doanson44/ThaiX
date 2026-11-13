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
    public void InterWaveDelay_ShouldBeApproximatelyTenSeconds()
    {
        // Arrange
        var delayMilliseconds = TowerDefenseMatch.InterWaveDelayTicks * TowerDefenseMatch.TickMilliseconds;

        // Act
        var delaySeconds = delayMilliseconds / 1000.0;

        // Assert
        delaySeconds.Should().BeInRange(9, 11);
    }
}
