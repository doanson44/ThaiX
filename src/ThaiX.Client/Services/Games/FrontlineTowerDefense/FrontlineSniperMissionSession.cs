namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineSniperMissionSession
{
    private const double HitRadius = 8.5;
    private readonly FrontlineGameSession _gameSession;
    private double _loading;
    private bool _canFire;

    public FrontlineSniperMissionSession(
        FrontlineLevelMapDefinition level,
        FrontlineWaveSet waveSet,
        FrontlinePlayerProgress progress,
        Random random)
    {
        ArgumentNullException.ThrowIfNull(level);
        if (!level.IsBonus || level.LevelNumber is < 1 or > 4)
            throw new ArgumentException("Sniper Elite requires one of the four bonus missions.", nameof(level));

        _gameSession = new FrontlineGameSession(level, waveSet, progress, random);
    }

    public FrontlineLevelMapDefinition Level => _gameSession.Level;
    public FrontlineGameSimulation Simulation => _gameSession.Simulation;
    public FrontlinePlayerProgress Progress => _gameSession.Progress;
    public FrontlineGameSessionState State => _gameSession.State;
    public int CurrentWave => _gameSession.CurrentWave;
    public int TotalWaves => _gameSession.TotalWaves;
    public bool CanStartNextWave => _gameSession.CanStartNextWave;
    public int ShotsFired { get; private set; }
    public int Hits { get; private set; }
    public bool CanFire => _canFire;

    public void Pause() => _gameSession.Pause();

    public void Resume() => _gameSession.Resume();

    public bool TryStartNextWave() => _gameSession.TryStartNextWave();

    public bool SetSpeedMultiplier(double multiplier) =>
        _gameSession.SetSpeedMultiplier(multiplier);

    public void Advance(double elapsedMilliseconds)
    {
        if (!double.IsFinite(elapsedMilliseconds) || elapsedMilliseconds < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedMilliseconds));

        if (State == FrontlineGameSessionState.Playing)
        {
            var elapsedUnits = Math.Min(
                elapsedMilliseconds * 0.08 * _gameSession.SpeedMultiplier,
                8);
            _loading += 8 * elapsedUnits;
            if (_loading >= 100)
                _canFire = true;
        }

        _gameSession.Advance(elapsedMilliseconds);
    }

    public bool FireAt(FrontlinePoint aim)
    {
        if (State != FrontlineGameSessionState.Playing
            || !_canFire
            || !double.IsFinite(aim.X)
            || !double.IsFinite(aim.Y)
            || !Simulation.TrySpendFunds(1))
            return false;

        ShotsFired++;
        _canFire = false;
        var target = Simulation.Enemies
            .Where(enemy => Math.Hypot(
                enemy.Position.X - aim.X,
                enemy.Position.Y - aim.Y) <= HitRadius)
            .OrderBy(enemy => Math.Hypot(
                enemy.Position.X - aim.X,
                enemy.Position.Y - aim.Y))
            .FirstOrDefault();

        if (target is null)
            return true;

        var damage = target.Definition.IsMetal
            ? new FrontlineDamageProfile(DirectDamage: 150, Burn: 5)
            : new FrontlineDamageProfile(DirectDamage: 300, Burn: 2);

        if (Simulation.ApplyDamage(target.Id, damage))
            Hits++;

        return true;
    }

    public bool FireAtEnemy(long enemyId)
    {
        var target = Simulation.Enemies.FirstOrDefault(enemy => enemy.Id == enemyId);
        return target is not null && FireAt(target.Position);
    }
}
