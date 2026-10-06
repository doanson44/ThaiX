using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.JsonBins.Queries.GetSharedJsonBin;

public sealed class GetSharedJsonBinQueryHandler : IRequestHandler<GetSharedJsonBinQuery, JsonBinSharedDto?>
{
    private readonly IApplicationDbContext _context;

    public GetSharedJsonBinQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JsonBinSharedDto?> Handle(GetSharedJsonBinQuery request, CancellationToken cancellationToken)
    {
        var token = request.Token.Trim().ToUpperInvariant();
        var nowUtc = DateTime.UtcNow;

        var row = await _context.JsonBins
            .AsNoTracking()
            .Where(x => x.ShareToken == token)
            .Select(x => new
            {
                x.Code,
                x.Name,
                x.ContentType,
                x.IsCompressed,
                x.Content,
                x.ShareExpiresAtUtc,
                x.ExpiredAtUtc,
                x.IsDeleted
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null || row.IsDeleted)
            return null;

        if (row.ShareExpiresAtUtc.HasValue && row.ShareExpiresAtUtc.Value <= nowUtc)
            return null;

        if (row.ExpiredAtUtc.HasValue && row.ExpiredAtUtc.Value <= nowUtc)
            return null;

        var bytes = row.IsCompressed
            ? await DecompressAsync(row.Content, cancellationToken)
            : row.Content;

        return new JsonBinSharedDto
        {
            Code = row.Code,
            Name = row.Name,
            ContentType = row.ContentType,
            ContentJson = Encoding.UTF8.GetString(bytes),
            ShareExpiresAtUtc = row.ShareExpiresAtUtc
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
