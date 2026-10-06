using System.Net.Http.Json;
using ThaiX.Client.Models.Slack;
using ThaiX.Client.Services.Api;

namespace ThaiX.Client.Services.Slack;

public sealed class BotCommandTesterService : IBotCommandTesterService
{
    private readonly HttpClient _httpClient;

    public BotCommandTesterService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<BotCommandExecutionDto> ExecuteCommandAsync(
        ExecuteBotCommandTesterRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            "api/bot-commands/execute", request, cancellationToken);

        return await ApiResponseReader.ReadSuccessDataAsync<BotCommandExecutionDto>(response, cancellationToken);
    }
}
