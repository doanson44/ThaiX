using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Application.UnitTests.Domain.Aggregates;

public sealed class PriceAlertTests
{
    [Fact]
    public void Create_ShouldNormalizeSymbol_AndPreserveNote()
    {
        var alert = PriceAlert.Create(
            " btcusdt ",
            AssetType.CryptoSpot,
            AlertCondition.Above,
            30000m,
            "  Buy low  ",
            true);

        alert.Symbol.Should().Be("BTCUSDT");
        alert.AssetType.Should().Be(AssetType.CryptoSpot);
        alert.Condition.Should().Be(AlertCondition.Above);
        alert.TargetPrice.Should().Be(30000m);
        alert.Note.Should().Be("Buy low");
        alert.IsEnabled.Should().BeTrue();
        alert.IsOneTime.Should().BeTrue();
        alert.TriggerCount.Should().Be(0);
        alert.LastTriggeredAt.Should().BeNull();
        alert.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_WithInvalidSymbol_ShouldThrow()
    {
        Action act = () => PriceAlert.Create(
            string.Empty,
            AssetType.VnStock,
            AlertCondition.Below,
            10m,
            null,
            false);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Update_ShouldApplyNewValues()
    {
        var alert = PriceAlert.Create("VNM", AssetType.VnStock, AlertCondition.Below, 10000m, "note", false);

        alert.Update(AlertCondition.Above, 9500m, "updated", true, false);

        alert.Condition.Should().Be(AlertCondition.Above);
        alert.TargetPrice.Should().Be(9500m);
        alert.Note.Should().Be("updated");
        alert.IsOneTime.Should().BeTrue();
        alert.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void RecordTrigger_ShouldIncrementCount_AndDisableOneTimeAlerts()
    {
        var alert = PriceAlert.Create("BTCUSDT", AssetType.CryptoSpot, AlertCondition.Above, 40000m, null, true);

        var now = DateTime.UtcNow;
        alert.RecordTrigger(now);

        alert.TriggerCount.Should().Be(1);
        alert.LastTriggeredAt.Should().Be(now);
        alert.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public void Disable_ShouldSetIsEnabledFalse()
    {
        var alert = PriceAlert.Create("BTCUSDT", AssetType.CryptoSpot, AlertCondition.Below, 15000m, null, false);

        alert.Disable();

        alert.IsEnabled.Should().BeFalse();
    }
}
