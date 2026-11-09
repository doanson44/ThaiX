using ThaiX.Client.Models.Games;

namespace ThaiX.Client.Services.Games;

public interface IGameLeaderboardClient
{
    Task<GameLeaderboardModel> GetAsync(string gameId, string? mode = null, CancellationToken cancellationToken = default);
    Task<GuestSessionModel> CreateGuestSessionAsync(CancellationToken cancellationToken = default);
    Task<SubmitGameScoreResultModel> SubmitAsync(SubmitGameScoreModel model, CancellationToken cancellationToken = default);
}
