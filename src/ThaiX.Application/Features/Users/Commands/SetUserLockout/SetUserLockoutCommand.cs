using MediatR;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Users.Commands.SetUserLockout;

/// <summary>
/// Command to set user lockout status (block/unblock).
/// Single command pattern: IsLocked flag determines block or unblock.
/// </summary>
public sealed record SetUserLockoutCommand : IAppCommand<Unit>
{
    /// <summary>
    /// User ID to modify lockout status.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Lockout status: true = blocked, false = unblocked.
    /// </summary>
    public required bool IsLocked { get; init; }

    /// <summary>
    /// Optional reason for lockout (required when IsLocked = true).
    /// </summary>
    public string? Reason { get; init; }

    /// <summary>
    /// Optional lockout duration in minutes (null = permanent until manual unblock).
    /// </summary>
    public int? LockoutDurationMinutes { get; init; }
}
