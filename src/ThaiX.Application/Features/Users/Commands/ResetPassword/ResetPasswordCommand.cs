using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Users.Commands.ResetPassword;

/// <summary>
/// Command to reset user password (admin operation).
/// Generates a new temporary password and optionally forces password change on next login.
/// </summary>
public sealed record ResetPasswordCommand : IAppCommand<ResetPasswordResult>
{
    /// <summary>
    /// User ID whose password will be reset.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// New temporary password (if not provided, system generates one).
    /// </summary>
    public string? NewPassword { get; init; }

    /// <summary>
    /// Force user to change password on next login.
    /// </summary>
    public bool RequirePasswordChange { get; init; } = true;
}

/// <summary>
/// Result of password reset operation.
/// </summary>
public sealed record ResetPasswordResult
{
    /// <summary>
    /// Temporary password (returned only if system-generated).
    /// </summary>
    public string? TemporaryPassword { get; init; }

    /// <summary>
    /// Indicates if user must change password on next login.
    /// </summary>
    public required bool RequirePasswordChange { get; init; }
}
