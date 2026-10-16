using System.Text.Json;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PublicHtml.Common;

public static class PublicHtmlPageIndex
{
    private const string IndexKey = "public-html/pages.json";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly AsyncLock IndexLock = AsyncLock.GetLockByKey("public-html-pages-index");

    public static async Task<T> ExecuteAsync<T>(
        IFileStorage storage,
        Func<List<StoredPublicHtmlPage>, CancellationToken, Task<(T Result, bool Save)>> operation,
        CancellationToken cancellationToken)
    {
        using (await IndexLock.LockAsync(cancellationToken))
        {
            var pages = await ReadAsync(storage, cancellationToken);
            var (result, save) = await operation(pages, cancellationToken);
            if (save)
                await WriteAsync(storage, pages, cancellationToken);

            return result;
        }
    }

    private static async Task<List<StoredPublicHtmlPage>> ReadAsync(IFileStorage storage, CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = await storage.GetAsync(IndexKey, cancellationToken);
            return await JsonSerializer.DeserializeAsync<List<StoredPublicHtmlPage>>(stream, JsonOptions, cancellationToken) ?? [];
        }
        catch (OperationFailedException)
        {
            return [];
        }
    }

    private static async Task WriteAsync(
        IFileStorage storage,
        List<StoredPublicHtmlPage> pages,
        CancellationToken cancellationToken)
    {
        await using var stream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(pages, JsonOptions));
        await storage.SaveAsync(stream, IndexKey, "application/json", cancellationToken);
    }
}
