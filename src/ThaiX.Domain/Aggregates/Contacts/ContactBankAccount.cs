namespace ThaiX.Domain.Aggregates.Contacts;

/// <summary>
/// Represents a bank account belonging to a contact.
/// Account number is stored encrypted; only the last 4 digits are in plain text.
/// References bank/branch codes by value, not FK.
/// Owned by the Contact aggregate.
/// </summary>
public sealed class ContactBankAccount
{
    public Guid Id { get; private set; }
    public Guid ContactId { get; private set; }
    public string BankCode { get; private set; } = string.Empty;
    public string? BranchName { get; private set; }
    public string EncryptedAccountNumber { get; private set; } = string.Empty;
    public string AccountNumberLast4 { get; private set; } = string.Empty;
    public string AccountName { get; private set; } = string.Empty;
    public string CurrencyCode { get; private set; } = string.Empty;
    public bool IsPrimary { get; private set; }
    public bool IsVerified { get; private set; }

    // Private constructor for EF Core
    private ContactBankAccount() { }

    public static ContactBankAccount Create(
        Guid contactId,
        string bankCode,
        string? branchName,
        string encryptedAccountNumber,
        string accountNumberLast4,
        string accountName,
        string currencyCode,
        bool isPrimary)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bankCode, nameof(bankCode));
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedAccountNumber, nameof(encryptedAccountNumber));
        ArgumentException.ThrowIfNullOrWhiteSpace(accountNumberLast4, nameof(accountNumberLast4));
        ArgumentException.ThrowIfNullOrWhiteSpace(accountName, nameof(accountName));
        ArgumentException.ThrowIfNullOrWhiteSpace(currencyCode, nameof(currencyCode));

        return new ContactBankAccount
        {
            Id = Guid.NewGuid(),
            ContactId = contactId,
            BankCode = bankCode.Trim().ToUpperInvariant(),
            BranchName = branchName?.Trim(),
            EncryptedAccountNumber = encryptedAccountNumber,
            AccountNumberLast4 = accountNumberLast4,
            AccountName = accountName.Trim(),
            CurrencyCode = currencyCode.Trim().ToUpperInvariant(),
            IsPrimary = isPrimary,
            IsVerified = false
        };
    }

    internal void SetPrimary(bool isPrimary) => IsPrimary = isPrimary;

    internal void Verify() => IsVerified = true;

    internal void Unverify() => IsVerified = false;
}
