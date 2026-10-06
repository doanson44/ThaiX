using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.ContactImport;

namespace ThaiX.Application.Features.Contacts.Commands.StartContactImport;

/// <summary>
/// Creates a ContactImportJob (Pending), persists it, enqueues background processing, returns job id.
/// </summary>
public sealed class StartContactImportCommandHandler : IRequestHandler<StartContactImportCommand, Guid>
{
    private const int DefaultBatchSize = 500;

    private readonly IApplicationDbContext _dbContext;
    private readonly IContactImportJobScheduler _scheduler;

    public StartContactImportCommandHandler(
        IApplicationDbContext dbContext,
        IContactImportJobScheduler scheduler)
    {
        _dbContext = dbContext;
        _scheduler = scheduler;
    }

    public async Task<Guid> Handle(StartContactImportCommand request, CancellationToken cancellationToken)
    {
        var batchSize = request.BatchSize ?? DefaultBatchSize;
        var job = ContactImportJob.Create(request.FilePath, batchSize);
        _dbContext.ContactImportJobs.Add(job);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _scheduler.Enqueue(job.Id);
        return job.Id;
    }
}
