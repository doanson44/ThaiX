using System.Net.Http.Json;
using ThaiX.Client.Services.Api;

namespace ThaiX.Client.Services.Games;

public sealed class GameProgressClient(HttpClient httpClient) : IGameProgressClient
{
    public async Task<int?> GetTowerDefenseGoldAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("api/games/tower-defense/gold", cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<int?>(response, cancellationToken);
    }

    public async Task SaveTowerDefenseGoldAsync(int gold, CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            "api/games/tower-defense/gold",
            new SaveTowerDefenseGoldRequestModel(gold),
            cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    public async Task<TowerDefenseProgressModel?> GetTowerDefenseProgressAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("api/games/tower-defense/progress", cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<TowerDefenseProgressModel>(response, cancellationToken);
    }

    public async Task SaveTowerDefenseProgressAsync(
        TowerDefenseProgressModel progress,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PutAsJsonAsync(
            "api/games/tower-defense/progress",
            progress,
            cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    private sealed record SaveTowerDefenseGoldRequestModel(int Gold);
}