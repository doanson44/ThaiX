using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateTransactionTag;

/// <summary>
/// Handler for CreateTransactionTagCommand.
/// </summary>
public sealed class CreateTransactionTagCommandHandler : IRequestHandler<CreateTransactionTagCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateTransactionTagCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateTransactionTagCommand request, CancellationToken cancellationToken)
    {
        var entity = TransactionTag.Create(request.TransactionId, request.TagId);
        _dbContext.TransactionTags.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
