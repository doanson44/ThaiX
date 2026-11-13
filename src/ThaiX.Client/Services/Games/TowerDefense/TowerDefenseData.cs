using ThaiX.Client.Constants;

namespace ThaiX.Client.Services.Games.TowerDefense;

public enum TowerKind { Gunner, Sniper, Rocket, Tesla, Flamethrower, SlowField, Shotgun, MachineGun, BioChemical, HeatRay }

public enum EnemyKind { Soldier, Armored, Jeep, Helicopter, Tank, Boss }

public enum EnemyClass { Infantry, Vehicle, Air }

public enum TowerEffect { Single, Splash, Chain, Burn, Slow, Poison }

public enum UpgradeTrack { Damage, Range, FireRate }

public enum TargetMode { First, Strongest, Closest }

public enum MatchState { Planning, Running, Won, Lost }

public sealed record TowerSpec(
    TowerKind Kind, string NameKey, string Icon, string Color, int Cost, double Range, double Damage, int Cooldown,
    TowerEffect Effect, bool HitsGround, bool HitsAir, double VsInfantry, double VsVehicle, double VsAir, string? ShotColor)
{
    public bool CanHit(EnemyClass enemyClass) => enemyClass == EnemyClass.Air ? HitsAir : HitsGround;

    public double Multiplier(EnemyClass enemyClass) => enemyClass switch
    {
        EnemyClass.Infantry => VsInfantry,
        EnemyClass.Vehicle => VsVehicle,
        _ => VsAir
    };
}

public sealed record EnemySpec(
    EnemyKind Kind, string NameKey, string Icon, string Color, int GlyphSize, EnemyClass Class, double Health, double Speed,
    double GoldMultiplier, int Score, int LifeCost, int Radius, string DeathColor)
{
    public bool IsFlying => Class == EnemyClass.Air;
}

public sealed record WaveGroup(EnemyKind Kind, int Count, int IntervalTicks);

public sealed record MapSpec(string Id, string NameKey, (int Column, int Row)[][] Routes, int[] Trees, int[] Rocks);

public sealed record DifficultySpec(string Id, string NameKey, int Lives, int StartGold, double HealthMultiplier, double GoldMultiplier);

/// <summary>Static, data-driven content of the tower defense game: towers, enemies, maps, waves and difficulties.</summary>
public static class TowerDefenseData
{
    public const int CampaignWaves = 20;

    public static readonly IReadOnlyList<TowerSpec> Towers =
    [
        new(TowerKind.Gunner, ResourceKeys.Games.TowerGun, "settings", "#bef264", 50, 100, 7, 9,
            TowerEffect.Single, true, true, 1, 0.6, 0.8, "#fde047"),
        new(TowerKind.Sniper, ResourceKeys.Games.TowerSniper, "search", "#fca5a5", 125, 200, 60, 45,
            TowerEffect.Single, true, true, 1.5, 0.8, 1, "#f8fafc"),
        new(TowerKind.Rocket, ResourceKeys.Games.TowerRocket, "alert", "#fb923c", 150, 115, 28, 40,
            TowerEffect.Splash, true, false, 0.8, 1.3, 0, "#fed7aa"),
        new(TowerKind.Tesla, ResourceKeys.Games.TowerTesla, "lightning", "#c4b5fd", 140, 95, 14, 22,
            TowerEffect.Chain, true, true, 1, 0.9, 1, "#c4b5fd"),
        new(TowerKind.Flamethrower, ResourceKeys.Games.TowerFlame, "heart", "#fdba74", 110, 62, 4, 3,
            TowerEffect.Burn, true, false, 0.5, 1.6, 0, "#fb923c"),
        new(TowerKind.SlowField, ResourceKeys.Games.TowerSlow, "cloud", "#67e8f9", 80, 85, 1, 20,
            TowerEffect.Slow, true, true, 1, 1, 1, null),
        new(TowerKind.Shotgun, ResourceKeys.Games.TowerShotgun, "sports_esports", "#fbbf24", 85, 72, 18, 16,
            TowerEffect.Splash, true, false, 1.5, 0.7, 0, "#fde68a"),
        new(TowerKind.MachineGun, ResourceKeys.Games.TowerMachineGun, "settings", "#a3e635", 115, 118, 5, 3,
            TowerEffect.Single, true, true, 1, 0.8, 0.7, "#bef264"),
        new(TowerKind.BioChemical, ResourceKeys.Games.TowerBioChemical, "cloud", "#86efac", 130, 92, 7, 20,
            TowerEffect.Poison, true, false, 1.2, 0.8, 0, "#86efac"),
        new(TowerKind.HeatRay, ResourceKeys.Games.TowerHeatRay, "visibility", "#fb7185", 180, 190, 40, 30,
            TowerEffect.Single, true, true, 1, 1.1, 0.9, "#fda4af")
    ];

    public static readonly IReadOnlyList<EnemySpec> Enemies =
    [
        new(EnemyKind.Soldier, ResourceKeys.Games.EnemySoldier, "user", "#7f1d1d", 18, EnemyClass.Infantry, 1, 1, 1, 10, 1, 8, "#e7e5e4"),
        new(EnemyKind.Armored, ResourceKeys.Games.EnemyArmored, "shield-check", "#365314", 20, EnemyClass.Infantry, 2.5, 0.85, 2, 20, 1, 9, "#d9f99d"),
        new(EnemyKind.Jeep, ResourceKeys.Games.EnemyJeep, "cart", "#9a3412", 24, EnemyClass.Vehicle, 1.2, 1.9, 2, 20, 1, 11, "#fb923c"),
        new(EnemyKind.Helicopter, ResourceKeys.Games.EnemyHelicopter, "video", "#1e3a8a", 26, EnemyClass.Air, 3, 1.5, 4, 40, 2, 12, "#93c5fd"),
        new(EnemyKind.Tank, ResourceKeys.Games.EnemyTank, "cube", "#450a0a", 30, EnemyClass.Vehicle, 10, 0.8, 5, 100, 3, 15, "#fb923c"),
        new(EnemyKind.Boss, ResourceKeys.Games.EnemyBoss, "ai-chip", "#4a044e", 38, EnemyClass.Vehicle, 50, 0.6, 30, 100, 5, 19, "#e879f9")
    ];

    public static readonly IReadOnlyList<MapSpec> Maps =
    [
        new("jungle", ResourceKeys.Games.MapJungle,
            [[(-1, 1), (7, 1), (7, 3), (2, 3), (2, 5), (10, 5)]],
            [9, 19, 50, 60], [30, 39]),
        new("switchback", ResourceKeys.Games.MapSwitchback,
            [[(-1, 0), (8, 0), (8, 2), (1, 2), (1, 4), (8, 4), (8, 6), (10, 6)]],
            [9, 20, 29, 40], [34, 55, 60]),
        new("crossfire", ResourceKeys.Games.MapCrossfire,
            [[(-1, 0), (6, 0), (6, 2), (3, 2), (3, 3), (10, 3)], [(-1, 6), (6, 6), (6, 4), (3, 4), (3, 3), (10, 3)]],
            [18, 21, 40, 59], [41, 58])
    ];

    public static readonly IReadOnlyList<DifficultySpec> Difficulties =
    [
        new("easy", ResourceKeys.Games.DifficultyEasy, 20, 200, 0.7, 1),
        new("normal", ResourceKeys.Games.DifficultyNormal, 15, 150, 1, 1),
        new("hard", ResourceKeys.Games.DifficultyHard, 10, 130, 1.2, 0.9)
    ];

    private static readonly WaveGroup[][] Waves =
    [
        [G(EnemyKind.Soldier, 8)],
        [G(EnemyKind.Soldier, 12)],
        [G(EnemyKind.Soldier, 10), G(EnemyKind.Armored, 3, 30)],
        [G(EnemyKind.Jeep, 6, 14), G(EnemyKind.Soldier, 8)],
        [G(EnemyKind.Armored, 8), G(EnemyKind.Jeep, 4, 14), G(EnemyKind.Tank, 1)],
        [G(EnemyKind.Helicopter, 3, 40), G(EnemyKind.Soldier, 10)],
        [G(EnemyKind.Soldier, 15, 14), G(EnemyKind.Armored, 6)],
        [G(EnemyKind.Jeep, 10, 10), G(EnemyKind.Helicopter, 4, 30)],
        [G(EnemyKind.Armored, 12, 20), G(EnemyKind.Tank, 1)],
        [G(EnemyKind.Soldier, 10, 16), G(EnemyKind.Boss, 1)],
        [G(EnemyKind.Soldier, 20, 12), G(EnemyKind.Jeep, 8, 12)],
        [G(EnemyKind.Helicopter, 8, 30), G(EnemyKind.Armored, 8)],
        [G(EnemyKind.Tank, 3, 60), G(EnemyKind.Jeep, 10, 10)],
        [G(EnemyKind.Armored, 16, 16), G(EnemyKind.Helicopter, 6, 30)],
        [G(EnemyKind.Tank, 2, 60), G(EnemyKind.Jeep, 12, 10), G(EnemyKind.Helicopter, 6, 30)],
        [G(EnemyKind.Soldier, 30, 8), G(EnemyKind.Armored, 12, 16)],
        [G(EnemyKind.Helicopter, 12, 24), G(EnemyKind.Tank, 3, 60)],
        [G(EnemyKind.Jeep, 20, 8), G(EnemyKind.Armored, 14, 16)],
        [G(EnemyKind.Tank, 5, 50), G(EnemyKind.Helicopter, 10, 24)],
        [G(EnemyKind.Boss, 2, 120), G(EnemyKind.Tank, 3, 60), G(EnemyKind.Helicopter, 6, 30)]
    ];

    public static TowerSpec Tower(TowerKind kind) => Towers[(int)kind];

    public static EnemySpec Enemy(EnemyKind kind) => Enemies[(int)kind];

    /// <summary>Wave composition; after the campaign the table repeats with growing groups (endless mode).</summary>
    public static IReadOnlyList<WaveGroup> WaveGroups(int wave)
    {
        var groups = Waves[(wave - 1) % CampaignWaves];
        if (wave <= CampaignWaves)
            return groups;

        var growth = 1 + (wave - CampaignWaves) * 0.05;
        return [.. groups.Select(group => group with { Count = (int)Math.Ceiling(group.Count * growth) })];
    }

    private static WaveGroup G(EnemyKind kind, int count, int intervalTicks = 22) => new(kind, count, intervalTicks);
}
