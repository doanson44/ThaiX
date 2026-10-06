namespace ThaiX.Domain.Aggregates.Blog;

/// <summary>
/// Join row for the many-to-many Post &lt;-&gt; Tag relationship. Plain composite-key mapping
/// row, not a full aggregate — managed directly by Application command handlers (mirrors
/// how ContactTag rows are queried/updated without loading the owning aggregate).
/// </summary>
public sealed class PostTag
{
    private PostTag()
    {
    }

    public Guid PostId { get; private set; }

    public Post Post { get; private set; } = null!;

    public Guid TagId { get; private set; }

    public Tag Tag { get; private set; } = null!;

    public static PostTag Create(Guid postId, Guid tagId)
    {
        return new PostTag
        {
            PostId = postId,
            TagId = tagId
        };
    }
}
