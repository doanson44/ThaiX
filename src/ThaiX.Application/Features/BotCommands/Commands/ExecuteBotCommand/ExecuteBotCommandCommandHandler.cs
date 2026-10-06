using MediatR;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.BotCommands.Common;
using DomainPermissions = ThaiX.Domain.Common.Constants.Permissions;

namespace ThaiX.Application.Features.BotCommands.Commands.ExecuteBotCommand;

public sealed class ExecuteBotCommandCommandHandler
    : IRequestHandler<ExecuteBotCommandCommand, BotCommandExecutionDto>
{
    private readonly IBotCommandParser _parser;
    private readonly IBotCommandRegistry _registry;
    private readonly ICurrentUserService _currentUser;

    public ExecuteBotCommandCommandHandler(
        IBotCommandParser parser,
        IBotCommandRegistry registry,
        ICurrentUserService currentUser)
    {
        _parser = parser;
        _registry = registry;
        _currentUser = currentUser;
    }

    public async Task<BotCommandExecutionDto> Handle(
        ExecuteBotCommandCommand request,
        CancellationToken cancellationToken)
    {
        var parseResult = _parser.Parse(request.RawText);
        if (!parseResult.Success || parseResult.Command is null)
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Rejected,
                PlainText = parseResult.ErrorMessage ?? "Unable to parse command.",
                ErrorCode = ErrorCodes.INVALID_REQUEST
            };
        }

        if (!_registry.TryResolve(parseResult.Command.Name, out var module) || module is null)
        {
            var available = GetVisibleCommands();
            var suffix = available.Count == 0
                ? string.Empty
                : $" Available commands: {string.Join(", ", available.Select(x => "/" + x))}";

            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Rejected,
                PlainText = $"Unknown command '/{parseResult.Command.Name}'. Use /help.{suffix}",
                ErrorCode = ErrorCodes.INVALID_REQUEST
            };
        }

        if (!IsAuthorized(module.Metadata))
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Rejected,
                PlainText = "You do not have permission to execute this command.",
                ErrorCode = ErrorCodes.INSUFFICIENT_PERMISSIONS
            };
        }

        var context = new BotCommandContext
        {
            Channel = request.Channel,
            ExternalChannelId = request.ExternalChannelId,
            ParsedCommand = parseResult.Command
        };

        try
        {
            return await module.ExecuteAsync(context, cancellationToken);
        }
        catch (Exception ex)
        {
            return new BotCommandExecutionDto
            {
                Status = BotCommandExecutionStatus.Failed,
                PlainText = $"Command execution failed: {ex.Message}",
                ErrorCode = ErrorCodes.INTERNAL_ERROR
            };
        }
    }

    private bool IsAuthorized(BotCommandMetadata metadata)
    {
        if (metadata.RequiredPermissions.Count == 0)
        {
            return true;
        }

        // System M2M clients with BotCommand.Execute bypass per-command permission checks.
        // Endpoint-level authorization already verified they can execute bot commands.
        if (_currentUser.HasAllPermissions(DomainPermissions.BotCommandExecute))
        {
            return true;
        }

        return _currentUser.IsAuthenticated
               && _currentUser.HasAllPermissions(metadata.RequiredPermissions.ToArray());
    }

    private IReadOnlyList<string> GetVisibleCommands()
    {
        return _registry.GetAll()
            .Where(m => IsAuthorized(m.Metadata))
            .Select(m => m.Metadata.Name)
            .ToList();
    }
}
