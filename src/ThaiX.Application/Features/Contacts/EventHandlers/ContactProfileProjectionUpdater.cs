using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Common.Events;

namespace ThaiX.Application.Features.Contacts.EventHandlers;

/// <summary>
/// Updates the UserProfile read model when a contact's profile (name/avatar) changes.
/// Invoked via Outbox -> Hangfire -> MediatR when ContactProfileUpdatedEvent is processed.
/// </summary>
public sealed class ContactProfileProjectionUpdater : INotificationHandler<ContactProfileUpdatedEvent>
{
    private readonly IApplicationDbContext _dbContext;

    public ContactProfileProjectionUpdater(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ContactProfileUpdatedEvent notification, CancellationToken cancellationToken)
    {
        var profiles = await _dbContext.UserProfiles
            .Where(p => p.ContactId == notification.ContactId)
            .ToListAsync(cancellationToken);

        foreach (var profile in profiles)
        {
            profile.FullName = notification.FullName;
            profile.AvatarUrl = notification.AvatarUrl;
            profile.UpdatedAt = DateTime.UtcNow;
        }

        if (profiles.Count > 0)
            await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
