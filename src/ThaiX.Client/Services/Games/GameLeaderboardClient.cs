using System.Net.Http.Json;
using ThaiX.Client.Models.Games;
using ThaiX.Client.Services.Api;

namespace ThaiX.Client.Services.Games;

public sealed class GameLeaderboardClient(HttpClient httpClient) : IGameLeaderboardClient
{
    public async Task<GameLeaderboardModel> GetAsync(
        string gameId,
        string? mode = null,
        CancellationToken cancellationToken = default)
    {
        var path = $"api/games/leaderboards/{Uri.EscapeDataString(gameId)}";
        if (!string.IsNullOrWhiteSpace(mode))
            path += $"?mode={Uri.EscapeDataString(mode)}";

        using var response = await httpClient.GetAsync(path, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<GameLeaderboardModel>(response, cancellationToken);
    }

    public async Task<GuestSessionModel> CreateGuestSessionAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsync("api/games/guest-session", content: null, cancellationToken: cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<GuestSessionModel>(response, cancellationToken);
    }

    public async Task<SubmitGameScoreResultModel> SubmitAsync(
        SubmitGameScoreModel model,
        CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.PostAsJsonAsync("api/games/scores", model, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<SubmitGameScoreResultModel>(response, cancellationToken);
    }
}
