namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public sealed record FrontlineLevelRoute(
    FrontlineGridPoint Start,
    FrontlineGridPoint End,
    FrontlinePath Path);

public sealed class FrontlineLevelRouteFactory(FrontlineGridPathfinder pathfinder)
{
    private readonly FrontlineGridPathfinder _pathfinder =
        pathfinder ?? throw new ArgumentNullException(nameof(pathfinder));

    public bool TryCreateRandomRoute(
        FrontlineLevelMapDefinition level,
        Random random,
        out FrontlineLevelRoute? route)
    {
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(random);

        route = null;
        if (level.StartPoints.Count == 0 || level.EndPoints.Count == 0)
            return false;

        var map = level.CreateGridMap();
        var reachableRoutes = new List<FrontlineLevelRoute>();

        foreach (var start in level.StartPoints)
        {
            foreach (var end in level.EndPoints)
            {
                var gridPath = _pathfinder.FindPath(map, start, end);
                if (gridPath is null || gridPath.Count < 2)
                    continue;

                reachableRoutes.Add(new FrontlineLevelRoute(
                    start,
                    end,
                    FrontlinePath.FromGridPath(gridPath)));
            }
        }

        if (reachableRoutes.Count == 0)
            return false;

        route = reachableRoutes[random.Next(reachableRoutes.Count)];
        return true;
    }
}
