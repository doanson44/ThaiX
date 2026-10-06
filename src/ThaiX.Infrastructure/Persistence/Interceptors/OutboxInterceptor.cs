using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;
using ThaiX.Domain.Aggregates.Outbox;
using ThaiX.Domain.Common.Entities;

namespace ThaiX.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that persists domain events to Outbox table.
/// Domain events are serialized and stored for asynchronous processing by Hangfire.
/// </summary>
/// <remarks>
/// Architecture Note:
/// - Domain raises events (framework-agnostic)
/// - Infrastructure persists events to Outbox (EF Core SaveChanges interceptor)
/// - Hangfire processes Outbox asynchronously (background job)
/// - MediatR publishes events to handlers (in background job, not request thread)
/// </remarks>
public sealed class OutboxInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
        {
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        ConvertDomainEventsToOutboxMessages(dbContext);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        var dbContext = eventData.Context;

        if (dbContext is null)
        {
            return base.SavingChanges(eventData, result);
        }

        ConvertDomainEventsToOutboxMessages(dbContext);

        return base.SavingChanges(eventData, result);
    }

    private static void ConvertDomainEventsToOutboxMessages(DbContext dbContext)
    {
        // Get all entities that inherit from BaseEntity
        var entities = dbContext.ChangeTracker
            .Entries<BaseEntity>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // Convert domain events to outbox messages
        var outboxMessages = entities
            .SelectMany(entity =>
            {
                var domainEvents = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();

                return domainEvents.Select(domainEvent =>
                {
                    var eventType = domainEvent.GetType().FullName ?? domainEvent.GetType().Name;
                    var eventContent = JsonSerializer.Serialize<object>(domainEvent, JsonOptions);

                    return new OutboxMessage(eventType, eventContent);
                });
            })
            .ToList();

        // Add outbox messages to DbContext
        dbContext.Set<OutboxMessage>().AddRange(outboxMessages);
    }
}
