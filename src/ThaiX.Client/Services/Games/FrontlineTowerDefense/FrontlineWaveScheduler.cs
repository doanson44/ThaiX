namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public sealed record FrontlineWaveGroup(
    string EnemyType,
    int Count,
    double SpawnIntervalUnits,
    double DelayAfterUnits);

public sealed record FrontlineWaveSchedule(IReadOnlyList<FrontlineWaveGroup> Groups);

public sealed class FrontlineWaveScheduler
{
    private sealed class ActiveSpawner(FrontlineWaveGroup group)
    {
        public FrontlineWaveGroup Group { get; } = group;
        public int Remaining { get; set; } = group.Count;
        public double ElapsedUnits { get; set; }
    }

    private readonly Queue<FrontlineWaveGroup> _pendingGroups;
    private readonly List<ActiveSpawner> _activeSpawners = [];
    private double _nextGroupCountdownUnits = 1000;

    public FrontlineWaveScheduler(FrontlineWaveSchedule schedule)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        _pendingGroups = new Queue<FrontlineWaveGroup>(schedule.Groups);
        if (_pendingGroups.Any(group => group.Count <= 0
            || group.SpawnIntervalUnits <= 0
            || group.DelayAfterUnits < 0
            || string.IsNullOrWhiteSpace(group.EnemyType)))
            throw new ArgumentException("Wave groups must contain valid spawn definitions.", nameof(schedule));

        TotalGroups = _pendingGroups.Count;
    }

    public int TotalGroups { get; }
    public int CurrentGroup { get; private set; }
    public int ActiveSpawnerCount => _activeSpawners.Count;
    public bool AllGroupsDispatched { get; private set; }
    public bool IsComplete { get; private set; }

    public IReadOnlyList<string> Advance(double elapsedUnits, bool enemiesRemain)
    {
        if (elapsedUnits < 0 || double.IsNaN(elapsedUnits) || double.IsInfinity(elapsedUnits))
            throw new ArgumentOutOfRangeException(nameof(elapsedUnits));

        if (IsComplete)
            return [];

        var spawned = new List<string>();

        for (var index = _activeSpawners.Count - 1; index >= 0; index--)
        {
            var spawner = _activeSpawners[index];
            if (spawner.Remaining <= 0)
            {
                _activeSpawners.RemoveAt(index);
                continue;
            }

            spawner.ElapsedUnits += elapsedUnits;
            if (spawner.ElapsedUnits < spawner.Group.SpawnIntervalUnits)
                continue;

            spawner.ElapsedUnits = 0;
            spawner.Remaining--;
            spawned.Add(spawner.Group.EnemyType);
        }

        if (!AllGroupsDispatched)
        {
            _nextGroupCountdownUnits -= elapsedUnits;
            if (_nextGroupCountdownUnits <= 0)
            {
                if (_pendingGroups.TryDequeue(out var nextGroup))
                {
                    CurrentGroup++;
                    _activeSpawners.Add(new ActiveSpawner(nextGroup));
                    _nextGroupCountdownUnits = nextGroup.DelayAfterUnits * 2;
                }
                else
                {
                    AllGroupsDispatched = true;
                }
            }
        }

        if (AllGroupsDispatched && _activeSpawners.Count == 0 && !enemiesRemain)
            IsComplete = true;

        return spawned;
    }
}
