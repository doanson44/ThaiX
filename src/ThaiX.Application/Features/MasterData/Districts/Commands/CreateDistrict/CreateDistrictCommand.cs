using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.MasterData.Districts.Commands.CreateDistrict;

/// <summary>
/// Command to create a new district.
/// </summary>
public sealed record CreateDistrictCommand : IAppCommand<Guid>
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string CityCode { get; init; }
}
