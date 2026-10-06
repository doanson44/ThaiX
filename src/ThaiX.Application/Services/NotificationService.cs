using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Services;

public sealed class NotificationService : INotificationService
{
    private const int DefaultMaxAttempts = 3;

    private readonly IApplicationDbContext _dbContext;
    private readonly INotificationTemplateProvider _templateProvider;
    private readonly INotificationRouteResolver _routeResolver;

    public NotificationService(
        IApplicationDbContext dbContext,
        INotificationTemplateProvider templateProvider,
        INotificationRouteResolver routeResolver)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _templateProvider = templateProvider ?? throw new ArgumentNullException(nameof(templateProvider));
        _routeResolver = routeResolver ?? throw new ArgumentNullException(nameof(routeResolver));
    }

    public async Task<Guid> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var (text, dataJson, subject) = BuildFromCard(request);

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Notification text is required.", nameof(request));
        }

        var template = _templateProvider.GetTemplate(request.Kind, request.TemplateKey);
        if (string.IsNullOrWhiteSpace(subject))
        {
            subject = template.DefaultSubject;
        }

        var deduplicationKey = string.IsNullOrWhiteSpace(request.DeduplicationKey)
            ? CreateHashKey(BuildDeduplicationSource(request, subject))
            : request.DeduplicationKey.Trim();

        var existing = await _dbContext.Notifications
            .Where(notification => notification.DeduplicationKey == deduplicationKey)
            .Select(notification => notification.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != Guid.Empty)
        {
            return existing;
        }

        var routes = await _routeResolver.ResolveAsync(
            new NotificationRouteRequest
            {
                Kind = request.Kind,
                Severity = request.Severity,
                RecipientUserId = request.RecipientUserId,
                Channels = request.Channels
            },
            cancellationToken);

        if (routes.Count == 0)
        {
            throw new InvalidOperationException("No notification delivery routes were resolved.");
        }

        var notification = Notification.Create(
            request.Kind,
            request.Severity,
            template.Key,
            subject,
            text,
            dataJson,
            request.RecipientUserId,
            request.SourceEventId,
            deduplicationKey,
            request.ScheduledAtUtc);

        foreach (var route in routes.DistinctBy(route =>
                     $"{route.Channel}:{route.Provider}:{route.Destination}".ToUpperInvariant()))
        {
            var idempotencyKey = CreateHashKey(
                $"{deduplicationKey}:{route.Channel}:{route.Provider}:{route.Destination}");

            notification.AddDelivery(
                route.Channel,
                route.Destination,
                route.Provider,
                idempotencyKey,
                DefaultMaxAttempts);
        }

        _dbContext.Notifications.Add(notification);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return notification.Id;
    }

    private static string BuildDeduplicationSource(CreateNotificationRequest request, string subject)
    {
        var channelSegment = request.Channels.Count == 0
            ? "default"
            : string.Join(",", request.Channels.OrderBy(static channel => channel));

        var cardJson = request.Card?.ToJson() ?? string.Empty;

        return string.Join(
            "|",
            request.Kind,
            request.Severity,
            subject,
            cardJson,
            request.RecipientUserId?.ToString() ?? "system",
            request.SourceEventId ?? string.Empty,
            request.ScheduledAtUtc?.ToString("O") ?? string.Empty,
            channelSegment);
    }

    private static string CreateHashKey(string source)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static (string Text, string? DataJson, string? Subject) BuildFromCard(CreateNotificationRequest request)
    {
        if (request.Card is null)
        {
            return (request.Text, request.DataJson, request.Title);
        }

        var card = request.Card;
        var dataJson = card.ToJson();
        var subject = request.Title ?? card.Title;

        // Build plain text preview from card for DB storage and deduplication
        var textBuilder = new StringBuilder();
        textBuilder.AppendLine($"{card.Title} | {card.Category}");
        textBuilder.AppendLine($"Severity: {card.Severity}");

        foreach (var metric in card.Metrics)
        {
            textBuilder.AppendLine($"{metric.Label}: {metric.Value}");
        }

        foreach (var section in card.Sections)
        {
            if (!string.IsNullOrWhiteSpace(section.Title))
            {
                textBuilder.AppendLine(section.Title.Trim());
            }
            textBuilder.AppendLine(section.Text.Trim());
        }

        foreach (var action in card.Actions)
        {
            textBuilder.AppendLine($"{action.Label.Trim()}: {action.Url.Trim()}");
        }

        return (textBuilder.ToString().Trim(), dataJson, subject);
    }
}
