using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Resumes;

namespace ThaiX.Application.Features.Resumes.Commands.UpsertResume;

public sealed class UpsertResumeCommandHandler : IRequestHandler<UpsertResumeCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpsertResumeCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(UpsertResumeCommand request, CancellationToken cancellationToken)
    {
        var normalizedSlug = request.Slug.Trim().Trim('/').ToLowerInvariant();

        var existing = await _context.ResumeProfiles
            .FirstOrDefaultAsync(r => r.OwnerId == _currentUser.UserId, cancellationToken);

        var slugTaken = await _context.ResumeProfiles
            .AnyAsync(r => r.Slug == normalizedSlug && r.Id != (existing != null ? existing.Id : Guid.Empty), cancellationToken);

        if (slugTaken)
        {
            throw new OperationFailedException(ErrorCodes.DUPLICATE_ENTRY, $"Slug '{normalizedSlug}' is already in use.");
        }

        var contentJson = request.Content.ToJson();

        if (existing is null)
        {
            var resume = ResumeProfile.Create(
                _currentUser.UserId,
                normalizedSlug,
                request.FullName,
                request.Headline,
                request.MetaDescription,
                contentJson);
            resume.SetPublished(request.IsPublished);

            _context.ResumeProfiles.Add(resume);
            await _context.SaveChangesAsync(cancellationToken);
            return resume.Id;
        }

        existing.UpdateContent(
            normalizedSlug,
            request.FullName,
            request.Headline,
            request.MetaDescription,
            contentJson);
        existing.SetPublished(request.IsPublished);

        await _context.SaveChangesAsync(cancellationToken);
        return existing.Id;
    }
}
