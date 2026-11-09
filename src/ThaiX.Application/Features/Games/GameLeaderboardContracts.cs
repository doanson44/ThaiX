namespace ThaiX.Application.Features.Games;

public sealed record GameLeaderboardEntryDto(
    string PlayerName,
    decimal Score,
    DateTime AchievedAtUtc,
    int GamesPlayed,
    string? Mode);

public sealed record GameLeaderboardDto(
    string GameId,
    string? Mode,
    IReadOnlyList<GameLeaderboardEntryDto> Entries);

public sealed record SubmitGameScoreResultDto(
    bool IsPersonalBest,
    int Rank,
    IReadOnlyList<GameLeaderboardEntryDto> Entries);

internal sealed class GameLeaderboardDocument
{
    public int SchemaVersion { get; set; } = 1;
    public string GameId { get; set; } = string.Empty;
    public DateTime UpdatedAtUtc { get; set; }
    public List<GameLeaderboardRecord> Entries { get; set; } = [];
    public List<Guid> ProcessedRunIds { get; set; } = [];
}

internal sealed class GameLeaderboardRecord
{
    public string PlayerId { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public DateTime AchievedAtUtc { get; set; }
    public int GamesPlayed { get; set; }
    public string? Mode { get; set; }
}
