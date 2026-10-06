using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Users.Queries.GetUsers;

/// <summary>
/// Query to retrieve a paginated list of users.
/// Example implementation demonstrating pagination infrastructure.
/// </summary>
public sealed record GetUsersQuery : PagedRequest, IAppQuery<PagedResult<UserListItemDto>>
{
    /// <summary>
    /// Optional search term to filter users by name or email.
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Optional filter by active status.
    /// </summary>
    public bool? IsActive { get; init; }
}

/// <summary>
/// DTO for user list item.
/// </summary>
public sealed record UserListItemDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required bool EmailConfirmed { get; init; }
    public required bool LockoutEnabled { get; init; }
    public DateTimeOffset? LockoutEnd { get; init; }
}
