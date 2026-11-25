using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineTutorialTrackerTests
{
    [Fact]
    public void Evaluate_WithoutTowers_OpensTheWelcomeStep()
    {
        var tracker = new FrontlineTutorialTracker();

        tracker.Evaluate(null, 0, 0, 0).Should().Be(0);
        FrontlineTutorialTracker.CanClose(0).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_WhenALaterStepTriggers_ReplacesTheOpenDialog()
    {
        var tracker = new FrontlineTutorialTracker();
        tracker.Evaluate(null, 0, 0, 0).Should().Be(0);

        // Buying a tower fulfils the placement step and replaces the open welcome dialog.
        tracker.Evaluate(null, 1, 0, 0).Should().Be(1);
    }

    [Fact]
    public void Evaluate_AfterClosingTheWelcomeStep_AdvancesToThePlacementStep()
    {
        var tracker = new FrontlineTutorialTracker();
        tracker.Evaluate(null, 0, 0, 0).Should().Be(0);
        tracker.Close();

        tracker.Evaluate(null, 1, 0, 0).Should().Be(1);
        FrontlineTutorialTracker.CanClose(1).Should().BeFalse();
    }

    [Fact]
    public void Evaluate_AfterClosingPlacementStep_RequiresADroppedTowerForTheUpgradeStep()
    {
        var tracker = new FrontlineTutorialTracker();
        tracker.Evaluate(null, 0, 0, 0);
        tracker.Close();
        tracker.Evaluate(null, 1, 0, 0);
        tracker.Close();

        tracker.Evaluate(null, 1, 0, 0).Should().BeNull();

        var tower = BuildTower();
        tracker.Evaluate(tower, 1, 0, 0).Should().Be(2);
        FrontlineTutorialTracker.CanClose(2).Should().BeTrue();
    }

    [Fact]
    public void Evaluate_AfterUpgradeStep_WaitsForAFirstKill()
    {
        var tracker = new FrontlineTutorialTracker();
        tracker.Evaluate(null, 0, 0, 0);
        tracker.Close();
        tracker.Evaluate(null, 1, 0, 0);
        tracker.Close();
        tracker.Evaluate(BuildTower(), 1, 0, 0);
        tracker.Close();

        tracker.Evaluate(null, 1, 2, 0).Should().BeNull();
        tracker.Evaluate(null, 1, 0, 20).Should().Be(3);
    }

    [Fact]
    public void Evaluate_WhenSupportStepPrecedesTheFinalStep_WaitsForTheEnemyWaveToThinOut()
    {
        var tracker = new FrontlineTutorialTracker();
        tracker.Evaluate(null, 0, 0, 0);
        tracker.Close();
        tracker.Evaluate(null, 1, 0, 0);
        tracker.Close();
        tracker.Evaluate(BuildTower(), 1, 0, 0);
        tracker.Close();
        tracker.Evaluate(null, 1, 0, 20);
        tracker.Close();

        tracker.Evaluate(null, 1, 10, 20).Should().Be(4);
        tracker.Close();

        tracker.Evaluate(null, 1, 8, 20).Should().BeNull();
        tracker.Evaluate(null, 1, 3, 20).Should().Be(5);
        tracker.Close();

        tracker.IsFinished.Should().BeTrue();
        tracker.Evaluate(null, 1, 0, 20).Should().BeNull();
    }

    [Fact]
    public void Reset_RestartsTheDialogSequenceForTheNextMission()
    {
        var tracker = new FrontlineTutorialTracker();
        tracker.Evaluate(null, 0, 0, 0).Should().Be(0);

        tracker.Reset();

        tracker.Current.Should().BeNull();
        tracker.IsFinished.Should().BeFalse();
        tracker.Evaluate(null, 0, 0, 0).Should().Be(0);
    }

    private static FrontlineTower BuildTower()
    {
        var simulation = new FrontlineGameSimulation(
            new FrontlinePath([new FrontlinePoint(0, 0), new FrontlinePoint(100, 0)]),
            1000);
        var definition = FrontlineTowerCatalog.Find("tower2")!.CreateDefinition();
        simulation.TryBuildTower(definition, new FrontlinePoint(50, 50)).Should().BeTrue();
        return simulation.Towers.Single();
    }
}
