namespace ThaiX.Client.Services.Games;

public interface IGameProgressClient
{
    Task<int?> GetTowerDefenseGoldAsync(CancellationToken cancellationToken = default);
    Task SaveTowerDefenseGoldAsync(int gold, CancellationToken cancellationToken = default);
    Task<TowerDefenseProgressModel?> GetTowerDefenseProgressAsync(CancellationToken cancellationToken = default);
    Task SaveTowerDefenseProgressAsync(TowerDefenseProgressModel progress, CancellationToken cancellationToken = default);
}
