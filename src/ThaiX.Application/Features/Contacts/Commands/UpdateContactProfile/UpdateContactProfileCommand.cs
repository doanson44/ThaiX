using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.UpdateContactProfile;

/// <summary>
/// Command to update a contact's profile fields.
/// </summary>
[InvalidateCache(CacheGroups.Contacts)]
[InvalidateCache(CacheGroups.UserProfiles)]
public sealed record UpdateContactProfileCommand : IAppCommand<Guid>
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string? AvatarUrl { get; init; }
    public DateOnly? Birthday { get; init; }
    public string? Notes { get; init; }
}
