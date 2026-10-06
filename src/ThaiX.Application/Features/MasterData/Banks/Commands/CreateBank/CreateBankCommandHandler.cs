using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.MasterData;

namespace ThaiX.Application.Features.MasterData.Banks.Commands.CreateBank;

/// <summary>
/// Handler for CreateBankCommand.
/// </summary>
public sealed class CreateBankCommandHandler : IRequestHandler<CreateBankCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateBankCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateBankCommand request, CancellationToken cancellationToken)
    {
        var countryCode = request.CountryCode.Trim().ToUpperInvariant();
        var country = await _dbContext.Countries
            .FirstOrDefaultAsync(c => c.Code == countryCode, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Country with code '{countryCode}' not found.");

        var codeNormalized = request.Code.Trim().ToUpperInvariant();
        var exists = await _dbContext.Banks
            .AnyAsync(b => b.Code == codeNormalized, cancellationToken);

        if (exists)
            throw new OperationFailedException(ErrorCodes.DUPLICATE_ENTRY, $"Bank with code '{codeNormalized}' already exists.");

        var bank = Bank.Create(request.Code, request.Name, country.Code, country.Id);

        _dbContext.Banks.Add(bank);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return bank.Id;
    }
}
