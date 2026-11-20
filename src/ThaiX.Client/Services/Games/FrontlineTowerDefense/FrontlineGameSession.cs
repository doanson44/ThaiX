namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public enum FrontlineGameSessionState
{
    Playing,
    Paused,
    Victory,
    Defeat,
    RouteUnavailable
}

public sealed class FrontlineGameSession
{
    private readonly FrontlineLevelRouteFactory _routeFactory;
    private readonly Random _random;
    private readonly FrontlineWaveScheduler _waveScheduler;
    private readonly FrontlinePlayerProgress _progress;
    private FrontlineLevelRoute? _reservedInitialRoute;

    public FrontlineGameSession(
        FrontlineLevelMapDefinition level,
        FrontlineWaveSet waveSet,
        FrontlinePlayerProgress progress,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(waveSet);
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(random);

        if (!string.Equals(level.Id, waveSet.Id, StringComparison.Ordinal))
            throw new ArgumentException("The map and wave schedule must describe the same level.");

        if (!progress.IsLevelUnlocked(level.UnlockIndex))
            throw new InvalidOperationException($"Level '{level.Id}' is locked.");

        Level = level;
        WaveSet = waveSet;
        _progress = progress;
        _random = random;
        _routeFactory = new FrontlineLevelRouteFactory(new FrontlineGridPathfinder());

        if (!_routeFactory.TryCreateRandomRoute(level, random, out _reservedInitialRoute)
            || _reservedInitialRoute is null)
        {
            State = FrontlineGameSessionState.RouteUnavailable;
            throw new InvalidOperationException($"No valid route is available for level '{level.Id}'.");
        }

        Simulation = new FrontlineGameSimulation(
            _reservedInitialRoute.Path,
            progress.StartingFunds,
            levelMap: level);
        _waveScheduler = new FrontlineWaveScheduler(waveSet.ToSchedule());
        State = FrontlineGameSessionState.Playing;
    }

    public FrontlineLevelMapDefinition Level { get; }
    public FrontlineWaveSet WaveSet { get; }
    public FrontlineGameSimulation Simulation { get; }
    public FrontlinePlayerProgress Progress => _progress;
    public bool CanStartNextWave => State == FrontlineGameSessionState.Playing && _waveScheduler.CanManuallyStart;
    public FrontlineGameSessionState State { get; private set; }
    public double SpeedMultiplier { get; private set; } = 1;
    public int CurrentWave => _waveScheduler.CurrentGroup;
    public int TotalWaves => _waveScheduler.TotalGroups;
    public bool AllWavesDispatched => _waveScheduler.AllGroupsDispatched;

    public void Pause()
    {
        if (State == FrontlineGameSessionState.Playing)
            State = FrontlineGameSessionState.Paused;
    }

    public void Resume()
    {
        if (State == FrontlineGameSessionState.Paused)
            State = FrontlineGameSessionState.Playing;
    }

    public bool TryStartNextWave() =>
        State == FrontlineGameSessionState.Playing && _waveScheduler.TryStartNextGroup();

    public bool SetSpeedMultiplier(double multiplier)
    {
        if (!double.IsFinite(multiplier) || multiplier <= 0 || multiplier > 8)
            return false;

        SpeedMultiplier = multiplier;
        return true;
    }

    public void Advance(double elapsedMilliseconds)
    {
        if (State is FrontlineGameSessionState.Victory
            or FrontlineGameSessionState.Defeat
            or FrontlineGameSessionState.RouteUnavailable)
            return;

        if (!double.IsFinite(elapsedMilliseconds) || elapsedMilliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedMilliseconds));

        if (State == FrontlineGameSessionState.Paused)
        {
            Simulation.Advance(0);
            _waveScheduler.Advance(0, Simulation.Enemies.Count > 0);
            return;
        }

        if (elapsedMilliseconds == 0)
            return;

        var elapsedUnits = Math.Min(elapsedMilliseconds * 0.08 * SpeedMultiplier, 8);
        Simulation.Advance(elapsedUnits);

        if (Simulation.Status == FrontlineGameStatus.Defeat)
        {
            State = FrontlineGameSessionState.Defeat;
            return;
        }

        var spawnRequests = _waveScheduler.Advance(elapsedUnits, Simulation.Enemies.Count > 0);
        foreach (var request in spawnRequests)
        {
            var archetype = FrontlineEnemyCatalog.Find(request.EnemyType)
                ?? throw new InvalidOperationException($"Unknown Frontline enemy type '{request.EnemyType}'.");

            FrontlinePath enemyPath;
            if (_reservedInitialRoute is not null)
            {
                enemyPath = _reservedInitialRoute.Path;
                _reservedInitialRoute = null;
            }
            else if (_routeFactory.TryCreateRandomRoute(Level, _random, out var route)
                && route is not null)
            {
                enemyPath = route.Path;
            }
            else
            {
                State = FrontlineGameSessionState.RouteUnavailable;
                return;
            }

            Simulation.SpawnEnemy(archetype.CreateDefinition(request.WaveNumber), enemyPath);
        }

        if (_waveScheduler.IsComplete && Simulation.Enemies.Count == 0)
        {
            _progress.SaveLevelScore(Level.ProgressIndex, Simulation.Score);
            State = FrontlineGameSessionState.Victory;
        }
    }
}
