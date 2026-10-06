namespace ThaiX.Domain.Aggregates.CredentialAccounts;

public enum CredentialAccountAuditAction
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
    PasswordViewed = 4,
    PasswordCopied = 5,
    PasswordChanged = 6,
    MarkedUsed = 7,
    ResetUsed = 8
}
