using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.PublicHtml;

public sealed class PublicHtmlPageSlug : BaseEntity
{
    private PublicHtmlPageSlug()
    {
    }

    public Guid PublicHtmlPageId { get; private set; }
    public string Slug { get; private set; } = null!;
    public bool IsCanonical { get; private set; }

    internal static PublicHtmlPageSlug Create(Guid publicHtmlPageId, string slug, bool isCanonical) =>
        new()
        {
            Id = Guid.NewGuid(),
            PublicHtmlPageId = publicHtmlPageId,
            Slug = slug,
            IsCanonical = isCanonical
        };

    internal void SetCanonical(bool isCanonical)
    {
        IsCanonical = isCanonical;
    }
}
