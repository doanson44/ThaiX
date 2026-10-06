using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Identity.Commands.LinkUserToContact;

/// <summary>
/// Links an Identity user to a Contact and creates the initial UserProfile projection.
/// </summary>
[InvalidateCache(CacheGroups.UserProfiles)]
[InvalidateCache(CacheGroups.Suggestions)]
public sealed record LinkUserToContactCommand : IAppCommand<Unit>
{
    public Guid UserId { get; init; }
    public Guid ContactId { get; init; }
}
