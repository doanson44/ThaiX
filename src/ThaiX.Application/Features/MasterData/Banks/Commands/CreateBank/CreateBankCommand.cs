using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.MasterData.Banks.Commands.CreateBank;

/// <summary>
/// Command to create a new bank.
/// </summary>
public sealed record CreateBankCommand : IAppCommand<Guid>
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string CountryCode { get; init; }
}
