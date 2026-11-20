namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public sealed record FrontlineEnemyArchetype(
    string Id,
    double BaseHealth,
    double BaseSpeed,
    int BaseReward,
    bool IsMetal,
    double HeatResistance = 1,
    double RadiationResistance = 1,
    double PoisonResistance = 1,
    double ColdResistance = 1)
{
    public double HealthForWave(int waveNumber)
    {
        if (waveNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(waveNumber));

        return BaseHealth * (1 + waveNumber * 0.1);
    }

    public FrontlineEnemyDefinition CreateDefinition(int waveNumber) =>
        new(
            Id,
            HealthForWave(waveNumber),
            BaseSpeed,
            BaseReward,
            IsMetal,
            HeatResistance,
            RadiationResistance,
            PoisonResistance,
            ColdResistance);
}

public static class FrontlineEnemyCatalog
{
    public static IReadOnlyList<FrontlineEnemyArchetype> Verified { get; } =
    [
        new("enemy1", 50, 1, 10, false, HeatResistance: 0.8),
        new("enemy2", 50, 0.8, 10, false, RadiationResistance: 0.6),
        new("enemy3", 50, 1, 10, false),
        new("enemy4", 50, 1, 10, false, PoisonResistance: 0.3),
        new("enemy5", 50, 1, 10, true),
        new("bot_1", 1000, 0.8, 10, true),
        new("Jeep1", 150, 0.9, 10, true, HeatResistance: 0.7, RadiationResistance: 0.8),
        new("Jeep2", 200, 1, 10, true, HeatResistance: 0.8),
        new("Jeep3", 150, 1.2, 10, true, HeatResistance: 0.7),
        new("Jeep4", 120, 1.3, 10, true, HeatResistance: 0.9),
        new("Tank1", 500, 1, 10, true, HeatResistance: 0.7),
        new("Tank2", 400, 0.7, 10, true, HeatResistance: 0.6),
        new("Tank3", 550, 0.6, 10, true, HeatResistance: 0.6)
    ];
}
