namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public sealed record FrontlineWaveGroup(
    string EnemyType,
    int Count,
    double SpawnIntervalUnits,
    double DelayAfterUnits);

public sealed record FrontlineWaveSchedule(IReadOnlyList<FrontlineWaveGroup> Groups)
{
    public static FrontlineWaveSchedule CampaignLevelOne { get; } = new(
    [
        new("enemy3", 4, 15, 300),
        new("enemy3", 4, 15, 300),
        new("enemy3", 5, 15, 300),
        new("enemy4", 15, 15, 800),
        new("enemy4", 8, 15, 500),
        new("enemy4", 8, 15, 500),
        new("enemy2", 7, 15, 1000),
        new("enemy2", 7, 15, 1000),
        new("enemy1", 8, 15, 1000),
        new("enemy1", 8, 15, 1000),
        new("enemy5", 6, 15, 1000),
        new("enemy5", 6, 15, 1000),
        new("enemy1", 10, 15, 1000),
        new("enemy1", 10, 15, 1000),
        new("Jeep4", 3, 45, 1000),
        new("enemy5", 6, 15, 1000),
        new("enemy5", 6, 15, 1000),
        new("enemy1", 10, 15, 1000),
        new("enemy1", 10, 15, 1000),
        new("Jeep3", 5, 45, 1000),
        new("Jeep4", 5, 45, 1000),
        new("enemy1", 12, 15, 500),
        new("enemy5", 12, 15, 500),
        new("Jeep1", 5, 45, 50)
    ]);
}

public sealed class FrontlineWaveScheduler
{
    private readonly Queue<FrontlineWaveGroup> _pendingGroups;
    private FrontlineWaveGroup? _activeGroup;
    private int _remainingInGroup;
    private double _spawnElapsedUnits;
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
    public bool AllGroupsDispatched { get; private set; }
    public bool IsComplete { get; private set; }

    public IReadOnlyList<string> Advance(double elapsedUnits, bool enemiesRemain)
    {
        if (elapsedUnits < 0 || double.IsNaN(elapsedUnits) || double.IsInfinity(elapsedUnits))
            throw new ArgumentOutOfRangeException(nameof(elapsedUnits));

        if (IsComplete)
            return [];

        var spawned = new List<string>();

        if (_activeGroup is not null)
        {
            _spawnElapsedUnits += elapsedUnits;
            if (_remainingInGroup > 0 && _spawnElapsedUnits >= _activeGroup.SpawnIntervalUnits)
            {
                _spawnElapsedUnits = 0;
                _remainingInGroup--;
                spawned.Add(_activeGroup.EnemyType);
            }

            if (_remainingInGroup == 0)
                _activeGroup = null;
        }

        if (!AllGroupsDispatched)
        {
            _nextGroupCountdownUnits -= elapsedUnits;
            if (_nextGroupCountdownUnits <= 0)
            {
                if (_pendingGroups.TryDequeue(out var nextGroup))
                {
                    CurrentGroup++;
                    _activeGroup = nextGroup;
                    _remainingInGroup = nextGroup.Count;
                    _spawnElapsedUnits = 0;
                    _nextGroupCountdownUnits = nextGroup.DelayAfterUnits * 2;
                }
                else
                {
                    AllGroupsDispatched = true;
                }
            }
        }
        else if (_activeGroup is null && !enemiesRemain)
        {
            IsComplete = true;
        }

        return spawned;
    }
}
