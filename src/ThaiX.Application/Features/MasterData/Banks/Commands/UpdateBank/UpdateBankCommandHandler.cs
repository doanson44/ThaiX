using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.MasterData.Banks.Commands.UpdateBank;

/// <summary>
/// Handler for UpdateBankCommand.
/// </summary>
public sealed class UpdateBankCommandHandler : IRequestHandler<UpdateBankCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateBankCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateBankCommand request, CancellationToken cancellationToken)
    {
        var bank = await _dbContext.Banks
            .FirstOrDefaultAsync(b => b.Id == request.Id, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Bank with ID '{request.Id}' not found.");

        var countryCode = request.CountryCode.Trim().ToUpperInvariant();
        var country = await _dbContext.Countries
            .FirstOrDefaultAsync(c => c.Code == countryCode, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Country with code '{countryCode}' not found.");

        var codeNormalized = request.Code.Trim().ToUpperInvariant();
        var duplicateExists = await _dbContext.Banks
            .AnyAsync(b => b.Code == codeNormalized && b.Id != request.Id, cancellationToken);

        if (duplicateExists)
            throw new OperationFailedException(ErrorCodes.DUPLICATE_ENTRY, $"Bank with code '{codeNormalized}' already exists.");

        bank.UpdateDetails(request.Code, request.Name, country.Code, country.Id);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
