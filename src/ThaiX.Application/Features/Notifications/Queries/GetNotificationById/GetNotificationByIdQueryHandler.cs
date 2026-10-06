using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Notifications.Queries.GetNotificationById;

public sealed class GetNotificationByIdQueryHandler
    : IRequestHandler<GetNotificationByIdQuery, NotificationDto?>
{
    private readonly IApplicationDbContext _dbContext;

    public GetNotificationByIdQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<NotificationDto?> Handle(
        GetNotificationByIdQuery request,
        CancellationToken cancellationToken)
    {
        return _dbContext.Notifications
            .AsNoTracking()
            .Where(notification => notification.Id == request.Id)
            .Select(notification => new NotificationDto
            {
                Id = notification.Id,
                Kind = notification.Kind,
                Severity = notification.Severity,
                TemplateKey = notification.TemplateKey,
                Subject = notification.Subject,
                Body = notification.Body,
                DataJson = notification.DataJson,
                RecipientUserId = notification.RecipientUserId,
                SourceEventId = notification.SourceEventId,
                DeduplicationKey = notification.DeduplicationKey,
                ScheduledAtUtc = notification.ScheduledAtUtc,
                Status = notification.Status,
                CreatedAt = notification.CreatedAt,
                UpdatedAt = notification.UpdatedAt,
                Deliveries = notification.Deliveries
                    .OrderBy(delivery => delivery.CreatedAt)
                    .Select(delivery => new NotificationDeliveryDto
                    {
                        Id = delivery.Id,
                        Channel = delivery.Channel,
                        Destination = delivery.Destination,
                        Provider = delivery.Provider,
                        Status = delivery.Status,
                        AttemptCount = delivery.AttemptCount,
                        MaxAttempts = delivery.MaxAttempts,
                        NextAttemptAtUtc = delivery.NextAttemptAtUtc,
                        LastAttemptAtUtc = delivery.LastAttemptAtUtc,
                        ProviderMessageId = delivery.ProviderMessageId,
                        ErrorCode = delivery.ErrorCode,
                        ErrorMessage = delivery.ErrorMessage,
                        IdempotencyKey = delivery.IdempotencyKey
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
