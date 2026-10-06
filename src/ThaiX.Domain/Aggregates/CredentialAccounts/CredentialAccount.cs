using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.CredentialAccounts;

public sealed class CredentialAccount : BaseAuditableEntity
{
    private CredentialAccount()
    {
    }

    public string Username { get; private set; } = null!;

    public string PasswordEncrypted { get; private set; } = null!;

    public string? Description { get; private set; }

    public bool IsUsed { get; private set; }

    public DateTime? UsedAt { get; private set; }

    public string? UsedBy { get; private set; }

    public int UsageCount { get; private set; }

    public static CredentialAccount Create(
        string username,
        string passwordEncrypted,
        string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordEncrypted);

        return new CredentialAccount
        {
            Id = Guid.NewGuid(),
            Username = username.Trim(),
            PasswordEncrypted = passwordEncrypted.Trim(),
            Description = NormalizeOptional(description),
            IsUsed = false,
            UsageCount = 0
        };
    }

    public void UpdateInformation(string username, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);

        Username = username.Trim();
        Description = NormalizeOptional(description);
    }

    public void UpdatePassword(string encryptedPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedPassword);
        PasswordEncrypted = encryptedPassword.Trim();
    }

    public void MarkUsed(string userName, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userName);

        IsUsed = true;
        UsedAt = utcNow;
        UsedBy = userName.Trim();
        UsageCount++;
    }

    public void ResetUsage()
    {
        IsUsed = false;
        UsedAt = null;
        UsedBy = null;
    }

    public void SoftDelete()
    {
        Delete();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
