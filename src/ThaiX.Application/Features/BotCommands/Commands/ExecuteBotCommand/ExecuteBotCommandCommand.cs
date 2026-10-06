using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.BotCommands.Common;

namespace ThaiX.Application.Features.BotCommands.Commands.ExecuteBotCommand;

public sealed record ExecuteBotCommandCommand : IAppQuery<BotCommandExecutionDto>
{
    public required string RawText { get; init; }
    public required string Channel { get; init; }
    public required string ExternalChannelId { get; init; }
}
