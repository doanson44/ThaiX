using MediatR;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.MasterData.Countries.Commands.UpdateCountry;

/// <summary>
/// Command to update an existing country.
/// </summary>
public sealed record UpdateCountryCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
}
