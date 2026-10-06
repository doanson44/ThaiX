using MediatR;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Users.Commands.SetUserLockout;

/// <summary>
/// Handler for SetUserLockoutCommand.
/// Now lives in Application layer using IIdentityUserService abstraction.
/// </summary>
public sealed class SetUserLockoutCommandHandler : IRequestHandler<SetUserLockoutCommand, Unit>
{
    private readonly IIdentityUserService _identityUserService;
    private readonly ILogger<SetUserLockoutCommandHandler> _logger;

    public SetUserLockoutCommandHandler(
        IIdentityUserService identityUserService,
        ILogger<SetUserLockoutCommandHandler> logger)
    {
        _identityUserService = identityUserService;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        SetUserLockoutCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Setting lockout for user {UserId}: IsLocked={IsLocked}, Duration={Duration}",
            request.UserId,
            request.IsLocked,
            request.LockoutDurationMinutes);

        await _identityUserService.SetUserLockoutAsync(
            userId: request.UserId,
            isLocked: request.IsLocked,
            lockoutDurationMinutes: request.LockoutDurationMinutes,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Lockout set successfully for user: {UserId}",
            request.UserId);

        return Unit.Value;
    }
}
