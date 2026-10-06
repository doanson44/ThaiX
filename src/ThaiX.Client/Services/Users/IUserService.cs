using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.Users;

namespace ThaiX.Client.Services.Users;

public interface IUserService
{
    Task<PagedApiResponse<UserListItemDto>> GetUsersAsync(
        GetUsersRequest request,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetPermissionsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Guid> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task SetLockoutAsync(Guid userId, SetUserLockoutRequest request, CancellationToken cancellationToken = default);
    Task SetPermissionsAsync(Guid userId, SetUserPermissionsRequest request, CancellationToken cancellationToken = default);
    Task<ResetPasswordResult> ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken cancellationToken = default);

    Task<LinkedContactDto?> GetLinkedContactAsync(Guid userId, CancellationToken cancellationToken = default);
    Task UnlinkContactFromUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets current user profile (FullName, AvatarUrl from linked contact). Returns null if not authenticated or no profile.
    /// </summary>
    Task<UserProfileDto?> GetCurrentUserProfileAsync(CancellationToken cancellationToken = default);
}
