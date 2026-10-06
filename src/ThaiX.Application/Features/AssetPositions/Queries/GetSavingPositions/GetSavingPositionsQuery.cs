using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Application.Features.AssetPositions.Queries.GetSavingPositions;

public sealed record GetSavingPositionsQuery : PagedRequest, IAppQuery<PagedResult<SavingPositionListItemDto>>, ICacheableQuery
{
    public required Guid PortfolioId { get; init; }
    public SavingStatus? Status { get; init; }

    public string CacheKey => CacheKeys.SavingPositions.List(PortfolioId, Status, PageNumber, PageSize);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.SavingPositions;
    public bool IsVersionedList => true;
}

public sealed record SavingPositionListItemDto
{
    public required Guid Id { get; init; }
    public required Guid PortfolioId { get; init; }
    public required string BankName { get; init; }
    public string? AccountNumber { get; init; }
    public required decimal PrincipalAmount { get; init; }
    public required decimal InterestRate { get; init; }
    public required InterestType InterestType { get; init; }
    public required DateOnly DepositDate { get; init; }
    public DateOnly? MaturityDate { get; init; }
    public required SavingStatus Status { get; init; }
    public string? Note { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
