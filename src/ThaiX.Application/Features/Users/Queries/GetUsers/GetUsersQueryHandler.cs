using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Users.Queries.GetUsers;

/// <summary>
/// Handler for GetUsersQuery.
/// Now lives in Application layer using IIdentityUserService abstraction.
/// </summary>
public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserListItemDto>>
{
    private readonly IIdentityUserService _identityUserService;

    public GetUsersQueryHandler(IIdentityUserService identityUserService)
    {
        _identityUserService = identityUserService;
    }

    public async Task<PagedResult<UserListItemDto>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken)
    {
        var pagedResult = await _identityUserService.GetUsersAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            searchTerm: request.SearchTerm,
            isActive: request.IsActive,
            sortBy: request.SortBy,
            sortDescending: request.SortDescending,
            cancellationToken: cancellationToken);

        return pagedResult;
    }
}
