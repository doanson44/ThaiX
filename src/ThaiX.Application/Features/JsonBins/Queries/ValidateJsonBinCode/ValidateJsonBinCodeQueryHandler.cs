using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.JsonBins.Queries.ValidateJsonBinCode;

public sealed class ValidateJsonBinCodeQueryHandler
    : IRequestHandler<ValidateJsonBinCodeQuery, ValidateJsonBinCodeResult>
{
    private readonly IApplicationDbContext _context;

    public ValidateJsonBinCodeQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ValidateJsonBinCodeResult> Handle(
        ValidateJsonBinCodeQuery request,
        CancellationToken cancellationToken)
    {
        var code = JsonBin.NormalizeCode(request.Code);

        var query = _context.JsonBins
            .AsNoTracking()
            .Where(x => x.Code == code);

        if (request.ExcludeId.HasValue)
            query = query.Where(x => x.Id != request.ExcludeId.Value);

        var exists = await query.AnyAsync(cancellationToken);

        return new ValidateJsonBinCodeResult
        {
            Code = code,
            Exists = exists
        };
    }
}
