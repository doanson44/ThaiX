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
    public const double DefaultGridCellSize = 32;

    public static FrontlinePath FromGridPath(
        IReadOnlyList<FrontlineGridPoint> gridPath,
        double cellSize = DefaultGridCellSize)
    {
        ArgumentNullException.ThrowIfNull(gridPath);
        if (gridPath.Count < 2)
            throw new ArgumentException("A movement path requires at least two grid cells.", nameof(gridPath));
        if (cellSize <= 0 || double.IsNaN(cellSize) || double.IsInfinity(cellSize))
            throw new ArgumentOutOfRangeException(nameof(cellSize));

        return new FrontlinePath(gridPath
            .Select(point => new FrontlinePoint(
                point.X * cellSize + cellSize * 0.5,
                point.Y * cellSize + cellSize * 0.5))
            .ToArray());
    }

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

public enum FrontlineTargetMode
{
    Near,
    Far,
    Strong,
    Weak,
    Heat,
    Cold
}

public sealed record FrontlineTowerDefinition(
    string Id,
    int Cost,
    double Range,
    double Damage,
    double RateOfFire,
    FrontlineTargetMode TargetMode = FrontlineTargetMode.Near);

public sealed class FrontlineEnemy
{
    internal FrontlineEnemy(long id, FrontlineEnemyDefinition definition, FrontlinePath path)
    {
        Id = id;
        Definition = definition;
        Path = path;
        Health = definition.Health;
        CoreTemperature = definition.IsMetal ? 200 : 35;
        BaseTemperature = definition.IsMetal ? 200 : 35;
        UpperTemperature = definition.IsMetal ? 500 : 200;
        LowerTemperature = definition.IsMetal ? 0 : -200;
    }

    public long Id { get; }
    public FrontlineEnemyDefinition Definition { get; }
    public FrontlinePath Path { get; }
    public double Health { get; internal set; }
    public double Distance { get; internal set; }
    public double CoreTemperature { get; private set; }
    public double BaseTemperature { get; }
    public double UpperTemperature { get; }
    public double LowerTemperature { get; }
    public double BurnDuration { get; private set; }
    public double RadiationDuration { get; private set; }
    public double PoisonDuration { get; private set; }
    public double FreezeDuration { get; private set; }
    public bool IsFrozen { get; private set; }
    public bool AwardScoreOnDeath { get; private set; }
    public FrontlinePoint Position => Path.PointAt(Distance);

    internal void ApplyDamage(FrontlineDamageProfile damage)
    {
        Health -= damage.DirectDamage;
        AwardScoreOnDeath = damage.DirectDamage > 0 && Health <= 0;
        RadiationDuration += damage.Radiation * Definition.RadiationResistance;
        CoreTemperature += damage.Burn * Definition.HeatResistance;
        PoisonDuration += damage.Poison * Definition.PoisonResistance;
        CoreTemperature -= damage.Freeze * Definition.ColdResistance;

        if (CoreTemperature > UpperTemperature)
            BurnDuration = CoreTemperature;

        if (CoreTemperature < LowerTemperature)
        {
            FreezeDuration = Math.Abs(CoreTemperature * 0.2);
            IsFrozen = true;
        }
    }

    internal void AdvanceEffects()
    {
        CoreTemperature = CoreTemperature > BaseTemperature
            ? Math.Max(BaseTemperature, CoreTemperature - 0.02)
            : Math.Min(BaseTemperature, CoreTemperature + 0.02);

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
    private double _damageLevel;
    private double _rangeLevel;
    private double _rateOfFireLevel;

    public double Loading { get; internal set; }
    public long? TargetId { get; internal set; }
    public int Kills { get; internal set; }
    public double DamageDealt { get; internal set; }
    public double Range => Definition.Range * (1 + _rangeLevel);
    public double Damage => Definition.Damage * (1 + _damageLevel);
    public double RateOfFire => Definition.RateOfFire * (1 + _rateOfFireLevel);

    public bool UpgradeDamage(ref int funds) => Upgrade(ref _damageLevel, ref funds);
    public bool UpgradeRange(ref int funds) => Upgrade(ref _rangeLevel, ref funds);
    public bool UpgradeRateOfFire(ref int funds) => Upgrade(ref _rateOfFireLevel, ref funds);

    private bool Upgrade(ref double level, ref int funds)
    {
        var cost = (int)Math.Floor(Definition.Cost * 0.25);
        if (funds < cost)
            return false;

        funds -= cost;
        level += 0.2;
        return true;
    }
}

public sealed class FrontlineGameSimulation
{
    private readonly List<FrontlineEnemy> _enemies = [];
    private readonly List<FrontlineTower> _towers = [];
    private long _nextEnemyId = 1;
    private long _nextTowerId = 1;

    public FrontlineGameSimulation(FrontlinePath path, int startingFunds, double levelHealth = 1000)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (path.Points.Count < 2)
            throw new ArgumentException("A game path requires at least two points.", nameof(path));
        if (startingFunds < 0)
            throw new ArgumentOutOfRangeException(nameof(startingFunds));
        if (levelHealth <= 0)
            throw new ArgumentOutOfRangeException(nameof(levelHealth));

        Path = path;
        Funds = startingFunds;
        LevelHealth = levelHealth;
    }

    public FrontlinePath Path { get; }
    public FrontlineGameStatus Status { get; private set; } = FrontlineGameStatus.Planning;
    public int Funds { get; private set; }
    public double LevelHealth { get; private set; }
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
            || definition.RateOfFire <= 0
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
            || definition.Reward < 0)
            return false;

        _enemies.Add(new FrontlineEnemy(_nextEnemyId++, definition, Path));
        Status = FrontlineGameStatus.Running;
        return true;
    }

    public void Advance(double elapsedUnits = 1)
    {
        if (elapsedUnits < 0 || double.IsNaN(elapsedUnits) || double.IsInfinity(elapsedUnits))
            throw new ArgumentOutOfRangeException(nameof(elapsedUnits));

        if (Status != FrontlineGameStatus.Running)
            return;

        Tick++;
        MoveEnemies(elapsedUnits);
        if (LevelHealth <= 0)
        {
            Status = FrontlineGameStatus.Defeat;
            return;
        }

        foreach (var tower in _towers)
            Fire(tower, elapsedUnits);

        ResolveDeaths();
        if (_enemies.Count == 0 && Status == FrontlineGameStatus.Running)
            Status = FrontlineGameStatus.Planning;
    }

    private void MoveEnemies(double elapsedUnits)
    {
        for (var index = _enemies.Count - 1; index >= 0; index--)
        {
            var enemy = _enemies[index];
            enemy.AdvanceEffects();
            if (enemy.Health <= 0)
                continue;

            enemy.Distance += enemy.Definition.Speed * enemy.MovementMultiplier * elapsedUnits;
            if (enemy.Distance < enemy.Path.Length)
                continue;

            LevelHealth = Math.Max(0, LevelHealth - enemy.Health);
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

    private void Fire(FrontlineTower tower, double elapsedUnits)
    {
        tower.Loading += tower.RateOfFire * elapsedUnits;
        var target = tower.TargetId is long targetId
            ? _enemies.FirstOrDefault(enemy => enemy.Id == targetId)
            : null;

        if (target is null || target.Health <= 0)
        {
            target = FindTarget(tower);
            tower.TargetId = target?.Id;
            return;
        }

        if (Math.Hypot(target.Position.X - tower.Position.X, target.Position.Y - tower.Position.Y) > tower.Range)
        {
            target = FindTarget(tower);
            tower.TargetId = target?.Id;
            tower.Loading = 90;
            return;
        }

        if (tower.Loading < 100)
            return;

        var damage = tower.Damage;
        target.ApplyDamage(new FrontlineDamageProfile(DirectDamage: damage));
        tower.DamageDealt += damage;
        if (target.Health <= 0)
            tower.Kills++;

        tower.Loading = 0;
        ResolveDeaths();
    }

    private FrontlineEnemy? FindTarget(FrontlineTower tower)
    {
        var candidates = _enemies
            .Where(enemy => enemy.Health > 0
                && Math.Hypot(enemy.Position.X - tower.Position.X, enemy.Position.Y - tower.Position.Y)
                    <= tower.Range)
            .ToList();

        return tower.Definition.TargetMode switch
        {
            FrontlineTargetMode.Near => candidates.OrderByDescending(enemy => enemy.Distance).FirstOrDefault(),
            FrontlineTargetMode.Far => candidates.OrderBy(enemy => enemy.Distance).FirstOrDefault(),
            FrontlineTargetMode.Strong => candidates.OrderByDescending(enemy => enemy.Health).FirstOrDefault(),
            FrontlineTargetMode.Weak => candidates.OrderBy(enemy => enemy.Health).FirstOrDefault(),
            FrontlineTargetMode.Heat => candidates
                .Where(enemy => enemy.CoreTemperature > 120)
                .OrderByDescending(enemy => enemy.CoreTemperature)
                .FirstOrDefault() ?? candidates.FirstOrDefault(),
            FrontlineTargetMode.Cold => FindColdTarget(candidates),
            _ => null
        };
    }

    private static FrontlineEnemy? FindColdTarget(IReadOnlyList<FrontlineEnemy> candidates)
    {
        if (candidates.Count == 0)
            return null;

        var selected = candidates[0];
        foreach (var candidate in candidates.Skip(1))
        {
            if (candidate.CoreTemperature < selected.CoreTemperature && !candidate.Definition.IsMetal)
                selected = candidate;
        }

        return selected;
    }

    private void ResolveDeaths()
    {
        for (var index = _enemies.Count - 1; index >= 0; index--)
        {
            var enemy = _enemies[index];
            if (enemy.Health > 0)
                continue;

            Funds += enemy.Definition.Reward;
            if (enemy.AwardScoreOnDeath)
                Score += enemy.Definition.Reward * 2;
            _enemies.RemoveAt(index);
        }
    }
}