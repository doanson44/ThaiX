using ThaiX.Client.Models.Slack;

namespace ThaiX.Client.Services.Slack;

public interface IBotCommandTesterService
{
    Task<BotCommandExecutionDto> ExecuteCommandAsync(
        ExecuteBotCommandTesterRequest request,
        CancellationToken cancellationToken = default);
}
