using MediatR;

namespace ThaiX.Application.Features.Games.Queries.GetTowerDefenseGold;

public sealed record GetTowerDefenseGoldQuery(Guid UserId) : IRequest<int?>;
