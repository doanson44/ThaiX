using System.Text.Json;
using Microsoft.JSInterop;
using ThaiX.Client.Models.Games;

namespace ThaiX.Client.Services.Games;

public sealed class GameScoreStore(IJSRuntime jsRuntime)
{
    private const string GuestSessionKey = "ThaiX.Games.GuestSession";
    private const string GuestNameKey = "ThaiX.Games.GuestName";
    private const string GuestChoiceKey = "ThaiX.Games.LeaderboardChoice";
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

        if (!state.BestScore.HasValue || IsBetter(gameId, score, state.BestScore.Value))
            state.BestScore = score;

        await jsRuntime.InvokeVoidAsync(
            "localStorage.setItem",
            GetScoreKey(gameId),
            JsonSerializer.Serialize(state, JsonOptions));

        return state;
    }

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

    private static bool IsBetter(string gameId, decimal candidate, decimal current) =>
        gameId is "minesweeper" or "sudoku" or "memory-cards" or "15-puzzle" or "lights-out" or "wordle"
            ? candidate < current
            : candidate > current;
}
