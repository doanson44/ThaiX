using MediatR;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.MasterData.Banks.Commands.DeleteBank;

/// <summary>
/// Command to soft-delete a bank.
/// </summary>
public sealed record DeleteBankCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
