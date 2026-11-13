namespace ThaiX.Client.Services.Games;

public interface IGameProgressClient
{
    Task<int?> GetTowerDefenseGoldAsync(CancellationToken cancellationToken = default);
    Task SaveTowerDefenseGoldAsync(int gold, CancellationToken cancellationToken = default);
}
