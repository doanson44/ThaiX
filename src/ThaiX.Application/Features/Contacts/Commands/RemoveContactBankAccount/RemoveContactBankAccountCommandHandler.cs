using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactBankAccount;

/// <summary>
/// Removes a contact bank account by querying/deleting ContactBankAccounts directly without loading the Contact aggregate.
/// </summary>
public sealed class RemoveContactBankAccountCommandHandler : IRequestHandler<RemoveContactBankAccountCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public RemoveContactBankAccountCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(RemoveContactBankAccountCommand request, CancellationToken cancellationToken)
    {
        var rowsDeleted = await _dbContext.ContactBankAccounts
            .Where(ba => ba.Id == request.BankAccountId && ba.ContactId == request.ContactId)
            .ExecuteDeleteAsync(cancellationToken);

        if (rowsDeleted == 0)
            throw new OperationFailedException(ErrorCodes.CONTACT_ITEM_NOT_FOUND, $"Bank account with ID '{request.BankAccountId}' not found for this contact.");

        return request.BankAccountId;
    }
}
