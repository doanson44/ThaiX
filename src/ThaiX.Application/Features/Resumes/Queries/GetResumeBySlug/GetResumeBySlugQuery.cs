using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Resumes.Queries.GetResumeBySlug;

/// <summary>
/// Public lookup used by the anonymous /resume/{slug} page. Returns null unless a published
/// resume exists at that slug.
/// </summary>
public sealed record GetResumeBySlugQuery : IAppQuery<ResumeDto?>, ICacheableQuery
{
    public required string Slug { get; init; }

    public string CacheKey => CacheKeys.Resumes.BySlug(Slug);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(10);
    public string CacheGroup => CacheGroups.Resumes;
    public bool IsVersionedList => true;
}
