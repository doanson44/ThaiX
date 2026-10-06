using MediatR;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.MasterData.Districts.Commands.DeleteDistrict;

/// <summary>
/// Command to soft-delete a district.
/// </summary>
public sealed record DeleteDistrictCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
