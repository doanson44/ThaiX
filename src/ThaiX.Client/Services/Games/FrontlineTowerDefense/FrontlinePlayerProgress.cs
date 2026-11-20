namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public enum FrontlineWarMartItemType
{
    Tower,
    Bonus,
    Level
}

public sealed record FrontlineLegacySaveData(
    bool HasPlayed,
    int Credits,
    int StartingFunds,
    IReadOnlyList<int> TowerUnlocks,
    IReadOnlyList<int> LevelScores);

public sealed class FrontlinePlayerProgress
{
    private readonly int[] _towerUnlocks;
    private readonly int[] _levelScores;

    private FrontlinePlayerProgress(
        int credits,
        int startingFunds,
        int[] towerUnlocks,
        int[] levelScores)
    {
        Credits = credits;
        StartingFunds = startingFunds;
        _towerUnlocks = towerUnlocks;
        _levelScores = levelScores;
    }

    public const int TowerSlotCount = 20;
    public const int LevelSlotCount = 40;
    public const int DefaultCredits = 1000;
    public const int DefaultStartingFunds = 500;

    public int Credits { get; private set; }
    public int StartingFunds { get; private set; }
    public IReadOnlyList<int> TowerUnlocks => _towerUnlocks;
    public IReadOnlyList<int> LevelScores => _levelScores;

    public static FrontlinePlayerProgress CreateNew()
    {
        var towers = new int[TowerSlotCount];
        towers[2] = 1;
        towers[3] = 1;
        towers[4] = 1;
        towers[13] = 1;

        var levels = new int[LevelSlotCount];
        levels[0] = 1;

        return new FrontlinePlayerProgress(
            DefaultCredits,
            DefaultStartingFunds,
            towers,
            levels);
    }

    public static FrontlinePlayerProgress Load(FrontlineLegacySaveData save)
    {
        ArgumentNullException.ThrowIfNull(save);
        if (!save.HasPlayed)
            return CreateNew();

        if (save.Credits < 0 || save.StartingFunds < 0
            || save.TowerUnlocks.Count != TowerSlotCount
            || save.LevelScores.Count != LevelSlotCount
            || save.TowerUnlocks.Any(value => value is not 0 and not 1)
            || save.LevelScores.Any(value => value < 0))
            throw new ArgumentException("The legacy Frontline save data is invalid.", nameof(save));

        return new FrontlinePlayerProgress(
            save.Credits,
            save.StartingFunds,
            save.TowerUnlocks.ToArray(),
            save.LevelScores.ToArray());
    }

    public FrontlineLegacySaveData ToLegacySaveData() =>
        new(true, Credits, StartingFunds, _towerUnlocks.ToArray(), _levelScores.ToArray());

    public bool IsTowerUnlocked(int towerIndex) =>
        towerIndex >= 0 && towerIndex < TowerSlotCount && _towerUnlocks[towerIndex] == 1;

    public bool IsLevelUnlocked(int levelIndex) =>
        levelIndex >= 0 && levelIndex < LevelSlotCount && _levelScores[levelIndex] > 0;

    public bool UnlockTower(int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= TowerSlotCount)
            return false;

        _towerUnlocks[towerIndex] = 1;
        return true;
    }

    public bool UnlockLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= LevelSlotCount)
            return false;

        _levelScores[levelIndex] = Math.Max(1, _levelScores[levelIndex]);
        return true;
    }

    public bool SaveLevelScore(int levelIndex, int score)
    {
        if (levelIndex < 0 || levelIndex >= LevelSlotCount || score < 0)
            return false;

        if (_levelScores[levelIndex] >= score)
            return false;

        _levelScores[levelIndex] = score;
        return true;
    }

    public bool TryPurchase(FrontlineWarMartItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (Credits < item.Cost)
            return false;

        if (item.Type == FrontlineWarMartItemType.Tower && IsTowerUnlocked(item.ItemId))
            return false;

        if (item.Type == FrontlineWarMartItemType.Level && IsLevelUnlocked(item.ItemId - 1))
            return false;

        switch (item.Type)
        {
            case FrontlineWarMartItemType.Tower:
                if (!UnlockTower(item.ItemId))
                    return false;
                break;
            case FrontlineWarMartItemType.Bonus:
                StartingFunds += item.ItemId;
                break;
            case FrontlineWarMartItemType.Level:
                if (!UnlockLevel(item.ItemId - 1))
                    return false;
                break;
            default:
                return false;
        }

        Credits -= item.Cost;
        return true;
    }
}

public sealed record FrontlineWarMartItem(
    string Id,
    string Name,
    int Cost,
    int ItemId,
    FrontlineWarMartItemType Type);

public static class FrontlineWarMartCatalog
{
    public static IReadOnlyList<FrontlineWarMartItem> All { get; } =
    [
        new("flame-thrower", "Flame Thrower", 5000, 9, FrontlineWarMartItemType.Tower),
        new("napalm-support-strike", "Napalm Support Strike", 6000, 15, FrontlineWarMartItemType.Tower),
        new("starting-funds", "Bonus Start Funds", 1000, 200, FrontlineWarMartItemType.Bonus),
        new("sniper-mission-1", "Sniper Mission 1", 5000, 16, FrontlineWarMartItemType.Level),
        new("sniper-mission-2", "Sniper Mission 2", 6000, 18, FrontlineWarMartItemType.Level),
        new("sniper-mission-3", "Sniper Mission 3", 8000, 20, FrontlineWarMartItemType.Level),
        new("sniper-mission-4", "Sniper Mission 4", 7000, 22, FrontlineWarMartItemType.Level),
        new("bio-cannon", "Bio Cannon", 6000, 8, FrontlineWarMartItemType.Tower),
        new("bio-mortar", "Bio Mortar", 10000, 7, FrontlineWarMartItemType.Tower),
        new("particle-beam-cannon", "Particle Beam Cannon", 7000, 6, FrontlineWarMartItemType.Tower),
        new("mounted-machine-gun", "Mounted Machine Gun", 4000, 10, FrontlineWarMartItemType.Tower),
        new("plasma-cannon", "Plasma Cannon", 9000, 11, FrontlineWarMartItemType.Tower),
        new("missile-launcher", "Missile Launcher", 8000, 1, FrontlineWarMartItemType.Tower),
        new("shotgun-tower", "Shotgun Tower", 2000, 5, FrontlineWarMartItemType.Tower),
        new("bio-chemical-support-strike", "Bio-Chemical Support Strike", 12000, 14, FrontlineWarMartItemType.Tower)
    ];
}
