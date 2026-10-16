namespace ThaiX.Client.Models.PublicHtml;

public sealed class PublicHtmlPageModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public bool IsPublished { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PublishedAtUtc { get; set; }

    public string EditSlug { get; set; } = string.Empty;
}
