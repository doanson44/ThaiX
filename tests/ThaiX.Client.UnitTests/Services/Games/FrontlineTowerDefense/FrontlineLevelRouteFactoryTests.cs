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
            var expectedRandom = new Random(seed);
            var expectedStart = level.StartPoints[expectedRandom.Next(level.StartPoints.Count)];
            var expectedEnd = level.EndPoints[expectedRandom.Next(level.EndPoints.Count)];

            var created = _factory.TryCreateRandomRoute(level, new Random(seed), out var route);

            created.Should().BeTrue();
            route.Should().NotBeNull();
            route!.Start.Should().Be(expectedStart);
            route.End.Should().Be(expectedEnd);
            route.Path.Points.Should().HaveCountGreaterThan(1);
        }
    }

    [Fact]
    public void TryCreateRandomRoute_AllMaps_SelectsAuthoredEndpointsEvenWhenPathIsUnreachable()
    {
        foreach (var level in FrontlineLevelMapCatalog.All)
        {
            var created = _factory.TryCreateRandomRoute(level, new Random(42), out var route);

            created.Should().Be(level.StartPoints.Count > 0 && level.EndPoints.Count > 0);
            if (created)
            {
                route.Should().NotBeNull();
                route!.Path.Points.Should().HaveCountGreaterThan(1);
            }
            else
            {
                route.Should().BeNull();
            }
        }
    }

    [Fact]
    public void TryCreateRandomRoute_LevelWithoutPath_PreservesInvalidLegacyMovement()
    {
        var level = FrontlineLevelMapCatalog.Find("level-08")!;

        var created = _factory.TryCreateRandomRoute(level, new Random(42), out var route);

        created.Should().BeTrue();
        route.Should().NotBeNull();
        route!.Path.UsesGridMovement.Should().BeTrue();
        double.IsNaN(route.Path.Points[1].X).Should().BeTrue();
        double.IsNaN(route.Path.Points[1].Y).Should().BeTrue();
    }
}
