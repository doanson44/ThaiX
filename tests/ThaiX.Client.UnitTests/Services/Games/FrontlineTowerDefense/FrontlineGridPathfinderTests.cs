using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineGridPathfinderTests
{
    private readonly FrontlineGridPathfinder _pathfinder = new();

    [Fact]
    public void FindPath_WithOpenDiagonal_CanMoveDiagonally()
    {
        var map = new FrontlineGridMap(new bool[,]
        {
            { true, true },
            { true, true }
        });

        var path = _pathfinder.FindPath(map, new FrontlineGridPoint(0, 0), new FrontlineGridPoint(1, 1));

        path.Should().NotBeNull();
        path.Should().Equal(
            new FrontlineGridPoint(0, 0),
            new FrontlineGridPoint(1, 1));
    }

    [Fact]
    public void FindPath_WhenDiagonalWouldCutBlockedCorner_UsesOrthogonalRoute()
    {
        var map = new FrontlineGridMap(new bool[,]
        {
            { true, false },
            { false, true }
        });

        var path = _pathfinder.FindPath(map, new FrontlineGridPoint(0, 0), new FrontlineGridPoint(1, 1));

        path.Should().BeNull();
    }

    [Fact]
    public void FindPath_WhenRouteIsBlocked_ReturnsNull()
    {
        var map = new FrontlineGridMap(new bool[,]
        {
            { true, false, true },
            { true, false, true },
            { true, false, true }
        });

        var path = _pathfinder.FindPath(map, new FrontlineGridPoint(0, 0), new FrontlineGridPoint(2, 0));

        path.Should().BeNull();
    }

    [Fact]
    public void FindPath_WhenStartEqualsEnd_ReturnsSingleCell()
    {
        var map = new FrontlineGridMap(new bool[,] { { true } });

        var path = _pathfinder.FindPath(map, new FrontlineGridPoint(0, 0), new FrontlineGridPoint(0, 0));

        path.Should().ContainSingle().Which.Should().Be(new FrontlineGridPoint(0, 0));
    }

    [Fact]
    public void FromGridPath_MapsCellsToWorldSpaceCenters()
    {
        var gridPath = new[]
        {
            new FrontlineGridPoint(0, 0),
            new FrontlineGridPoint(1, 1)
        };

        var path = FrontlinePath.FromGridPath(gridPath);

        path.Points.Should().Equal(
            new FrontlinePoint(16, 16),
            new FrontlinePoint(48, 48));
    }

    [Fact]
    public void FindPath_WhenStartIsBlocked_ReturnsNull()
    {
        var map = new FrontlineGridMap(new bool[,] { { false, true } });

        var path = _pathfinder.FindPath(map, new FrontlineGridPoint(0, 0), new FrontlineGridPoint(1, 0));

        path.Should().BeNull();
    }
}
