using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.JsonBins.Queries.GetJsonBinContent;

public sealed class GetJsonBinContentQueryHandler : IRequestHandler<GetJsonBinContentQuery, JsonBinContentDto?>
{
    private readonly IApplicationDbContext _context;

    public GetJsonBinContentQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<JsonBinContentDto?> Handle(GetJsonBinContentQuery request, CancellationToken cancellationToken)
    {
        var row = await _context.JsonBins
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new { x.Id, x.ContentType, x.IsCompressed, x.Content, x.SizeBytes })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return null;

        var content = request.Decompress && row.IsCompressed
            ? await DecompressAsync(row.Content, cancellationToken)
            : row.Content;

        return new JsonBinContentDto
        {
            Id = row.Id,
            ContentType = row.ContentType,
            IsCompressed = row.IsCompressed && !request.Decompress,
            Content = content,
            SizeBytes = content.LongLength
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
