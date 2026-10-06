using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Application.Features.AssetPositions.Commands.CreateSavingPosition;

[InvalidateCache(CacheGroups.SavingPositions)]
public sealed record CreateSavingPositionCommand : IAppCommand<Guid>
{
    public required Guid PortfolioId { get; init; }
    public required string BankName { get; init; }
    public string? AccountNumber { get; init; }
    public required decimal PrincipalAmount { get; init; }
    public required decimal InterestRate { get; init; }
    public required InterestType InterestType { get; init; }
    public required DateOnly DepositDate { get; init; }
    public DateOnly? MaturityDate { get; init; }
    public string? Note { get; init; }
}
