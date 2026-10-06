using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactBankAccount;

/// <summary>
/// Handler for AddContactBankAccountCommand.
/// Encrypts the account number before storing.
/// Queries/updates ContactBankAccounts directly without loading the Contact aggregate.
/// </summary>
public sealed class AddContactBankAccountCommandHandler : IRequestHandler<AddContactBankAccountCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IEncryptionService _encryptionService;

    public AddContactBankAccountCommandHandler(
        IApplicationDbContext dbContext,
        IEncryptionService encryptionService)
    {
        _dbContext = dbContext;
        _encryptionService = encryptionService;
    }

    public async Task<Guid> Handle(AddContactBankAccountCommand request, CancellationToken cancellationToken)
    {
        var contactExists = await _dbContext.Contacts
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ContactId, cancellationToken);
        if (!contactExists)
            throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.ContactId}' not found.");

        if (request.IsPrimary)
        {
            await _dbContext.ContactBankAccounts
                .Where(ba => ba.ContactId == request.ContactId)
                .ExecuteUpdateAsync(s => s.SetProperty(ba => ba.IsPrimary, false), cancellationToken);
        }

        var encryptedAccountNumber = _encryptionService.Encrypt(request.AccountNumber);
        var last4 = request.AccountNumber.Length >= 4
            ? request.AccountNumber[^4..]
            : request.AccountNumber;

        var bankAccount = ContactBankAccount.Create(
            request.ContactId,
            request.BankCode,
            request.BranchName,
            encryptedAccountNumber,
            last4,
            request.AccountName,
            request.CurrencyCode,
            request.IsPrimary);
        _dbContext.ContactBankAccounts.Add(bankAccount);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return bankAccount.Id;
    }
}
