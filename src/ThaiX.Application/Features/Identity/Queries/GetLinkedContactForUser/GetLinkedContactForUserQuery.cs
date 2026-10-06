using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.Identity.Dtos;

namespace ThaiX.Application.Features.Identity.Queries.GetLinkedContactForUser;

/// <summary>
/// Gets the contact linked to the given user (if any).
/// </summary>
public sealed record GetLinkedContactForUserQuery(Guid UserId) : IAppQuery<LinkedContactDto?>, ICacheableQuery
{
    public string CacheKey => CacheKeys.UserProfiles.LinkedContact(UserId);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.UserProfiles;
}
