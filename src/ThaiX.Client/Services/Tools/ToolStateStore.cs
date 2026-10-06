using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using ThaiX.Client.Constants;
using ThaiX.Client.Models.JsonBins;
using ThaiX.Client.Services.JsonBins;

namespace ThaiX.Client.Services.Tools;

public sealed class ToolStateStore : IToolStateStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly IJSRuntime _js;
    private readonly IJsonBinService _jsonBins;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly NavigationManager _navigation;

    public ToolStateStore(
        IJSRuntime js,
        IJsonBinService jsonBins,
        AuthenticationStateProvider authStateProvider,
        NavigationManager navigation)
    {
        _js = js;
        _jsonBins = jsonBins;
        _authStateProvider = authStateProvider;
        _navigation = navigation;
    }

    public async Task<ToolStateEnvelope?> LoadAsync(ToolDefinition tool, CancellationToken cancellationToken = default)
    {
        var userId = await GetCloudUserIdAsync();
        if (userId is not null && tool.AllowCloudSave)
        {
            try
            {
                var cloud = await TryLoadCloudEnvelopeAsync(userId, tool.Slug, cancellationToken);
                if (cloud is not null)
                    return cloud;

                var localJson = await ReadLocalRawAsync(tool.Slug);
                if (!string.IsNullOrWhiteSpace(localJson))
                {
                    var envelope = DeserializeEnvelope(localJson);
                    if (envelope is not null)
                    {
                        await UpsertCloudAsync(tool, userId, envelope, cancellationToken);
                        await ClearLocalRawAsync(tool.Slug);
                        return envelope;
                    }
                }

                return null;
            }
            catch
            {
                var fallback = await ReadLocalRawAsync(tool.Slug);
                return string.IsNullOrWhiteSpace(fallback) ? null : DeserializeEnvelope(fallback);
            }
        }

        var local = await ReadLocalRawAsync(tool.Slug);
        return string.IsNullOrWhiteSpace(local) ? null : DeserializeEnvelope(local);
    }

    public async Task SaveLocalAsync(ToolDefinition tool, string stateJson, CancellationToken cancellationToken = default)
    {
        var envelope = CreateEnvelope(stateJson);
        var content = JsonSerializer.Serialize(envelope, JsonOptions);

        var userId = await GetCloudUserIdAsync();
        if (userId is not null && tool.AllowCloudSave)
        {
            try
            {
                await UpsertCloudAsync(tool, userId, envelope, cancellationToken);
                await ClearLocalRawAsync(tool.Slug);
                return;
            }
            catch
            {
                // ponytail: fall back to browser storage when cloud is unreachable
            }
        }

        await WriteLocalRawAsync(tool.Slug, content);
    }

    public async Task ClearLocalAsync(ToolDefinition tool, CancellationToken cancellationToken = default)
    {
        await ClearLocalRawAsync(tool.Slug);

        var userId = await GetCloudUserIdAsync();
        if (userId is null || !tool.AllowCloudSave)
            return;

        try
        {
            var code = ToolCatalog.BuildCloudCode(userId, tool.Slug);
            var existing = await _jsonBins.GetByCodeAsync(code, cancellationToken);
            if (existing is not null)
                await _jsonBins.DeleteAsync(existing.Id, cancellationToken);
        }
        catch
        {
            // Best effort — local draft is already cleared.
        }
    }

    public async Task SaveCloudAsync(
        ToolDefinition tool,
        string stateJson,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        if (!tool.AllowCloudSave)
            throw new InvalidOperationException("Cloud save is disabled for this tool.");

        var userId = await RequireCloudUserIdAsync();
        await UpsertCloudAsync(tool, userId, CreateEnvelope(stateJson), cancellationToken, displayName);
    }

    public async Task<string> ShareAsync(
        ToolDefinition tool,
        string stateJson,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        if (!tool.AllowShare)
            throw new InvalidOperationException("Share is disabled for this tool.");

        await SaveCloudAsync(tool, stateJson, displayName, cancellationToken);

        var userId = await RequireCloudUserIdAsync();
        var code = ToolCatalog.BuildCloudCode(userId, tool.Slug);
        var detail = await _jsonBins.GetByCodeAsync(code, cancellationToken)
            ?? throw new InvalidOperationException("Share bin not found after save.");

        var result = await _jsonBins.GenerateShareLinkAsync(
            detail.Id,
            DateTime.UtcNow.AddHours(4),
            cancellationToken);

        return _navigation.ToAbsoluteUri($"/public/json-bins/share/{result.Token}").ToString();
    }

    public async Task<bool> CanCloudWriteAsync()
    {
        return await GetCloudUserIdAsync() is not null;
    }

    private async Task<ToolStateEnvelope?> TryLoadCloudEnvelopeAsync(
        string userId,
        string toolSlug,
        CancellationToken cancellationToken)
    {
        var code = ToolCatalog.BuildCloudCode(userId, toolSlug);
        var detail = await _jsonBins.GetByCodeAsync(code, cancellationToken);
        if (string.IsNullOrWhiteSpace(detail?.ContentJson))
            return null;

        return DeserializeEnvelope(detail.ContentJson);
    }

    private async Task UpsertCloudAsync(
        ToolDefinition tool,
        string userId,
        ToolStateEnvelope envelope,
        CancellationToken cancellationToken,
        string? displayName = null)
    {
        var content = JsonSerializer.Serialize(envelope, JsonOptions);
        var code = ToolCatalog.BuildCloudCode(userId, tool.Slug);
        var name = displayName ?? BuildCloudDisplayName(tool);

        var existing = await _jsonBins.GetByCodeAsync(code, cancellationToken);
        if (existing is null)
        {
            await _jsonBins.CreateAsync(new CreateJsonBinModel
            {
                Code = code,
                Name = name,
                Category = ToolCatalog.JsonBinCategory,
                ContentJson = content,
                ContentType = "application/json",
                Tags = "devtools"
            }, cancellationToken);
        }
        else
        {
            await _jsonBins.UpdateAsync(existing.Id, new UpdateJsonBinModel
            {
                Code = code,
                Name = name,
                Category = ToolCatalog.JsonBinCategory,
                ContentJson = content,
                ContentType = "application/json",
                Tags = "devtools"
            }, cancellationToken);
        }
    }

    private static ToolStateEnvelope CreateEnvelope(string stateJson) =>
        new()
        {
            V = ToolCatalog.StateEnvelopeVersion,
            SavedAtUtc = DateTime.UtcNow,
            StateJson = stateJson ?? "{}"
        };

    private static string BuildCloudDisplayName(ToolDefinition tool) =>
        $"DevTools · {tool.Slug}";

    private async Task<string?> ReadLocalRawAsync(string slug) =>
        await _js.InvokeAsync<string?>("localStorage.getItem", ToolCatalog.LocalDraftKey(slug));

    private async Task WriteLocalRawAsync(string slug, string json) =>
        await _js.InvokeVoidAsync("localStorage.setItem", ToolCatalog.LocalDraftKey(slug), json);

    private async Task ClearLocalRawAsync(string slug) =>
        await _js.InvokeVoidAsync("localStorage.removeItem", ToolCatalog.LocalDraftKey(slug));

    private async Task<string> RequireCloudUserIdAsync()
    {
        var userId = await GetCloudUserIdAsync();
        if (userId is null)
            throw new InvalidOperationException("Sign in with JsonBin.Write permission required.");

        return userId;
    }

    private async Task<string?> GetCloudUserIdAsync()
    {
        var user = await GetUserAsync();
        if (user is null || !user.HasClaim("scope", PermissionNames.JsonBinWrite))
            return null;

        return GetUserId(user);
    }

    private async Task<ClaimsPrincipal?> GetUserAsync()
    {
        var state = await _authStateProvider.GetAuthenticationStateAsync();
        var user = state.User;
        return user.Identity?.IsAuthenticated == true ? user : null;
    }

    private static string? GetUserId(ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? user.FindFirst("sub")?.Value
        ?? user.FindFirst("uid")?.Value;

    private static ToolStateEnvelope? DeserializeEnvelope(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ToolStateEnvelope>(json, JsonOptions);
        }
        catch (JsonException)
        {
            return new ToolStateEnvelope { StateJson = json, SavedAtUtc = DateTime.UtcNow };
        }
    }
}
