using MediatR;
using ThaiX.Domain.Common.Constants;

namespace ThaiX.Application.Features.Users.Queries.GetPermissions;

/// <summary>
/// Returns all permissions defined in the domain.
/// </summary>
public sealed class GetPermissionsQueryHandler : IRequestHandler<GetPermissionsQuery, IReadOnlyCollection<string>>
{
    public Task<IReadOnlyCollection<string>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
    {
        var permissions = Permissions.GetAll();
        return Task.FromResult(permissions);
    }
}
