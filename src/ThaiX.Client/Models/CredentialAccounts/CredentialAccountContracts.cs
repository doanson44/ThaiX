namespace ThaiX.Client.Models.CredentialAccounts;

public class CredentialAccountDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsUsed { get; init; }
    public DateTime? UsedAt { get; init; }
    public string? UsedBy { get; init; }
    public int UsageCount { get; init; }
    public string LastUsedAgo { get; init; } = string.Empty;
}

public sealed class CredentialAccountDetailDto : CredentialAccountDto
{
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed class CredentialAccountPasswordDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public sealed class CredentialAccountAuditDto
{
    public Guid Id { get; init; }
    public Guid CredentialAccountId { get; init; }
    public string Action { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}

public sealed class CredentialAccountsListRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public bool? IsUsed { get; set; }
}

public sealed class CredentialAccountAuditListRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class CreateCredentialAccountRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class UpdateCredentialAccountRequest
{
    public string Username { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public sealed class ChangeCredentialAccountPasswordRequest
{
    public string Password { get; set; } = string.Empty;
}
