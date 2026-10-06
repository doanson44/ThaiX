namespace ThaiX.Client.Models.Contacts;

/// <summary>
/// Initial data for the contact form dialog (e.g. when editing).
/// </summary>
public class ContactFormDialogInitialData
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public DateOnly? Birthday { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Initial data for edit mode; includes contact Id for update.
/// </summary>
public sealed class ContactFormDialogInitialDataWithId : ContactFormDialogInitialData
{
    public required Guid Id { get; init; }
}

/// <summary>
/// Result of the contact form dialog submit.
/// When EditId is null, parent should create contact and optionally add children.
/// When EditId is set, parent should update contact profile only.
/// </summary>
public sealed record ContactFormDialogResult
{
    public Guid? EditId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string? AvatarUrl { get; init; }
    public DateOnly? Birthday { get; init; }
    public string? Notes { get; init; }
    public string? Email { get; init; }
    public bool EmailIsPrimary { get; init; } = true;
    public string? Phone { get; init; }
    public bool PhoneIsPrimary { get; init; } = true;
    public ContactFormAddressPart? Address { get; init; }
    public ContactFormIdentityPart? Identity { get; init; }
    public ContactFormBankPart? Bank { get; init; }
    public IReadOnlyList<string>? Tags { get; init; }
    public IReadOnlyList<ContactFormSocialLinkPart>? SocialLinks { get; init; }
    public IReadOnlyList<ContactFormCustomFieldPart>? CustomFields { get; init; }
}

public sealed record ContactFormSocialLinkPart(string Platform, string Url);

public sealed record ContactFormCustomFieldPart(string Key, string Value);

public sealed record ContactFormAddressPart
{
    public required string Street { get; init; }
    public required string CountryCode { get; init; }
    public string CityCode { get; init; } = string.Empty;
    public string DistrictCode { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
}

public sealed record ContactFormIdentityPart
{
    public required string DocumentType { get; init; }
    public required string DocumentNumber { get; init; }
    public string IssuedBy { get; init; } = string.Empty;
    public string IssuedPlace { get; init; } = string.Empty;
    public required DateOnly IssuedDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }
    public bool IsPrimary { get; init; }
}

public sealed record ContactFormBankPart
{
    public required string BankCode { get; init; }
    public string? BranchName { get; init; }
    public required string AccountNumber { get; init; }
    public required string AccountName { get; init; }
    public required string CurrencyCode { get; init; }
    public bool IsPrimary { get; init; }
}
