using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Users.Queries.GetPermissions;

/// <summary>
/// Query to retrieve all available permission codes (for dropdowns, assignment UI).
/// </summary>
public sealed record GetPermissionsQuery : IAppQuery<IReadOnlyCollection<string>>;
