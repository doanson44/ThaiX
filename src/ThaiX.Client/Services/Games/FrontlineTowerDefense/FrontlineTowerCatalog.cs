namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public sealed record FrontlineTowerArchetype(
    string Id,
    string Name,
    int UnlockIndex,
    int BuildCost,
    double Damage,
    double RateOfFire,
    double Range,
    FrontlineTargetMode TargetMode,
    int? WarMartUnlockCost = null,
    bool IsSupport = false,
    int ShotLimit = 0)
{
    public FrontlineTowerDefinition CreateDefinition() =>
        new(Id, BuildCost, Range, Damage, RateOfFire, TargetMode);
}

public static class FrontlineTowerCatalog
{
    public static IReadOnlyList<FrontlineTowerArchetype> All { get; } =
    [
        new("tower1", "Missile Launcher", 1, 800, 80, 1, 150, FrontlineTargetMode.Heat, 8000),
        new("tower2", "Rifle Tower", 2, 50, 10, 3, 60, FrontlineTargetMode.Weak),
        new("tower3", "Sniper Tower", 3, 400, 80, 2, 120, FrontlineTargetMode.Near),
        new("tower4", "Electro Tower", 4, 250, 50, 3, 60, FrontlineTargetMode.Near),
        new("tower5", "Shotgun Tower", 5, 150, 35, 2, 70, FrontlineTargetMode.Strong, 2000),
        new("tower6", "Particle Beam Cannon", 6, 600, 150, 1, 100, FrontlineTargetMode.Strong, 7000),
        new("tower7", "Grenade Launcher", 7, 950, 40, 1, 170, FrontlineTargetMode.Strong, 10000),
        new("tower8", "Bio Mortar", 8, 300, 12, 3, 85, FrontlineTargetMode.Cold, 6000),
        new("tower9", "Flame Thrower", 9, 250, 2, 12, 55, FrontlineTargetMode.Near, 5000),
        new("tower10", "Mounted Machine Gun", 10, 500, 25, 7, 90, FrontlineTargetMode.Near, 4000),
        new("tower11", "Plasma Cannon", 11, 1000, 40, 1, 160, FrontlineTargetMode.Heat, 9000),
        new("suptower1", "Napalm Support Strike", 15, 0, 100, 3, 80, FrontlineTargetMode.Near, 6000, true, 6),
        new("suptower2", "Bio-Chemical Support Strike", 14, 0, 50, 3, 60, FrontlineTargetMode.Near, 12000, true, 3),
        new("suptower3", "Support Strike", 13, 0, 50, 8, 60, FrontlineTargetMode.Near, IsSupport: true, ShotLimit: 8)
    ];

    public static IReadOnlyList<FrontlineTowerArchetype> Standard { get; } =
        All.Where(tower => !tower.IsSupport).ToArray();

    public static IReadOnlyList<FrontlineTowerArchetype> Support { get; } =
        All.Where(tower => tower.IsSupport).ToArray();

    public static FrontlineTowerArchetype? Find(string id) =>
        All.FirstOrDefault(tower => string.Equals(tower.Id, id, StringComparison.Ordinal));
}
