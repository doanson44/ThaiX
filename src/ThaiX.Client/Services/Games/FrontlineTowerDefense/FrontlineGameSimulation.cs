namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public enum FrontlineGameStatus
{
    Planning,
    Running,
    Victory,
    Defeat
}

public sealed record FrontlinePath(IReadOnlyList<FrontlinePoint> Points)
{
    public double Length => Points.Zip(Points.Skip(1),
        (from, to) => Math.Hypot(to.X - from.X, to.Y - from.Y)).Sum();

    public FrontlinePoint PointAt(double distance)
    {
        if (Points.Count == 0)
            return default;

        var remaining = Math.Max(0, distance);
        for (var index = 1; index < Points.Count; index++)
        {
            var from = Points[index - 1];
            var to = Points[index];
            var segmentLength = Math.Hypot(to.X - from.X, to.Y - from.Y);
            if (remaining <= segmentLength)
            {
                var progress = segmentLength <= 0 ? 0 : remaining / segmentLength;
                return new FrontlinePoint(
                    from.X + (to.X - from.X) * progress,
                    from.Y + (to.Y - from.Y) * progress);
            }

            remaining -= segmentLength;
        }

        return Points[^1];
    }
}

public readonly record struct FrontlinePoint(double X, double Y);

public sealed record FrontlineEnemyDefinition(
    string Id,
    double Health,
    double Speed,
    int Reward,
    int LifeDamage,
    bool IsMetal = false,
    double HeatResistance = 1,
    double RadiationResistance = 1,
    double PoisonResistance = 1,
    double ColdResistance = 1);

public sealed record FrontlineDamageProfile(
    double DirectDamage = 0,
    double Burn = 0,
    double Radiation = 0,
    double Poison = 0,
    double Freeze = 0);

public sealed record FrontlineTowerDefinition(
    string Id,
    int Cost,
    double Range,
    double Damage,
    int CooldownTicks);

public sealed class FrontlineEnemy
{
    internal FrontlineEnemy(long id, FrontlineEnemyDefinition definition, FrontlinePath path)
    {
        Id = id;
        Definition = definition;
        Path = path;
        Health = definition.Health;
    }

    public long Id { get; }
    public FrontlineEnemyDefinition Definition { get; }
    public FrontlinePath Path { get; }
    public double Health { get; internal set; }
    public double Distance { get; internal set; }
    public double CoreTemperature { get; private set; } = 35;
    public double BurnDuration { get; private set; }
    public double RadiationDuration { get; private set; }
    public double PoisonDuration { get; private set; }
    public double FreezeDuration { get; private set; }
    public bool IsFrozen { get; private set; }
    public FrontlinePoint Position => Path.PointAt(Distance);

    internal void ApplyDamage(FrontlineDamageProfile damage)
    {
        Health -= damage.DirectDamage;
        RadiationDuration += damage.Radiation * Definition.RadiationResistance;
        CoreTemperature += damage.Burn * Definition.HeatResistance;
        PoisonDuration += damage.Poison * Definition.PoisonResistance;
        CoreTemperature -= damage.Freeze * Definition.ColdResistance;

        if (CoreTemperature > 200)
            BurnDuration = CoreTemperature;

        if (CoreTemperature < -200)
        {
            FreezeDuration = Math.Abs(CoreTemperature * 0.2);
            IsFrozen = true;
        }
    }

    internal void AdvanceEffects()
    {
        CoreTemperature = CoreTemperature > 35
            ? Math.Max(35, CoreTemperature - 0.02)
            : Math.Min(35, CoreTemperature + 0.02);

        if (BurnDuration > 0)
        {
            BurnDuration--;
            FreezeDuration = 0;
            Health -= 0.5;
        }

        if (FreezeDuration > 0)
            FreezeDuration--;

        if (RadiationDuration > 0)
        {
            RadiationDuration--;
            Health -= 0.3;
        }

        if (PoisonDuration > 0)
        {
            if (Definition.IsMetal)
                PoisonDuration = 0;
            else
            {
                PoisonDuration--;
                Health -= 0.5;
            }
        }

        if (IsFrozen && FreezeDuration <= 0)
            IsFrozen = false;
    }

    internal double MovementMultiplier
    {
        get
        {
            if (FreezeDuration > 0)
                return 0;

            if (PoisonDuration > 0)
                return 0.6;

            if (RadiationDuration > 0)
                return Definition.IsMetal ? 0.4 : 0.6;

            if (BurnDuration > 0 && !Definition.IsMetal)
                return 1.2;

            return 1;
        }
    }
    public double Progress => Path.Length <= 0 ? 1 : Math.Clamp(Distance / Path.Length, 0, 1);
}

public sealed class FrontlineTower
{
    internal FrontlineTower(long id, FrontlineTowerDefinition definition, FrontlinePoint position)
    {
        Id = id;
        Definition = definition;
        Position = position;
    }

    public long Id { get; }
    public FrontlineTowerDefinition Definition { get; }
    public FrontlinePoint Position { get; }
    public int CooldownRemaining { get; internal set; }
    public int Kills { get; internal set; }
    public double DamageDealt { get; internal set; }
}

public sealed class FrontlineGameSimulation
{
    private readonly List<FrontlineEnemy> _enemies = [];
    private readonly List<FrontlineTower> _towers = [];
    private long _nextEnemyId = 1;
    private long _nextTowerId = 1;

    public FrontlineGameSimulation(FrontlinePath path, int startingFunds, int lives)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (path.Points.Count < 2)
            throw new ArgumentException("A game path requires at least two points.", nameof(path));
        if (startingFunds < 0)
            throw new ArgumentOutOfRangeException(nameof(startingFunds));
        if (lives <= 0)
            throw new ArgumentOutOfRangeException(nameof(lives));

        Path = path;
        Funds = startingFunds;
        Lives = lives;
    }

    public FrontlinePath Path { get; }
    public FrontlineGameStatus Status { get; private set; } = FrontlineGameStatus.Planning;
    public int Funds { get; private set; }
    public int Lives { get; private set; }
    public int Tick { get; private set; }
    public int Score { get; private set; }
    public IReadOnlyList<FrontlineEnemy> Enemies => _enemies;
    public IReadOnlyList<FrontlineTower> Towers => _towers;

    public bool TryBuildTower(FrontlineTowerDefinition definition, FrontlinePoint position)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (Status is FrontlineGameStatus.Victory or FrontlineGameStatus.Defeat
            || definition.Cost < 0
            || definition.Range <= 0
            || definition.Damage <= 0
            || definition.CooldownTicks <= 0
            || Funds < definition.Cost
            || _towers.Any(tower => Math.Hypot(tower.Position.X - position.X, tower.Position.Y - position.Y) < 1))
            return false;

        Funds -= definition.Cost;
        _towers.Add(new FrontlineTower(_nextTowerId++, definition, position));
        return true;
    }

    public bool SpawnEnemy(FrontlineEnemyDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (Status is FrontlineGameStatus.Victory or FrontlineGameStatus.Defeat
            || definition.Health <= 0
            || definition.Speed <= 0
            || definition.Reward < 0
            || definition.LifeDamage <= 0)
            return false;

        _enemies.Add(new FrontlineEnemy(_nextEnemyId++, definition, Path));
        Status = FrontlineGameStatus.Running;
        return true;
    }

    public void Advance()
    {
        if (Status != FrontlineGameStatus.Running)
            return;

        Tick++;
        MoveEnemies();
        if (Lives <= 0)
        {
            Status = FrontlineGameStatus.Defeat;
            return;
        }

        foreach (var tower in _towers)
            Fire(tower);

        ResolveDeaths();
        if (_enemies.Count == 0 && Status == FrontlineGameStatus.Running)
            Status = FrontlineGameStatus.Planning;
    }

    private void MoveEnemies()
    {
        for (var index = _enemies.Count - 1; index >= 0; index--)
        {
            var enemy = _enemies[index];
            enemy.AdvanceEffects();
            enemy.Distance += enemy.Definition.Speed * enemy.MovementMultiplier;
            if (enemy.Distance < enemy.Path.Length)
                continue;

            Lives = Math.Max(0, Lives - enemy.Definition.LifeDamage);
            _enemies.RemoveAt(index);
        }
    }

    public bool ApplyDamage(long enemyId, FrontlineDamageProfile damage)
    {
        ArgumentNullException.ThrowIfNull(damage);
        if (Status is FrontlineGameStatus.Victory or FrontlineGameStatus.Defeat
            || damage.DirectDamage < 0
            || damage.Burn < 0
            || damage.Radiation < 0
            || damage.Poison < 0
            || damage.Freeze < 0)
            return false;

        var enemy = _enemies.FirstOrDefault(candidate => candidate.Id == enemyId);
        if (enemy is null)
            return false;

        enemy.ApplyDamage(damage);
        ResolveDeaths();
        return true;
    }

    private void Fire(FrontlineTower tower)
    {
        if (tower.CooldownRemaining > 0)
        {
            tower.CooldownRemaining--;
            return;
        }

        var target = _enemies
            .Where(enemy => enemy.Health > 0
                && Math.Hypot(enemy.Position.X - tower.Position.X, enemy.Position.Y - tower.Position.Y)
                    <= tower.Definition.Range)
            .OrderByDescending(enemy => enemy.Progress)
            .FirstOrDefault();

        if (target is null)
            return;

        target.ApplyDamage(new FrontlineDamageProfile(DirectDamage: tower.Definition.Damage));
        tower.DamageDealt += tower.Definition.Damage;
        tower.CooldownRemaining = tower.Definition.CooldownTicks;
    }

    private void ResolveDeaths()
    {
        for (var index = _enemies.Count - 1; index >= 0; index--)
        {
            var enemy = _enemies[index];
            if (enemy.Health > 0)
                continue;

            Funds += enemy.Definition.Reward;
            Score += enemy.Definition.Reward;
            _enemies.RemoveAt(index);
        }
    }
}