using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.JsonBins.Queries.GetJsonBinById;

public sealed class GetJsonBinByIdQueryHandler : IRequestHandler<GetJsonBinByIdQuery, JsonBinDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetJsonBinByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JsonBinDetailDto?> Handle(GetJsonBinByIdQuery request, CancellationToken cancellationToken)
    {
        var nowUtc = DateTime.UtcNow;
        var row = await _context.JsonBins
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name,
                x.Category,
                x.ContentType,
                x.SizeBytes,
                x.IsCompressed,
                x.Tags,
                x.ReferenceId,
                x.ExpiredAtUtc,
                x.CreatedAtUtc,
                x.CreatedAt,
                x.UpdatedAt,
                x.Content,
                x.ShareToken,
                x.ShareExpiresAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return null;

        var bytes = row.IsCompressed
            ? await DecompressAsync(row.Content, cancellationToken)
            : row.Content;

        var isShared = !string.IsNullOrEmpty(row.ShareToken)
            && (!row.ShareExpiresAtUtc.HasValue || row.ShareExpiresAtUtc.Value > nowUtc)
            && (!row.ExpiredAtUtc.HasValue || row.ExpiredAtUtc.Value > nowUtc);

        return new JsonBinDetailDto
        {
            Id = row.Id,
            Code = row.Code,
            Name = row.Name,
            Category = row.Category,
            ContentType = row.ContentType,
            SizeBytes = row.SizeBytes,
            IsCompressed = row.IsCompressed,
            Tags = row.Tags,
            ReferenceId = row.ReferenceId,
            ExpiredAtUtc = row.ExpiredAtUtc,
            CreatedAtUtc = row.CreatedAtUtc,
            CreatedAt = row.CreatedAt,
            UpdatedAt = row.UpdatedAt,
            ContentJson = Encoding.UTF8.GetString(bytes),
            ShareToken = row.ShareToken,
            ShareExpiresAtUtc = row.ShareExpiresAtUtc,
            IsShared = isShared
        };
    }

    private static async Task<byte[]> DecompressAsync(byte[] data, CancellationToken cancellationToken)
    {
        await using var input = new MemoryStream(data);
        await using var gzip = new GZipStream(input, CompressionMode.Decompress);
        await using var output = new MemoryStream();
        await gzip.CopyToAsync(output, cancellationToken);
        return output.ToArray();
    }
}
