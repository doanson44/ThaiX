using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Users.Queries.GetUserPermissions;

/// <summary>
/// Query to retrieve the current permissions assigned to a specific user.
/// </summary>
public sealed record GetUserPermissionsQuery(Guid UserId) : IAppQuery<IReadOnlyCollection<string>>;
