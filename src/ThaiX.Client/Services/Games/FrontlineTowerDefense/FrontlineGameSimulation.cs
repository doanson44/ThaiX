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
    public bool UsesGridMovement { get; init; }

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
            .ToArray())
        {
            UsesGridMovement = true
        };
    }

    public double Length => Points.Zip(Points.Skip(1),
        (from, to) => from.DistanceTo(to)).Sum();

    public FrontlinePoint PointAt(double distance)
    {
        if (Points.Count == 0)
            return default;

        var remaining = Math.Max(0, distance);
        for (var index = 1; index < Points.Count; index++)
        {
            var from = Points[index - 1];
            var to = Points[index];
            var segmentLength = from.DistanceTo(to);
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

public readonly record struct FrontlinePoint(double X, double Y)
{
    public double Length => Math.Sqrt(X * X + Y * Y);

    public double DistanceTo(FrontlinePoint other)
    {
        var deltaX = X - other.X;
        var deltaY = Y - other.Y;
        return Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
    }
}

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
    double MetalDamageUpgradeScale = 1,
    double ExperienceLimit = 500);

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
        Position = path.Points[0];
        Health = definition.Health;
        CoreTemperature = definition.IsMetal ? 200 : 35;
        SetVelocity(path.Points[1]);
        BaseTemperature = definition.IsMetal ? 200 : 35;
        UpperTemperature = definition.IsMetal ? 500 : 200;
        LowerTemperature = definition.IsMetal ? 0 : -200;
    }

    public long Id { get; }
    public FrontlineEnemyDefinition Definition { get; }
    public FrontlinePath Path { get; }
    public double Health { get; internal set; }
    public double Distance { get; private set; }
    public double CoreTemperature { get; private set; }
    public double BaseTemperature { get; }
    public double UpperTemperature { get; }
    public double LowerTemperature { get; }
    public double BurnDuration { get; private set; }
    public double RadiationDuration { get; private set; }
    public double PoisonDuration { get; private set; }
    public double FreezeDuration { get; private set; }
    public bool IsFrozen { get; private set; }
    public FrontlinePoint Position { get; private set; }
    /// <summary>Direction of movement in radians, used to pick the facing sprite frame.</summary>
    public double MovementAngle { get; private set; }
    internal bool HasReachedEnd => Path.UsesGridMovement
        ? _targetIndex >= Path.Points.Count
        : Distance >= Path.Length;

    internal void Move(double elapsedUnits)
    {
        var movement = Definition.Speed * MovementMultiplier * elapsedUnits;
        if (!Path.UsesGridMovement)
        {
            Distance += movement;
            Position = Path.PointAt(Distance);
            return;
        }

        Position = new FrontlinePoint(
            Position.X + _velocityX * movement,
            Position.Y + _velocityY * movement);
        Distance += movement;

        var target = Path.Points[_targetIndex];
        if (!double.IsFinite(Position.X)
            || !double.IsFinite(Position.Y)
            || !double.IsFinite(target.X)
            || !double.IsFinite(target.Y))
            return;

        var gridX = (int)Math.Floor(Position.X / FrontlinePath.DefaultGridCellSize);
        var gridY = (int)Math.Floor(Position.Y / FrontlinePath.DefaultGridCellSize);
        if (gridX == (int)Math.Floor(target.X / FrontlinePath.DefaultGridCellSize)
            && gridY == (int)Math.Floor(target.Y / FrontlinePath.DefaultGridCellSize))
        {
            _targetIndex++;
            if (!HasReachedEnd)
                SetVelocity(Path.Points[_targetIndex]);
        }
    }

    internal FrontlinePoint PredictPosition(double lookAhead) =>
        new(
            Position.X + _velocityX * lookAhead,
            Position.Y + _velocityY * lookAhead);

    private void SetVelocity(FrontlinePoint target)
    {
        var deltaX = target.X - Position.X;
        var deltaY = target.Y - Position.Y;
        var distance = new FrontlinePoint(deltaX, deltaY).Length;
        _velocityX = distance <= 0 ? 0 : deltaX / distance;
        _velocityY = distance <= 0 ? 0 : deltaY / distance;
        MovementAngle = Math.Atan2(_velocityY, _velocityX);
        SetDesiredAngle();
    }

    /// <summary>Smoothed vehicle hull angle in degrees, matching the legacy setAngle easing.</summary>
    public double DesiredAngleDegrees { get; private set; }

    private void SetDesiredAngle()
    {
        var degrees = MovementAngle * 180 / Math.PI;
        if (degrees != DesiredAngleDegrees)
        {
            var delta = ClosestAngle(DesiredAngleDegrees, degrees) * 0.3;
            if (Math.Abs(delta) < 0.1)
                DesiredAngleDegrees = degrees;
            else
                DesiredAngleDegrees += delta;
        }

        if (DesiredAngleDegrees >= 180)
            DesiredAngleDegrees -= 360;
        else if (DesiredAngleDegrees <= -180)
            DesiredAngleDegrees += 360;
    }

    private static double ClosestAngle(double target, double angle)
    {
        var delta = angle - target;
        delta %= 360;
        delta += 540;
        delta %= 360;
        return delta - 180;
    }

    internal void MarkRemoved() => Position = new FrontlinePoint(-50, -50);

    internal bool ApplyDamage(FrontlineDamageProfile damage)
    {
        var previousHealth = Health;
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

        return damage.DirectDamage > 0 && previousHealth > 0 && Health <= 0;
    }

    internal bool AdvanceEffects()
    {
        if (CoreTemperature > BaseTemperature)
            CoreTemperature -= 0.02;

        if (CoreTemperature < BaseTemperature)
            CoreTemperature += 0.02;

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

        _movementMultiplier = FreezeDuration > 0
            ? 0
            : PoisonDuration > 0
                ? 0.6
                : RadiationDuration > 0
                    ? Definition.IsMetal ? 0.4 : 0.6
                    : BurnDuration > 0 && !Definition.IsMetal
                        ? 1.2
                        : 1;

        FxTick++;
        if (FxTick <= 4)
            return false;

        FxTick = 0;
        return true;
    }

    private int FxTick { get; set; }

    private int _targetIndex = 1;
    private double _velocityX;
    private double _velocityY;
    private double _movementMultiplier = 1;

    internal double MovementMultiplier => _movementMultiplier;
    internal double VelocityX => _velocityX;
    internal double VelocityY => _velocityY;
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
        ExperienceLimit = definition.ExperienceLimit;
    }

    public long Id { get; }
    public FrontlineTowerDefinition Definition { get; }
    public FrontlineGridPoint Cell { get; internal set; }
    public FrontlinePoint Position { get; internal set; }
    public bool IsHeld { get; internal set; }
    public double SaleValue => Definition.Cost * 0.5;
    private double _damageLevel;
    private double _rangeLevel;
    private double _rateOfFireLevel;

    public double Loading { get; internal set; }
    public long? TargetId { get; internal set; }
    public int Kills { get; internal set; }
    public int ShotsFired { get; internal set; }
    public double DamageDealt { get; internal set; }
    public double Experience { get; internal set; }
    public double ExperienceLimit { get; internal set; }
    public double Range => Definition.Range * (1 + _rangeLevel);
    public double Damage => Definition.Damage * (1 + _damageLevel);
    public double RateOfFire => Definition.RateOfFire * (1 + _rateOfFireLevel);
    public double DamageUpgradeLevel => _damageLevel;
    public double RangeUpgradeLevel => _rangeLevel;
    public double RateOfFireUpgradeLevel => _rateOfFireLevel;

    internal void AddExperience(double damage) =>
        Experience += Math.Floor(damage);

    internal void AdvanceExperience()
    {
        if (Experience < ExperienceLimit)
            return;

        Experience -= ExperienceLimit;
        ExperienceLimit *= 1.2;
    }

    public bool UpgradeDamage(ref int funds) => Upgrade(ref _damageLevel, ref funds);
    public bool UpgradeRange(ref int funds) => Upgrade(ref _rangeLevel, ref funds);
    public bool UpgradeRateOfFire(ref int funds) => Upgrade(ref _rateOfFireLevel, ref funds);

    private bool Upgrade(ref double level, ref int funds)
    {
        var cost = (int)Math.Floor(Definition.Cost * 0.25);
        if (level >= 1 || funds <= cost)
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
        bool createsPoisonCloud,
        FrontlineTower owner)
    {
        public FrontlinePoint Impact { get; } = impact;
        public FrontlineTower Owner { get; } = owner;
        public double Damage { get; } = damage;
        public FrontlineDamageProfile StatusDamage { get; } = statusDamage;
        public bool CreatesFireCloud { get; } = createsFireCloud;
        public bool CreatesPoisonCloud { get; } = createsPoisonCloud;
        public double Height { get; private set; } = 450;
        public double VerticalVelocity { get; private set; } = -0.5;
        public int Frame { get; private set; }
        private double _frameTime;

        public bool Advance(double elapsedUnits)
        {
            Height += VerticalVelocity * elapsedUnits;
            VerticalVelocity -= 0.09 * elapsedUnits;
            _frameTime += 0.2 * elapsedUnits;
            Frame = (int)Math.Floor(_frameTime);
            if (Frame >= FrontlineSpriteStrip.Shell.FrameCount)
            {
                _frameTime = 0;
                Frame = 0;
            }

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

            if (_elapsed < Interval)
                return false;

            _elapsed = 0;
            return true;
        }
    }

    private readonly List<FrontlineEnemy> _enemies = [];
    private readonly List<FrontlineTower> _towers = [];
    private readonly List<FrontlineSupportProjectile> _supportProjectiles = [];
    private readonly List<FrontlineProjectile> _projectiles = [];
    private readonly List<FrontlineCombatTrace> _combatTraces = [];
    private readonly List<FrontlineAreaEffect> _areaEffects = [];
    private readonly List<FrontlineParticle> _particles = [];
    private readonly List<FrontlineDecal> _decals = [];
    private readonly List<FrontlineSpark> _sparks = [];
    private readonly List<FrontlineSound> _sounds = [];
    private readonly List<object> _projectileActors = [];
    private readonly List<object> _renderElements = [];
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
    public IReadOnlyList<FrontlineParticle> Particles => _particles;
    public IReadOnlyList<FrontlineDecal> Decals => _decals;
    public IReadOnlyList<FrontlineSpark> Sparks => _sparks;
    /// <summary>Queued sound cues for the host audio service.</summary>
    public IReadOnlyList<FrontlineSound> Sounds => _sounds;
    public int ActiveProjectileCount => _projectiles.Count + _areaEffects.Count;
    public int ActiveSupportProjectileCount => _supportProjectiles.Count;
    public int ActiveAreaEffectCount => _areaEffects.Count;

    internal void AddCombatTrace(
        FrontlinePoint start,
        FrontlinePoint end,
        FrontlineCombatTraceKind kind,
        double life,
        double weight = 1)
    {
        if (!double.IsFinite(start.X)
            || !double.IsFinite(start.Y)
            || !double.IsFinite(end.X)
            || !double.IsFinite(end.Y)
            || !double.IsFinite(life)
            || life <= 0)
            return;

        AddCombatTrace(new FrontlineCombatTrace(start, end, kind, life, weight));
    }

    /// <summary>
    /// Adds a legacy effect particle. The effect stream deliberately does not consume the
    /// simulation random source, so cosmetic jitter can never change gameplay outcomes.
    /// </summary>
    internal void AddParticle(
        FrontlineSpriteStrip strip,
        FrontlinePoint position,
        double velocityX,
        double velocityY,
        double frameRate,
        double lifeSpan,
        bool glow = false)
    {
        if (!double.IsFinite(position.X) || !double.IsFinite(position.Y))
            return;

        _particles.Add(new FrontlineParticle(
            strip,
            position,
            velocityX,
            velocityY,
            frameRate,
            lifeSpan,
            glow));
    }

    internal void AddSpark(FrontlinePoint position, int life)
    {
        if (!double.IsFinite(position.X) || !double.IsFinite(position.Y))
            return;

        _sparks.Add(new FrontlineSpark(
            position,
            life,
            -4 + Random.Shared.NextDouble() * 8,
            -4 + Random.Shared.NextDouble() * 4,
            fade: true));
    }

    internal void AddDecal(FrontlineSpriteStrip strip, FrontlinePoint position)
    {
        if (!double.IsFinite(position.X) || !double.IsFinite(position.Y))
            return;

        var jittered = new FrontlinePoint(
            position.X + (-4 + Random.Shared.NextDouble() * 8),
            position.Y + (-4 + Random.Shared.NextDouble() * 8));
        _decals.Add(new FrontlineDecal(jittered, strip, Random.Shared.Next(strip.FrameCount)));
    }

    internal void AddSound(FrontlineSound sound)
    {
        if (_sounds.Count < 64)
            _sounds.Add(sound);
    }

    /// <summary>Returns the queued sound cues and clears the queue.</summary>
    public IReadOnlyList<FrontlineSound> DrainSounds()
    {
        if (_sounds.Count == 0)
            return [];

        var drained = _sounds.ToArray();
        _sounds.Clear();
        return drained;
    }

    public bool TryBuildTower(FrontlineTowerDefinition definition, FrontlinePoint position, bool costAlreadyPaid = false)
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
                    || !double.IsFinite(projectileProfile.VerticalVelocityScale) || projectileProfile.VerticalVelocityScale < 0
                    || (projectileProfile.Lifetime is double lifetime
                        && (!double.IsFinite(lifetime) || lifetime <= 0))))
            || (definition.IsSupport && definition.ShotLimit <= 0)
            || (!costAlreadyPaid && Funds < definition.Cost))
            return false;

        var cell = ToGridCell(position);

        // Support strikes are placed anywhere: the legacy X.dropTower skips the grid
        // occupancy check and never writes a tower value into the level grid.
        if (!definition.IsSupport)
        {
            if (LevelMap is not null && !LevelMap.IsBuildable(cell))
                return false;

            if (_towers.Any(tower => !tower.Definition.IsSupport && tower.Cell == cell))
                return false;
        }

        if (!costAlreadyPaid)
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
        if (_towers.Any(candidate => candidate.Id != towerId
                && !candidate.Definition.IsSupport
                && candidate.Cell == cell)
            || (LevelMap is not null && !LevelMap.IsBuildable(cell)))
            return false;

        Funds -= 10;
        tower.Cell = cell;
        tower.Position = ToCellCenter(cell);
        tower.TargetId = null;
        return true;
    }

    public bool TryBeginMoveTower(long towerId)
    {
        if (Status is FrontlineGameStatus.Victory or FrontlineGameStatus.Defeat
            || Funds < 10)
            return false;

        var tower = _towers.FirstOrDefault(candidate => candidate.Id == towerId);
        if (tower is null || tower.Definition.IsSupport || tower.IsHeld)
            return false;

        Funds -= 10;
        tower.Cell = new FrontlineGridPoint(-1, -1);
        tower.IsHeld = true;
        tower.TargetId = null;
        return true;
    }

    public bool TryCompleteMoveTower(long towerId, FrontlinePoint position)
    {
        if (Status is FrontlineGameStatus.Victory or FrontlineGameStatus.Defeat
            || !IsValidPosition(position))
            return false;

        var tower = _towers.FirstOrDefault(candidate => candidate.Id == towerId);
        if (tower is null || !tower.IsHeld)
            return false;

        var cell = ToGridCell(position);
        if (_towers.Any(candidate => candidate.Id != towerId
                && !candidate.Definition.IsSupport
                && candidate.Cell == cell)
            || (LevelMap is not null && !LevelMap.IsBuildable(cell)))
            return false;

        tower.Cell = cell;
        tower.Position = ToCellCenter(cell);
        tower.IsHeld = false;
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
        AdvanceBeforeWaveScheduler(elapsedUnits);
        AdvanceAfterWaveScheduler(elapsedUnits);
    }

    internal void AdvanceBeforeWaveScheduler(double elapsedUnits)
    {
        if (elapsedUnits < 0 || double.IsNaN(elapsedUnits) || double.IsInfinity(elapsedUnits))
            throw new ArgumentOutOfRangeException(nameof(elapsedUnits));

        if (Status is FrontlineGameStatus.Victory or FrontlineGameStatus.Defeat)
            return;

        Tick++;
        if (Status == FrontlineGameStatus.Running)
            MoveEnemies(elapsedUnits);

        ResolveDeaths();
        AdvanceProjectileActors(elapsedUnits);

        var initialTowerCount = _towers.Count;
        for (var index = 0; index < initialTowerCount && index < _towers.Count; index++)
        {
            var tower = _towers[index];
            if (tower.IsHeld)
                continue;

            // The legacy tower stores its firing meter through a uint32 coercion every tick,
            // which truncates the fractional part before the 100 threshold check.
            tower.Loading = Math.Floor(tower.Loading + tower.RateOfFire * elapsedUnits);

            if (tower.Definition.IsSupport)
                FireSupportStrike(tower);
            else if (Status == FrontlineGameStatus.Running)
                Fire(tower);

            tower.AdvanceExperience();
        }

        ResolveDeaths();
    }

    /// <summary>Ends the mission as a defeat, matching the legacy quit button that zeroed level health.</summary>
    public void Forfeit()
    {
        if (Status is FrontlineGameStatus.Victory or FrontlineGameStatus.Defeat)
            return;

        LevelHealth = 0;
        Status = FrontlineGameStatus.Defeat;
    }

    internal void AdvanceAfterWaveScheduler(double elapsedUnits)
    {
        if (Status is FrontlineGameStatus.Victory or FrontlineGameStatus.Defeat)
            return;

        AdvanceRenderElements(elapsedUnits);

        if (LevelHealth <= 0)
        {
            Status = FrontlineGameStatus.Defeat;
            return;
        }

        if (_enemies.Count == 0
            && _projectiles.Count == 0
            && _supportProjectiles.Count == 0
            && _areaEffects.Count == 0
            && Status == FrontlineGameStatus.Running)
            Status = FrontlineGameStatus.Planning;
    }

    private void MoveEnemies(double elapsedUnits)
    {
        var initialEnemyCount = _enemies.Count;
        for (var index = 0; index < initialEnemyCount && index < _enemies.Count; index++)
        {
            var enemy = _enemies[index];
            if (enemy.HasReachedEnd)
            {
                LevelHealth = Math.Max(0, LevelHealth - enemy.Health);
                enemy.MarkRemoved();
                _enemies.RemoveAt(index);
                continue;
            }

            enemy.Move(elapsedUnits);
            if (enemy.HasReachedEnd)
            {
                LevelHealth = Math.Max(0, LevelHealth - enemy.Health);
                enemy.MarkRemoved();
                _enemies.RemoveAt(index);
                continue;
            }

            var emitsStatusEffect = enemy.AdvanceEffects();
            if (emitsStatusEffect)
                AddStatusEffects(enemy);

            if (enemy.Health <= 0)
            {
                AddDeathEffects(enemy);
                Funds += enemy.Definition.Reward;
                enemy.MarkRemoved();
                _enemies.RemoveAt(index);
            }
        }
    }

    /// <summary>
    /// Emits the legacy <c>Enemy.makeFX</c> status particles: one 16px sprite per active
    /// damage-over-time channel, every fifth simulation tick.
    /// </summary>
    private void AddStatusEffects(FrontlineEnemy enemy)
    {
        var origin = new FrontlinePoint(enemy.Position.X, enemy.Position.Y - 8);
        if (enemy.BurnDuration > 0)
        {
            AddParticle(
                FrontlineSpriteStrip.Flame,
                origin,
                -0.2 + Random.Shared.NextDouble() * 0.4,
                -Random.Shared.NextDouble() * 0.2,
                0.1,
                8,
                glow: true);
        }

        if (enemy.PoisonDuration > 0)
        {
            AddParticle(
                FrontlineSpriteStrip.Poison,
                origin,
                -0.2 + Random.Shared.NextDouble() * 0.4,
                -Random.Shared.NextDouble() * 0.5,
                0.2,
                8);
        }

        if (enemy.RadiationDuration > 0)
        {
            AddParticle(
                FrontlineSpriteStrip.Radiation,
                origin,
                -0.2 + Random.Shared.NextDouble() * 0.4,
                -Random.Shared.NextDouble() * 0.5,
                0.2,
                8,
                glow: true);
        }

        if (enemy.FreezeDuration > 0)
        {
            AddParticle(
                FrontlineSpriteStrip.Freeze,
                origin,
                -0.2 + Random.Shared.NextDouble() * 0.4,
                -Random.Shared.NextDouble() * 0.5,
                0.1,
                8,
                glow: true);
        }
    }

    /// <summary>Emits the legacy per-archetype <c>deathFX</c> particles and decals.</summary>
    private void AddDeathEffects(FrontlineEnemy enemy)
    {
        if (!double.IsFinite(enemy.Position.X) || !double.IsFinite(enemy.Position.Y))
            return;

        var position = enemy.Position;
        switch (enemy.Definition.Id)
        {
            case "bot_1":
                AddBloodSplats(position, 2);
                AddDecal(FrontlineSpriteStrip.Crater, position);
                AddSound(FrontlineSound.Explode);
                return;
            case "enemy1":
            case "enemy2":
            case "enemy3":
            case "enemy4":
            case "enemy5":
                AddBloodSplats(position, 4);
                AddDecal(FrontlineSpriteStrip.BloodDecal, position);
                AddSound(FrontlineSound.Splat);
                return;
            default:
                AddVehicleDeath(position);
                AddSound(FrontlineSound.Explode);
                return;
        }
    }

    private void AddBloodSplats(FrontlinePoint position, int count)
    {
        for (var index = 0; index < count; index++)
        {
            AddParticle(
                FrontlineSpriteStrip.Blood,
                position,
                -1 + Random.Shared.NextDouble() * 2,
                -Random.Shared.NextDouble() * 2,
                0.5,
                8);
        }
    }

    private void AddVehicleDeath(FrontlinePoint position)
    {
        AddParticle(FrontlineSpriteStrip.FlameShot, position,
            -0.5 + Random.Shared.NextDouble(), -Random.Shared.NextDouble() * 2, 0.1, 10, glow: true);
        AddParticle(FrontlineSpriteStrip.FlameShot, position,
            -0.5 + Random.Shared.NextDouble(), -Random.Shared.NextDouble(), 0.2, 10, glow: true);
        AddParticle(FrontlineSpriteStrip.Smoke, position,
            -0.5 + Random.Shared.NextDouble(), -Random.Shared.NextDouble(), 0.1, 10);
        AddParticle(FrontlineSpriteStrip.Smoke, position,
            -0.5 + Random.Shared.NextDouble(), -Random.Shared.NextDouble() * 1.2, 0.1, 10);
        AddParticle(FrontlineSpriteStrip.Smoke, position,
            -0.5 + Random.Shared.NextDouble(), -Random.Shared.NextDouble(), 0.2, 10);
        AddDecal(FrontlineSpriteStrip.Crater, position);
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

    public void AddFunds(int amount)
    {
        if (amount < 0)
            throw new ArgumentOutOfRangeException(nameof(amount));

        Funds += amount;
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

        ApplyDamageToEnemy(enemy, damage);
        return true;
    }

    private void Fire(FrontlineTower tower)
    {
        var target = tower.TargetId is long targetId
            ? _enemies.FirstOrDefault(enemy => enemy.Id == targetId)
            : null;

        if (target is null || target.Health <= 0)
        {
            target = FindTarget(tower);
            tower.TargetId = target?.Id;
            return;
        }

        if (tower.Loading < 100)
            return;

        if (target.Position.DistanceTo(tower.Position) > tower.Range)
        {
            target = FindTarget(tower);
            tower.TargetId = target?.Id;
            tower.Loading = 90;
            return;
        }

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
            var projectileAimPosition = tower.Definition.Id == "tower1"
                ? target.PredictPosition(4)
                : target.Position;
            var deltaX = projectileAimPosition.X - tower.Position.X;
            var deltaY = projectileAimPosition.Y - tower.Position.Y;
            var distance = new FrontlinePoint(deltaX, deltaY).Length;
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

            var projectile = new FrontlineProjectile(
                _nextProjectileId++,
                tower,
                target,
                position,
                damageProfile,
                projectileProfile,
                initialVelocityX,
                initialVelocityY);
            _projectiles.Add(projectile);
            if (projectileProfile.Kind == FrontlineProjectileKind.HomingMissile)
                _projectileActors.Add(projectile);
            else
                _renderElements.Add(projectile);

            AddSound(projectileProfile.Kind switch
            {
                FrontlineProjectileKind.HomingMissile => FrontlineSound.Missile,
                FrontlineProjectileKind.TrackingPoison => FrontlineSound.Bio,
                FrontlineProjectileKind.TrackingFlame => FrontlineSound.Flame,
                _ => FrontlineSound.Launcher
            });
        }
        else
        {
            FireHitscan(tower, target, damageProfile);
        }

        tower.Loading = 0;
        if (tower.Definition.Id == "tower6")
            tower.TargetId = null;

        ResolveDeaths();
    }

    /// <summary>
    /// Reproduces the legacy hitscan firing path, including the muzzle flash, tapered
    /// tracer, laser blast chain and impact spark/blood particles of each tower.
    /// </summary>
    private void FireHitscan(
        FrontlineTower tower,
        FrontlineEnemy target,
        FrontlineDamageProfile damage)
    {
        var towerId = tower.Definition.Id;
        var targetPosition = target.Position;
        var velocityX = target.VelocityX;
        var velocityY = target.VelocityY;
        var impactPosition = new FrontlinePoint(targetPosition.X, targetPosition.Y - 6);
        AddSound(towerId switch
        {
            "tower2" => FrontlineSound.Rifle,
            "tower3" => FrontlineSound.Sniper,
            "tower4" => FrontlineSound.Electro,
            "tower5" => FrontlineSound.Shotgun,
            "tower6" => FrontlineSound.Laser,
            "tower10" => FrontlineSound.MachineGun,
            _ => FrontlineSound.Rifle
        });

        if (towerId == "tower4")
        {
            var towerMuzzle = new FrontlinePoint(tower.Position.X, tower.Position.Y - 24);
            AddCombatTrace(new FrontlineCombatTrace(
                towerMuzzle,
                new FrontlinePoint(targetPosition.X, targetPosition.Y - 8),
                FrontlineCombatTraceKind.Lightning,
                tower.Damage));
            AddParticle(
                FrontlineSpriteStrip.BlueFlash,
                new FrontlinePoint(targetPosition.X, targetPosition.Y - 8),
                velocityX,
                velocityY,
                0.5,
                8,
                glow: true);
            AddParticle(FrontlineSpriteStrip.BlueFlash, towerMuzzle, 0, 0, 0.5, 8, glow: true);
            AddSpark(new FrontlinePoint(targetPosition.X, targetPosition.Y - 8), 20);
            ApplyDamageToEnemy(target, damage, tower);
            return;
        }

        var aimX = targetPosition.X + velocityX * 4 - tower.Position.X;
        var aimY = targetPosition.Y + velocityY * 4 - tower.Position.Y;
        var aimLength = new FrontlinePoint(aimX, aimY).Length;
        var offsetX = aimLength <= 0 ? 0 : aimX / aimLength * 20;
        var offsetY = aimLength <= 0 ? 0 : aimY / aimLength * 20;

        var muzzle = towerId switch
        {
            "tower2" => new FrontlinePoint(
                tower.Position.X + offsetX - offsetY * 0.1,
                tower.Position.Y + offsetY - 10 + offsetX * 0.1),
            "tower3" => new FrontlinePoint(
                tower.Position.X + offsetX - offsetY * 0.1,
                tower.Position.Y + offsetY - 25 + offsetX * 0.1),
            "tower5" => new FrontlinePoint(
                tower.Position.X + offsetX - offsetY * 0.1,
                tower.Position.Y + offsetY - 12 + offsetX * 0.1),
            "tower6" => new FrontlinePoint(
                tower.Position.X + offsetX - offsetY * 0.1,
                tower.Position.Y + offsetY - 10 + offsetX * 0.1),
            _ => new FrontlinePoint(
                tower.Position.X + offsetX,
                tower.Position.Y + offsetY - 8)
        };

        AddParticle(FrontlineSpriteStrip.MuzzleFlash, muzzle, 0, 0, 1, 8, glow: true);

        if (towerId == "tower6")
        {
            var stepX = aimLength <= 0 ? 0 : aimX / aimLength * 5;
            var stepY = aimLength <= 0 ? 0 : aimY / aimLength * 5;
            var blastX = tower.Position.X + offsetX;
            var blastY = tower.Position.Y + offsetY - 10;
            var blastCount = (int)Math.Round(aimLength / 5) - 4;
            var blastRate = 2.0;
            for (var index = 0; index < blastCount; index++)
            {
                if (blastRate > 0.3)
                    blastRate -= 0.1;

                AddParticle(
                    FrontlineSpriteStrip.LaserBlast,
                    new FrontlinePoint(blastX, blastY),
                    0,
                    0,
                    blastRate,
                    8,
                    glow: true);
                blastX += stepX;
                blastY += stepY;
            }

            AddCombatTrace(new FrontlineCombatTrace(
                muzzle,
                impactPosition,
                FrontlineCombatTraceKind.EnergyBeam,
                6,
                2));
            AddSpark(impactPosition, 20);
            AddSpark(impactPosition, 20);
            AddSpark(impactPosition, 20);
            ApplyDamageToEnemy(target, damage, tower);
            return;
        }

        if (towerId == "tower5")
        {
            for (var pellet = 0; pellet < 3; pellet++)
            {
                var spreadX = Random.Shared.NextDouble() * 16 - 8;
                var spreadY = Random.Shared.NextDouble() * 16 - 8;
                AddCombatTrace(new FrontlineCombatTrace(
                    muzzle,
                    new FrontlinePoint(impactPosition.X + spreadX, impactPosition.Y + spreadY),
                    FrontlineCombatTraceKind.Shotgun,
                    5,
                    3));
            }
        }
        else
        {
            AddCombatTrace(new FrontlineCombatTrace(
                muzzle,
                impactPosition,
                FrontlineCombatTraceKind.Bullet,
                6,
                2));
        }

        if (target.Definition.IsMetal)
        {
            AddSpark(impactPosition, 20);
        }
        else
        {
            AddParticle(
                FrontlineSpriteStrip.Blood,
                targetPosition,
                offsetX * 0.05,
                offsetY * 0.05,
                0.5,
                8);
        }

        ApplyDamageToEnemy(target, damage, tower);
    }

    private void AddCombatTrace(FrontlineCombatTrace trace)
    {
        _combatTraces.Add(trace);
        _renderElements.Add(trace);
    }

    private void AdvanceProjectileActors(double elapsedUnits)
    {
        var initialActorCount = _projectileActors.Count;
        for (var index = 0; index < initialActorCount && index < _projectileActors.Count; index++)
        {
            var actor = _projectileActors[index];
            if (actor is FrontlineProjectile projectile)
            {
                var target = _enemies.FirstOrDefault(enemy => enemy.Id == projectile.TargetId)
                    ?? projectile.Target;

                if (projectile.Profile.Kind == FrontlineProjectileKind.HomingMissile)
                {
                    projectile.TrailTick++;
                    if (projectile.TrailTick >= 4)
                    {
                        projectile.TrailTick = 0;
                        AddParticle(
                            FrontlineSpriteStrip.Trail,
                            new FrontlinePoint(
                                projectile.Position.X - projectile.VelocityX * 3,
                                projectile.Position.Y - projectile.VelocityY * 3 - 6),
                            0,
                            0,
                            0.5,
                            10);
                    }
                }

                if (!projectile.Advance(target, elapsedUnits))
                {
                    if (projectile.IsExpired)
                    {
                        AddMissileExplosion(projectile.Position);
                        _projectiles.Remove(projectile);
                        _projectileActors.RemoveAt(index);
                    }

                    continue;
                }

                if (projectile.Profile.SplashRadius > 0)
                {
                    var splashDamage = projectile.Damage with
                    {
                        DirectDamage = projectile.Damage.DirectDamage * projectile.Profile.SplashDamageScale
                    };
                    ApplyAreaDamage(
                        projectile.Position,
                        projectile.Profile.SplashRadius,
                        splashDamage,
                        projectile.Owner);
                    AddMissileExplosion(projectile.Position);
                }
                else
                {
                    ApplyDamageToEnemy(target, projectile.Damage, projectile.Owner);
                }

                projectile.Expire();
                _projectiles.Remove(projectile);
                _projectileActors.RemoveAt(index);
                continue;
            }

            if (actor is FrontlineAreaEffect effect)
            {
                var pulse = effect.Advance(elapsedUnits);
                if (effect.LifeRemaining <= 0)
                {
                    _areaEffects.Remove(effect);
                    _projectileActors.RemoveAt(index);
                }

                if (pulse)
                    ApplyAreaDamage(effect.Position, effect.Radius, effect.Damage);

                AddCloudParticle(effect);
            }
        }
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

        var projectile = new FrontlineSupportProjectile(
            impact,
            tower.Damage * 0.5,
            statusDamage,
            tower.Definition.Id == "suptower1",
            tower.Definition.Id == "suptower2",
            tower);
        _supportProjectiles.Add(projectile);
        _renderElements.Add(projectile);
        AddSound(FrontlineSound.Bio);

        tower.Loading = 0;
        tower.ShotsFired++;
        if (tower.ShotsFired >= tower.Definition.ShotLimit)
            _towers.Remove(tower);
    }

    private void AdvanceRenderElements(double elapsedUnits)
    {
        var initialElementCount = _renderElements.Count;
        for (var index = 0; index < initialElementCount && index < _renderElements.Count; index++)
        {
            var element = _renderElements[index];
            if (element is FrontlineCombatTrace trace)
            {
                if (!trace.Advance())
                {
                    _combatTraces.Remove(trace);
                    _renderElements.RemoveAt(index);
                }

                continue;
            }

            if (element is FrontlineProjectile projectile)
            {
                var target = _enemies.FirstOrDefault(enemy => enemy.Id == projectile.TargetId)
                    ?? projectile.Target;

                if (projectile.EmitsTrail
                    && projectile.Profile.Kind == FrontlineProjectileKind.BallisticShell
                    && Random.Shared.NextDouble() > 0.5)
                {
                    AddParticle(
                        FrontlineSpriteStrip.Trail,
                        new FrontlinePoint(
                            projectile.Position.X,
                            projectile.Position.Y - projectile.Height),
                        0,
                        -0.1,
                        0.2,
                        10);
                }

                if (!projectile.Advance(target, elapsedUnits))
                {
                    if (projectile.IsExpired)
                    {
                        _projectiles.Remove(projectile);
                        _renderElements.RemoveAt(index);
                    }

                    continue;
                }

                if (projectile.Profile.SplashRadius > 0)
                {
                    var splashDamage = projectile.Damage with
                    {
                        DirectDamage = projectile.Damage.DirectDamage * projectile.Profile.SplashDamageScale
                    };
                    ApplyAreaDamage(
                        projectile.Position,
                        projectile.Profile.SplashRadius,
                        splashDamage,
                        projectile.Owner);
                    AddParticle(
                        FrontlineSpriteStrip.Explosion,
                        projectile.Position,
                        0,
                        -0.1,
                        0.5,
                        16,
                        glow: true);
                    AddSound(FrontlineSound.Explode);
                }
                else
                {
                    ApplyDamageToEnemy(target, projectile.Damage, projectile.Owner);
                }

                projectile.Expire();
                _projectiles.Remove(projectile);
                _renderElements.RemoveAt(index);
                continue;
            }

            if (element is FrontlineSupportProjectile supportProjectile)
            {
                if (Random.Shared.NextDouble() > 0.5)
                {
                    AddParticle(
                        FrontlineSpriteStrip.Trail,
                        new FrontlinePoint(
                            supportProjectile.Impact.X,
                            supportProjectile.Impact.Y - supportProjectile.Height),
                        0,
                        -0.1,
                        0.2,
                        10);
                }

                if (!supportProjectile.Advance(elapsedUnits))
                    continue;

                var explosionDamage = new FrontlineDamageProfile(
                    supportProjectile.Damage,
                    supportProjectile.StatusDamage.Burn,
                    supportProjectile.StatusDamage.Radiation,
                    supportProjectile.StatusDamage.Poison,
                    supportProjectile.StatusDamage.Freeze);
                ApplyAreaDamage(supportProjectile.Impact, 80, explosionDamage, supportProjectile.Owner);
                AddParticle(
                    FrontlineSpriteStrip.Explosion,
                    supportProjectile.Impact,
                    0,
                    -0.1,
                    0.5,
                    16,
                    glow: true);
                AddSound(FrontlineSound.Explode);

                if (supportProjectile.CreatesFireCloud)
                {
                    var effect = new FrontlineAreaEffect(
                        supportProjectile.Impact,
                        40,
                        new FrontlineDamageProfile(Burn: 100),
                        500,
                        10);
                    _areaEffects.Add(effect);
                    _projectileActors.Add(effect);
                    AddDecal(FrontlineSpriteStrip.Crater, supportProjectile.Impact);
                }
                else if (supportProjectile.CreatesPoisonCloud)
                {
                    var effect = new FrontlineAreaEffect(
                        supportProjectile.Impact,
                        40,
                        new FrontlineDamageProfile(Poison: 20),
                        1000,
                        20);
                    _areaEffects.Add(effect);
                    _projectileActors.Add(effect);
                    AddDecal(FrontlineSpriteStrip.Crater, supportProjectile.Impact);
                }

                _supportProjectiles.Remove(supportProjectile);
                _renderElements.RemoveAt(index);
            }
        }

        AdvanceParticles(elapsedUnits);
        ResolveDeaths();
    }

    private void AdvanceParticles(double elapsedUnits)
    {
        for (var index = _particles.Count - 1; index >= 0; index--)
        {
            if (!_particles[index].Advance(elapsedUnits))
                _particles.RemoveAt(index);
        }

        for (var index = _sparks.Count - 1; index >= 0; index--)
        {
            if (!_sparks[index].Advance())
                _sparks.RemoveAt(index);
        }
    }

    private void AddMissileExplosion(FrontlinePoint position)
    {
        AddParticle(FrontlineSpriteStrip.FireExplosion, position, 0, 0, 0.5, 16, glow: true);
        AddParticle(FrontlineSpriteStrip.Explosion, position, 0, -1, 0.5, 16, glow: true);
        AddParticle(
            FrontlineSpriteStrip.Smoke,
            position,
            -0.5 + Random.Shared.NextDouble(),
            -Random.Shared.NextDouble(),
            0.2,
            10);
        AddDecal(FrontlineSpriteStrip.Crater, position);
        AddSound(FrontlineSound.Explode);
    }

    private void AddCloudParticle(FrontlineAreaEffect effect)
    {
        if (Random.Shared.NextDouble() <= 0.7)
            return;

        var isFire = effect.Damage.Burn > 0;
        AddParticle(
            isFire ? FrontlineSpriteStrip.FlameShot : FrontlineSpriteStrip.BioCloud,
            effect.Position,
            -0.2 + Random.Shared.NextDouble() * 0.4,
            -(Random.Shared.NextDouble() * (isFire ? 0.5 : 0.3)),
            0.1,
            10,
            glow: isFire);
    }

    private FrontlineEnemy? FindTarget(FrontlineTower tower)
    {
        FrontlineEnemy? selected = null;
        foreach (var enemy in _enemies)
        {
            if (enemy.Health <= 0
                || enemy.Position.DistanceTo(tower.Position) > tower.Range)
                continue;

            if (selected is null)
            {
                selected = enemy;
                continue;
            }

            switch (tower.Definition.TargetMode)
            {
                // The legacy enemy exposes a `traveled` counter that the runtime never
                // advances, so near and far both resolve to the first in-range enemy.
                case FrontlineTargetMode.Near:
                case FrontlineTargetMode.Far:
                    break;
                case FrontlineTargetMode.Strong:
                    if (enemy.Health > selected.Health)
                        selected = enemy;
                    break;
                case FrontlineTargetMode.Weak:
                    if (enemy.Health < selected.Health)
                        selected = enemy;
                    break;
                case FrontlineTargetMode.Heat:
                    if (enemy.CoreTemperature > selected.CoreTemperature
                        && enemy.CoreTemperature > 120)
                        selected = enemy;
                    break;
                case FrontlineTargetMode.Cold:
                    if (enemy.CoreTemperature < selected.CoreTemperature
                        && !enemy.Definition.IsMetal)
                        selected = enemy;
                    break;
            }
        }

        return selected;
    }

    private bool ApplyDamageToEnemy(
        FrontlineEnemy enemy,
        FrontlineDamageProfile damage,
        FrontlineTower? owner = null)
    {
        if (!_enemies.Contains(enemy))
            return false;

        var killed = enemy.ApplyDamage(damage);
        if (owner is not null)
        {
            owner.DamageDealt += damage.DirectDamage;
            owner.AddExperience(damage.DirectDamage);
        }

        if (!killed)
            return false;

        AddDeathEffects(enemy);
        Score += enemy.Definition.Reward * 2;
        Funds += enemy.Definition.Reward;
        if (owner is not null)
            owner.Kills++;

        enemy.MarkRemoved();
        _enemies.Remove(enemy);
        return true;
    }

    private void ApplyAreaDamage(
        FrontlinePoint position,
        double radius,
        FrontlineDamageProfile damage,
        FrontlineTower? owner = null)
    {
        var initialEnemyCount = _enemies.Count;
        for (var index = 0; index < initialEnemyCount && index < _enemies.Count; index++)
        {
            var enemy = _enemies[index];
            if (enemy.Position.DistanceTo(position) <= radius)
                ApplyDamageToEnemy(enemy, damage, owner);
        }
    }

    private void ResolveDeaths()
    {
        for (var index = _enemies.Count - 1; index >= 0; index--)
        {
            var enemy = _enemies[index];
            if (enemy.Health > 0)
                continue;

            Funds += enemy.Definition.Reward;
            enemy.MarkRemoved();
            _enemies.RemoveAt(index);
        }
    }
}