using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.MasterData.Countries.Commands.CreateCountry;

/// <summary>
/// Command to create a new country.
/// </summary>
public sealed record CreateCountryCommand : IAppCommand<Guid>
{
    public required string Code { get; init; }
    public required string Name { get; init; }
}
