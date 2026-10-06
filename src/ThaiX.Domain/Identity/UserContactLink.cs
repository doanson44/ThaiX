using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Identity;

/// <summary>
/// Links an Identity user to a Contact for profile projection.
/// One-to-one: unique UserId and unique ContactId.
/// </summary>
public sealed class UserContactLink : BaseEntity
{
    /// <summary>
    /// Identity user ID (ApplicationUser.Id).
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Contact ID.
    /// </summary>
    public Guid ContactId { get; private set; }

    private UserContactLink() { }

    /// <summary>
    /// Creates a link between a user and a contact.
    /// </summary>
    public static UserContactLink Create(Guid userId, Guid contactId)
    {
        return new UserContactLink
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ContactId = contactId
        };
    }
}
