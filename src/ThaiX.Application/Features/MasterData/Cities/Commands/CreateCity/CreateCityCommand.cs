using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.MasterData.Cities.Commands.CreateCity;

/// <summary>
/// Command to create a new city.
/// </summary>
public sealed record CreateCityCommand : IAppCommand<Guid>
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string CountryCode { get; init; }
}
