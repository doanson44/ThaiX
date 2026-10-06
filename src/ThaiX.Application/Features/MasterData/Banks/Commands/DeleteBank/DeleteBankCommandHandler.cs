using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.MasterData.Banks.Commands.DeleteBank;

/// <summary>
/// Handler for DeleteBankCommand.
/// </summary>
public sealed class DeleteBankCommandHandler : IRequestHandler<DeleteBankCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteBankCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteBankCommand request, CancellationToken cancellationToken)
    {
        var bank = await _dbContext.Banks
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Bank with ID '{request.Id}' not found.");

        bank.SoftDelete();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
