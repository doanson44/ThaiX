using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Queries.GetContactById;

/// <summary>
/// Query to retrieve a single contact with all details.
/// </summary>
public sealed record GetContactByIdQuery : IAppQuery<ContactDetailDto>, ICacheableQuery
{
    public required Guid Id { get; init; }

    public string CacheKey => CacheKeys.Contacts.Projection(Id);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.Contacts;
}

/// <summary>
/// Full detail DTO for a contact.
/// </summary>
public sealed record ContactDetailDto
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string? AvatarUrl { get; init; }
    public DateOnly? Birthday { get; init; }
    public string? Notes { get; init; }
    public bool IsArchived { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public Dictionary<string, string> CustomFields { get; init; } = new();
    public List<ContactEmailDto> Emails { get; init; } = new();
    public List<ContactPhoneDto> Phones { get; init; } = new();
    public List<ContactAddressDto> Addresses { get; init; } = new();
    public List<ContactSocialLinkDto> SocialLinks { get; init; } = new();
    public List<ContactTagDto> Tags { get; init; } = new();
    public List<ContactBankAccountDto> BankAccounts { get; init; } = new();
    public List<ContactIdentityDocumentDto> IdentityDocuments { get; init; } = new();
    /// <summary>User linked to this contact (if any).</summary>
    public Guid? LinkedUserId { get; init; }
    /// <summary>Email of the linked user (for display).</summary>
    public string? LinkedUserEmail { get; init; }
}

public sealed record ContactEmailDto
{
    public required Guid Id { get; init; }
    public required string Value { get; init; }
    public bool IsPrimary { get; init; }
}

public sealed record ContactPhoneDto
{
    public required Guid Id { get; init; }
    public required string Value { get; init; }
    public bool IsPrimary { get; init; }
}

public sealed record ContactAddressDto
{
    public required Guid Id { get; init; }
    public required string Street { get; init; }
    public required string CountryCode { get; init; }
    public string? CityCode { get; init; }
    public string? DistrictCode { get; init; }
    public string? PostalCode { get; init; }
    public bool IsPrimary { get; init; }
}

public sealed record ContactSocialLinkDto
{
    public required Guid Id { get; init; }
    public required string Platform { get; init; }
    public required string Url { get; init; }
}

public sealed record ContactTagDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
}

public sealed record ContactBankAccountDto
{
    public required Guid Id { get; init; }
    public required string BankCode { get; init; }
    public string? BranchName { get; init; }
    public required string AccountNumberLast4 { get; init; }
    public required string AccountName { get; init; }
    public required string CurrencyCode { get; init; }
    public bool IsPrimary { get; init; }
    public bool IsVerified { get; init; }
}

public sealed record ContactIdentityDocumentDto
{
    public required Guid Id { get; init; }
    public required string DocumentType { get; init; }
    public required string DocumentNumberLast4 { get; init; }
    public required string IssuedBy { get; init; }
    public required string IssuedPlace { get; init; }
    public required DateOnly IssuedDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }
}
