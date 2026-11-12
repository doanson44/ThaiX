using System.Text.Json;
using Microsoft.JSInterop;
using ThaiX.Client.Models.Games;

namespace ThaiX.Client.Services.Games;

public sealed class GameScoreStore(IJSRuntime jsRuntime)
{
    private const string GuestSessionKey = "ThaiX.Games.GuestSession";
    private const string GuestNameKey = "ThaiX.Games.GuestName";
    private const string GuestChoiceKey = "ThaiX.Games.LeaderboardChoice";
    private const string TowerDefenseProgressKey = "ThaiX.Games.TowerDefense.Progress.v1";
    private const int HistoryLimit = 500;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<GameLocalScoreState> GetAsync(string gameId)
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", GetScoreKey(gameId));
        if (string.IsNullOrWhiteSpace(json))
            return new GameLocalScoreState();

        try
        {
            var state = JsonSerializer.Deserialize<GameLocalScoreState>(json, JsonOptions)
                ?? new GameLocalScoreState();
            state.History ??= [];
            state.ModeBestScores ??= new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
            return state;
        }
        catch (JsonException)
        {
            return new GameLocalScoreState();
        }
    }

    public async Task<GameLocalScoreState> SaveAsync(
        string gameId,
        Guid runId,
        decimal score,
        int durationSeconds,
        string? mode)
    {
        var state = await GetAsync(gameId);
        var run = new GameRunRecord
        {
            RunId = runId,
            Score = score,
            AchievedAtUtc = DateTime.UtcNow,
            DurationSeconds = durationSeconds,
            Mode = mode
        };

        state.History.Insert(0, run);
        if (state.History.Count > HistoryLimit)
            state.History.RemoveRange(HistoryLimit, state.History.Count - HistoryLimit);

        if (IsCumulativeWinGame(gameId))
        {
            state.TotalWins += score;
        }
        else
        {
            if (!state.BestScore.HasValue || IsBetter(gameId, score, state.BestScore.Value))
                state.BestScore = score;

            if (!string.IsNullOrWhiteSpace(mode)
                && (!state.ModeBestScores.TryGetValue(mode, out var modeBest)
                    || IsBetter(gameId, score, modeBest)))
                state.ModeBestScores[mode] = score;
        }

        await jsRuntime.InvokeVoidAsync(
            "localStorage.setItem",
            GetScoreKey(gameId),
            JsonSerializer.Serialize(state, JsonOptions));

        return state;
    }

    public async Task<TowerDefenseProgress> GetTowerDefenseProgressAsync()
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", TowerDefenseProgressKey);
        if (string.IsNullOrWhiteSpace(json))
            return new TowerDefenseProgress();

        try
        {
            var progress = JsonSerializer.Deserialize<TowerDefenseProgress>(json, JsonOptions)
                ?? new TowerDefenseProgress();
            progress.MissionStars ??= [];
            progress.UnlockedMission = Math.Clamp(progress.UnlockedMission, 1, 14);
            return progress;
        }
        catch (JsonException)
        {
            return new TowerDefenseProgress();
        }
    }

    public Task SaveTowerDefenseProgressAsync(TowerDefenseProgress progress) =>
        jsRuntime.InvokeVoidAsync(
            "localStorage.setItem",
            TowerDefenseProgressKey,
            JsonSerializer.Serialize(progress, JsonOptions)).AsTask();

    public async Task<GuestSessionModel?> GetGuestSessionAsync()
    {
        var json = await jsRuntime.InvokeAsync<string?>("localStorage.getItem", GuestSessionKey);
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var session = JsonSerializer.Deserialize<GuestSessionModel>(json, JsonOptions);
            return session is not null && session.ExpiresAtUtc > DateTime.UtcNow.AddMinutes(1) ? session : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public Task SaveGuestSessionAsync(GuestSessionModel session) =>
        jsRuntime.InvokeVoidAsync(
            "localStorage.setItem",
            GuestSessionKey,
            JsonSerializer.Serialize(session, JsonOptions)).AsTask();

    public Task<string?> GetGuestNameAsync() =>
        jsRuntime.InvokeAsync<string?>("localStorage.getItem", GuestNameKey).AsTask();

    public Task SetGuestNameAsync(string name) =>
        jsRuntime.InvokeVoidAsync("localStorage.setItem", GuestNameKey, name.Trim()).AsTask();

    public Task<string?> GetGuestChoiceAsync() =>
        jsRuntime.InvokeAsync<string?>("localStorage.getItem", GuestChoiceKey).AsTask();

    public Task SetGuestChoiceAsync(bool join) =>
        jsRuntime.InvokeVoidAsync("localStorage.setItem", GuestChoiceKey, join ? "joined" : "declined").AsTask();

    private static string GetScoreKey(string gameId) => $"ThaiX.Games.Scores.{gameId}";

    private static bool IsCumulativeWinGame(string gameId) =>
        gameId is "pong" or "tic-tac-toe" or "connect-four";

    private static bool IsBetter(string gameId, decimal candidate, decimal current) =>
        gameId is "minesweeper" or "sudoku" or "memory-cards" or "15-puzzle" or "lights-out" or "wordle"
            ? candidate < current
            : candidate > current;
}