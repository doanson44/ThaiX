using MediatR;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Users.Commands.SetUserPermissions;

/// <summary>
/// Handler for SetUserPermissionsCommand.
/// Now lives in Application layer using IIdentityUserService abstraction.
/// </summary>
public sealed class SetUserPermissionsCommandHandler : IRequestHandler<SetUserPermissionsCommand, Unit>
{
    private readonly IIdentityUserService _identityUserService;
    private readonly ILogger<SetUserPermissionsCommandHandler> _logger;

    public SetUserPermissionsCommandHandler(
        IIdentityUserService identityUserService,
        ILogger<SetUserPermissionsCommandHandler> logger)
    {
        _identityUserService = identityUserService;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        SetUserPermissionsCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Setting permissions for user {UserId}: [{Permissions}]",
            request.UserId,
            string.Join(", ", request.Permissions));

        await _identityUserService.SetUserPermissionsAsync(
            userId: request.UserId,
            permissions: request.Permissions,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Permissions set successfully for user: {UserId}",
            request.UserId);

        return Unit.Value;
    }
}
