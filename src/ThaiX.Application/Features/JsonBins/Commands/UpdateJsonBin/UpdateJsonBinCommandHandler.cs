using MediatR;
using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using System.Text;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.JsonBins.Commands.UpdateJsonBin;

public sealed class UpdateJsonBinCommandHandler : IRequestHandler<UpdateJsonBinCommand, string>
{
    private readonly IApplicationDbContext _context;

    public UpdateJsonBinCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> Handle(UpdateJsonBinCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.JsonBins
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity is null)
        {
            throw new OperationFailedException(
                ErrorCodes.RESOURCE_NOT_FOUND,
                $"JsonBin with ID '{request.Id}' not found.");
        }

        // Blank code keeps the current value; explicit code is uniqueness-checked.
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var code = await JsonBinCodeResolver.ResolveUniqueAsync(
                _context,
                request.Code,
                request.Category,
                excludeId: request.Id,
                cancellationToken);

            if (!string.Equals(entity.Code, code, StringComparison.Ordinal))
                entity.ChangeCode(code);
        }

        entity.Rename(request.Name);
        entity.Update(request.Category, request.Tags, request.ReferenceId);

        if (request.ClearExpiration)
            entity.ChangeExpiration(null);
        else if (request.ExpiredAtUtc.HasValue)
            entity.ChangeExpiration(request.ExpiredAtUtc);

        if (!string.IsNullOrWhiteSpace(request.ContentJson))
        {
            var compress = request.Compress ?? entity.IsCompressed;
            var raw = Encoding.UTF8.GetBytes(request.ContentJson);
            var payload = compress
                ? await CompressAsync(raw, cancellationToken)
                : raw;
            entity.ReplaceContent(payload, request.ContentType, compress);
        }

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
