using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.Identity.Dtos;

namespace ThaiX.Application.Features.Identity.Queries.SuggestUsersByContactPhones;

/// <summary>
/// Suggests users to link to a contact by matching contact phone numbers to ApplicationUser.PhoneNumber.
/// Excludes users already linked in UserContactLinks.
/// </summary>
public sealed record SuggestUsersByContactPhonesQuery(Guid ContactId) : IAppQuery<IReadOnlyList<SuggestedUserDto>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.Suggestions.UsersByPhone(ContactId);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(2);
    public string CacheGroup => CacheGroups.Suggestions;
}
