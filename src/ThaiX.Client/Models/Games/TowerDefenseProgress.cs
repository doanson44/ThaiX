namespace ThaiX.Client.Models.Games;

public sealed class TowerDefenseProgress
{
    public int UnlockedMission { get; set; } = 1;
    public Dictionary<int, int> MissionStars { get; set; } = [];
    public int ResearchPoints { get; set; }
    public int DamageResearchLevel { get; set; }
    public int SupplyResearchLevel { get; set; }
    public int ReinforcementResearchLevel { get; set; }
}