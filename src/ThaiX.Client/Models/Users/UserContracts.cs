namespace ThaiX.Client.Models.Users;

public sealed record GetUsersRequest
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string SortBy { get; init; } = "email";
    public bool SortDescending { get; init; }
    public string? SearchTerm { get; init; }
    public bool? IsActive { get; init; }
}

public sealed record UserListItemDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required bool EmailConfirmed { get; init; }
    public required bool LockoutEnabled { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }
}

public sealed record CreateUserRequest
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public bool RequirePasswordChange { get; init; }
    public IReadOnlyCollection<string>? Permissions { get; init; }
    public string? PhoneNumber { get; init; }
    /// <summary>
    /// When true, send activation email (email remains unconfirmed).
    /// When false, do not send email and mark email as confirmed (user can log in immediately).
    /// </summary>
    public bool SendActivationEmail { get; init; }
}

public sealed record UpdateUserRequest
{
    public string? Email { get; init; }
    public string? PhoneNumber { get; init; }
    public bool? EmailConfirmed { get; init; }
    public bool? TwoFactorEnabled { get; init; }
}

public sealed record SetUserLockoutRequest
{
    public required bool IsLocked { get; init; }
    public string? Reason { get; init; }
    public int? LockoutDurationMinutes { get; init; }
}

public sealed record SetUserPermissionsRequest
{
    public required IReadOnlyCollection<string> Permissions { get; init; }
}

public sealed record ResetPasswordRequest
{
    public string? NewPassword { get; init; }
    public bool RequirePasswordChange { get; init; } = true;
}

public sealed record ResetPasswordResult
{
    public string? TemporaryPassword { get; init; }
    public required bool RequirePasswordChange { get; init; }
}

/// <summary>
/// Contact linked to a user (for display and unlink).
/// </summary>
public sealed record LinkedContactDto
{
    public required Guid ContactId { get; init; }
    public required string DisplayName { get; init; }
}

/// <summary>
/// Current user profile (FullName, AvatarUrl from linked contact).
/// </summary>
public sealed record UserProfileDto
{
    public string FullName { get; init; } = string.Empty;
    public string? AvatarUrl { get; init; }
    public string? ResumeSlug { get; init; }
}
