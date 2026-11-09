namespace ThaiX.Client.Models.Games;

public sealed record GameLeaderboardEntryModel(
    string PlayerName,
    decimal Score,
    DateTime AchievedAtUtc,
    int GamesPlayed,
    string? Mode);

public sealed record GameLeaderboardModel(
    string GameId,
    string? Mode,
    IReadOnlyList<GameLeaderboardEntryModel> Entries);

public sealed record SubmitGameScoreModel(
    string GameId,
    Guid RunId,
    decimal Score,
    int DurationSeconds,
    string? Mode,
    string? GuestPlayerId,
    string? GuestToken,
    string? GuestPlayerName);

public sealed record SubmitGameScoreResultModel(
    bool IsPersonalBest,
    int Rank,
    IReadOnlyList<GameLeaderboardEntryModel> Entries);

public sealed class GameRunRecord
{
    public Guid RunId { get; set; }
    public decimal Score { get; set; }
    public DateTime AchievedAtUtc { get; set; }
    public int DurationSeconds { get; set; }
    public string? Mode { get; set; }
}

public sealed class GameLocalScoreState
{
    public decimal? BestScore { get; set; }
    public decimal TotalWins { get; set; }
    public List<GameRunRecord> History { get; set; } = [];
}

public sealed record GuestSessionModel(Guid PlayerId, string Token, DateTime ExpiresAtUtc);
