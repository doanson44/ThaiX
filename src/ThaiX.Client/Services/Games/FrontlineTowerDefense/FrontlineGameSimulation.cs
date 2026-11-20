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
    int LifeDamage);

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
    public FrontlinePoint Position => Path.PointAt(Distance);
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
            enemy.Distance += enemy.Definition.Speed;
            if (enemy.Distance < enemy.Path.Length)
                continue;

            Lives = Math.Max(0, Lives - enemy.Definition.LifeDamage);
            _enemies.RemoveAt(index);
        }
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

        target.Health -= tower.Definition.Damage;
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