namespace ThaiX.Application.Features.Contacts.Models;

/// <summary>
/// Full aggregate snapshot for contact import. Not a domain entity.
/// Represents the complete desired state for upsert/synchronization.
/// </summary>
public sealed record ContactImportRow
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public IReadOnlyList<string> Emails { get; init; } = [];
    public IReadOnlyList<string> Phones { get; init; } = [];
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public DateOnly? Birthday { get; init; }
    public string? Notes { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = [];
    public IReadOnlyList<ContactImportAddress> Addresses { get; init; } = [];
    public IReadOnlyList<ContactImportSocialLink> SocialLinks { get; init; } = [];
    public IReadOnlyList<ContactImportBankAccount> BankAccounts { get; init; } = [];
    public ContactImportIdentityDocument? IdentityDocument { get; init; }
}
