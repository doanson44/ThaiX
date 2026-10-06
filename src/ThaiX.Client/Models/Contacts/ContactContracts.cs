namespace ThaiX.Client.Models.Contacts;

/// <summary>
/// DTO for contact list item (matches API response).
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

/// <summary>
/// DTO for contact detail (matches API response).
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
    public IReadOnlyList<ContactEmailDto> Emails { get; init; } = [];
    public IReadOnlyList<ContactPhoneDto> Phones { get; init; } = [];
    public IReadOnlyList<ContactAddressDto> Addresses { get; init; } = [];
    public IReadOnlyList<ContactSocialLinkDto> SocialLinks { get; init; } = [];
    public IReadOnlyList<ContactTagDto> Tags { get; init; } = [];
    public IReadOnlyList<ContactBankAccountDto> BankAccounts { get; init; } = [];
    public IReadOnlyList<ContactIdentityDocumentDto> IdentityDocuments { get; init; } = [];
    public Guid? LinkedUserId { get; init; }
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

/// <summary>
/// Query parameters for contact list endpoint.
/// </summary>
public sealed record ContactListRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string SortBy { get; init; } = "lastname";
    public bool SortDescending { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsArchived { get; init; }
    public string? Tag { get; init; }
}

/// <summary>
/// Query parameters for contact export endpoint.
/// </summary>
public sealed record ContactExportRequest
{
    public string? SearchTerm { get; init; }
    public bool? IsArchived { get; init; }
    public string? Tag { get; init; }
}

/// <summary>
/// Create contact request.
/// </summary>
public sealed record CreateContactRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string? AvatarUrl { get; init; }
    public DateOnly? Birthday { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Update contact profile request.
/// </summary>
public sealed record UpdateContactProfileRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string? AvatarUrl { get; init; }
    public DateOnly? Birthday { get; init; }
    public string? Notes { get; init; }
}

public sealed record ArchiveContactRequest
{
    public required bool Archive { get; init; }
}

public sealed record AddEmailRequest
{
    public required string Value { get; init; }
    public bool IsPrimary { get; init; }
}

public sealed record AddPhoneRequest
{
    public required string Value { get; init; }
    public bool IsPrimary { get; init; }
}

public sealed record AddAddressRequest
{
    public required string Street { get; init; }
    public required string CountryCode { get; init; }
    public string CityCode { get; init; } = string.Empty;
    public string DistrictCode { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
}

public sealed record AddSocialLinkRequest
{
    public required string Platform { get; init; }
    public required string Url { get; init; }
}

public sealed record AddTagRequest
{
    public required string Name { get; init; }
}

public sealed record AddBankAccountRequest
{
    public required string BankCode { get; init; }
    public string? BranchName { get; init; }
    public required string AccountNumber { get; init; }
    public required string AccountName { get; init; }
    public required string CurrencyCode { get; init; }
    public bool IsPrimary { get; init; }
}

public sealed record AddIdentityDocumentRequest
{
    public required string DocumentType { get; init; }
    public required string DocumentNumber { get; init; }
    public required string IssuedBy { get; init; }
    public required string IssuedPlace { get; init; }
    public required DateOnly IssuedDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }
}

public sealed record SetCustomFieldRequest
{
    public required string Key { get; init; }
    public required string Value { get; init; }
}

/// <summary>
/// File response for contact export endpoint.
/// </summary>
public sealed record ContactExportFileResult
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required byte[] FileContent { get; init; }
}

/// <summary>
/// Result of CSV import operation.
/// </summary>
public sealed record ContactImportResultDto
{
    public int TotalRows { get; init; }
    public int InsertedCount { get; init; }
    public int UpdatedCount { get; init; }
    public int SkippedCount { get; init; }
    public required IReadOnlyList<ContactImportRowErrorDto> Errors { get; init; }
}

public sealed record ContactImportRowErrorDto
{
    public int RowNumber { get; init; }
    public string? RawData { get; init; }
    public required string Error { get; init; }
}

/// <summary>
/// Status of an asynchronous contact CSV import job.
/// </summary>
public sealed record ContactImportJobDto
{
    public Guid Id { get; init; }
    public string Status { get; init; } = string.Empty;
    public int ProgressPercent { get; init; }
    public ContactImportResultDto? Result { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? CompletedAtUtc { get; init; }
}

/// <summary>
/// DTO for user suggested for linking to a contact (phone number match).
/// </summary>
public sealed record SuggestedUserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public string? PhoneNumber { get; init; }
}
