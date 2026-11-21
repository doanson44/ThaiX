namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineSniperMissionSession
{
    private const double EnemyHitRadiusSquared = 8 * 8 * 2;
    private const int AimSampleCount = 10;
    private const double AimSampleSpacing = 5;
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
    public int TotalScore => _gameSession.TotalScore;
    public int CreditsEarned => _gameSession.CreditsEarned;
    public bool CanStartNextWave => _gameSession.CanStartNextWave;
    public int ShotsFired { get; private set; }
    public int Hits { get; private set; }
    public bool CanFire => _canFire;
    public FrontlinePoint ShooterPosition => GetShooterPosition(Level);
    public FrontlinePoint? LastAim { get; private set; }

    public static FrontlinePoint GetShooterPosition(FrontlineLevelMapDefinition level)
    {
        ArgumentNullException.ThrowIfNull(level);
        return level.LevelNumber == 3
            ? new FrontlinePoint(302, 102)
            : new FrontlinePoint(560, 240);
    }

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
            if (_loading >= 100 && Simulation.Funds >= 0)
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
            || !Simulation.TrySpendSniperShot())
            return false;

        ShotsFired++;
        _canFire = false;
        LastAim = aim;

        var deltaX = aim.X - ShooterPosition.X;
        var deltaY = aim.Y - ShooterPosition.Y;
        var distance = Math.Hypot(deltaX, deltaY);
        FrontlineEnemy? target = null;

        if (distance > 0)
        {
            var directionX = deltaX / distance;
            var directionY = deltaY / distance;
            var stepX = directionX * AimSampleSpacing;
            var stepY = directionY * AimSampleSpacing;
            var sampleX = aim.X;
            var sampleY = aim.Y;
            var traceEnd = aim;

            for (var sample = 0; sample < AimSampleCount; sample++)
            {
                target = Simulation.Enemies.FirstOrDefault(enemy =>
                {
                    var offsetX = enemy.Position.X - sampleX;
                    var offsetY = enemy.Position.Y - sampleY;
                    return offsetX * offsetX + offsetY * offsetY <= EnemyHitRadiusSquared;
                });

                if (target is not null)
                {
                    traceEnd = new FrontlinePoint(sampleX - stepX, sampleY - stepY);
                    break;
                }

                sampleX -= stepX;
                sampleY -= stepY;
            }

            var muzzleVelocityX = directionX * 20;
            var muzzleVelocityY = directionY * 20;
            var traceStart = new FrontlinePoint(
                ShooterPosition.X + muzzleVelocityX - muzzleVelocityY * 0.1,
                ShooterPosition.Y + muzzleVelocityY - 25 + muzzleVelocityX * 0.1);
            Simulation.AddCombatTrace(
                traceStart,
                traceEnd,
                FrontlineCombatTraceKind.Sniper,
                2);
        }

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
