using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Resumes.Queries.GetResumeBySlug;

public sealed class GetResumeBySlugQueryHandler : IRequestHandler<GetResumeBySlugQuery, ResumeDto?>
{
    private readonly IApplicationDbContext _context;

    public GetResumeBySlugQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ResumeDto?> Handle(GetResumeBySlugQuery request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().Trim('/').ToLowerInvariant();

        var resume = await _context.ResumeProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Slug == normalizedSlug && r.IsPublished, cancellationToken);

        if (resume is null)
        {
            return null;
        }

        return new ResumeDto
        {
            Id = resume.Id,
            Slug = resume.Slug,
            FullName = resume.FullName,
            Headline = resume.Headline,
            MetaDescription = resume.MetaDescription,
            IsPublished = resume.IsPublished,
            Content = ResumeContentDto.FromJson(resume.ContentJson)
        };
    }
}
