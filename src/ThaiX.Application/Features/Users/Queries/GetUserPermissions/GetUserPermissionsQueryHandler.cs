using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Users.Queries.GetUserPermissions;

/// <summary>
/// Handler for GetUserPermissionsQuery.
/// </summary>
public sealed class GetUserPermissionsQueryHandler : IRequestHandler<GetUserPermissionsQuery, IReadOnlyCollection<string>>
{
    private readonly IIdentityUserService _identityUserService;

    public GetUserPermissionsQueryHandler(IIdentityUserService identityUserService)
    {
        _identityUserService = identityUserService;
    }

    public async Task<IReadOnlyCollection<string>> Handle(
        GetUserPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        return await _identityUserService.GetUserPermissionsAsync(request.UserId, cancellationToken);
    }
}
