using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.Identity.Dtos;

namespace ThaiX.Application.Features.Identity.Queries.GetCurrentUserProfile;

/// <summary>
/// Gets the current user's profile (FullName, AvatarUrl) from the UserProfile projection.
/// </summary>
public sealed record GetCurrentUserProfileQuery(Guid UserId) : IAppQuery<UserProfileDto?>, ICacheableQuery
{
    public string CacheKey => CacheKeys.UserProfiles.Profile(UserId);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.UserProfiles;
}
