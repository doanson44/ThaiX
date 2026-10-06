using MediatR;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.MasterData.Districts.Commands.UpdateDistrict;

/// <summary>
/// Command to update an existing district.
/// </summary>
public sealed record UpdateDistrictCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string CityCode { get; init; }
}
