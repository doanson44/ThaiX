using MediatR;
using ThaiX.Application.Features.BotCommands.Common;

namespace ThaiX.Application.Features.BotCommands.Queries.GetBotCommandExecutionStatus;

public sealed class GetBotCommandExecutionStatusQueryHandler
    : IRequestHandler<GetBotCommandExecutionStatusQuery, BotAsyncExecutionStatusDto?>
{
    private readonly IBotAsyncExecutionService _asyncExecutionService;

    public GetBotCommandExecutionStatusQueryHandler(IBotAsyncExecutionService asyncExecutionService)
    {
        _asyncExecutionService = asyncExecutionService;
    }

    public Task<BotAsyncExecutionStatusDto?> Handle(
        GetBotCommandExecutionStatusQuery request,
        CancellationToken cancellationToken)
    {
        return _asyncExecutionService.GetStatusAsync(request.ExecutionId, cancellationToken);
    }
}
