namespace ThaiX.Application.Features.Games;

public sealed record TowerDefenseProgressDto(
    int Credits,
    int BonusStartingGold,
    string[] ClearedMissions,
    string[] UnlockedTowers,
    string[] BonusMissions);