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
    public void TryCreateRandomRoute_BonusLevelFour_SelectsStartAndEndIndependently()
    {
        var level = FrontlineLevelMapCatalog.Find("bonus-04")!;

        for (var seed = 0; seed < 100; seed++)
        {
            var created = _factory.TryCreateRandomRoute(level, new Random(seed), out var route);

            created.Should().BeTrue();
            route.Should().NotBeNull();
            level.StartPoints.Should().Contain(route!.Start);
            level.EndPoints.Should().Contain(route.End);
            route.Path.Points.Should().OnlyContain(point => double.IsFinite(point.X) && double.IsFinite(point.Y));
            route.Path.Points.Should().HaveCountGreaterThan(1);
        }
    }

    [Fact]
    public void TryCreateRandomRoute_AllMaps_ReturnsOnlyReachableAuthoredEndpoints()
    {
        foreach (var level in FrontlineLevelMapCatalog.All)
        {
            var created = _factory.TryCreateRandomRoute(level, new Random(42), out var route);

            if (created)
            {
                route.Should().NotBeNull();
                route!.Path.Points.Should().HaveCountGreaterThan(1);
                route.Path.Points.Should().OnlyContain(point =>
                    double.IsFinite(point.X) && double.IsFinite(point.Y));
            }
            else
            {
                route.Should().BeNull();
            }
        }
    }

    [Fact]
    public void TryCreateRandomRoute_EveryLevel_HasAtLeastOneReachableRoute()
    {
        foreach (var level in FrontlineLevelMapCatalog.All)
        {
            var created = _factory.TryCreateRandomRoute(level, new Random(42), out var route);

            created.Should().BeTrue(
                $"level '{level.Id}' must have at least one connected start/end pair");
            route.Should().NotBeNull();
        }
    }
}
