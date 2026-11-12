namespace ThaiX.Client.Models.Games;

public sealed class TowerDefenseProgress
{
    public int UnlockedMission { get; set; } = 1;
    public Dictionary<int, int> MissionStars { get; set; } = [];
}