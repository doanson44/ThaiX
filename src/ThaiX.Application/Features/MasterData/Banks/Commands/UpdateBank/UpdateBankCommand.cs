using MediatR;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.MasterData.Banks.Commands.UpdateBank;

/// <summary>
/// Command to update an existing bank.
/// </summary>
public sealed record UpdateBankCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string CountryCode { get; init; }
}
