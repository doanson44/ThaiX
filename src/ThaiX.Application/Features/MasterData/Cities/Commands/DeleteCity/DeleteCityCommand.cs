using MediatR;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.MasterData.Cities.Commands.DeleteCity;

/// <summary>
/// Command to soft-delete a city.
/// </summary>
public sealed record DeleteCityCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
