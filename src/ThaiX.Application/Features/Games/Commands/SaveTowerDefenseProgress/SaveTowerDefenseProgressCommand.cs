using MediatR;
using ThaiX.Application.Features.Games;

namespace ThaiX.Application.Features.Games.Commands.SaveTowerDefenseProgress;

public sealed record SaveTowerDefenseProgressCommand(
    Guid UserId,
    TowerDefenseProgressDto Progress) : IRequest;