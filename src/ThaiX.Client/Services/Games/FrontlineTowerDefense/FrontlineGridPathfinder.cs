namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public readonly record struct FrontlineGridPoint(int X, int Y);

public sealed class FrontlineGridMap
{
    private readonly bool[,] _walkable;

    public FrontlineGridMap(bool[,] walkable)
    {
        ArgumentNullException.ThrowIfNull(walkable);
        if (walkable.GetLength(0) == 0 || walkable.GetLength(1) == 0)
            throw new ArgumentException("The grid must have at least one cell.", nameof(walkable));

        _walkable = (bool[,])walkable.Clone();
    }

    public int Width => _walkable.GetLength(0);
    public int Height => _walkable.GetLength(1);

    public bool IsWalkable(FrontlineGridPoint point) =>
        point.X >= 0 && point.X < Width
        && point.Y >= 0 && point.Y < Height
        && _walkable[point.X, point.Y];
}

public sealed class FrontlineGridPathfinder
{
    private static readonly FrontlineGridPoint[] OrthogonalNeighbors =
    [
        new(1, 0),
        new(-1, 0),
        new(0, 1),
        new(0, -1)
    ];

    private static readonly FrontlineGridPoint[] DiagonalNeighbors =
    [
        new(1, 1),
        new(-1, -1),
        new(-1, 1),
        new(1, -1)
    ];

    public IReadOnlyList<FrontlineGridPoint>? FindPath(
        FrontlineGridMap map,
        FrontlineGridPoint start,
        FrontlineGridPoint end)
    {
        ArgumentNullException.ThrowIfNull(map);
        if (!map.IsWalkable(end))
            return null;

        var visited = new HashSet<FrontlineGridPoint> { start };
        var parents = new Dictionary<FrontlineGridPoint, FrontlineGridPoint>();
        var queue = new Queue<FrontlineGridPoint>();
        queue.Enqueue(start);

        while (queue.TryDequeue(out var current))
        {
            if (current == end)
                return ReconstructPath(parents, start, end);

            foreach (var offset in OrthogonalNeighbors)
                TryVisit(new FrontlineGridPoint(current.X + offset.X, current.Y + offset.Y));

            foreach (var offset in DiagonalNeighbors)
            {
                var next = new FrontlineGridPoint(current.X + offset.X, current.Y + offset.Y);
                var horizontal = new FrontlineGridPoint(next.X, current.Y);
                var vertical = new FrontlineGridPoint(current.X, next.Y);

                if (map.IsWalkable(horizontal) && map.IsWalkable(vertical))
                    TryVisit(next);
            }

            void TryVisit(FrontlineGridPoint next)
            {
                if (!map.IsWalkable(next) || !visited.Add(next))
                    return;

                parents[next] = current;
                queue.Enqueue(next);
            }
        }

        return null;
    }

    private static IReadOnlyList<FrontlineGridPoint> ReconstructPath(
        IReadOnlyDictionary<FrontlineGridPoint, FrontlineGridPoint> parents,
        FrontlineGridPoint start,
        FrontlineGridPoint end)
    {
        var path = new List<FrontlineGridPoint>();
        var current = end;

        while (current != start)
        {
            path.Add(current);
            current = parents.TryGetValue(current, out var parent)
                ? parent
                : throw new InvalidOperationException("The path parent chain is incomplete.");
        }

        path.Add(start);
        path.Reverse();
        return path;
    }
}
