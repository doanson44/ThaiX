using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Queries.GetContactImportJob;

/// <summary>
/// Returns the import job by id, or null if not found.
/// </summary>
public sealed class GetContactImportJobQueryHandler : IRequestHandler<GetContactImportJobQuery, ContactImportJobDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetContactImportJobQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ContactImportJobDto?> Handle(GetContactImportJobQuery request, CancellationToken cancellationToken)
    {
        var job = await _dbContext.ContactImportJobs
            .AsNoTracking()
            .Where(j => j.Id == request.JobId)
            .Select(j => new ContactImportJobDto
            {
                Id = j.Id,
                Status = j.Status,
                CreatedAtUtc = j.CreatedAtUtc,
                CompletedAtUtc = j.CompletedAtUtc,
                TotalRows = j.TotalRows,
                InsertedCount = j.InsertedCount,
                UpdatedCount = j.UpdatedCount,
                SkippedCount = j.SkippedCount,
                ErrorMessage = j.ErrorMessage
            })
            .FirstOrDefaultAsync(cancellationToken);

        return job;
    }
}
