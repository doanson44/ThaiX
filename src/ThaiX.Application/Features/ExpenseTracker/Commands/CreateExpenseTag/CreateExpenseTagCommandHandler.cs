using MediatR;
using ThaiX.Application.Common.Interfaces;
using ExpenseTag = ThaiX.Domain.Aggregates.ExpenseTracker.Tag;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateExpenseTag;

/// <summary>
/// Handler for CreateExpenseTagCommand.
/// </summary>
public sealed class CreateExpenseTagCommandHandler : IRequestHandler<CreateExpenseTagCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateExpenseTagCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateExpenseTagCommand request, CancellationToken cancellationToken)
    {
        var entity = ExpenseTag.Create(request.Name, request.ColorHex);
        _dbContext.ExpenseTags.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
