using System.Net.Http.Json;
using ThaiX.Client.Models.Slack;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.Slack;

public sealed class SlackService : ISlackService
{
    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public SlackService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<IReadOnlyList<ChannelInfoDto>> GetCommandChannelsAsync(CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.SlackChannels,
            "all",
            async ct =>
            {
                using var response = await _httpClient.GetAsync("api/slack/commands/channels", ct);
                return await ApiResponseReader.ReadSuccessDataAsync<IReadOnlyList<ChannelInfoDto>>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public Task<IReadOnlyList<ChannelInfoDto>> GetTelegramChannelsAsync(CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.TelegramChannels,
            "all",
            async ct =>
            {
                using var response = await _httpClient.GetAsync("api/telegram/commands/channels", ct);
                return await ApiResponseReader.ReadSuccessDataAsync<IReadOnlyList<ChannelInfoDto>>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public async Task<BotCommandExecutionDto> ExecuteCommandAsync(
        ExecuteSlackBotCommandRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync("api/slack/commands/execute", request, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<BotCommandExecutionDto>(response, cancellationToken);
    }

    public async Task<BotAsyncExecutionStatusDto> GetCommandExecutionStatusAsync(
        string executionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(executionId))
        {
            throw new ArgumentException("Execution id is required.", nameof(executionId));
        }

        var encodedExecutionId = Uri.EscapeDataString(executionId.Trim());
        using var response = await _httpClient.GetAsync($"api/slack/commands/executions/{encodedExecutionId}", cancellationToken);

        return await ApiResponseReader.ReadSuccessDataAsync<BotAsyncExecutionStatusDto>(response, cancellationToken);
    }
}
