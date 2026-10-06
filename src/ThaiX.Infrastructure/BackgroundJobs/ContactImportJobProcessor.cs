using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Contacts.Commands.ImportContacts;
using ThaiX.Domain.Aggregates.ContactImport;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire job that processes a contact import: loads job, sends ImportContactsCommand (orchestrator), updates job status.
/// </summary>
public sealed class ContactImportJobProcessor
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMediator _mediator;
    private readonly ILogger<ContactImportJobProcessor> _logger;

    public ContactImportJobProcessor(
        IApplicationDbContext dbContext,
        IMediator mediator,
        ILogger<ContactImportJobProcessor> logger)
    {
        _dbContext = dbContext;
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Called by Hangfire. Loads the job, opens the file, runs the import engine, updates the job.
    /// </summary>
    public async Task ProcessAsync(Guid jobId, CancellationToken cancellationToken)
    {
        var job = await _dbContext.ContactImportJobs
            .FirstOrDefaultAsync(j => j.Id == jobId, cancellationToken);

        if (job is null)
        {
            _logger.LogWarning("Contact import job {JobId} not found", jobId);
            return;
        }

        if (job.Status != ContactImportJobStatus.Pending)
        {
            _logger.LogInformation("Contact import job {JobId} already in status {Status}", jobId, job.Status);
            return;
        }

        job.MarkRunning();
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!File.Exists(job.FilePath))
        {
            job.MarkFailed("File not found.");
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogError("Contact import job {JobId}: file not found at {Path}", jobId, job.FilePath);
            return;
        }

        try
        {
            await using var stream = new FileStream(job.FilePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
            var result = await _mediator.Send(new ImportContactsCommand { CsvStream = stream, BatchSize = job.BatchSize }, cancellationToken);
            job.MarkCompleted(result.TotalRows, result.InsertedCount, result.UpdatedCount, result.SkippedCount);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation(
                "Contact import job {JobId} completed: {Inserted} inserted, {Updated} updated, {Skipped} skipped",
                jobId, result.InsertedCount, result.UpdatedCount, result.SkippedCount);
        }
        catch (Exception ex)
        {
            job.MarkFailed(ex.Message);
            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogError(ex, "Contact import job {JobId} failed", jobId);
        }
        finally
        {
            try
            {
                if (File.Exists(job.FilePath))
                    File.Delete(job.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not delete import file {Path}", job.FilePath);
            }
        }
    }
}
