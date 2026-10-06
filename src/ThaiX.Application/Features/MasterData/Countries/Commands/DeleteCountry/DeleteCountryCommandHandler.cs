using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.MasterData.Countries.Commands.DeleteCountry;

/// <summary>
/// Handler for DeleteCountryCommand.
/// </summary>
public sealed class DeleteCountryCommandHandler : IRequestHandler<DeleteCountryCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteCountryCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteCountryCommand request, CancellationToken cancellationToken)
    {
        var country = await _dbContext.Countries
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Country with ID '{request.Id}' not found.");

        country.SoftDelete();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
