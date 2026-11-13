using MediatR;

namespace ThaiX.Application.Features.Games.Commands.SaveTowerDefenseGold;

public sealed record SaveTowerDefenseGoldCommand(Guid UserId, int Gold) : IRequest;
