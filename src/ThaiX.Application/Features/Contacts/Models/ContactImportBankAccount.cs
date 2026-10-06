namespace ThaiX.Application.Features.Contacts.Models;

/// <summary>
/// Bank account data for contact import. AccountNumber is plain; handler encrypts before storing.
/// </summary>
public sealed record ContactImportBankAccount
{
    public required string BankCode { get; init; }
    public string? BranchName { get; init; }
    public required string AccountNumber { get; init; }
    public required string AccountName { get; init; }
    public required string CurrencyCode { get; init; }
    public bool IsPrimary { get; init; }
}
