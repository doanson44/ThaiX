using MediatR;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// Handler for UpdateUserCommand.
/// Now lives in Application layer using IIdentityUserService abstraction.
/// </summary>
public sealed class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
{
    private readonly IIdentityUserService _identityUserService;
    private readonly ILogger<UpdateUserCommandHandler> _logger;

    public UpdateUserCommandHandler(
        IIdentityUserService identityUserService,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _identityUserService = identityUserService;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        UpdateUserCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating user: {UserId}",
            request.UserId);

        await _identityUserService.UpdateUserAsync(
            userId: request.UserId,
            email: request.Email,
            phoneNumber: request.PhoneNumber,
            emailConfirmed: request.EmailConfirmed,
            twoFactorEnabled: request.TwoFactorEnabled,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "User updated successfully: {UserId}",
            request.UserId);

        return Unit.Value;
    }
}
