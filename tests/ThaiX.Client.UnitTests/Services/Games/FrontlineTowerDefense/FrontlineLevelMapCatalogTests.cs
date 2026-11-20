using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineLevelMapCatalogTests
{
    [Fact]
    public void All_ContainsFourteenCampaignAndFourBonusMaps()
    {
        FrontlineLevelMapCatalog.All.Should().HaveCount(18);
        FrontlineLevelMapCatalog.Campaign.Should().HaveCount(14);
        FrontlineLevelMapCatalog.Bonus.Should().HaveCount(4);
    }

    [Fact]
    public void Find_LevelOne_PreservesAuthoredEndpointsAndGridCells()
    {
        var level = FrontlineLevelMapCatalog.Find("level-01");

        level.Should().NotBeNull();
        level!.ProgressIndex.Should().Be(1);
        level.UnlockIndex.Should().Be(0);
        level.StartPoints.Should().ContainSingle()
            .Which.Should().Be(new FrontlineGridPoint(1, -1));
        level.EndPoints.Should().ContainSingle()
            .Which.Should().Be(new FrontlineGridPoint(16, 0));
        level.WalkableCells.Should().HaveCount(92);
        level.BlockedCells.Should().HaveCount(55);
    }

    [Fact]
    public void Find_LevelOne_CanBuildAPathFromItsAuthoredStartToEnd()
    {
        var level = FrontlineLevelMapCatalog.Find("level-01")!;
        var pathfinder = new FrontlineGridPathfinder();

        var path = pathfinder.FindPath(
            level.CreateGridMap(),
            level.StartPoints[0],
            level.EndPoints[0]);

        path.Should().NotBeNull();
        path!.First().Should().Be(level.StartPoints[0]);
        path.Last().Should().Be(level.EndPoints[0]);
    }

    [Fact]
    public void Find_BonusLevelOne_PreservesMultipleSpawnAndExitPoints()
    {
        var level = FrontlineLevelMapCatalog.Find("bonus-01");

        level.Should().NotBeNull();
        level!.StartPoints.Should().HaveCount(8);
        level.EndPoints.Should().HaveCount(4);
        level.ProgressIndex.Should().Be(16);
        level.UnlockIndex.Should().Be(15);
        level.WalkableCells.Should().HaveCount(208);
        level.BlockedCells.Should().HaveCount(21);
    }
}
