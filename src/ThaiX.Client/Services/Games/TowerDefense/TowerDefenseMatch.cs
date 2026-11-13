namespace ThaiX.Client.Services.Games.TowerDefense;

public sealed class Tower
{
    public required int Id { get; init; }
    public required int Cell { get; set; }
    public required TowerSpec Spec { get; init; }
    public int[] Levels { get; } = new int[3];
    public int Invested { get; set; }
    public int Cooldown { get; set; }
    public double Angle { get; set; }
    public int FlashUntil { get; set; }
    public TargetMode Mode { get; set; }

    public int Level(UpgradeTrack track) => Levels[(int)track];
}

public sealed class Enemy
{
    public required EnemySpec Spec { get; init; }
    public required (double X, double Y)[] Path { get; init; }
    public required double PathLength { get; init; }
    public required double MaxHealth { get; init; }
    public required double Speed { get; init; }
    public double Health { get; set; }
    public double Distance { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public int SlowTicks { get; set; }
    public int HitUntil { get; set; }

    public double Progress => Distance / PathLength;
}

public readonly record struct Shot(double X1, double Y1, double X2, double Y2, string Color, int ExpiresAt);

public readonly record struct Blast(double X, double Y, double Radius, string Color, bool Ring, int Start);

public readonly record struct Popup(double X, double Y, string Text, string Color, int Start);

/// <summary>One tower defense round: build, upgrade and sell towers, run waves, and reach victory or defeat. UI-free so it can be unit tested.</summary>
public sealed class TowerDefenseMatch
{
    public const int Columns = 10;
    public const int Rows = 7;
    public const int CellSize = 48;
    public const int Top = 40;
    public const int TickMilliseconds = 33;
    public const int MaxLevel = 3;
    public const int MaxScore = 100_000;
    public const int BlastTicks = 8;
    public const int PopupTicks = 24;
    public const int AirstrikeCooldownTicks = 1200;
    public const int AirstrikeRadius = 70;
    public const int InterWaveDelayTicks = 300;

    private const int ChillTicks = 30;
    private const int ShotTicks = 3;
    private const int SplashRadius = 36;
    private const int ChainLinks = 2;
    private const int ChainRange = 70;
    private const int GroupGapTicks = 30;
    private const int FirstAirstrikeTick = 400;
    private const double BaseHealth = 40;
    private const double HealthGrowth = 1.14;
    private const double SellRatio = 0.7;
    private const double UpgradeCostFactor = 0.5;
    private const double DamagePerLevel = 0.6;
    private const double RangePerLevel = 0.1;
    private const double CooldownPerLevel = 0.15;

    private readonly List<Tower> _towers = [];
    private readonly List<Enemy> _enemies = [];
    private readonly List<Shot> _shots = [];
    private readonly List<Blast> _blasts = [];
    private readonly List<Popup> _popups = [];
    private readonly List<(int At, EnemyKind Kind)> _spawns = [];
    private readonly (double X, double Y)[][] _roads;
    private readonly HashSet<int> _blocked;
    private int _spawnIndex;
    private int _spawned;
    private int _waveTick;
    private int _nextTowerId = 1;
    private int _airstrikeReadyAt = FirstAirstrikeTick;
    private int _autoWaveAt;

    public TowerDefenseMatch(MapSpec map, DifficultySpec difficulty, bool endless, int startingGoldBonus = 0)
    {
        Map = map;
        Difficulty = difficulty;
        Endless = endless;
        Gold = difficulty.StartGold + Math.Max(0, startingGoldBonus);
        Lives = difficulty.Lives;
        _roads = [.. map.Routes.Select(route =>
            route.Select(point => (point.Column * CellSize + CellSize / 2.0, Top + point.Row * CellSize + CellSize / 2.0)).ToArray())];
        _blocked = [.. RoadCells(map), .. map.Trees, .. map.Rocks];
    }

    public MapSpec Map { get; }
    public DifficultySpec Difficulty { get; }
    public bool Endless { get; }
    public MatchState State { get; private set; } = MatchState.Planning;
    public int Gold { get; private set; }
    public int Lives { get; private set; }
    public int Wave { get; private set; }
    public int Score { get; private set; }
    public int Tick { get; private set; }
    public int TotalWaves => Endless ? 0 : TowerDefenseData.CampaignWaves;
    public IReadOnlyList<Tower> Towers => _towers;
    public IReadOnlyList<Enemy> Enemies => _enemies;
    public IReadOnlyList<Shot> Shots => _shots;
    public IReadOnlyList<Blast> Blasts => _blasts;
    public IReadOnlyList<Popup> Popups => _popups;
    public IReadOnlyList<(double X, double Y)[]> Roads => _roads;
    public bool IsOver => State is MatchState.Won or MatchState.Lost;
    public bool AirstrikeReady => Tick >= _airstrikeReadyAt;
    public double AirstrikeCharge => AirstrikeReady ? 1 : 1 - (_airstrikeReadyAt - Tick) / (double)AirstrikeCooldownTicks;
    public int FinalScore => Math.Min(MaxScore, Score);
    public int MissionReward => Gold + 100 + Wave * 10 + Lives * 5;
    public IReadOnlyList<WaveGroup> NextWaveGroups => TowerDefenseData.WaveGroups(Wave + 1);
    public bool NextWaveCountdownActive => State == MatchState.Planning && _autoWaveAt > Tick && HasMoreWaves;
    public int NextWaveCountdownSeconds => NextWaveCountdownActive
        ? (int)Math.Ceiling((_autoWaveAt - Tick) * TickMilliseconds / 1000.0)
        : 0;

    private bool HasMoreWaves => Endless || Wave < TowerDefenseData.CampaignWaves;

    public static int CellX(int cell) => cell % Columns * CellSize + CellSize / 2;

    public static int CellY(int cell) => Top + cell / Columns * CellSize + CellSize / 2;

    public bool IsBlocked(int cell) => cell is < 0 or >= Columns * Rows || _blocked.Contains(cell);

    public Tower? TowerAt(int cell) => _towers.Find(tower => tower.Cell == cell);

    public int FirstFreeCell(int preferred) =>
        Enumerable.Range(0, Columns * Rows).OrderBy(cell => Math.Abs(cell - preferred)).First(cell => !IsBlocked(cell));

    public double Range(Tower tower)
    {
        var range = tower.Spec.Range * (1 + RangePerLevel * tower.Level(UpgradeTrack.Range));
        if (tower.Spec.Effect == TowerEffect.SupportRange)
            return range;

        var (x, y) = (CellX(tower.Cell), CellY(tower.Cell));
        return range + _towers
            .Where(support => support != tower && support.Spec.Effect == TowerEffect.SupportRange
                && double.Hypot(CellX(support.Cell) - x, CellY(support.Cell) - y)
                    <= support.Spec.Range * (1 + RangePerLevel * support.Level(UpgradeTrack.Range)))
            .Sum(support => support.Spec.SupportValue);
    }

    public double Damage(Tower tower)
    {
        var damage = tower.Spec.SupportValue * (1 + DamagePerLevel * tower.Level(UpgradeTrack.Damage));
        if (tower.Spec.IsSupport)
            return damage;

        damage = tower.Spec.Damage * (1 + DamagePerLevel * tower.Level(UpgradeTrack.Damage));

        var (x, y) = (CellX(tower.Cell), CellY(tower.Cell));
        return damage + _towers
            .Where(support => support != tower && support.Spec.Effect == TowerEffect.SupportDamage
                && double.Hypot(CellX(support.Cell) - x, CellY(support.Cell) - y)
                    <= support.Spec.Range * (1 + RangePerLevel * support.Level(UpgradeTrack.Range)))
            .Sum(support => support.Spec.SupportValue * (1 + DamagePerLevel * support.Level(UpgradeTrack.Damage)));
    }

    public int CooldownTicks(Tower tower) =>
        Math.Max(2, (int)Math.Round(tower.Spec.Cooldown * (1 - CooldownPerLevel * tower.Level(UpgradeTrack.FireRate))));

    public int? UpgradeCost(Tower tower, UpgradeTrack track)
    {
        if (tower.Spec.IsSupport && track == UpgradeTrack.FireRate)
            return null;

        var level = tower.Level(track);
        return level >= MaxLevel ? null : (int)(Math.Ceiling(tower.Spec.Cost * UpgradeCostFactor * (level + 1) / 5) * 5);
    }

    public int SellValue(Tower tower) => (int)(tower.Invested * SellRatio);

    public Tower? Build(int cell, TowerKind kind)
    {
        var spec = TowerDefenseData.Tower(kind);
        if (IsOver || (Map.RequiredTower is { } required && kind != required)
            || IsBlocked(cell) || TowerAt(cell) is not null || Gold < spec.Cost)
            return null;

        Gold -= spec.Cost;
        var tower = new Tower { Id = _nextTowerId++, Cell = cell, Spec = spec, Invested = spec.Cost };
        _towers.Add(tower);
        return tower;
    }

    public bool Move(Tower tower, int cell)
    {
        if (IsOver || !_towers.Contains(tower) || IsBlocked(cell) || TowerAt(cell) is not null)
            return false;

        tower.Cell = cell;
        return true;
    }

    public bool Upgrade(Tower tower, UpgradeTrack track)
    {
        if (IsOver || !_towers.Contains(tower) || UpgradeCost(tower, track) is not { } cost || Gold < cost)
            return false;

        Gold -= cost;
        tower.Invested += cost;
        tower.Levels[(int)track]++;
        return true;
    }

    public bool Sell(Tower tower)
    {
        if (IsOver || !_towers.Remove(tower))
            return false;

        var refund = SellValue(tower);
        Gold += refund;
        AddPopup(CellX(tower.Cell), CellY(tower.Cell), $"+{refund}", "#facc15");
        return true;
    }

    public void CycleTarget(Tower tower) =>
        tower.Mode = (TargetMode)(((int)tower.Mode + 1) % Enum.GetValues<TargetMode>().Length);

    /// <summary>Calls the next wave. Returns false while a wave is running, the campaign is complete or the match is over.</summary>
    public bool StartWave()
    {
        if (State != MatchState.Planning || !HasMoreWaves)
            return false;

        _autoWaveAt = 0;
        Wave++;
        _spawns.Clear();
        _spawnIndex = 0;
        _waveTick = 0;
        var at = 0;
        foreach (var group in TowerDefenseData.WaveGroups(Wave))
        {
            for (var i = 0; i < group.Count; i++)
            {
                _spawns.Add((at, group.Kind));
                at += group.IntervalTicks;
            }

            at += GroupGapTicks;
        }

        State = MatchState.Running;
        return true;
    }

    /// <summary>Drops a bomb on every enemy around the point. Returns false while recharging or outside a running wave.</summary>
    public bool Airstrike(double x, double y)
    {
        if (State != MatchState.Running || !AirstrikeReady)
            return false;

        _airstrikeReadyAt = Tick + AirstrikeCooldownTicks;
        var damage = BaseHealth * Math.Pow(HealthGrowth, Wave - 1) * 1.5;
        foreach (var enemy in _enemies.Where(enemy => double.Hypot(enemy.X - x, enemy.Y - y) <= AirstrikeRadius))
            Hit(enemy, damage);

        _blasts.Add(new Blast(x, y, AirstrikeRadius, "#fde047", false, Tick));
        ResolveDeaths();
        CheckWaveEnd();
        return true;
    }

    public void Advance()
    {
        Tick++;
        _shots.RemoveAll(shot => shot.ExpiresAt <= Tick);
        _blasts.RemoveAll(blast => Tick - blast.Start >= BlastTicks);
        _popups.RemoveAll(popup => Tick - popup.Start >= PopupTicks);
        if (State == MatchState.Planning && _autoWaveAt > 0 && Tick >= _autoWaveAt)
            StartWave();

        if (State != MatchState.Running)
            return;

        _waveTick++;
        Spawn();
        MoveEnemies();
        if (Lives == 0)
        {
            State = MatchState.Lost;
            return;
        }

        foreach (var tower in _towers)
            Fire(tower);

        ResolveDeaths();
        CheckWaveEnd();
    }

    private void Spawn()
    {
        while (_spawnIndex < _spawns.Count && _spawns[_spawnIndex].At <= _waveTick)
        {
            var spec = TowerDefenseData.Enemy(_spawns[_spawnIndex++].Kind);
            var route = _spawned++ % _roads.Length;
            var road = _roads[route];
            (double X, double Y)[] path = spec.IsFlying ? [road[0], road[^1]] : road;
            var health = BaseHealth * Math.Pow(HealthGrowth, Wave - 1) * spec.Health * Difficulty.HealthMultiplier;
            _enemies.Add(new Enemy
            {
                Spec = spec,
                Path = path,
                PathLength = path.Zip(path.Skip(1), (from, to) => double.Hypot(to.X - from.X, to.Y - from.Y)).Sum(),
                MaxHealth = health,
                Health = health,
                Speed = (1.3 + Math.Min(Wave, TowerDefenseData.CampaignWaves) * 0.04) * spec.Speed,
                X = path[0].X,
                Y = path[0].Y
            });
        }
    }

    private void MoveEnemies()
    {
        foreach (var enemy in _enemies)
        {
            enemy.Distance += enemy.Speed * (enemy.SlowTicks > 0 ? 0.5 : 1);
            enemy.SlowTicks = Math.Max(0, enemy.SlowTicks - 1);
            (enemy.X, enemy.Y) = GameLogic.PointOnPath(enemy.Path, enemy.Distance);
        }

        foreach (var leaked in _enemies.Where(enemy => enemy.Distance >= enemy.PathLength).ToList())
        {
            Lives = Math.Max(0, Lives - leaked.Spec.LifeCost);
            _enemies.Remove(leaked);
        }
    }

    private void Fire(Tower tower)
    {
        if (tower.Spec.IsSupport)
            return;

        if (tower.Cooldown > 0)
        {
            tower.Cooldown--;
            return;
        }

        var (x, y) = (CellX(tower.Cell), CellY(tower.Cell));
        var range = Range(tower);
        var candidates = _enemies.Where(enemy => enemy.Health > 0 && tower.Spec.CanHit(enemy.Spec.Class)
            && double.Hypot(enemy.X - x, enemy.Y - y) <= range).ToList();
        if (candidates.Count == 0)
            return;

        var target = tower.Mode switch
        {
            TargetMode.Strongest => candidates.MaxBy(enemy => enemy.Health)!,
            TargetMode.Closest => candidates.MinBy(enemy => double.Hypot(enemy.X - x, enemy.Y - y))!,
            _ => candidates.MaxBy(enemy => enemy.Progress)!
        };

        tower.Cooldown = CooldownTicks(tower);
        tower.Angle = Math.Atan2(target.Y - y, target.X - x) * 180 / Math.PI;
        tower.FlashUntil = Tick + ShotTicks;
        var damage = Damage(tower);
        var color = tower.Spec.ShotColor ?? tower.Spec.Color;
        switch (tower.Spec.Effect)
        {
            case TowerEffect.Single:
                Hit(target, damage * tower.Spec.Multiplier(target.Spec.Class));
                _shots.Add(new Shot(x, y, target.X, target.Y, color, Tick + ShotTicks));
                break;
            case TowerEffect.Splash:
                _shots.Add(new Shot(x, y, target.X, target.Y, color, Tick + ShotTicks));
                _blasts.Add(new Blast(target.X, target.Y, SplashRadius, color, false, Tick));
                foreach (var enemy in candidates.Where(enemy => double.Hypot(enemy.X - target.X, enemy.Y - target.Y) <= SplashRadius))
                    Hit(enemy, damage * tower.Spec.Multiplier(enemy.Spec.Class));
                break;
            case TowerEffect.Chain:
                ChainLightning(tower, target, candidates, damage, color);
                break;
            case TowerEffect.Burn:
                foreach (var enemy in candidates)
                {
                    Hit(enemy, damage * tower.Spec.Multiplier(enemy.Spec.Class));
                    _shots.Add(new Shot(x, y, enemy.X, enemy.Y, color, Tick + ShotTicks));
                }

                break;
            case TowerEffect.Slow:
                _blasts.Add(new Blast(x, y, range, color, true, Tick));
                foreach (var enemy in candidates)
                {
                    Hit(enemy, damage);
                    enemy.SlowTicks = ChillTicks;
                }

                break;
        }
    }

    private void ChainLightning(Tower tower, Enemy first, List<Enemy> candidates, double damage, string color)
    {
        double fromX = CellX(tower.Cell), fromY = CellY(tower.Cell);
        Enemy? current = first;
        var hit = new HashSet<Enemy>();
        for (var link = 0; link <= ChainLinks && current is not null; link++)
        {
            hit.Add(current);
            Hit(current, damage * tower.Spec.Multiplier(current.Spec.Class));
            _shots.Add(new Shot(fromX, fromY, current.X, current.Y, color, Tick + ShotTicks));
            (fromX, fromY) = (current.X, current.Y);
            var (x, y) = (current.X, current.Y);
            current = candidates.Where(enemy => !hit.Contains(enemy) && double.Hypot(enemy.X - x, enemy.Y - y) <= ChainRange)
                .MinBy(enemy => double.Hypot(enemy.X - x, enemy.Y - y));
        }
    }

    private void Hit(Enemy enemy, double amount)
    {
        enemy.Health -= amount;
        enemy.HitUntil = Tick + 2;
    }

    private void ResolveDeaths()
    {
        foreach (var enemy in _enemies.Where(enemy => enemy.Health <= 0).ToList())
        {
            var gold = (int)Math.Round((5 + Wave) * enemy.Spec.GoldMultiplier * Difficulty.GoldMultiplier);
            Gold += gold;
            Score += enemy.Spec.Score;
            _blasts.Add(new Blast(enemy.X, enemy.Y, enemy.Spec.Radius * 1.8, enemy.Spec.DeathColor, false, Tick));
            AddPopup(enemy.X, enemy.Y, $"+{gold}", "#facc15");
            _enemies.Remove(enemy);
        }
    }

    private void CheckWaveEnd()
    {
        if (State != MatchState.Running || _spawnIndex < _spawns.Count || _enemies.Count > 0)
            return;

        Score += Wave * 20;
        if (!Endless && Wave >= TowerDefenseData.CampaignWaves)
        {
            Score += Lives * 100;
            State = MatchState.Won;
            return;
        }

        Gold += (int)Math.Round((15 + 3 * Wave) * Difficulty.GoldMultiplier);
        State = MatchState.Planning;
        _autoWaveAt = Tick + InterWaveDelayTicks;
    }

    private void AddPopup(double x, double y, string text, string color) => _popups.Add(new Popup(x, y, text, color, Tick));

    private static IEnumerable<int> RoadCells(MapSpec map)
    {
        foreach (var route in map.Routes)
        {
            for (var i = 1; i < route.Length; i++)
            {
                var (column, row) = route[i - 1];
                var (endColumn, endRow) = route[i];
                while (true)
                {
                    if (column is >= 0 and < Columns && row is >= 0 and < Rows)
                        yield return row * Columns + column;

                    if ((column, row) == (endColumn, endRow))
                        break;

                    column += Math.Sign(endColumn - column);
                    row += Math.Sign(endRow - row);
                }
            }
        }
    }
}
