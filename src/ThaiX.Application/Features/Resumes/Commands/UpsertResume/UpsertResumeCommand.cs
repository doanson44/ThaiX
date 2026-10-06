using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Resumes.Commands.UpsertResume;

[InvalidateCache(CacheGroups.Resumes)]
public sealed record UpsertResumeCommand : IAppCommand<Guid>
{
    public required string Slug { get; init; }
    public required string FullName { get; init; }
    public required string Headline { get; init; }
    public string? MetaDescription { get; init; }
    public required bool IsPublished { get; init; }
    public required ResumeContentDto Content { get; init; }
}
