using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.JsonBins.Queries.GetJsonBins;

public sealed class GetJsonBinsQueryHandler : IRequestHandler<GetJsonBinsQuery, PagedResult<JsonBinListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetJsonBinsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<JsonBinListItemDto>> Handle(
        GetJsonBinsQuery request,
        CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var query = _context.JsonBins.AsNoTracking();

        if (request.Category.HasValue)
            query = query.Where(x => x.Category == request.Category.Value);

        if (request.ReferenceId.HasValue)
            query = query.Where(x => x.ReferenceId == request.ReferenceId.Value);

        if (request.IncludeExpired != true)
            query = query.Where(x => !x.ExpiredAtUtc.HasValue || x.ExpiredAtUtc > nowUtc);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var pattern = $"%{request.SearchTerm.Trim()}%";
            query = query.Where(x =>
                EF.Functions.Like(x.Code, pattern) ||
                EF.Functions.Like(x.Name, pattern) ||
                (x.Tags != null && EF.Functions.Like(x.Tags, pattern)));
        }

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "code" => request.SortDescending
                ? query.OrderByDescending(x => x.Code)
                : query.OrderBy(x => x.Code),
            "name" => request.SortDescending
                ? query.OrderByDescending(x => x.Name)
                : query.OrderBy(x => x.Name),
            "sizebytes" => request.SortDescending
                ? query.OrderByDescending(x => x.SizeBytes)
                : query.OrderBy(x => x.SizeBytes),
            "expiredatutc" => request.SortDescending
                ? query.OrderByDescending(x => x.ExpiredAtUtc)
                : query.OrderBy(x => x.ExpiredAtUtc),
            _ => request.SortDescending
                ? query.OrderByDescending(x => x.CreatedAtUtc)
                : query.OrderBy(x => x.CreatedAtUtc)
        };

        return await query
            .Select(x => new JsonBinListItemDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Category = x.Category,
                ContentType = x.ContentType,
                SizeBytes = x.SizeBytes,
                IsCompressed = x.IsCompressed,
                Tags = x.Tags,
                ReferenceId = x.ReferenceId,
                ExpiredAtUtc = x.ExpiredAtUtc,
                CreatedAtUtc = x.CreatedAtUtc,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
