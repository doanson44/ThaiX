using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineTowerCatalogTests
{
    [Fact]
    public void All_ContainsElevenStandardAndThreeSupportTypes()
    {
        FrontlineTowerCatalog.All.Should().HaveCount(14);
        FrontlineTowerCatalog.Standard.Should().HaveCount(11);
        FrontlineTowerCatalog.Support.Should().HaveCount(3);
    }

    [Fact]
    public void Find_MissileLauncher_ReturnsVerifiedStatsAndUnlockCost()
    {
        var tower = FrontlineTowerCatalog.Find("tower1");

        tower.Should().NotBeNull();
        tower!.BuildCost.Should().Be(800);
        tower.Damage.Should().Be(80);
        tower.RateOfFire.Should().Be(1);
        tower.Range.Should().Be(150);
        tower.TargetMode.Should().Be(FrontlineTargetMode.Heat);
        tower.WarMartUnlockCost.Should().Be(8000);
    }

    [Fact]
    public void Find_DefaultUnlockedTower_HasNoWarMartUnlockCost()
    {
        var tower = FrontlineTowerCatalog.Find("tower4");

        tower.Should().NotBeNull();
        tower!.UnlockIndex.Should().Be(4);
        tower.WarMartUnlockCost.Should().BeNull();
    }

    [Fact]
    public void Find_NapalmSupportStrike_UsesSupportShotLimit()
    {
        var tower = FrontlineTowerCatalog.Find("suptower1");

        tower.Should().NotBeNull();
        tower!.IsSupport.Should().BeTrue();
        tower.ShotLimit.Should().Be(6);
        tower.WarMartUnlockCost.Should().Be(6000);
    }
}
