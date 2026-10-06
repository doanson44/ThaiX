using MediatR;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// Command to update user profile information.
/// </summary>
public sealed record UpdateUserCommand : IAppCommand<Unit>
{
    /// <summary>
    /// User ID to update (immutable, used for lookup only).
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// New email address (must be unique).
    /// </summary>
    public string? Email { get; init; }

    /// <summary>
    /// New phone number.
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Confirm email after update (admin privilege).
    /// </summary>
    public bool? EmailConfirmed { get; init; }

    /// <summary>
    /// Enable/disable two-factor authentication.
    /// </summary>
    public bool? TwoFactorEnabled { get; init; }
}
