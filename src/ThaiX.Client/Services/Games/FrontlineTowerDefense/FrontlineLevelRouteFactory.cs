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

        var reachableRoutes = new List<FrontlineLevelRoute>();
        foreach (var start in level.StartPoints)
        {
            foreach (var end in level.EndPoints)
            {
                var gridPath = _pathfinder.FindPath(level.CreateGridMap(), start, end);
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

    public FrontlinePath CreateDefaultPath(FrontlineLevelMapDefinition level)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (level.StartPoints.Count == 0 || level.EndPoints.Count == 0)
            throw new FrontlineRouteUnavailableException(level.Id);

        foreach (var start in level.StartPoints)
        {
            foreach (var end in level.EndPoints)
            {
                var gridPath = _pathfinder.FindPath(level.CreateGridMap(), start, end);
                if (gridPath is not null && gridPath.Count >= 2)
                    return FrontlinePath.FromGridPath(gridPath);
            }
        }

        throw new FrontlineRouteUnavailableException(level.Id);
    }

    private FrontlinePath CreatePath(
        FrontlineLevelMapDefinition level,
        FrontlineGridPoint start,
        FrontlineGridPoint end)
    {
        var gridPath = _pathfinder.FindPath(level.CreateGridMap(), start, end);
        if (gridPath is not null && gridPath.Count >= 2)
            return FrontlinePath.FromGridPath(gridPath);

        var startPosition = new FrontlinePoint(
            start.X * FrontlinePath.DefaultGridCellSize + FrontlinePath.DefaultGridCellSize * 0.5,
            start.Y * FrontlinePath.DefaultGridCellSize + FrontlinePath.DefaultGridCellSize * 0.5);
        if (gridPath is { Count: 1 })
        {
            return new FrontlinePath([startPosition, startPosition])
            {
                UsesGridMovement = true
            };
        }

        return new FrontlinePath(
        [
            startPosition,
            new FrontlinePoint(double.NaN, double.NaN)
        ])
        {
            UsesGridMovement = true
        };
    }
}
