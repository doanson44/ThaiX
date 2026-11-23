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
    int ShotLimit = 0,
    FrontlineDamageProfile? DamageProfile = null,
    FrontlineDamageProfile? MetalDamageProfile = null,
    FrontlineProjectileProfile? ProjectileProfile = null,
    double DamageUpgradeScale = 1,
    double StatusUpgradeScale = 0,
    double MetalDamageUpgradeScale = 1,
    double ExperienceLimit = 500)
{
    public FrontlineTowerDefinition CreateDefinition() =>
        new(
            Id,
            BuildCost,
            Range,
            Damage,
            RateOfFire,
            TargetMode,
            IsSupport,
            Id switch
            {
                "suptower1" => new FrontlineDamageProfile(Burn: 100),
                "suptower2" => new FrontlineDamageProfile(Poison: 80),
                "suptower3" => new FrontlineDamageProfile(Burn: 20),
                _ => null
            },
            ShotLimit,
            DamageProfile,
            MetalDamageProfile,
            ProjectileProfile,
            DamageUpgradeScale,
            StatusUpgradeScale,
            MetalDamageUpgradeScale,
            ExperienceLimit);
}

public static class FrontlineTowerCatalog
{
    public static IReadOnlyList<FrontlineTowerArchetype> All { get; } =
    [
        new("tower1", "Missile Launcher", 1, 800, 80, 1, 150, FrontlineTargetMode.Heat, 8000,
            DamageProfile: new FrontlineDamageProfile(80, Burn: 50),
            ProjectileProfile: new FrontlineProjectileProfile(
                FrontlineProjectileKind.HomingMissile, 4, 6, SplashRadius: 110),
            ExperienceLimit: 2000),
        new("tower2", "Rifle Tower", 2, 50, 10, 3, 60, FrontlineTargetMode.Weak,
            DamageProfile: new FrontlineDamageProfile(10, Burn: 2),
            MetalDamageProfile: new FrontlineDamageProfile(5, Burn: 5),
            ExperienceLimit: 500),
        new("tower3", "Sniper Tower", 3, 400, 80, 2, 120, FrontlineTargetMode.Near,
            DamageProfile: new FrontlineDamageProfile(80, Burn: 2),
            MetalDamageProfile: new FrontlineDamageProfile(40, Burn: 5),
            ExperienceLimit: 1000),
        new("tower4", "Electro Tower", 4, 250, 50, 3, 60, FrontlineTargetMode.Near,
            DamageProfile: new FrontlineDamageProfile(50),
            MetalDamageProfile: new FrontlineDamageProfile(50),
            DamageUpgradeScale: 0.6,
            MetalDamageUpgradeScale: 1,
            ExperienceLimit: 1200),
        new("tower5", "Shotgun Tower", 5, 150, 35, 2, 70, FrontlineTargetMode.Strong, 2000,
            DamageProfile: new FrontlineDamageProfile(35, Burn: 10),
            MetalDamageProfile: new FrontlineDamageProfile(17.5, Burn: 20),
            ExperienceLimit: 800),
        new("tower6", "Particle Beam Cannon", 6, 600, 150, 1, 100, FrontlineTargetMode.Strong, 7000,
            DamageProfile: new FrontlineDamageProfile(150, Burn: 40),
            ExperienceLimit: 1500),
        new("tower7", "Bio Mortar", 7, 950, 40, 1, 170, FrontlineTargetMode.Strong, 10000,
            DamageProfile: new FrontlineDamageProfile(40, Poison: 30),
            ProjectileProfile: new FrontlineProjectileProfile(
                FrontlineProjectileKind.BallisticShell, 1.6, 6, SplashRadius: 80,
                SplashDamageScale: 0.5, StartOffset: 12, VerticalOffset: -8, Lifetime: 500),
            DamageUpgradeScale: 0,
            MetalDamageUpgradeScale: 0,
            ExperienceLimit: 2500),
        new("tower8", "Bio Cannon", 8, 300, 12, 3, 85, FrontlineTargetMode.Cold, 6000,
            DamageProfile: new FrontlineDamageProfile(12, Poison: 60),
            ProjectileProfile: new FrontlineProjectileProfile(
                FrontlineProjectileKind.TrackingPoison, 3, 6, StartOffset: 20, VerticalOffset: -8, Lifetime: 500),
            DamageUpgradeScale: 0,
            StatusUpgradeScale: 0,
            MetalDamageUpgradeScale: 0,
            ExperienceLimit: 800),
        new("tower9", "Flame Thrower", 9, 250, 2, 12, 55, FrontlineTargetMode.Near, 5000,
            DamageProfile: new FrontlineDamageProfile(2, Burn: 30),
            ProjectileProfile: new FrontlineProjectileProfile(
                FrontlineProjectileKind.TrackingFlame, 1.5, 6, StartOffset: 20, VerticalOffset: -8, Lifetime: 50),
            DamageUpgradeScale: 0,
            StatusUpgradeScale: 0,
            MetalDamageUpgradeScale: 0,
            ExperienceLimit: 500),
        new("tower10", "Mounted Machine Gun", 10, 500, 25, 7, 90, FrontlineTargetMode.Near, 4000,
            DamageProfile: new FrontlineDamageProfile(25, Burn: 2),
            MetalDamageProfile: new FrontlineDamageProfile(12.5, Burn: 5),
            ExperienceLimit: 1500),
        new("tower11", "Plasma Cannon", 11, 1000, 40, 1, 160, FrontlineTargetMode.Heat, 9000,
            DamageProfile: new FrontlineDamageProfile(40, Radiation: 20),
            ProjectileProfile: new FrontlineProjectileProfile(
                FrontlineProjectileKind.BallisticShell, 1.6, 6, SplashRadius: 80,
                SplashDamageScale: 0.5, StartOffset: 12, VerticalOffset: -8, Lifetime: 500),
            DamageUpgradeScale: 0,
            MetalDamageUpgradeScale: 0,
            ExperienceLimit: 2500),
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
