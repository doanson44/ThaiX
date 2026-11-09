using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.Games.Commands.SubmitGameScore;

public sealed class SubmitGameScoreCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SubmitGameScoreCommand, SubmitGameScoreResultDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<SubmitGameScoreResultDto> Handle(
        SubmitGameScoreCommand request,
        CancellationToken cancellationToken)
    {
        // The serializable transaction protects this read-modify-write across application instances.
        // The keyed lock reduces contention between same-process submissions.
        using var gate = await AsyncLock.GetLockByKey(
            $"games:leaderboard:{request.GameId}").LockAsync(cancellationToken);

        var code = GameScoreRules.BuildBinCode(request.GameId);
        var bin = await context.JsonBins
            .FirstOrDefaultAsync(item => item.Code == code, cancellationToken);

        var document = bin is null
            ? new GameLeaderboardDocument { GameId = request.GameId }
            : Deserialize(bin.Content);

        document.Entries ??= [];
        document.ProcessedRunIds ??= [];
        if (document.ProcessedRunIds.Contains(request.RunId))
            return CreateResult(document, request, isPersonalBest: false);

        var playerName = request.PlayerName;
        if (request.UserId.HasValue)
        {
            var fullName = await context.UserProfiles
                .AsNoTracking()
                .Where(profile => profile.UserId == request.UserId.Value)
                .Select(profile => profile.FullName)
                .FirstOrDefaultAsync(cancellationToken);
            playerName = string.IsNullOrWhiteSpace(fullName) ? "Player" : fullName.Trim();
            if (playerName.Length > 64)
                playerName = playerName[..64];
        }

        var existing = document.Entries.FirstOrDefault(entry =>
            entry.PlayerId == request.PlayerId
            && string.Equals(entry.Mode, request.Mode, StringComparison.OrdinalIgnoreCase));

        var now = DateTime.UtcNow;
        var cumulativeWins = GameScoreRules.IsCumulativeWinGame(request.GameId);
        var isPersonalBest = existing is null
            || (cumulativeWins
                ? request.Score > 0
                : GameScoreRules.IsBetter(request.GameId, request.Score, existing.Score));

        if (existing is null)
        {
            document.Entries.Add(new GameLeaderboardRecord
            {
                PlayerId = request.PlayerId,
                PlayerName = playerName,
                Score = request.Score,
                AchievedAtUtc = now,
                GamesPlayed = 1,
                Mode = request.Mode
            });
        }
        else
        {
            existing.GamesPlayed++;
            existing.PlayerName = playerName;
            if (cumulativeWins)
            {
                existing.Score += request.Score;
                if (request.Score > 0)
                    existing.AchievedAtUtc = now;
            }
            else if (isPersonalBest)
            {
                existing.Score = request.Score;
                existing.AchievedAtUtc = now;
            }
        }

        document.ProcessedRunIds.Add(request.RunId);
        if (document.ProcessedRunIds.Count > 5_000)
            document.ProcessedRunIds.RemoveRange(0, document.ProcessedRunIds.Count - 5_000);

        document.UpdatedAtUtc = now;
        document.Entries = document.Entries
            .OrderByDescending(entry => GameScoreRules.IsLowerBetter(request.GameId)
                ? -entry.Score
                : entry.Score)
            .ThenBy(entry => entry.AchievedAtUtc)
            .ToList();

        var payload = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
        if (bin is null)
        {
            bin = JsonBin.Create(
                code,
                $"Games leaderboard: {request.GameId}",
                JsonBinCategories.Config,
                payload,
                tags: "games-leaderboard");
            context.JsonBins.Add(bin);
        }
        else
        {
            bin.ReplaceContent(payload, "application/json", isCompressed: false);
        }

        await context.SaveChangesAsync(cancellationToken);

        return CreateResult(document, request, isPersonalBest);
    }

    private static SubmitGameScoreResultDto CreateResult(
        GameLeaderboardDocument document,
        SubmitGameScoreCommand request,
        bool isPersonalBest)
    {
        var ranked = document.Entries
            .Where(entry => string.Equals(entry.Mode, request.Mode, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        var rank = Array.FindIndex(ranked, entry => entry.PlayerId == request.PlayerId) + 1;
        var entries = ranked.Take(100)
            .Select(entry => new GameLeaderboardEntryDto(
                entry.PlayerName,
                entry.Score,
                entry.AchievedAtUtc,
                entry.GamesPlayed,
                entry.Mode))
            .ToArray();

        return new SubmitGameScoreResultDto(isPersonalBest, rank, entries);
    }

    private static GameLeaderboardDocument Deserialize(byte[] content)
    {
        try
        {
            var document = JsonSerializer.Deserialize<GameLeaderboardDocument>(content, JsonOptions)
                ?? throw new InvalidOperationException("Leaderboard data is empty.");
            document.Entries ??= [];
            document.ProcessedRunIds ??= [];
            return document;
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException("Leaderboard data is invalid and was not overwritten.", exception);
        }
    }
}
