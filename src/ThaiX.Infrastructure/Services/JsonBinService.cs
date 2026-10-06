using Microsoft.EntityFrameworkCore;
using System.IO.Compression;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="IJsonBinService"/> using EF Core and the application database.
/// </summary>
public sealed class JsonBinService : IJsonBinService
{
    private readonly IApplicationDbContext _context;

    public JsonBinService(IApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Guid> SaveAsync(
        string name,
        JsonBinCategories category,
        Stream content,
        bool compress,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        ArgumentNullException.ThrowIfNull(content, nameof(content));

        await using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);

        var rawBytes = buffer.ToArray();
        var payload = compress
            ? await CompressAsync(rawBytes, cancellationToken)
            : rawBytes;

        var jsonBin = JsonBin.Create(
            code: JsonBin.GenerateCode(category),
            name: name,
            category: category,
            content: payload,
            isCompressed: compress);

        _context.JsonBins.Add(jsonBin);
        await _context.SaveChangesAsync(cancellationToken);

        return jsonBin.Id;
    }

    public async Task<Stream> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _context.JsonBins
            .Where(x => x.Id == id)
            .Select(x => new { x.Content, x.IsCompressed })
            .FirstOrDefaultAsync(cancellationToken);

        if (item == null)
        {
            throw new OperationFailedException(
                ErrorCodes.RESOURCE_NOT_FOUND,
                $"JsonBin with ID '{id}' not found.");
        }

        if (!item.IsCompressed)
        {
            return new MemoryStream(item.Content, writable: false);
        }

        var decompressed = await DecompressAsync(item.Content, cancellationToken);
        return new MemoryStream(decompressed, writable: false);
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _context.JsonBins
            .Where(x => x.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    private static async Task<byte[]> CompressAsync(byte[] data, CancellationToken cancellationToken)
    {
        await using var outputStream = new MemoryStream();
        await using (var gzipStream = new GZipStream(outputStream, CompressionLevel.Optimal, leaveOpen: true))
        {
            await gzipStream.WriteAsync(data, cancellationToken);
        }

        return outputStream.ToArray();
    }

    private static async Task<byte[]> DecompressAsync(byte[] data, CancellationToken cancellationToken)
    {
        await using var inputStream = new MemoryStream(data);
        await using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);
        await using var outputStream = new MemoryStream();
        await gzipStream.CopyToAsync(outputStream, cancellationToken);
        return outputStream.ToArray();
    }
}
