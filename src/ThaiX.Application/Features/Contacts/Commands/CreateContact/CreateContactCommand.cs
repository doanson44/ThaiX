using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.CreateContact;

/// <summary>
/// Command to create a new contact with basic profile information.
/// </summary>
[InvalidateCache(CacheGroups.Contacts)]
public sealed record CreateContactCommand : IAppCommand<Guid>
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string? AvatarUrl { get; init; }
    public DateOnly? Birthday { get; init; }
    public string? Notes { get; init; }
}
