namespace ThaiX.Application.Common.Interfaces;

/// <summary>
/// Framework-agnostic abstraction for accessing current user context.
/// Implemented in Infrastructure layer.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Current user's unique identifier.
    /// </summary>
    Guid UserId { get; }

    /// <summary>
    /// Current user's email address.
    /// </summary>
    string Email { get; }

    /// <summary>
    /// Current user's display name or username.
    /// </summary>
    string UserName { get; }

    /// <summary>
    /// Indicates whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets all permissions for the current user.
    /// </summary>
    IReadOnlyCollection<string> GetPermissions();

    /// <summary>
    /// Checks if the current user has a specific permission.
    /// </summary>
    bool HasPermission(string permission);

    /// <summary>
    /// Checks if the current user has all specified permissions.
    /// </summary>
    bool HasAllPermissions(params string[] permissions);

    /// <summary>
    /// Checks if the current user has any of the specified permissions.
    /// </summary>
    bool HasAnyPermission(params string[] permissions);
}
