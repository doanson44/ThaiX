namespace ThaiX.Application.Common.Interfaces;

public sealed record PublicHtmlPageDto(Guid Id, string Title, string Slug, IReadOnlyCollection<string> PreviousSlugs, long SizeBytes, bool IsPublished, DateTime CreatedAtUtc, DateTime? PublishedAtUtc);

public interface IPublicHtmlPageStore
{
    Task<IReadOnlyCollection<PublicHtmlPageDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PublicHtmlPageDto> UploadAsync(string title, string slug, string fileName, Stream content, long sizeBytes, CancellationToken cancellationToken = default);
    Task<PublicHtmlPageDto?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<Stream?> OpenPublishedContentAsync(string slug, CancellationToken cancellationToken = default);
    Task<PublicHtmlPageDto> RenameAsync(Guid id, string slug, CancellationToken cancellationToken = default);
    Task<PublicHtmlPageDto> SetPublishedAsync(Guid id, bool isPublished, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
