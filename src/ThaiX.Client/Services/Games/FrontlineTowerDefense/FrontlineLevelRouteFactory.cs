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

        var start = level.StartPoints[random.Next(level.StartPoints.Count)];
        var end = level.EndPoints[random.Next(level.EndPoints.Count)];
        var gridPath = _pathfinder.FindPath(level.CreateGridMap(), start, end);
        if (gridPath is null || gridPath.Count < 2)
            return false;

        route = new FrontlineLevelRoute(start, end, FrontlinePath.FromGridPath(gridPath));
        return true;
    }
}
