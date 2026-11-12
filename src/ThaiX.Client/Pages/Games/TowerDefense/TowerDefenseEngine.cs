using ThaiX.Client.Constants;

namespace ThaiX.Client.Pages.Games.TowerDefense;

public enum TowerDefenseMode { Campaign, Endless }
public enum TowerTargeting { First, Last, Strongest, Weakest, Closest, Farthest }

public sealed record TowerDefinition(
    string Id, string NameKey, int Cost, double Damage, double Range, int Cooldown,
    string Color, int SplashRadius = 0, int SlowTicks = 0, bool AntiAir = false);

public sealed record EnemyDefinition(
    string Id, string NameKey, double Health, double Speed, int Armor, int Reward,
    int Score, string Color, bool IsFlying = false, bool IsBoss = false);

public sealed record TowerDefenseMapDefinition(
    string Id, string NameKey, (int Column, int Row)[] Path, int[] Obstacles);

public sealed record TowerDefenseMissionDefinition(
    int Number, string MapId, int WaveCount, int StartingGold, int StartingLives, double Difficulty);

public sealed class TowerDefenseTowerState
{
    public int Cell { get; init; }
    public required TowerDefinition Definition { get; init; }
    public int DamageLevel { get; set; } = 1;
    public int RangeLevel { get; set; } = 1;
    public int RateLevel { get; set; } = 1;
    public int Cooldown { get; set; }
    public double Angle { get; set; }
    public TowerTargeting Targeting { get; set; } = TowerTargeting.First;
    public int TotalLevel => DamageLevel + RangeLevel + RateLevel - 2;
    public double PermanentDamageMultiplier { get; init; } = 1;
    public double Damage => Definition.Damage * (1 + (DamageLevel - 1) * 0.65) * PermanentDamageMultiplier;
    public double Range => Definition.Range + (RangeLevel - 1) * 12;
    public int AttackCooldown => Math.Max(3, Definition.Cooldown - (RateLevel - 1) * 2);
}

public sealed class TowerDefenseEnemyState
{
    public required EnemyDefinition Definition { get; init; }
    public double Distance { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Health { get; set; }
    public double MaxHealth { get; init; }
    public double Speed { get; init; }
    public int SlowTicks { get; set; }
    public int BuffTicks { get; set; }
    public int ArmorBonus { get; set; }
    public double Angle { get; set; }
}

public sealed record TowerDefenseShot(double X1, double Y1, double X2, double Y2, string Color, int ExpiresAt);
public sealed record TowerDefenseEffect(double X, double Y, double Radius, string Color, int StartedAt);

public sealed class TowerDefenseEngine
{
    public const int Columns = 10;
    public const int Rows = 7;
    public const int CellSize = 48;
    public const int Top = 40;
    public const int MaxScore = 100_000;
    private const int SpawnInterval = 24;
    private const int EffectDuration = 12;
    private const int AbilityCooldown = 300;

    public static IReadOnlyList<TowerDefinition> TowerDefinitions { get; } =
    [
        new("rifleman", ResourceKeys.Games.TowerDefenseTowerRifleman, 50, 8, 105, 12, "#d6b45a"),
        new("sniper", ResourceKeys.Games.TowerDefenseTowerSniper, 120, 34, 190, 48, "#9ca3af"),
        new("machine-gun", ResourceKeys.Games.TowerDefenseTowerMachineGun, 150, 5, 125, 4, "#65a30d"),
        new("cannon", ResourceKeys.Games.TowerDefenseTowerCannon, 200, 24, 115, 30, "#d97706", 34),
        new("flamethrower", ResourceKeys.Games.TowerDefenseTowerFlamethrower, 175, 4, 78, 5, "#ea580c", 24),
        new("freezer", ResourceKeys.Games.TowerDefenseTowerFreezer, 125, 3, 110, 18, "#0891b2", 18, 42),
        new("electric", ResourceKeys.Games.TowerDefenseTowerElectric, 225, 14, 120, 20, "#a78bfa", 28, 12, true),
        new("rocket", ResourceKeys.Games.TowerDefenseTowerRocket, 300, 48, 155, 42, "#b45309", 42, 0, true),
        new("plasma", ResourceKeys.Games.TowerDefenseTowerPlasma, 350, 28, 145, 18, "#7c3aed", 20, 0, true),
        new("artillery", ResourceKeys.Games.TowerDefenseTowerArtillery, 400, 72, 230, 60, "#475569", 54, 0, true)
    ];

    public static IReadOnlyList<EnemyDefinition> EnemyDefinitions { get; } =
    [
        new("infantry", ResourceKeys.Games.TowerDefenseEnemyInfantry, 24, 1.25, 0, 10, 10, "#a3a3a3"),
        new("scout", ResourceKeys.Games.TowerDefenseEnemyScout, 18, 2.1, 0, 12, 10, "#f59e0b"),
        new("jeep", ResourceKeys.Games.TowerDefenseEnemyJeep, 58, 1.65, 2, 18, 20, "#ca8a04"),
        new("armored", ResourceKeys.Games.TowerDefenseEnemyArmored, 110, 0.95, 7, 25, 20, "#78716c"),
        new("tank", ResourceKeys.Games.TowerDefenseEnemyTank, 240, 0.72, 12, 42, 30, "#991b1b"),
        new("helicopter", ResourceKeys.Games.TowerDefenseEnemyHelicopter, 76, 1.8, 1, 30, 20, "#64748b", true),
        new("medic", ResourceKeys.Games.TowerDefenseEnemyMedic, 90, 0.9, 2, 35, 30, "#16a34a"),
        new("commander", ResourceKeys.Games.TowerDefenseEnemyCommander, 900, 0.58, 16, 150, 100, "#7f1d1d", false, true)
    ];

    public static IReadOnlyList<TowerDefenseMapDefinition> Maps { get; } =
    [
        new("green-valley", ResourceKeys.Games.TowerDefenseMapGreenValley,
            [(-1, 1), (7, 1), (7, 3), (2, 3), (2, 5), (10, 5)], [9, 19, 50, 60, 30, 39]),
        new("desert-outpost", ResourceKeys.Games.TowerDefenseMapDesertOutpost,
            [(-1, 5), (3, 5), (3, 2), (8, 2), (8, 4), (10, 4)], [1, 11, 21, 41, 51, 61]),
        new("frozen-mountains", ResourceKeys.Games.TowerDefenseMapFrozenMountains,
            [(-1, 2), (2, 2), (2, 5), (6, 5), (6, 1), (10, 1)], [7, 17, 27, 47, 57, 67]),
        new("urban-battlefield", ResourceKeys.Games.TowerDefenseMapUrbanBattlefield,
            [(-1, 4), (4, 4), (4, 1), (8, 1), (8, 5), (10, 5)], [0, 10, 20, 49, 59, 69]),
        new("military-base", ResourceKeys.Games.TowerDefenseMapMilitaryBase,
            [(-1, 1), (1, 1), (1, 5), (5, 5), (5, 2), (9, 2), (9, 5), (10, 5)], [8, 18, 28, 38, 48, 58])
    ];

    public static IReadOnlyList<TowerDefenseMissionDefinition> Missions { get; } =
    [
        new(1, "green-valley", 5, 180, 20, 0.85),
        new(2, "green-valley", 6, 190, 20, 0.95),
        new(3, "green-valley", 7, 200, 18, 1.05),
        new(4, "desert-outpost", 7, 210, 18, 1.1),
        new(5, "desert-outpost", 8, 220, 18, 1.2),
        new(6, "desert-outpost", 8, 230, 17, 1.3),
        new(7, "frozen-mountains", 8, 240, 17, 1.35),
        new(8, "frozen-mountains", 9, 250, 16, 1.45),
        new(9, "frozen-mountains", 9, 260, 16, 1.55),
        new(10, "urban-battlefield", 9, 270, 15, 1.65),
        new(11, "urban-battlefield", 10, 280, 15, 1.75),
        new(12, "urban-battlefield", 10, 290, 14, 1.85),
        new(13, "military-base", 10, 300, 14, 1.95),
        new(14, "military-base", 12, 320, 12, 2.1)
    ];

    private readonly List<TowerDefenseTowerState> _towers = [];
    private readonly List<TowerDefenseEnemyState> _enemies = [];
    private readonly List<TowerDefenseShot> _shots = [];
    private readonly List<TowerDefenseEffect> _effects = [];
    private readonly Dictionary<string, int> _abilityCooldowns = new(StringComparer.Ordinal);
    private (double X, double Y)[] _path = [];
    private (double X, double Y)[] _flightPath = [];
    private HashSet<int> _blockedCells = [];
    private int _waveTimer;
    private int _spawnRemaining;
    private int _spawnIndex;
    private int _tick;
    private double _difficulty = 1;
    private int _damageResearchLevel;
    private int _startingGoldBonus;
    private int _startingLivesBonus;

    public TowerDefenseMode? Mode { get; private set; }
    public int MissionNumber { get; private set; }
    public int UnlockedMission { get; private set; } = 1;
    public int Gold { get; private set; }
    public int Lives { get; private set; }
    public int Wave { get; private set; }
    public int Score { get; private set; }
    public int SelectedTowerType { get; set; }
    public int SelectedCell { get; set; } = 24;
    public bool IsFinished { get; private set; }
    public bool MissionCompleted { get; private set; }
    public bool IsRunning => Mode.HasValue && !IsFinished;
    public TowerDefenseMapDefinition? CurrentMap { get; private set; }
    public IReadOnlyList<TowerDefenseTowerState> Towers => _towers;
    public IReadOnlyList<TowerDefenseEnemyState> Enemies => _enemies;
    public IReadOnlyList<TowerDefenseShot> Shots => _shots;
    public IReadOnlyList<TowerDefenseEffect> Effects => _effects;
    public IReadOnlySet<int> BlockedCells => _blockedCells;
    public IReadOnlyList<(double X, double Y)> Path => _path;
    public int TotalWaves => Mode == TowerDefenseMode.Campaign
        ? Missions.First(mission => mission.Number == MissionNumber).WaveCount
        : 0;
    public TowerDefenseTowerState? SelectedTower => _towers.FirstOrDefault(tower => tower.Cell == SelectedCell);
    public bool CanBuildSelected => IsRunning && SelectedCell >= 0 && SelectedCell < Columns * Rows
        && !_blockedCells.Contains(SelectedCell)
        && _towers.All(tower => tower.Cell != SelectedCell)
        && Gold >= TowerDefinitions[SelectedTowerType].Cost;

    public void SetProgress(int unlockedMission, int damageResearchLevel = 0, int supplyResearchLevel = 0, int reinforcementResearchLevel = 0)
    {
        UnlockedMission = Math.Clamp(unlockedMission, 1, Missions.Count);
        _damageResearchLevel = Math.Clamp(damageResearchLevel, 0, 5);
        _startingGoldBonus = Math.Clamp(supplyResearchLevel, 0, 5) * 25;
        _startingLivesBonus = Math.Clamp(reinforcementResearchLevel, 0, 5);
    }

    public void StartCampaign(int missionNumber)
    {
        var mission = Missions.FirstOrDefault(item => item.Number == missionNumber);
        if (mission is null || missionNumber > UnlockedMission)
            return;

        Start(TowerDefenseMode.Campaign, mission.Number, mission.MapId, mission.StartingGold + _startingGoldBonus,
            mission.StartingLives + _startingLivesBonus, mission.Difficulty);
    }

    public void StartEndless() => Start(TowerDefenseMode.Endless, 0, Maps[0].Id,
        200 + _startingGoldBonus, 20 + _startingLivesBonus, 1);

    public bool Build(int cell, int towerIndex)
    {
        if (!IsRunning || towerIndex < 0 || towerIndex >= TowerDefinitions.Count
            || cell < 0 || cell >= Columns * Rows || _blockedCells.Contains(cell)
            || _towers.Any(tower => tower.Cell == cell))
            return false;

        var definition = TowerDefinitions[towerIndex];
        if (Gold < definition.Cost)
            return false;

        Gold -= definition.Cost;
        _towers.Add(new TowerDefenseTowerState
        {
            Cell = cell,
            Definition = definition,
            PermanentDamageMultiplier = 1 + _damageResearchLevel * 0.05
        });
        SelectedCell = cell;
        SelectedTowerType = towerIndex;
        return true;
    }

    public bool Upgrade(int cell, string branch)
    {
        var tower = _towers.FirstOrDefault(item => item.Cell == cell);
        if (!IsRunning || tower is null)
            return false;

        var level = branch switch
        {
            "damage" => tower.DamageLevel,
            "range" => tower.RangeLevel,
            "rate" => tower.RateLevel,
            _ => 3
        };
        if (level >= 3)
            return false;

        var cost = tower.Definition.Cost * (level + 1) / 2;
        if (Gold < cost)
            return false;

        switch (branch)
        {
            case "damage": tower.DamageLevel++; break;
            case "range": tower.RangeLevel++; break;
            case "rate": tower.RateLevel++; break;
            default: return false;
        }

        Gold -= cost;
        return true;
    }

    public bool Sell(int cell)
    {
        var tower = _towers.FirstOrDefault(item => item.Cell == cell);
        if (!IsRunning || tower is null)
            return false;

        Gold += (int)(tower.Definition.Cost * 0.6)
            + (tower.DamageLevel + tower.RangeLevel + tower.RateLevel - 3) * tower.Definition.Cost / 4;
        _towers.Remove(tower);
        return true;
    }

    public bool SetTargeting(int cell, TowerTargeting targeting)
    {
        var tower = _towers.FirstOrDefault(item => item.Cell == cell);
        if (!IsRunning || tower is null)
            return false;

        tower.Targeting = targeting;
        return true;
    }

    public bool UseAbility(string ability)
    {
        if (!IsRunning || _abilityCooldowns.GetValueOrDefault(ability) > _tick)
            return false;

        switch (ability)
        {
            case "airstrike":
                if (Gold < 100) return false;
                Gold -= 100;
                DamageEnemies(_enemies.Where(enemy => enemy.Definition.IsFlying).ToArray(), 180);
                break;
            case "artillery":
                if (Gold < 125) return false;
                Gold -= 125;
                var target = _enemies.OrderByDescending(enemy => enemy.Distance).FirstOrDefault();
                if (target is not null)
                    DamageEnemies(_enemies.Where(enemy => Distance(enemy.X, enemy.Y, target.X, target.Y) <= 95).ToArray(), 220);
                break;
            case "emp":
                if (Gold < 90) return false;
                Gold -= 90;
                foreach (var enemy in _enemies)
                    enemy.SlowTicks = Math.Max(enemy.SlowTicks, 150);
                break;
            case "reinforce":
                if (Gold < 150 || Lives >= 30) return false;
                Gold -= 150;
                Lives = Math.Min(30, Lives + 3);
                break;
            case "supply":
                _abilityCooldowns[ability] = _tick + AbilityCooldown;
                Gold += 125;
                return true;
            default:
                return false;
        }

        _abilityCooldowns[ability] = _tick + AbilityCooldown;
        return true;
    }

    public int AbilityCooldownRemaining(string ability) =>
        Math.Max(0, _abilityCooldowns.GetValueOrDefault(ability) - _tick);

    public void Tick()
    {
        if (!IsRunning)
            return;

        _tick++;
        _shots.RemoveAll(shot => shot.ExpiresAt <= _tick);
        _effects.RemoveAll(effect => _tick - effect.StartedAt >= EffectDuration);
        SpawnEnemies();
        MoveEnemies();
        ApplySupportAuras();

        foreach (var tower in _towers)
            Attack(tower);

        ResolveDeaths();
        ResolveLeaks();

        if (Lives <= 0)
        {
            Lives = 0;
            IsFinished = true;
            return;
        }

        if (_spawnRemaining == 0 && _enemies.Count == 0)
        {
            if (Mode == TowerDefenseMode.Campaign && Wave >= TotalWaves)
            {
                MissionCompleted = true;
                IsFinished = true;
                return;
            }

            _waveTimer++;
            if (_waveTimer >= 90)
                StartNextWave();
        }
    }

    public int CalculateStars()
    {
        if (!MissionCompleted)
            return 0;

        var startingLives = Missions[MissionNumber - 1].StartingLives;
        var remaining = Lives / (double)startingLives;
        return remaining >= 0.9 ? 3 : remaining >= 0.5 ? 2 : 1;
    }

    public void MoveSelection(int columnDelta, int rowDelta)
    {
        var column = Math.Clamp(SelectedCell % Columns + columnDelta, 0, Columns - 1);
        var row = Math.Clamp(SelectedCell / Columns + rowDelta, 0, Rows - 1);
        SelectedCell = row * Columns + column;
    }

    public static int CellX(int cell) => cell % Columns * CellSize + CellSize / 2;
    public static int CellY(int cell) => Top + cell / Columns * CellSize + CellSize / 2;

    private void Start(TowerDefenseMode mode, int missionNumber, string mapId, int gold, int lives, double difficulty)
    {
        Mode = mode;
        MissionNumber = missionNumber;
        Gold = gold;
        Lives = lives;
        Wave = 0;
        Score = 0;
        SelectedTowerType = 0;
        SelectedCell = 24;
        IsFinished = false;
        MissionCompleted = false;
        _tick = 0;
        _waveTimer = 60;
        _spawnRemaining = 0;
        _spawnIndex = 0;
        _difficulty = difficulty;
        _towers.Clear();
        _enemies.Clear();
        _shots.Clear();
        _effects.Clear();
        _abilityCooldowns.Clear();
        CurrentMap = Maps.First(map => map.Id == mapId);
        _path = CurrentMap.Path.Select(point =>
            (point.Column * CellSize + CellSize / 2d, Top + point.Row * CellSize + CellSize / 2d)).ToArray();
        _flightPath = [_path[0], _path[^1]];
        _blockedCells = BuildBlockedCells(CurrentMap);
    }

    private void StartNextWave()
    {
        Wave++;
        _spawnRemaining = 5 + Wave * (Mode == TowerDefenseMode.Endless ? 2 : 1);
        _spawnIndex = 0;
        _waveTimer = SpawnInterval;
    }

    private void SpawnEnemies()
    {
        if (_spawnRemaining <= 0)
            return;

        if (--_waveTimer > 0)
            return;

        var definition = SelectEnemy();
        var healthScale = _difficulty * Math.Pow(Mode == TowerDefenseMode.Endless ? 1.12 : 1.1, Wave - 1);
        if (Mode == TowerDefenseMode.Endless)
            healthScale *= 1 + Math.Max(0, Wave - 10) * 0.025;

        var health = definition.Health * healthScale;
        var spawn = _path[0];
        _enemies.Add(new TowerDefenseEnemyState
        {
            Definition = definition,
            X = spawn.X,
            Y = spawn.Y,
            Health = health,
            MaxHealth = health,
            Speed = definition.Speed * (1 + Math.Min(Wave, 25) * 0.012)
        });

        _spawnRemaining--;
        _spawnIndex++;
        _waveTimer = SpawnInterval;
    }

    private EnemyDefinition SelectEnemy()
    {
        if (Wave % 10 == 0 && _spawnRemaining == 1)
            return EnemyDefinitions[7];
        if (Wave >= 6 && _spawnIndex % 7 == 4)
            return EnemyDefinitions[5];
        if (Wave >= 5 && _spawnIndex % 5 == 2)
            return EnemyDefinitions[4];
        if (Wave >= 4 && _spawnIndex % 4 == 1)
            return EnemyDefinitions[6];
        if (Wave >= 3 && _spawnIndex % 3 == 1)
            return EnemyDefinitions[3];
        if (Wave >= 2 && _spawnIndex % 2 == 1)
            return EnemyDefinitions[2];
        return _spawnIndex % 4 == 3 ? EnemyDefinitions[1] : EnemyDefinitions[0];
    }

    private void MoveEnemies()
    {
        foreach (var enemy in _enemies)
        {
            var previousX = enemy.X;
            var previousY = enemy.Y;
            enemy.Distance += enemy.Speed * (enemy.SlowTicks > 0 ? 0.5 : 1);
            enemy.SlowTicks = Math.Max(0, enemy.SlowTicks - 1);
            var point = PointOnPath(enemy.Definition.IsFlying ? _flightPath : _path, enemy.Distance);
            enemy.X = point.X;
            enemy.Y = point.Y;
            enemy.Angle = Math.Atan2(enemy.Y - previousY, enemy.X - previousX) * 180 / Math.PI;
        }
    }

    private void ApplySupportAuras()
    {
        foreach (var enemy in _enemies)
        {
            if (enemy.BuffTicks > 0 && --enemy.BuffTicks == 0)
                enemy.ArmorBonus = 0;
        }

        if (_tick % 90 == 0)
        {
            foreach (var medic in _enemies.Where(enemy => enemy.Definition.Id == "medic"))
            {
                foreach (var ally in _enemies.Where(enemy => enemy != medic
                    && Distance(enemy.X, enemy.Y, medic.X, medic.Y) <= 85))
                    ally.Health = Math.Min(ally.MaxHealth, ally.Health + 12);
            }
        }

        if (_tick % 120 == 0)
        {
            foreach (var commander in _enemies.Where(enemy => enemy.Definition.IsBoss))
            {
                foreach (var ally in _enemies.Where(enemy => enemy != commander
                    && Distance(enemy.X, enemy.Y, commander.X, commander.Y) <= 110))
                {
                    ally.ArmorBonus = Math.Max(ally.ArmorBonus, 4);
                    ally.BuffTicks = 120;
                }
            }
        }
    }

    private void Attack(TowerDefenseTowerState tower)
    {
        if (tower.Cooldown > 0)
        {
            tower.Cooldown--;
            return;
        }

        var x = CellX(tower.Cell);
        var y = CellY(tower.Cell);
        var targets = _enemies.Where(enemy =>
            (tower.Definition.AntiAir || !enemy.Definition.IsFlying)
            && Distance(enemy.X, enemy.Y, x, y) <= tower.Range).ToArray();
        if (targets.Length == 0)
            return;

        var target = SelectTarget(targets, tower.Targeting, x, y);
        tower.Cooldown = tower.AttackCooldown;
        tower.Angle = Math.Atan2(target.Y - y, target.X - x) * 180 / Math.PI;
        _shots.Add(new TowerDefenseShot(x, y, target.X, target.Y, tower.Definition.Color, _tick + 3));

        TowerDefenseEnemyState[] impact = tower.Definition.SplashRadius == 0
            ? [target]
            : targets.Where(enemy => Distance(enemy.X, enemy.Y, target.X, target.Y)
                <= tower.Definition.SplashRadius).ToArray();

        foreach (var enemy in impact)
        {
            var armor = Math.Max(0, enemy.Definition.Armor + enemy.ArmorBonus - (tower.DamageLevel - 1) * 2);
            enemy.Health -= Math.Max(1, tower.Damage - armor);
            if (tower.Definition.SlowTicks > 0)
                enemy.SlowTicks = Math.Max(enemy.SlowTicks, tower.Definition.SlowTicks);
        }

        if (tower.Definition.SplashRadius > 0)
            _effects.Add(new TowerDefenseEffect(target.X, target.Y, tower.Definition.SplashRadius, tower.Definition.Color, _tick));
    }

    private static TowerDefenseEnemyState SelectTarget(
        IReadOnlyList<TowerDefenseEnemyState> targets, TowerTargeting targeting, double x, double y) =>
        targeting switch
        {
            TowerTargeting.Last => targets.MinBy(enemy => enemy.Distance)!,
            TowerTargeting.Strongest => targets.MaxBy(enemy => enemy.Health)!,
            TowerTargeting.Weakest => targets.MinBy(enemy => enemy.Health)!,
            TowerTargeting.Closest => targets.MinBy(enemy => Distance(enemy.X, enemy.Y, x, y))!,
            TowerTargeting.Farthest => targets.MaxBy(enemy => Distance(enemy.X, enemy.Y, x, y))!,
            _ => targets.MaxBy(enemy => enemy.Distance)!
        };

    private void DamageEnemies(IReadOnlyList<TowerDefenseEnemyState> enemies, double damage)
    {
        foreach (var enemy in enemies)
        {
            enemy.Health -= Math.Max(1, damage - enemy.Definition.Armor);
            _effects.Add(new TowerDefenseEffect(enemy.X, enemy.Y, 24, "#f97316", _tick));
        }
    }

    private void ResolveDeaths()
    {
        foreach (var enemy in _enemies.Where(enemy => enemy.Health <= 0))
        {
            Gold += enemy.Definition.Reward + Wave;
            Score = Math.Min(MaxScore, Score + enemy.Definition.Score);
            _effects.Add(new TowerDefenseEffect(enemy.X, enemy.Y, 22, enemy.Definition.Color, _tick));
        }

        _enemies.RemoveAll(enemy => enemy.Health <= 0);
    }

    private void ResolveLeaks()
    {
        var leaked = _enemies.RemoveAll(enemy => enemy.Distance >= PathLength(enemy.Definition.IsFlying ? _flightPath : _path));
        Lives = Math.Max(0, Lives - leaked);
    }

    private static HashSet<int> BuildBlockedCells(TowerDefenseMapDefinition map)
    {
        var blocked = new HashSet<int>(map.Obstacles);
        for (var index = 1; index < map.Path.Length; index++)
        {
            var (column, row) = map.Path[index - 1];
            var (endColumn, endRow) = map.Path[index];
            while (true)
            {
                if (column is >= 0 and < Columns && row is >= 0 and < Rows)
                    blocked.Add(row * Columns + column);
                if ((column, row) == (endColumn, endRow))
                    break;
                column += Math.Sign(endColumn - column);
                row += Math.Sign(endRow - row);
            }
        }

        return blocked;
    }

    private static (double X, double Y) PointOnPath((double X, double Y)[] path, double distance)
    {
        var remaining = Math.Max(0, distance);
        for (var index = 1; index < path.Length; index++)
        {
            var from = path[index - 1];
            var to = path[index];
            var length = Distance(from.X, from.Y, to.X, to.Y);
            if (remaining <= length)
            {
                var ratio = length == 0 ? 0 : remaining / length;
                return (from.X + (to.X - from.X) * ratio, from.Y + (to.Y - from.Y) * ratio);
            }

            remaining -= length;
        }

        return path[^1];
    }

    private static double PathLength((double X, double Y)[] path) =>
        path.Zip(path.Skip(1), (from, to) => Distance(from.X, from.Y, to.X, to.Y)).Sum();

    private static double Distance(double x1, double y1, double x2, double y2) =>
        double.Hypot(x2 - x1, y2 - y1);
}