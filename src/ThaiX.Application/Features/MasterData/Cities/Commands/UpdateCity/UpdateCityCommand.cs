using MediatR;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.MasterData.Cities.Commands.UpdateCity;

/// <summary>
/// Command to update an existing city.
/// </summary>
public sealed record UpdateCityCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string CountryCode { get; init; }
}
