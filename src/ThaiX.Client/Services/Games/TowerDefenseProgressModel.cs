namespace ThaiX.Client.Services.Games;

public sealed record TowerDefenseProgressModel(
    int Credits,
    int BonusStartingGold,
    string[] ClearedMissions,
    string[] UnlockedTowers,
    string[] BonusMissions);
