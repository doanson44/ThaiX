using MediatR;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Users.Commands.ResetPassword;

/// <summary>
/// Handler for ResetPasswordCommand.
/// Now lives in Application layer using IIdentityUserService abstraction.
/// </summary>
public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ResetPasswordResult>
{
    private readonly IIdentityUserService _identityUserService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IIdentityUserService identityUserService,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _identityUserService = identityUserService;
        _logger = logger;
    }

    public async Task<ResetPasswordResult> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Resetting password for user: {UserId}",
            request.UserId);

        var resetResult = await _identityUserService.ResetPasswordAsync(
            userId: request.UserId,
            newPassword: request.NewPassword,
            requirePasswordChange: request.RequirePasswordChange,
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Password reset successfully for user: {UserId}. System-generated: {IsSystemGenerated}",
            request.UserId,
            resetResult.TemporaryPassword != null);

        return resetResult;
    }
}
