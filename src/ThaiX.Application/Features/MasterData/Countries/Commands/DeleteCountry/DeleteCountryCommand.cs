using MediatR;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.MasterData.Countries.Commands.DeleteCountry;

/// <summary>
/// Command to soft-delete a country.
/// </summary>
public sealed record DeleteCountryCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
