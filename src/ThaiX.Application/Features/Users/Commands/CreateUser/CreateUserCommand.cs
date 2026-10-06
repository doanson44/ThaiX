using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Users.Commands.CreateUser;

/// <summary>
/// Command to create a new user with initial permissions.
/// </summary>
public sealed record CreateUserCommand : IAppCommand<Guid>
{
    /// <summary>
    /// User email address (will be used as username).
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Initial password for the user.
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Optional: Force user to change password on first login.
    /// </summary>
    public bool RequirePasswordChange { get; init; }

    /// <summary>
    /// Optional: Initial permissions to assign.
    /// </summary>
    public IReadOnlyCollection<string>? Permissions { get; init; }

    /// <summary>
    /// Optional: Phone number.
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// When true, send activation email and leave email unconfirmed.
    /// When false, do not send email and set email as confirmed (user can log in immediately).
    /// </summary>
    public bool SendActivationEmail { get; init; }
}
