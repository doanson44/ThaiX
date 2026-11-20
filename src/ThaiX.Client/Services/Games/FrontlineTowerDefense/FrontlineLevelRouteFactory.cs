namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public sealed record FrontlineLevelRoute(
    FrontlineGridPoint Start,
    FrontlineGridPoint End,
    FrontlinePath Path);

public sealed class FrontlineLevelRouteFactory(FrontlineGridPathfinder pathfinder)
{
    private readonly FrontlineGridPathfinder _pathfinder =
        pathfinder ?? throw new ArgumentNullException(nameof(pathfinder));
    private readonly Dictionary<string, IReadOnlyList<FrontlineLevelRoute>> _routeCache =
        new(StringComparer.Ordinal);

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

        if (!_routeCache.TryGetValue(level.Id, out var reachableRoutes))
        {
            var map = level.CreateGridMap();
            var routes = new List<FrontlineLevelRoute>();

            foreach (var start in level.StartPoints)
            {
                foreach (var end in level.EndPoints)
                {
                    var gridPath = _pathfinder.FindPath(map, start, end);
                    if (gridPath is null || gridPath.Count < 2)
                        continue;

                    routes.Add(new FrontlineLevelRoute(
                        start,
                        end,
                        FrontlinePath.FromGridPath(gridPath)));
                }
            }

            reachableRoutes = routes;
            _routeCache[level.Id] = reachableRoutes;
        }

        if (reachableRoutes.Count == 0)
            return false;

        route = reachableRoutes[random.Next(reachableRoutes.Count)];
        return true;
    }
}
