using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.CredentialAccounts;

public sealed class CredentialAccountAudit : BaseEntity
{
    private CredentialAccountAudit()
    {
    }

    public Guid CredentialAccountId { get; private set; }

    public CredentialAccountAuditAction Action { get; private set; }

    public Guid UserId { get; private set; }

    public string UserName { get; private set; } = null!;

    public static CredentialAccountAudit Create(
        Guid credentialAccountId,
        CredentialAccountAuditAction action,
        Guid userId,
        string userName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        return new CredentialAccountAudit
        {
            Id = Guid.NewGuid(),
            CredentialAccountId = credentialAccountId,
            Action = action,
            UserId = userId,
            UserName = userName.Trim()
        };
    }
}
