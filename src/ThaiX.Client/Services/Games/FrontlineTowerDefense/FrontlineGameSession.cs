namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineLevelLockedException(string levelId)
    : InvalidOperationException($"Level '{levelId}' is locked.");

public sealed class FrontlineRouteUnavailableException(string levelId)
    : InvalidOperationException($"No valid route is available for level '{levelId}'.");

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
            throw new FrontlineLevelLockedException(level.Id);

        Level = level;
        WaveSet = waveSet;
        _progress = progress;
        _random = random;
        _routeFactory = new FrontlineLevelRouteFactory(new FrontlineGridPathfinder());

        Simulation = new FrontlineGameSimulation(
            _routeFactory.CreateDefaultPath(level),
            progress.StartingFunds,
            levelMap: level,
            random: random);
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
    public int TotalScore { get; private set; }
    public int CreditsEarned { get; private set; }

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
        if (multiplier is not 1 and not 2)
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

        var elapsedUnits = State == FrontlineGameSessionState.Paused
            ? 0
            : Math.Min(elapsedMilliseconds * 0.08 * SpeedMultiplier, 8);
        if (State != FrontlineGameSessionState.Paused && elapsedMilliseconds == 0)
            return;

        Simulation.AdvanceBeforeWaveScheduler(elapsedUnits);
        var spawnRequests = _waveScheduler.Advance(elapsedUnits, Simulation.Enemies.Count > 0);
        foreach (var request in spawnRequests)
        {
            var archetype = FrontlineEnemyCatalog.Find(request.EnemyType)
                ?? throw new InvalidOperationException($"Unknown Frontline enemy type '{request.EnemyType}'.");

            if (!_routeFactory.TryCreateRandomRoute(Level, _random, out var route)
                || route is null)
            {
                State = FrontlineGameSessionState.RouteUnavailable;
                return;
            }

            Simulation.SpawnEnemy(archetype.CreateDefinition(request.WaveNumber), route.Path);
        }

        Simulation.AdvanceAfterWaveScheduler(elapsedUnits);

        if (Simulation.Status == FrontlineGameStatus.Defeat)
        {
            State = FrontlineGameSessionState.Defeat;
            return;
        }

        if (_waveScheduler.IsComplete && Simulation.Enemies.Count == 0)
        {
            var calculatedScore = Math.Floor(
                Simulation.Score
                + Math.Floor(Simulation.LevelHealth * 5d)
                + Math.Floor(Simulation.Funds * 5d));
            TotalScore = (int)Math.Clamp(calculatedScore, 0, int.MaxValue);
            CreditsEarned = (int)Math.Floor(TotalScore * 0.25);
            _progress.SaveLevelScore(Level.ProgressIndex, TotalScore);
            _progress.AwardCredits(CreditsEarned);
            State = FrontlineGameSessionState.Victory;
        }
    }
}
