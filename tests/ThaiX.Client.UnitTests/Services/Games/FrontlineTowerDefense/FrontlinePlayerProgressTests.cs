using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlinePlayerProgressTests
{
    [Fact]
    public void CreateNew_UsesLegacyDefaultCreditsFundsAndUnlocks()
    {
        var progress = FrontlinePlayerProgress.CreateNew();

        progress.Credits.Should().Be(1000);
        progress.StartingFunds.Should().Be(500);
        progress.IsTowerUnlocked(2).Should().BeTrue();
        progress.IsTowerUnlocked(3).Should().BeTrue();
        progress.IsTowerUnlocked(4).Should().BeTrue();
        progress.IsTowerUnlocked(13).Should().BeTrue();
        progress.IsLevelUnlocked(0).Should().BeTrue();
        progress.TowerUnlocks.Should().HaveCount(20);
        progress.LevelScores.Should().HaveCount(40);
    }

    [Fact]
    public void TryPurchase_TowerUnlock_DeductsCreditsAndUnlocksSlot()
    {
        var progress = FrontlinePlayerProgress.CreateNew();
        var item = FrontlineWarMartCatalog.All.Single(value => value.Id == "missile-launcher");

        var purchased = progress.TryPurchase(item);

        purchased.Should().BeTrue();
        progress.Credits.Should().Be(1000);
        progress.IsTowerUnlocked(1).Should().BeTrue();
    }

    [Fact]
    public void TryPurchase_StartingFundsBonus_AddsFundsAndDeductsCredits()
    {
        var progress = FrontlinePlayerProgress.CreateNew();
        var item = FrontlineWarMartCatalog.All.Single(value => value.Id == "starting-funds");

        var purchased = progress.TryPurchase(item);

        purchased.Should().BeTrue();
        progress.Credits.Should().Be(0);
        progress.StartingFunds.Should().Be(700);
    }

    [Fact]
    public void SaveLevelScore_OnlyReplacesBestScore()
    {
        var progress = FrontlinePlayerProgress.CreateNew();

        progress.SaveLevelScore(0, 250).Should().BeTrue();
        progress.SaveLevelScore(0, 200).Should().BeFalse();
        progress.LevelScores[0].Should().Be(250);
    }

    [Fact]
    public void ToLegacySaveData_AndLoad_PreserveProgression()
    {
        var original = FrontlinePlayerProgress.CreateNew();
        original.TryPurchase(FrontlineWarMartCatalog.All.Single(item => item.Id == "missile-launcher"));
        original.SaveLevelScore(0, 350);

        var restored = FrontlinePlayerProgress.Load(original.ToLegacySaveData());

        restored.Credits.Should().Be(original.Credits);
        restored.StartingFunds.Should().Be(original.StartingFunds);
        restored.IsTowerUnlocked(1).Should().BeTrue();
        restored.LevelScores[0].Should().Be(350);
    }
}
