using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.BotCommands.Common;

namespace ThaiX.Application.Features.BotCommands.Queries.GetBotCommandExecutionStatus;

public sealed record GetBotCommandExecutionStatusQuery : IAppQuery<BotAsyncExecutionStatusDto?>
{
    public required string ExecutionId { get; init; }
}
