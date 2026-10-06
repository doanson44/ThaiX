namespace ThaiX.Application.Common.Models;

/// <summary>
/// Read model for current user profile (full name, avatar) from linked Contact.
/// Optimized for fast reads without joining Contacts.
/// </summary>
public class UserProfile
{
    /// <summary>
    /// Identity user ID (PK).
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Linked contact ID.
    /// </summary>
    public Guid ContactId { get; set; }

    /// <summary>
    /// Display full name (from Contact).
    /// </summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Avatar URL (from Contact).
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// When the projection was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
