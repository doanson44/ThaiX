using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.Portfolios;

namespace ThaiX.Application.Features.Portfolios.Queries.GetPortfolioDetail;

public sealed record GetPortfolioDetailQuery(Guid Id) : IAppQuery<PortfolioDetailDto?>;

public sealed record PortfolioDetailDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required PortfolioType PortfolioType { get; init; }
    public string? Description { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
