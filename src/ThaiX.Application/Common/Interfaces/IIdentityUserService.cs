using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.Identity.Dtos;
using ThaiX.Application.Features.Users.Commands.ResetPassword;
using ThaiX.Application.Features.Users.Queries.GetUsers;

namespace ThaiX.Application.Common.Interfaces;

/// <summary>
/// Framework-agnostic abstraction for user identity management.
/// Wraps ASP.NET Core Identity operations to maintain Application layer purity.
/// Implemented in Infrastructure layer.
/// </summary>
/// <remarks>
/// ARCHITECTURE NOTE:
/// This abstraction exists to keep Application layer free from Infrastructure dependencies.
/// All UserManager/SignInManager operations are encapsulated behind this interface.
/// This allows:
/// - Application layer handlers to remain framework-agnostic
/// - Easy testing via mocking
/// - Potential swapping of identity providers (Auth0, Azure AD B2C, etc.)
/// </remarks>
public interface IIdentityUserService
{
    #region User Creation & Management

    /// <summary>
    /// Creates a new user with email, password, and optional initial permissions.
    /// </summary>
    /// <param name="email">User's email address (also used as username)</param>
    /// <param name="password">Initial password</param>
    /// <param name="phoneNumber">Optional phone number</param>
    /// <param name="permissions">Optional initial permissions</param>
    /// <param name="requirePasswordChange">If true, user must change password on first login</param>
    /// <param name="emailConfirmed">If true, user's email is marked confirmed (no activation email). If false, send activation email.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The new user ID</returns>
    Task<Guid> CreateUserAsync(
        string email,
        string password,
        string? phoneNumber,
        IEnumerable<string>? permissions,
        bool requirePasswordChange,
        bool emailConfirmed,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates user profile information.
    /// </summary>
    /// <param name="userId">User ID to update</param>
    /// <param name="email">New email (null = no change)</param>
    /// <param name="phoneNumber">New phone number (null = no change)</param>
    /// <param name="emailConfirmed">Email confirmation status (null = no change)</param>
    /// <param name="twoFactorEnabled">2FA status (null = no change)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that completes when the operation succeeds</returns>
    Task UpdateUserAsync(
        Guid userId,
        string? email,
        string? phoneNumber,
        bool? emailConfirmed,
        bool? twoFactorEnabled,
        CancellationToken cancellationToken = default);

    #endregion

    #region Lockout Management

    /// <summary>
    /// Sets user lockout status (block/unblock).
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="isLocked">True to lock, false to unlock</param>
    /// <param name="lockoutDurationMinutes">Lockout duration (null = indefinite)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that completes when the operation succeeds</returns>
    Task SetUserLockoutAsync(
        Guid userId,
        bool isLocked,
        int? lockoutDurationMinutes,
        CancellationToken cancellationToken = default);

    #endregion

    #region Permission Management

    /// <summary>
    /// Sets user permissions (replaces existing permissions).
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="permissions">Complete list of permissions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that completes when the operation succeeds</returns>
    Task SetUserPermissionsAsync(
        Guid userId,
        IEnumerable<string> permissions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all permissions for a user.
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of permission strings</returns>
    Task<IReadOnlyCollection<string>> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that all permissions exist in the system.
    /// </summary>
    /// <param name="permissions">Permissions to validate</param>
    /// <returns>List of invalid permissions (empty if all valid)</returns>
    IReadOnlyCollection<string> ValidatePermissions(IEnumerable<string> permissions);

    #endregion

    #region Password Management

    /// <summary>
    /// Resets user password (admin operation).
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="newPassword">New password (null = generate random)</param>
    /// <param name="requirePasswordChange">Force password change on next login</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Optional generated password payload</returns>
    Task<ResetPasswordResult> ResetPasswordAsync(
        Guid userId,
        string? newPassword,
        bool requirePasswordChange,
        CancellationToken cancellationToken = default);

    #endregion

    #region Email Confirmation

    /// <summary>
    /// Generates an email confirmation token for a user.
    /// </summary>
    Task<string> GenerateEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Confirms a user's email address with a token.
    /// </summary>
    Task<bool> ConfirmEmailAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a user ID by email.
    /// </summary>
    Task<Guid?> FindUserIdByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    #endregion

    #region Password Reset (User-Initiated)

    /// <summary>
    /// Generates a password reset token for a user.
    /// </summary>
    Task<string> GeneratePasswordResetTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a user's password using a token (user-initiated flow).
    /// </summary>
    Task<bool> ResetPasswordWithTokenAsync(
        Guid userId,
        string token,
        string newPassword,
        CancellationToken cancellationToken = default);

    #endregion

    #region Two-Factor Authentication

    /// <summary>
    /// Gets the authenticator key for 2FA setup.
    /// </summary>
    Task<string> GetAuthenticatorKeyAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Verifies a 2FA token from an authenticator app.
    /// </summary>
    Task<bool> VerifyTwoFactorTokenAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates new recovery codes for 2FA.
    /// </summary>
    Task<IEnumerable<string>> GenerateNewTwoFactorRecoveryCodesAsync(
        Guid userId,
        int count,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets whether 2FA is enabled for a user.
    /// </summary>
    Task<bool> GetTwoFactorEnabledAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables or disables 2FA for a user.
    /// </summary>
    Task SetTwoFactorEnabledAsync(
        Guid userId,
        bool enabled,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts remaining recovery codes.
    /// </summary>
    Task<int> CountRecoveryCodesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets the authenticator key (disables current 2FA setup).
    /// </summary>
    Task ResetAuthenticatorKeyAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    #endregion

    #region External Logins

    /// <summary>
    /// Adds an external login to a user.
    /// </summary>
    Task<bool> AddExternalLoginAsync(
        Guid userId,
        string provider,
        string providerKey,
        string displayName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all external logins for a user.
    /// </summary>
    Task<IList<ExternalLoginDto>> GetExternalLoginsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an external login from a user.
    /// </summary>
    Task RemoveExternalLoginAsync(
        Guid userId,
        string provider,
        string providerKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a user ID by external login provider and key.
    /// </summary>
    Task<Guid?> FindUserIdByExternalLoginAsync(
        string provider,
        string providerKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether the user has a password set.
    /// </summary>
    Task<bool> HasPasswordAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Personal Data (GDPR)

    /// <summary>
    /// Gets all personal data for a user (for GDPR download).
    /// </summary>
    Task<Dictionary<string, string>> GetPersonalDataAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Permanently deletes a user account (GDPR right to erasure).
    /// </summary>
    Task DeleteUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    #endregion

    #region User Queries

    /// <summary>
    /// Gets paginated list of users with filtering and sorting.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    /// <param name="searchTerm">Search term (email/username)</param>
    /// <param name="isActive">Filter by active status (null = all)</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortDescending">Sort direction</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paged result of user DTOs</returns>
    Task<PagedResult<UserListItemDto>> GetUsersAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        bool? isActive,
        string? sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user exists by ID.
    /// </summary>
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user's email by ID (for display only).
    /// </summary>
    Task<string?> GetUserEmailAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email is already taken.
    /// </summary>
    Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users whose normalized phone matches one of the given normalized phone numbers.
    /// Excludes users already linked in UserContactLinks.
    /// </summary>
    /// <param name="normalizedPhones">Normalized phone values (digits only, same format as ContactPhone.NormalizedValue)</param>
    /// <param name="excludeUserIds">User IDs to exclude (e.g. already linked)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of suggested users (Id, Email, PhoneNumber)</returns>
    Task<IReadOnlyList<SuggestedUserDto>> GetUsersByNormalizedPhonesAsync(
        IReadOnlyList<string> normalizedPhones,
        IReadOnlyCollection<Guid> excludeUserIds,
        CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// DTO for external login information.
/// </summary>
public sealed record ExternalLoginDto
{
    public required string LoginProvider { get; init; }
    public required string ProviderDisplayName { get; init; }
    public required string ProviderKey { get; init; }
}
