using MediatR;

namespace ThaiX.Application.Features.Games.Queries.GetTowerDefenseProgress;

public sealed record GetTowerDefenseProgressQuery(Guid UserId) : IRequest<TowerDefenseProgressDto?>;
