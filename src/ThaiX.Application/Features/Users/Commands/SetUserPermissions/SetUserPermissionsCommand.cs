using MediatR;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Users.Commands.SetUserPermissions;

/// <summary>
/// Command to set user permissions.
/// Replaces ALL existing permissions with the provided list.
/// </summary>
public sealed record SetUserPermissionsCommand : IAppCommand<Unit>
{
    /// <summary>
    /// User ID to modify permissions.
    /// </summary>
    public required Guid UserId { get; init; }

    /// <summary>
    /// Complete list of permissions to assign to the user.
    /// Existing permissions will be replaced.
    /// </summary>
    public required IReadOnlyCollection<string> Permissions { get; init; }
}
