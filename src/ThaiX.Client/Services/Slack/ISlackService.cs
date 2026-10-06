using ThaiX.Client.Models.Slack;

namespace ThaiX.Client.Services.Slack;

public interface ISlackService
{
    Task<IReadOnlyList<ChannelInfoDto>> GetCommandChannelsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ChannelInfoDto>> GetTelegramChannelsAsync(CancellationToken cancellationToken = default);

    Task<BotCommandExecutionDto> ExecuteCommandAsync(
        ExecuteSlackBotCommandRequest request,
        CancellationToken cancellationToken = default);

    Task<BotAsyncExecutionStatusDto> GetCommandExecutionStatusAsync(
        string executionId,
        CancellationToken cancellationToken = default);
}
