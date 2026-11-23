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
        route = new FrontlineLevelRoute(start, end, CreatePath(level, start, end));
        return true;
    }

    public FrontlinePath CreateDefaultPath(FrontlineLevelMapDefinition level)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (level.StartPoints.Count == 0 || level.EndPoints.Count == 0)
            throw new FrontlineRouteUnavailableException(level.Id);

        return CreatePath(level, level.StartPoints[0], level.EndPoints[0]);
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
