using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Resumes.Queries.GetMyResume;

public sealed class GetMyResumeQueryHandler : IRequestHandler<GetMyResumeQuery, ResumeDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyResumeQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ResumeDto?> Handle(GetMyResumeQuery request, CancellationToken cancellationToken)
    {
        var resume = await _context.ResumeProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.OwnerId == _currentUser.UserId, cancellationToken);

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
