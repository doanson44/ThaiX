using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.JSInterop;

namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public sealed class FrontlinePlayerProgressStore(
    IJSRuntime jsRuntime,
    ILogger<FrontlinePlayerProgressStore> logger)
{
    public const string LegacyStorageKey = "fd2:so:FD-2-32-317";

    public async ValueTask<FrontlinePlayerProgress> LoadAsync(
        CancellationToken cancellationToken = default)
    {
        string? json;
        try
        {
            json = await jsRuntime.InvokeAsync<string?>(
                "localStorage.getItem",
                cancellationToken,
                LegacyStorageKey);
        }
        catch (JSException exception)
        {
            logger.LogWarning(exception, "Unable to read Frontline progress from browser storage.");
            return FrontlinePlayerProgress.CreateNew();
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            var newProgress = FrontlinePlayerProgress.CreateNew();
            await SaveAsync(newProgress, cancellationToken);
            return newProgress;
        }

        try
        {
            using var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object || GetInt(root, "bPlayed") != 1)
            {
                var newProgress = FrontlinePlayerProgress.CreateNew();
                await SaveAsync(newProgress, cancellationToken);
                return newProgress;
            }

            var towerUnlocks = Enumerable.Range(0, FrontlinePlayerProgress.TowerSlotCount)
                .Select(index => GetInt(root, $"Tower{index}"))
                .ToArray();
            var levelScores = Enumerable.Range(0, FrontlinePlayerProgress.LevelSlotCount)
                .Select(index => GetInt(root, $"Level{index}"))
                .ToArray();

            try
            {
                return FrontlinePlayerProgress.Load(new FrontlineLegacySaveData(
                    true,
                    GetInt(root, "credits"),
                    GetInt(root, "stFunds"),
                    towerUnlocks,
                    levelScores));
            }
            catch (ArgumentException exception)
            {
                logger.LogWarning(exception, "Frontline progress data is invalid; starting a new profile.");
                var newProgress = FrontlinePlayerProgress.CreateNew();
                await SaveAsync(newProgress, cancellationToken);
                return newProgress;
            }
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Frontline progress JSON is invalid; starting a new profile.");
            var newProgress = FrontlinePlayerProgress.CreateNew();
            await SaveAsync(newProgress, cancellationToken);
            return newProgress;
        }
    }

    public async ValueTask<bool> SaveAsync(
        FrontlinePlayerProgress progress,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(progress);

        try
        {
            var existingJson = await jsRuntime.InvokeAsync<string?>(
                "localStorage.getItem",
                cancellationToken,
                LegacyStorageKey);
            var root = ParseObject(existingJson);
            var save = progress.ToLegacySaveData();

            root["bPlayed"] = 1;
            root["credits"] = save.Credits;
            root["stFunds"] = save.StartingFunds;

            for (var index = 0; index < FrontlinePlayerProgress.TowerSlotCount; index++)
                root[$"Tower{index}"] = save.TowerUnlocks[index];

            for (var index = 0; index < FrontlinePlayerProgress.LevelSlotCount; index++)
                root[$"Level{index}"] = save.LevelScores[index];

            await jsRuntime.InvokeVoidAsync(
                "localStorage.setItem",
                cancellationToken,
                LegacyStorageKey,
                root.ToJsonString());

            return true;
        }
        catch (JSException exception)
        {
            logger.LogWarning(exception, "Unable to save Frontline progress to browser storage.");
            return false;
        }
    }

    private static JsonObject ParseObject(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            return JsonNode.Parse(json) as JsonObject ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static int GetInt(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            return 0;

        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
            return number;

        if (value.ValueKind == JsonValueKind.String
            && int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
            return number;

        return 0;
    }
}
