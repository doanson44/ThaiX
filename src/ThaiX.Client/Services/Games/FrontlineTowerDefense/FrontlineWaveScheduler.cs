namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public sealed record FrontlineWaveGroup(
    string EnemyType,
    int Count,
    double SpawnIntervalUnits,
    double DelayAfterUnits);

public sealed record FrontlineWaveSchedule(IReadOnlyList<FrontlineWaveGroup> Groups);

public sealed record FrontlineSpawnRequest(string EnemyType, int WaveNumber);

public sealed class FrontlineWaveScheduler
{
    private sealed class ActiveSpawner(FrontlineWaveGroup group, int waveNumber)
    {
        public FrontlineWaveGroup Group { get; } = group;
        public int WaveNumber { get; } = waveNumber;
        public int Remaining { get; set; } = group.Count;
        public double ElapsedUnits { get; set; }
    }

    private readonly Queue<FrontlineWaveGroup> _pendingGroups;
    private readonly List<ActiveSpawner> _activeSpawners = [];
    private double _nextGroupCountdownUnits = 500;
    private int _manualStartCooldownTicks = 200;

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
    public bool CanManuallyStart => _manualStartCooldownTicks >= 200 && _pendingGroups.Count > 0;
    public bool AllGroupsDispatched { get; private set; }
    public bool IsComplete { get; private set; }

    public bool TryStartNextGroup()
    {
        if (!CanManuallyStart || !_pendingGroups.TryDequeue(out var nextGroup))
            return false;

        StartGroup(nextGroup);
        _manualStartCooldownTicks = 0;
        return true;
    }

    public IReadOnlyList<FrontlineSpawnRequest> Advance(double elapsedUnits, bool enemiesRemain)
    {
        if (elapsedUnits < 0 || double.IsNaN(elapsedUnits) || double.IsInfinity(elapsedUnits))
            throw new ArgumentOutOfRangeException(nameof(elapsedUnits));

        if (IsComplete)
            return [];

        var wasAllGroupsDispatched = AllGroupsDispatched;
        _manualStartCooldownTicks++;
        var spawned = new List<FrontlineSpawnRequest>();

        var initialSpawnerCount = _activeSpawners.Count;
        for (var index = 0; index < initialSpawnerCount && index < _activeSpawners.Count; index++)
        {
            var spawner = _activeSpawners[index];
            spawner.ElapsedUnits += elapsedUnits;
            if (spawner.ElapsedUnits >= spawner.Group.SpawnIntervalUnits)
            {
                spawner.ElapsedUnits = 0;
                spawner.Remaining--;
                spawned.Add(new FrontlineSpawnRequest(spawner.Group.EnemyType, spawner.WaveNumber));
            }

            if (spawner.Remaining <= 0)
                _activeSpawners.RemoveAt(index);
        }

        if (!AllGroupsDispatched)
        {
            _nextGroupCountdownUnits -= elapsedUnits;
            if (_nextGroupCountdownUnits <= 0)
            {
                if (_pendingGroups.TryDequeue(out var nextGroup))
                    StartGroup(nextGroup);
                else
                    AllGroupsDispatched = true;
            }
        }

        if (wasAllGroupsDispatched && AllGroupsDispatched && !enemiesRemain && spawned.Count == 0)
            IsComplete = true;

        return spawned;
    }

    private void StartGroup(FrontlineWaveGroup group)
    {
        CurrentGroup++;
        _activeSpawners.Add(new ActiveSpawner(group, CurrentGroup));
        _nextGroupCountdownUnits = group.DelayAfterUnits * 2;
    }
}
