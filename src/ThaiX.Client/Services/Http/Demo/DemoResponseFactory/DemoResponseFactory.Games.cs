using ThaiX.Client.Models.Games;

namespace ThaiX.Client.Services.Http.Demo;

public sealed partial class DemoResponseFactory
{
    private async Task<HttpResponseMessage> HandleGamesAsync(
        string method,
        string path,
        string query,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (method == "GET" && path.StartsWith("api/games/leaderboards/", StringComparison.OrdinalIgnoreCase))
        {
            var gameId = path.Split('/', StringSplitOptions.RemoveEmptyEntries).Last();
            var mode = GetQueryValue(query, "mode");
            var entries = new[]
            {
                new GameLeaderboardEntryModel("Demo Champion", 9850, DateTime.UtcNow.AddMinutes(-15), 24, mode),
                new GameLeaderboardEntryModel("Player One", 7420, DateTime.UtcNow.AddHours(-2), 18, mode),
                new GameLeaderboardEntryModel("Guest Player", 5100, DateTime.UtcNow.AddDays(-1), 9, mode)
            };
            return DemoEnvelope.SuccessData(new GameLeaderboardModel(gameId, mode, entries));
        }

        if (method == "POST" && path.Equals("api/games/guest-session", StringComparison.OrdinalIgnoreCase))
        {
            return DemoEnvelope.SuccessData(new GuestSessionModel(
                Guid.NewGuid(),
                $"demo-{Guid.NewGuid():N}",
                DateTime.UtcNow.AddHours(1)));
        }

        if (method == "POST" && path.Equals("api/games/scores", StringComparison.OrdinalIgnoreCase))
        {
            var body = await ReadJsonAsync<SubmitGameScoreModel>(request, cancellationToken);
            if (body is null)
            {
                return DemoEnvelope.Failure("DEMO_INVALID_GAME_SCORE", "A game score payload is required.");
            }

            var entry = new GameLeaderboardEntryModel(
                string.IsNullOrWhiteSpace(body.GuestPlayerName) ? "Demo Player" : body.GuestPlayerName,
                body.Score,
                DateTime.UtcNow,
                1,
                body.Mode);
            return DemoEnvelope.SuccessData(new SubmitGameScoreResultModel(true, 1, [entry]));
        }

        return DemoEnvelope.Failure("DEMO_GAME_ENDPOINT_NOT_FOUND", "The demo game endpoint was not found.", System.Net.HttpStatusCode.NotFound);
    }
}
