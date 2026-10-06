using Hangfire;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Infrastructure.BackgroundJobs;

/// <summary>
/// Hangfire background job that processes outbox messages.
/// Reads unprocessed domain events from Outbox table and publishes them via MediatR.
/// </summary>
/// <remarks>
/// Architecture Note:
/// - Lives in Infrastructure layer (depends on IApplicationDbContext, IMediator)
/// - Executed by Hangfire server (configured as recurring job)
/// - Idempotent: Can be safely retried
/// - Batch processing: Processes up to 20 messages per run
/// - Fault-tolerant: Marks failed messages with error details
/// - MediatR handlers receive events via DomainEventNotification wrapper
/// </remarks>
public sealed class OutboxProcessorJob : HangfireJobBase
{
    private const int BatchSize = 20;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IApplicationDbContext _dbContext;
    private readonly IPublisher _publisher;
    private readonly ILogger<OutboxProcessorJob> _logger;

    public OutboxProcessorJob(
        IApplicationDbContext dbContext,
        IPublisher publisher,
        IHangfireJobState hangfireJobState,
        ILogger<OutboxProcessorJob> logger) : base(hangfireJobState, logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Processes pending outbox messages.
    /// Called by Hangfire recurring job (every 30 seconds by default).
    /// </summary>
    [Queue("critical")]
    public async Task ProcessPendingMessages(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.ProcessOutbox)) return;

        _logger.LogInformation("OutboxProcessor started");

        try
        {
            // Get unprocessed messages (oldest first)
            var messages = await _dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc == null)
                .OrderBy(m => m.OccurredOnUtc)
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            if (messages.Count == 0)
            {
                _logger.LogDebug("No pending outbox messages to process");
                return;
            }

            _logger.LogInformation("Processing {Count} outbox messages", messages.Count);

            foreach (var message in messages)
            {
                try
                {
                    // Deserialize domain event (resolve type from current or loaded assemblies)
                    var eventType = Type.GetType(message.Type)
                        ?? GetTypeFromLoadedAssemblies(message.Type);
                    if (eventType is null)
                    {
                        throw new InvalidOperationException($"Event type '{message.Type}' could not be resolved");
                    }

                    var domainEvent = JsonSerializer.Deserialize(message.Content, eventType, JsonOptions);
                    if (domainEvent is null)
                    {
                        throw new InvalidOperationException($"Failed to deserialize event of type '{message.Type}'");
                    }

                    // Publish via MediatR
                    // Note: Handlers receive the domain event via INotificationHandler<TDomainEvent>
                    // where TDomainEvent implements IDomainEvent
                    await _publisher.Publish(domainEvent, cancellationToken);

                    // Mark as processed
                    message.MarkAsProcessed();

                    _logger.LogDebug(
                        "Processed outbox message {MessageId} (Type: {EventType})",
                        message.Id,
                        message.Type);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Failed to process outbox message {MessageId} (Type: {EventType})",
                        message.Id,
                        message.Type);

                    // Mark as failed with error details
                    message.MarkAsFailed($"{ex.GetType().Name}: {ex.Message}");
                }
            }

            // Save changes (mark messages as processed or failed)
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "OutboxProcessor completed. Processed: {ProcessedCount}",
                messages.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OutboxProcessor failed with unhandled exception");
            throw;
        }
    }

    private static Type? GetTypeFromLoadedAssemblies(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            var type = assembly.GetType(typeName);
            if (type is not null)
                return type;
        }
        return null;
    }

    /// <summary>
    /// Cleans up old processed outbox messages.
    /// Called by Hangfire recurring job (daily at 2:00 AM UTC by default).
    /// </summary>
    [Queue("low")]
    public async Task CleanupOldMessages(CancellationToken cancellationToken = default)
    {
        if (!CanExecute(HangfireJobIds.CleanupOldOutboxMessages)) return;

        _logger.LogInformation("OutboxProcessor cleanup started");

        try
        {
            // Delete messages older than 30 days that have been processed
            var cutoffDate = DateTime.UtcNow.AddDays(-30);

            var oldMessages = await _dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc != null && m.ProcessedOnUtc < cutoffDate)
                .ToListAsync(cancellationToken);

            if (oldMessages.Count == 0)
            {
                _logger.LogDebug("No old outbox messages to clean up");
                return;
            }

            _dbContext.OutboxMessages.RemoveRange(oldMessages);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "OutboxProcessor cleanup completed. Deleted {DeletedCount} old messages",
                oldMessages.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OutboxProcessor cleanup failed");
            throw;
        }
    }
}
