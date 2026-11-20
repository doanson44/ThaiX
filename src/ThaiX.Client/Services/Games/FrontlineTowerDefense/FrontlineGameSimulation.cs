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
    FrontlineTargetMode TargetMode = FrontlineTargetMode.Near,
    bool IsSupport = false,
    FrontlineDamageProfile? SupportDamage = null,
    int ShotLimit = 0,
    FrontlineDamageProfile? DamageProfile = null,
    FrontlineDamageProfile? MetalDamageProfile = null,
    FrontlineProjectileProfile? ProjectileProfile = null,
    double DamageUpgradeScale = 1,
    double StatusUpgradeScale = 0,
    double MetalDamageUpgradeScale = 1);

public enum FrontlineTowerUpgradeType
{
    Damage,
    Range,
    RateOfFire
}

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
    public FrontlinePoint Position => Path.PointAt(Distance);

    internal void ApplyDamage(FrontlineDamageProfile damage)
    {
        Health -= damage.DirectDamage;
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
    internal FrontlineTower(
        long id,
        FrontlineTowerDefinition definition,
        FrontlineGridPoint cell,
        FrontlinePoint position)
    {
        Id = id;
        Definition = definition;
        Cell = cell;
        Position = position;
    }

    public long Id { get; }
    public FrontlineTowerDefinition Definition { get; }
    public FrontlineGridPoint Cell { get; internal set; }
    public FrontlinePoint Position { get; internal set; }
    public double SaleValue => Definition.Cost * 0.5;
    private double _damageLevel;
    private double _rangeLevel;
    private double _rateOfFireLevel;

    public double Loading { get; internal set; }
    public long? TargetId { get; internal set; }
    public int Kills { get; internal set; }
    public int ShotsFired { get; internal set; }
    public double DamageDealt { get; internal set; }
    public double Range => Definition.Range * (1 + _rangeLevel);
    public double Damage => Definition.Damage * (1 + _damageLevel);
    public double RateOfFire => Definition.RateOfFire * (1 + _rateOfFireLevel);
    public double DamageUpgradeLevel => _damageLevel;
    public double RangeUpgradeLevel => _rangeLevel;
    public double RateOfFireUpgradeLevel => _rateOfFireLevel;

    public bool UpgradeDamage(ref int funds) => Upgrade(ref _damageLevel, ref funds);
    public bool UpgradeRange(ref int funds) => Upgrade(ref _rangeLevel, ref funds);
    public bool UpgradeRateOfFire(ref int funds) => Upgrade(ref _rateOfFireLevel, ref funds);

    private bool Upgrade(ref double level, ref int funds)
    {
        var cost = (int)Math.Floor(Definition.Cost * 0.25);
        if (level >= 1 || funds < cost)
            return false;

        funds -= cost;
        level += 0.2;
        return true;
    }
}

public sealed class FrontlineGameSimulation
{
    public sealed class FrontlineSupportProjectile(
        FrontlinePoint impact,
        double damage,
        FrontlineDamageProfile statusDamage,
        bool createsFireCloud,
        bool createsPoisonCloud)
    {
        public FrontlinePoint Impact { get; } = impact;
        public double Damage { get; } = damage;
        public FrontlineDamageProfile StatusDamage { get; } = statusDamage;
        public bool CreatesFireCloud { get; } = createsFireCloud;
        public bool CreatesPoisonCloud { get; } = createsPoisonCloud;
        public double Height { get; private set; } = 450;
        public double VerticalVelocity { get; private set; } = -0.5;

        public bool Advance(double elapsedUnits)
        {
            Height += VerticalVelocity * elapsedUnits;
            VerticalVelocity -= 0.09 * elapsedUnits;
            return Height < 0;
        }
    }

    public sealed class FrontlineAreaEffect(
        FrontlinePoint position,
        double radius,
        FrontlineDamageProfile damage,
        double life,
        double interval)
    {
        public FrontlinePoint Position { get; } = position;
        public double Radius { get; } = radius;
        public FrontlineDamageProfile Damage { get; } = damage;
        public double LifeRemaining { get; private set; } = life;
        public double Interval { get; } = interval;
        private double _elapsed;

        public bool Advance(double elapsedUnits)
        {
            LifeRemaining -= elapsedUnits;
            _elapsed += elapsedUnits;
            var shouldPulse = _elapsed >= Interval;
            if (shouldPulse)
                _elapsed = 0;

            return shouldPulse;
        }
    }

    private readonly List<FrontlineEnemy> _enemies = [];
    private readonly List<FrontlineTower> _towers = [];
    private readonly List<FrontlineSupportProjectile> _supportProjectiles = [];
    private readonly List<FrontlineProjectile> _projectiles = [];
    private readonly List<FrontlineCombatTrace> _combatTraces = [];
    private readonly List<FrontlineAreaEffect> _areaEffects = [];
    private readonly Random _random;
    private long _nextEnemyId = 1;
    private long _nextTowerId = 1;
    private long _nextProjectileId = 1;

    public FrontlineGameSimulation(
        FrontlinePath path,
        int startingFunds,
        double levelHealth = 1000,
        FrontlineLevelMapDefinition? levelMap = null,
        Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(path);
        if (path.Points.Count < 2)
            throw new ArgumentException("A game path requires at least two points.", nameof(path));
        if (startingFunds < 0)
            throw new ArgumentOutOfRangeException(nameof(startingFunds));
        if (levelHealth <= 0)
            throw new ArgumentOutOfRangeException(nameof(levelHealth));

        Path = path;
        LevelMap = levelMap;
        Funds = startingFunds;
        LevelHealth = levelHealth;
        _random = random ?? new Random(0);
    }

    public FrontlinePath Path { get; }
    public FrontlineLevelMapDefinition? LevelMap { get; }
    public FrontlineGameStatus Status { get; private set; } = FrontlineGameStatus.Planning;
    public int Funds { get; private set; }
    public double LevelHealth { get; private set; }
    public int Tick { get; private set; }
    public int Score { get; private set; }
    public IReadOnlyList<FrontlineEnemy> Enemies => _enemies;
    public IReadOnlyList<FrontlineTower> Towers => _towers;
    public IReadOnlyList<FrontlineProjectile> Projectiles => _projectiles;
    public IReadOnlyList<FrontlineSupportProjectile> SupportProjectiles => _supportProjectiles;
    public IReadOnlyList<FrontlineCombatTrace> CombatTraces => _combatTraces;
    public IReadOnlyList<FrontlineAreaEffect> AreaEffects => _areaEffects;
    public int ActiveProjectileCount => _projectiles.Count;
    public int ActiveSupportProjectileCount => _supportProjectiles.Count;
    public int ActiveAreaEffectCount => _areaEffects.Count;

    public bool TryBuildTower(FrontlineTowerDefinition definition, FrontlinePoint position)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (Status is FrontlineGameStatus.Victory or FrontlineGameStatus.Defeat
            || string.IsNullOrWhiteSpace(definition.Id)
            || !IsValidPosition(position)
            || definition.Cost < 0
            || !double.IsFinite(definition.Range) || definition.Range <= 0
            || !double.IsFinite(definition.Damage) || definition.Damage <= 0
            || !double.IsFinite(definition.RateOfFire) || definition.RateOfFire <= 0
            || !double.IsFinite(definition.DamageUpgradeScale) || definition.DamageUpgradeScale < 0
            || !double.IsFinite(definition.StatusUpgradeScale) || definition.StatusUpgradeScale < 0
            || !double.IsFinite(definition.MetalDamageUpgradeScale) || definition.MetalDamageUpgradeScale < 0
            || (definition.ProjectileProfile is { } projectileProfile
                && (!double.IsFinite(projectileProfile.Speed) || projectileProfile.Speed <= 0
                    || !double.IsFinite(projectileProfile.CollisionRadius) || projectileProfile.CollisionRadius <= 0
                    || !double.IsFinite(projectileProfile.SplashRadius) || projectileProfile.SplashRadius < 0
                    || !double.IsFinite(projectileProfile.SplashDamageScale) || projectileProfile.SplashDamageScale < 0
                    || !double.IsFinite(projectileProfile.StartOffset) || projectileProfile.StartOffset < 0
                    || !double.IsFinite(projectileProfile.VerticalOffset)
                    || !double.IsFinite(projectileProfile.Gravity) || projectileProfile.Gravity <= 0
                    || !double.IsFinite(projectileProfile.VerticalVelocityScale) || projectileProfile.VerticalVelocityScale < 0))
            || (definition.IsSupport && definition.ShotLimit <= 0)
            || Funds < definition.Cost)
            return false;

        var cell = ToGridCell(position);
        if (LevelMap is not null && !LevelMap.IsBuildable(cell))
            return false;

        if (_towers.Any(tower => tower.Cell == cell))
            return false;

        Funds -= definition.Cost;
        _towers.Add(new FrontlineTower(_nextTowerId++, definition, cell, ToCellCenter(cell)));
        return true;
    }

    public bool TryMoveTower(long towerId, FrontlinePoint position)
    {
        if (Status is FrontlineGameStatus.Victory or FrontlineGameStatus.Defeat
            || !IsValidPosition(position)
            || Funds < 10)
            return false;

        var tower = _towers.FirstOrDefault(candidate => candidate.Id == towerId);
        if (tower is null || tower.Definition.IsSupport)
            return false;

        var cell = ToGridCell(position);
        if (_towers.Any(candidate => candidate.Id != towerId && candidate.Cell == cell)
            || (LevelMap is not null && !LevelMap.IsBuildable(cell)))
            return false;

        Funds -= 10;
        tower.Cell = cell;
        tower.Position = ToCellCenter(cell);
        tower.TargetId = null;
        return true;
    }

    public bool TrySellTower(long towerId)
    {
        if (Status is FrontlineGameStatus.Victory or FrontlineGameStatus.Defeat)
            return false;

        var tower = _towers.FirstOrDefault(candidate => candidate.Id == towerId);
        if (tower is null || tower.Definition.IsSupport)
            return false;

        Funds += (int)Math.Floor(tower.SaleValue);
        _towers.Remove(tower);
        return true;
    }

    public bool TryUpgradeTower(long towerId, FrontlineTowerUpgradeType upgradeType)
    {
        if (Status is FrontlineGameStatus.Victory or FrontlineGameStatus.Defeat)
            return false;

        var tower = _towers.FirstOrDefault(candidate => candidate.Id == towerId);
        if (tower is null || tower.Definition.IsSupport)
            return false;

        var funds = Funds;
        var upgraded = upgradeType switch
        {
            FrontlineTowerUpgradeType.Damage => tower.UpgradeDamage(ref funds),
            FrontlineTowerUpgradeType.Range => tower.UpgradeRange(ref funds),
            FrontlineTowerUpgradeType.RateOfFire => tower.UpgradeRateOfFire(ref funds),
            _ => false
        };

        if (upgraded)
            Funds = funds;

        return upgraded;
    }

    private static bool IsValidPosition(FrontlinePoint position) =>
        double.IsFinite(position.X) && double.IsFinite(position.Y)
        && position.X >= 0 && position.Y >= 0;

    private static FrontlineGridPoint ToGridCell(FrontlinePoint position) =>
        new((int)Math.Floor(Math.Min(position.X, 640) / FrontlinePath.DefaultGridCellSize),
            (int)Math.Floor(position.Y / FrontlinePath.DefaultGridCellSize));

    private static FrontlinePoint ToCellCenter(FrontlineGridPoint cell) =>
        new(cell.X * FrontlinePath.DefaultGridCellSize + FrontlinePath.DefaultGridCellSize * 0.5,
            cell.Y * FrontlinePath.DefaultGridCellSize + FrontlinePath.DefaultGridCellSize * 0.5);

    public bool SpawnEnemy(FrontlineEnemyDefinition definition) =>
        SpawnEnemy(definition, Path);

    public bool SpawnEnemy(FrontlineEnemyDefinition definition, FrontlinePath path)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(path);
        if (Status is FrontlineGameStatus.Victory or FrontlineGameStatus.Defeat
            || path.Points.Count < 2
            || !double.IsFinite(definition.Health) || definition.Health <= 0
            || !double.IsFinite(definition.Speed) || definition.Speed <= 0
            || definition.Reward < 0
            || !double.IsFinite(definition.HeatResistance) || definition.HeatResistance < 0
            || !double.IsFinite(definition.RadiationResistance) || definition.RadiationResistance < 0
            || !double.IsFinite(definition.PoisonResistance) || definition.PoisonResistance < 0
            || !double.IsFinite(definition.ColdResistance) || definition.ColdResistance < 0)
            return false;

        _enemies.Add(new FrontlineEnemy(_nextEnemyId++, definition, path));
        Status = FrontlineGameStatus.Running;
        return true;
    }

    public void Advance(double elapsedUnits = 1)
    {
        if (elapsedUnits < 0 || double.IsNaN(elapsedUnits) || double.IsInfinity(elapsedUnits))
            throw new ArgumentOutOfRangeException(nameof(elapsedUnits));

        if (Status is FrontlineGameStatus.Victory or FrontlineGameStatus.Defeat)
            return;

        Tick++;
        if (Status == FrontlineGameStatus.Running)
            MoveEnemies(elapsedUnits);

        AdvanceCombatTraces(elapsedUnits);
        AdvanceProjectiles(elapsedUnits);
        AdvanceSupportProjectiles(elapsedUnits);
        AdvanceAreaEffects(elapsedUnits);

        if (LevelHealth <= 0)
        {
            Status = FrontlineGameStatus.Defeat;
            return;
        }

        if (Status == FrontlineGameStatus.Running)
        {
            foreach (var tower in _towers.ToArray())
                Fire(tower, elapsedUnits);
        }

        ResolveDeaths();
        if (_enemies.Count == 0
            && _projectiles.Count == 0
            && _supportProjectiles.Count == 0
            && Status == FrontlineGameStatus.Running)
            Status = FrontlineGameStatus.Planning;
    }

    private void MoveEnemies(double elapsedUnits)
    {
        for (var index = _enemies.Count - 1; index >= 0; index--)
        {
            var enemy = _enemies[index];
            enemy.Distance += enemy.Definition.Speed * enemy.MovementMultiplier * elapsedUnits;
            if (enemy.Distance >= enemy.Path.Length)
            {
                LevelHealth = Math.Max(0, LevelHealth - enemy.Health);
                _enemies.RemoveAt(index);
                continue;
            }

            enemy.AdvanceEffects();
        }
    }

    public bool TrySpendFunds(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        if (Funds < amount)
            return false;

        Funds -= amount;
        return true;
    }

    public bool TrySpendSniperShot()
    {
        if (Funds < 0)
            return false;

        Funds--;
        return true;
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
        if (tower.Definition.IsSupport)
        {
            FireSupportStrike(tower);
            return;
        }

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

        if (tower.Definition.Id == "tower1" && target.CoreTemperature <= 60)
        {
            tower.Loading = 0;
            target = FindTarget(tower);
            tower.TargetId = target?.Id;
            return;
        }

        var baseProfile = target.Definition.IsMetal
            ? tower.Definition.MetalDamageProfile ?? tower.Definition.DamageProfile
            : tower.Definition.DamageProfile;
        var profile = baseProfile ?? new FrontlineDamageProfile(DirectDamage: tower.Damage);
        var damageUpgradeScale = target.Definition.IsMetal
            ? tower.Definition.MetalDamageUpgradeScale
            : tower.Definition.DamageUpgradeScale;
        var upgradeScale = 1 + tower.DamageUpgradeLevel * damageUpgradeScale;
        var statusUpgradeScale = 1 + tower.DamageUpgradeLevel * tower.Definition.StatusUpgradeScale;
        var damageProfile = profile with
        {
            DirectDamage = profile.DirectDamage * upgradeScale,
            Burn = profile.Burn * statusUpgradeScale,
            Radiation = profile.Radiation * statusUpgradeScale,
            Poison = profile.Poison * statusUpgradeScale,
            Freeze = profile.Freeze * statusUpgradeScale
        };

        if (tower.Definition.ProjectileProfile is { } projectileProfile)
        {
            var deltaX = target.Position.X - tower.Position.X;
            var deltaY = target.Position.Y - tower.Position.Y;
            var distance = Math.Hypot(deltaX, deltaY);
            var directionX = distance <= 0 ? 0 : deltaX / distance;
            var directionY = distance <= 0 ? 0 : deltaY / distance;
            var position = new FrontlinePoint(
                tower.Position.X + directionX * projectileProfile.StartOffset,
                tower.Position.Y + directionY * projectileProfile.StartOffset + projectileProfile.VerticalOffset);

            var initialVelocityX = projectileProfile.Kind is FrontlineProjectileKind.TrackingPoison
                or FrontlineProjectileKind.TrackingFlame
                ? -0.2 + _random.NextDouble() * 0.4
                : 0;
            var initialVelocityY = projectileProfile.Kind is FrontlineProjectileKind.TrackingPoison
                or FrontlineProjectileKind.TrackingFlame
                ? -0.5 - _random.NextDouble() * 0.5
                : 0;

            _projectiles.Add(new FrontlineProjectile(
                _nextProjectileId++,
                tower,
                target,
                position,
                damageProfile,
                projectileProfile,
                initialVelocityX,
                initialVelocityY));
        }
        else
        {
            target.ApplyDamage(damageProfile);
            tower.DamageDealt += damageProfile.DirectDamage;
            if (target.Health <= 0)
                tower.Kills++;

            var traceKind = tower.Definition.Id switch
            {
                "tower4" => FrontlineCombatTraceKind.Lightning,
                "tower5" => FrontlineCombatTraceKind.Shotgun,
                "tower6" => FrontlineCombatTraceKind.EnergyBeam,
                _ => FrontlineCombatTraceKind.Bullet
            };

            if (traceKind == FrontlineCombatTraceKind.Shotgun)
            {
                for (var pellet = 0; pellet < 3; pellet++)
                {
                    var spreadX = _random.NextDouble() * 16 - 8;
                    var spreadY = _random.NextDouble() * 16 - 8;
                    _combatTraces.Add(new FrontlineCombatTrace(
                        new FrontlinePoint(tower.Position.X, tower.Position.Y - 12),
                        new FrontlinePoint(
                            target.Position.X + spreadX,
                            target.Position.Y - 6 + spreadY),
                        traceKind,
                        3));
                }
            }
            else
            {
                var traceStart = traceKind == FrontlineCombatTraceKind.Lightning
                    ? new FrontlinePoint(tower.Position.X, tower.Position.Y - 24)
                    : tower.Position;
                var traceEnd = traceKind == FrontlineCombatTraceKind.Lightning
                    ? new FrontlinePoint(target.Position.X, target.Position.Y - 8)
                    : target.Position;
                var traceLife = traceKind == FrontlineCombatTraceKind.Lightning
                    ? tower.Damage
                    : 2;

                _combatTraces.Add(new FrontlineCombatTrace(
                    traceStart,
                    traceEnd,
                    traceKind,
                    traceLife));
            }
        }

        tower.Loading = 0;
        if (tower.Definition.Id == "tower4")
            tower.TargetId = null;

        ResolveDeaths();
    }

    private void AdvanceCombatTraces(double elapsedUnits)
    {
        for (var index = _combatTraces.Count - 1; index >= 0; index--)
        {
            if (!_combatTraces[index].Advance(elapsedUnits))
                _combatTraces.RemoveAt(index);
        }
    }

    private void AdvanceProjectiles(double elapsedUnits)
    {
        for (var index = _projectiles.Count - 1; index >= 0; index--)
        {
            var projectile = _projectiles[index];
            var target = _enemies.FirstOrDefault(enemy => enemy.Id == projectile.TargetId);
            if (target is null || target.Health <= 0)
            {
                projectile.Expire();
                _projectiles.RemoveAt(index);
                continue;
            }

            if (!projectile.Advance(target, elapsedUnits))
            {
                if (projectile.IsExpired)
                    _projectiles.RemoveAt(index);
                continue;
            }

            if (projectile.Profile.SplashRadius > 0)
            {
                foreach (var enemy in _enemies.ToArray())
                {
                    if (enemy.Health <= 0
                        || Math.Hypot(
                            enemy.Position.X - projectile.Position.X,
                            enemy.Position.Y - projectile.Position.Y) > projectile.Profile.SplashRadius)
                        continue;

                    var splashDamage = projectile.Damage with
                    {
                        DirectDamage = projectile.Damage.DirectDamage * projectile.Profile.SplashDamageScale
                    };
                    enemy.ApplyDamage(splashDamage);
                    projectile.Owner.DamageDealt += splashDamage.DirectDamage;
                    if (enemy.Health <= 0)
                        projectile.Owner.Kills++;
                }
            }
            else
            {
                target.ApplyDamage(projectile.Damage);
                projectile.Owner.DamageDealt += projectile.Damage.DirectDamage;
                if (target.Health <= 0)
                    projectile.Owner.Kills++;
            }

            projectile.Expire();
            _projectiles.RemoveAt(index);
        }

        ResolveDeaths();
    }

    private void FireSupportStrike(FrontlineTower tower)
    {
        if (tower.Loading < 100)
            return;

        var spread = tower.Definition.Id == "suptower1" ? 60 : 40;
        var impact = new FrontlinePoint(
            tower.Position.X + _random.NextDouble() * spread * 2 - spread,
            tower.Position.Y + _random.NextDouble() * spread * 2 - spread);
        var statusDamage = tower.Definition.SupportDamage ?? new FrontlineDamageProfile();

        _supportProjectiles.Add(new FrontlineSupportProjectile(
            impact,
            tower.Damage * 0.5,
            statusDamage,
            tower.Definition.Id == "suptower1",
            tower.Definition.Id == "suptower2"));

        tower.Loading = 0;
        tower.ShotsFired++;
        if (tower.ShotsFired >= tower.Definition.ShotLimit)
            _towers.Remove(tower);
    }

    private void AdvanceSupportProjectiles(double elapsedUnits)
    {
        for (var index = _supportProjectiles.Count - 1; index >= 0; index--)
        {
            var projectile = _supportProjectiles[index];
            if (!projectile.Advance(elapsedUnits))
                continue;

            var explosionDamage = new FrontlineDamageProfile(
                projectile.Damage,
                projectile.StatusDamage.Burn,
                projectile.StatusDamage.Radiation,
                projectile.StatusDamage.Poison,
                projectile.StatusDamage.Freeze);

            foreach (var enemy in _enemies.ToArray())
            {
                if (Math.Hypot(
                    enemy.Position.X - projectile.Impact.X,
                    enemy.Position.Y - projectile.Impact.Y) <= 80)
                    enemy.ApplyDamage(explosionDamage);
            }

            if (projectile.CreatesFireCloud)
            {
                _areaEffects.Add(new FrontlineAreaEffect(
                    projectile.Impact,
                    40,
                    new FrontlineDamageProfile(Burn: 100),
                    500,
                    10));
            }
            else if (projectile.CreatesPoisonCloud)
            {
                _areaEffects.Add(new FrontlineAreaEffect(
                    projectile.Impact,
                    40,
                    new FrontlineDamageProfile(Poison: 20),
                    1000,
                    20));
            }

            _supportProjectiles.RemoveAt(index);
        }

        ResolveDeaths();
    }

    private void AdvanceAreaEffects(double elapsedUnits)
    {
        for (var index = _areaEffects.Count - 1; index >= 0; index--)
        {
            var effect = _areaEffects[index];
            if (effect.Advance(elapsedUnits))
            {
                foreach (var enemy in _enemies.ToArray())
                {
                    if (Math.Hypot(
                        enemy.Position.X - effect.Position.X,
                        enemy.Position.Y - effect.Position.Y) <= effect.Radius)
                        enemy.ApplyDamage(effect.Damage);
                }
            }

            if (effect.LifeRemaining <= 0)
                _areaEffects.RemoveAt(index);
        }

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
            Score += enemy.Definition.Reward * 2;
            _enemies.RemoveAt(index);
        }
    }
}