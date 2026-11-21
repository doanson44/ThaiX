using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineLevelRouteFactoryTests
{
    private readonly FrontlineLevelRouteFactory _factory = new(new FrontlineGridPathfinder());

    [Fact]
    public void TryCreateRandomRoute_LevelOne_ReturnsAuthoredWorldPath()
    {
        var level = FrontlineLevelMapCatalog.Find("level-01")!;

        var created = _factory.TryCreateRandomRoute(level, new Random(42), out var route);

        created.Should().BeTrue();
        route.Should().NotBeNull();
        route!.Start.Should().Be(level.StartPoints[0]);
        route.End.Should().Be(level.EndPoints[0]);
        route.Path.Points[0].Should().Be(new FrontlinePoint(48, -16));
        route.Path.Points[^1].Should().Be(new FrontlinePoint(528, 16));
    }

    [Fact]
    public void TryCreateRandomRoute_BonusLevelFour_AlwaysSelectsAReachableEndpointPair()
    {
        var level = FrontlineLevelMapCatalog.Find("bonus-04")!;

        for (var seed = 0; seed < 100; seed++)
        {
            var created = _factory.TryCreateRandomRoute(level, new Random(seed), out var route);

            created.Should().BeTrue();
            route.Should().NotBeNull();
            var selectedRoute = route!;
            level.StartPoints.Should().Contain(selectedRoute.Start);
            level.EndPoints.Should().Contain(selectedRoute.End);
            selectedRoute.Path.Points.Should().HaveCountGreaterThan(1);
        }
    }

    [Fact]
    public void TryCreateRandomRoute_AllMaps_OnlySourceConnectedMapsReturnRoutes()
    {
        var reachableMapIds = new HashSet<string>(StringComparer.Ordinal)
        {
            "level-01",
            "bonus-01",
            "bonus-04"
        };

        foreach (var level in FrontlineLevelMapCatalog.All)
        {
            var created = _factory.TryCreateRandomRoute(level, new Random(42), out var route);

            created.Should().Be(reachableMapIds.Contains(level.Id));
            if (created)
                route.Should().NotBeNull();
            else
                route.Should().BeNull();
        }
    }

    [Fact]
    public void TryCreateRandomRoute_LevelWithoutPath_ReturnsFalse()
    {
        var level = FrontlineLevelMapCatalog.Find("level-08")!;

        var created = _factory.TryCreateRandomRoute(level, new Random(42), out var route);

        created.Should().BeFalse();
        route.Should().BeNull();
    }
}
