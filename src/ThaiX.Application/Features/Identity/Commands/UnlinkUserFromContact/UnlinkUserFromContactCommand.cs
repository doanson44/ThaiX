using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Identity.Commands.UnlinkUserFromContact;

/// <summary>
/// Unlinks the user currently linked to the given contact. Removes UserContactLink and UserProfile.
/// </summary>
[InvalidateCache(CacheGroups.UserProfiles)]
[InvalidateCache(CacheGroups.Suggestions)]
public sealed record UnlinkUserFromContactCommand(Guid ContactId) : IAppCommand<Unit>;
