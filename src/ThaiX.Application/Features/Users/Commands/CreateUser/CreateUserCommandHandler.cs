using MediatR;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Handler for CreateUserCommand.
/// Now lives in Application layer using IIdentityUserService abstraction.
/// </summary>
public sealed class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IIdentityUserService _identityUserService;
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(
        IIdentityUserService identityUserService,
        ILogger<CreateUserCommandHandler> logger)
    {
        _identityUserService = identityUserService;
        _logger = logger;
    }

    public async Task<Guid> Handle(
        CreateUserCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating user with email: {Email}",
            request.Email);

        var userId = await _identityUserService.CreateUserAsync(
            email: request.Email,
            password: request.Password,
            phoneNumber: request.PhoneNumber,
            permissions: request.Permissions,
            requirePasswordChange: request.RequirePasswordChange,
            emailConfirmed: !request.SendActivationEmail,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "User created successfully: {UserId} ({Email})",
            userId,
            request.Email);

        return userId;
    }
}
