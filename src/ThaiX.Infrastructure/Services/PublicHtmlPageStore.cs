using System.Text.Json;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Helpers;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Infrastructure.Services;

public sealed class PublicHtmlPageStore(IFileStorage storage) : IPublicHtmlPageStore
{
    private const string IndexKey = "public-html/pages.json";
    private const string ContentPrefix = "public-html/content";
    private const long MaxSizeBytes = 5 * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly AsyncLock IndexLock = AsyncLock.GetLockByKey("public-html-pages-index");

    public async Task<IReadOnlyCollection<PublicHtmlPageDto>> GetAllAsync(CancellationToken ct = default)
    {
        using (await IndexLock.LockAsync(ct))
            return (await ReadIndexAsync(ct)).OrderByDescending(x => x.CreatedAtUtc).Select(ToDto).ToArray();
    }

    public async Task<PublicHtmlPageDto> UploadAsync(string title, string slug, string fileName, Stream content, long sizeBytes, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (sizeBytes is <= 0 or > MaxSizeBytes)
            throw new ArgumentOutOfRangeException(nameof(sizeBytes), "HTML files must be between 1 byte and 5 MB.");
        var extension = Path.GetExtension(fileName);
        if (!extension.Equals(".html", StringComparison.OrdinalIgnoreCase) && !extension.Equals(".htm", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only .html and .htm files are supported.", nameof(fileName));

        slug = NormalizeSlug(slug);
        title = string.IsNullOrWhiteSpace(title) ? Path.GetFileNameWithoutExtension(fileName) : title.Trim();
        if (title.Length > 200) throw new ArgumentException("Title must not exceed 200 characters.", nameof(title));

        using (await IndexLock.LockAsync(ct))
        {
            var items = await ReadIndexAsync(ct);
            EnsureSlugAvailable(items, slug, null);
            var id = Guid.NewGuid();
            var key = $"{ContentPrefix}/{id:N}.html";
            await storage.SaveAsync(content, key, "text/html", ct);
            var now = DateTime.UtcNow;
            var item = new StoredPage(id, title, slug, [], key, sizeBytes, false, now, null);
            items.Add(item);
            try { await WriteIndexAsync(items, ct); }
            catch { await storage.DeleteAsync(key, ct); throw; }
            return ToDto(item);
        }
    }

    public async Task<PublicHtmlPageDto?> GetPublishedBySlugAsync(string slug, CancellationToken ct = default)
    {
        try { slug = NormalizeSlug(slug); }
        catch (ArgumentException) { return null; }
        using (await IndexLock.LockAsync(ct))
        {
            var item = (await ReadIndexAsync(ct)).FirstOrDefault(x => x.IsPublished &&
                (x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) || x.PreviousSlugs.Contains(slug, StringComparer.OrdinalIgnoreCase)));
            return item is null ? null : ToDto(item);
        }
    }

    public async Task<Stream?> OpenPublishedContentAsync(string slug, CancellationToken ct = default)
    {
        try { slug = NormalizeSlug(slug); }
        catch (ArgumentException) { return null; }

        string? storageKey;
        using (await IndexLock.LockAsync(ct))
        {
            var item = (await ReadIndexAsync(ct)).FirstOrDefault(x => x.IsPublished &&
                (x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) || x.PreviousSlugs.Contains(slug, StringComparer.OrdinalIgnoreCase)));
            storageKey = item?.StorageKey;
        }

        return storageKey is null ? null : await storage.GetAsync(storageKey, ct);
    }

    public async Task<PublicHtmlPageDto> RenameAsync(Guid id, string slug, CancellationToken ct = default)
    {
        slug = NormalizeSlug(slug);
        using (await IndexLock.LockAsync(ct))
        {
            var items = await ReadIndexAsync(ct);
            var index = items.FindIndex(x => x.Id == id);
            if (index < 0) throw new KeyNotFoundException("HTML page was not found.");
            EnsureSlugAvailable(items, slug, id);
            var item = items[index];
            if (!item.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase))
                item = item with { PreviousSlugs = item.PreviousSlugs.Append(item.Slug).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(), Slug = slug };
            items[index] = item;
            await WriteIndexAsync(items, ct);
            return ToDto(item);
        }
    }

    public async Task<PublicHtmlPageDto> SetPublishedAsync(Guid id, bool isPublished, CancellationToken ct = default)
    {
        using (await IndexLock.LockAsync(ct))
        {
            var items = await ReadIndexAsync(ct);
            var index = items.FindIndex(x => x.Id == id);
            if (index < 0) throw new KeyNotFoundException("HTML page was not found.");
            var item = items[index] with { IsPublished = isPublished, PublishedAtUtc = isPublished ? DateTime.UtcNow : null };
            items[index] = item;
            await WriteIndexAsync(items, ct);
            return ToDto(item);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        using (await IndexLock.LockAsync(ct))
        {
            var items = await ReadIndexAsync(ct);
            var item = items.FirstOrDefault(x => x.Id == id);
            if (item is null) return;
            items.Remove(item);
            await WriteIndexAsync(items, ct);
            await storage.DeleteAsync(item.StorageKey, ct);
        }
    }

    private async Task<List<StoredPage>> ReadIndexAsync(CancellationToken ct)
    {
        try
        {
            await using var stream = await storage.GetAsync(IndexKey, ct);
            return await JsonSerializer.DeserializeAsync<List<StoredPage>>(stream, JsonOptions, ct) ?? [];
        }
        catch (OperationFailedException) { return []; }
    }

    private async Task WriteIndexAsync(List<StoredPage> items, CancellationToken ct)
    {
        await using var stream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(items, JsonOptions));
        await storage.SaveAsync(stream, IndexKey, "application/json", ct);
    }

    private static void EnsureSlugAvailable(IEnumerable<StoredPage> items, string slug, Guid? currentId)
    {
        if (items.Any(x => x.Id != currentId &&
            (x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) || x.PreviousSlugs.Contains(slug, StringComparer.OrdinalIgnoreCase))))
            throw new InvalidOperationException("The slug is already in use.");
    }

    private static string NormalizeSlug(string slug)
    {
        slug = (slug ?? string.Empty).Trim().ToLowerInvariant();
        if (slug.Length is < 1 or > 80 || slug.Any(c => !(char.IsAsciiLetterOrDigit(c) || c == '-')) || slug.StartsWith('-') || slug.EndsWith('-') || slug.Contains("--", StringComparison.Ordinal))
            throw new ArgumentException("Slug must contain 1-80 lowercase letters, numbers, or single hyphens.", nameof(slug));
        return slug;
    }

    private static PublicHtmlPageDto ToDto(StoredPage x) => new(x.Id, x.Title, x.Slug, x.PreviousSlugs, x.SizeBytes, x.IsPublished, x.CreatedAtUtc, x.PublishedAtUtc);
    private sealed record StoredPage(Guid Id, string Title, string Slug, string[] PreviousSlugs, string StorageKey, long SizeBytes, bool IsPublished, DateTime CreatedAtUtc, DateTime? PublishedAtUtc);
}