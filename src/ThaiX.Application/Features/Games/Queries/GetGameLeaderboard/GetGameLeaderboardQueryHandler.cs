using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.Games.Queries.GetGameLeaderboard;

public sealed class GetGameLeaderboardQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetGameLeaderboardQuery, GameLeaderboardDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<GameLeaderboardDto> Handle(
        GetGameLeaderboardQuery request,
        CancellationToken cancellationToken)
    {
        if (!GameScoreRules.IsSupported(request.GameId))
            return new GameLeaderboardDto(request.GameId, request.Mode, []);

        var code = GameScoreRules.BuildBinCode(request.GameId);
        var bin = await context.JsonBins
            .AsNoTracking()
            .Where(item => item.Code == code)
            .Select(item => item.Content)
            .FirstOrDefaultAsync(cancellationToken);

        if (bin is null || bin.Length == 0)
            return new GameLeaderboardDto(request.GameId, request.Mode, []);

        GameLeaderboardDocument? document;
        try
        {
            document = JsonSerializer.Deserialize<GameLeaderboardDocument>(bin, JsonOptions);
        }
        catch (JsonException)
        {
            return new GameLeaderboardDto(request.GameId, request.Mode, []);
        }

        var entries = (document?.Entries ?? [])
            .Where(entry => string.IsNullOrWhiteSpace(request.Mode)
                || string.Equals(entry.Mode, request.Mode, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(entry => GameScoreRules.IsLowerBetter(request.GameId)
                ? -entry.Score
                : entry.Score)
            .ThenBy(entry => entry.AchievedAtUtc)
            .Take(100)
            .Select(entry => new GameLeaderboardEntryDto(
                entry.PlayerName,
                entry.Score,
                entry.AchievedAtUtc,
                entry.GamesPlayed,
                entry.Mode))
            .ToArray();

        return new GameLeaderboardDto(request.GameId, request.Mode, entries);
    }
}
