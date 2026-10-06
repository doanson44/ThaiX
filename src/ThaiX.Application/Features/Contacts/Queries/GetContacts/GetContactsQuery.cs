using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Contacts.Queries.GetContacts;

/// <summary>
/// Query to retrieve a paginated list of contacts.
/// </summary>
public sealed record GetContactsQuery : PagedRequest, IAppQuery<PagedResult<ContactListItemDto>>, ICacheableQuery
{
    /// <summary>
    /// Optional search term (searches in name, email, phone, company).
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Filter by archived status. null = all, true = archived only, false = active only.
    /// </summary>
    public bool? IsArchived { get; init; }

    /// <summary>
    /// Filter by tag name.
    /// </summary>
    public string? Tag { get; init; }

    public string CacheKey => CacheKeys.Contacts.ListVersioned(SearchTerm, PageNumber, PageSize, SortBy, SortDescending, IsArchived, Tag);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(2);
    public string CacheGroup => CacheGroups.Contacts;
    public bool IsVersionedList => true;
}

/// <summary>
/// DTO for contact list items.
/// </summary>
public sealed record ContactListItemDto
{
    public required Guid Id { get; init; }
    public required string DisplayName { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? PrimaryEmail { get; init; }
    public string? PrimaryPhone { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string? AvatarUrl { get; init; }
    public bool IsArchived { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastUpdated { get; init; }
}
