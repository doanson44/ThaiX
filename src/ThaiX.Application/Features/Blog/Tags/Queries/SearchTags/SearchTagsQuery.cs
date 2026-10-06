using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Blog.Tags.Queries.SearchTags;

/// <summary>
/// Powers the tag autocomplete/multi-select in the post editor. Not cached — search term
/// combinations are too varied for cache hits to pay off, and results are cheap to compute.
/// </summary>
public sealed record SearchTagsQuery : PagedRequest, IAppQuery<PagedResult<TagDto>>
{
    public string? Search { get; init; }
}
