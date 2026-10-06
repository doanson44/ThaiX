using MediatR;
using System.IO.Compression;
using System.Text;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.JsonBins.Commands.CreateJsonBin;

public sealed class CreateJsonBinCommandHandler : IRequestHandler<CreateJsonBinCommand, string>
{
    private readonly IApplicationDbContext _context;

    public CreateJsonBinCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(CreateJsonBinCommand request, CancellationToken cancellationToken)
    {
        var code = await JsonBinCodeResolver.ResolveUniqueAsync(
            _context,
            request.Code,
            request.Category,
            excludeId: null,
            cancellationToken);

        var raw = Encoding.UTF8.GetBytes(request.ContentJson);
        var payload = request.Compress
            ? await CompressAsync(raw, cancellationToken)
            : raw;

        var entity = JsonBin.Create(
            code: code,
            name: request.Name,
            category: request.Category,
            content: payload,
            contentType: request.ContentType,
            isCompressed: request.Compress,
            tags: request.Tags,
            referenceId: request.ReferenceId,
            expiredAtUtc: request.ExpiredAtUtc);

        _context.JsonBins.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Code;
    }

    private static async Task<byte[]> CompressAsync(byte[] data, CancellationToken cancellationToken)
    {
        await using var output = new MemoryStream();
        await using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            await gzip.WriteAsync(data, cancellationToken);
        }

        return output.ToArray();
    }
}
