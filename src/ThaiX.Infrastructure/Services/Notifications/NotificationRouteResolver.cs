using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Infrastructure.Configuration;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class NotificationRouteResolver : INotificationRouteResolver
{
    private const string SlackProvider = "Slack";
    private const string TelegramProvider = "Telegram";

    private readonly IApplicationDbContext _dbContext;
    private readonly NotificationSettings _notificationSettings;
    private readonly SlackSettings _slackSettings;
    private readonly TelegramSettings _telegramSettings;
    private readonly ILogger<NotificationRouteResolver> _logger;

    public NotificationRouteResolver(
        IApplicationDbContext dbContext,
        IOptions<NotificationSettings> notificationOptions,
        IOptions<SlackSettings> slackOptions,
        IOptions<TelegramSettings> telegramOptions,
        ILogger<NotificationRouteResolver> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _notificationSettings = notificationOptions?.Value ?? throw new ArgumentNullException(nameof(notificationOptions));
        _slackSettings = slackOptions?.Value ?? throw new ArgumentNullException(nameof(slackOptions));
        _telegramSettings = telegramOptions?.Value ?? throw new ArgumentNullException(nameof(telegramOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IReadOnlyList<NotificationRoute>> ResolveAsync(
        NotificationRouteRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.RecipientUserId is { } userId)
        {
            var preferenceRoutes = await ResolveUserPreferenceRoutesAsync(
                userId,
                request,
                cancellationToken);

            if (preferenceRoutes.Count > 0)
            {
                return preferenceRoutes;
            }
        }

        return ResolveConfiguredRoutes(request);
    }

    private async Task<IReadOnlyList<NotificationRoute>> ResolveUserPreferenceRoutesAsync(
        Guid userId,
        NotificationRouteRequest request,
        CancellationToken cancellationToken)
    {
        var requestedChannels = request.Channels.ToArray();

        var query = _dbContext.UserNotificationPreferences
            .Where(preference =>
                preference.UserId == userId
                && preference.Kind == request.Kind
                && preference.Enabled);

        if (requestedChannels.Length > 0)
        {
            query = query.Where(preference => requestedChannels.Contains(preference.Channel));
        }

        var preferences = await query.ToListAsync(cancellationToken);

        return preferences
            .Where(preference => preference.Allows(request.Severity))
            .Select(preference => new NotificationRoute
            {
                Channel = preference.Channel,
                Provider = preference.Channel.ToString(),
                Destination = preference.Destination
            })
            .ToList();
    }

    private IReadOnlyList<NotificationRoute> ResolveConfiguredRoutes(NotificationRouteRequest request)
    {
        var channels = request.Channels.Count > 0
            ? request.Channels
            : ResolveConfiguredTargetChannels(request.Kind);

        var routes = new List<NotificationRoute>();
        foreach (var channel in channels)
        {
            switch (channel)
            {
                case NotificationChannel.Slack:
                    routes.Add(new NotificationRoute
                    {
                        Channel = NotificationChannel.Slack,
                        Provider = SlackProvider,
                        Destination = ResolveDestination(
                            _slackSettings.Routing,
                            _slackSettings.Channels,
                            request.Kind.ToString(),
                            SlackProvider)
                    });
                    break;

                case NotificationChannel.Telegram:
                    routes.Add(new NotificationRoute
                    {
                        Channel = NotificationChannel.Telegram,
                        Provider = TelegramProvider,
                        Destination = ResolveDestination(
                            _telegramSettings.Routing,
                            _telegramSettings.Channels,
                            request.Kind.ToString(),
                            TelegramProvider)
                    });
                    break;

                default:
                    _logger.LogWarning(
                        "Notification channel {Channel} has no configured provider route",
                        channel);
                    break;
            }
        }

        return routes;
    }

    private IReadOnlyCollection<NotificationChannel> ResolveConfiguredTargetChannels(NotificationKind kind)
    {
        var eventKey = kind.ToString();
        if (!_notificationSettings.Targets.TryGetValue(eventKey, out var targetValue)
            || string.IsNullOrWhiteSpace(targetValue))
        {
            return [NotificationChannel.Slack, NotificationChannel.Telegram];
        }

        if (!Enum.TryParse<NotificationTarget>(targetValue, ignoreCase: true, out var target))
        {
            _logger.LogWarning(
                "Invalid notification target {Target} for {Kind}; falling back to both channels",
                targetValue,
                kind);
            return [NotificationChannel.Slack, NotificationChannel.Telegram];
        }

        return target switch
        {
            NotificationTarget.Slack => [NotificationChannel.Slack],
            NotificationTarget.Telegram => [NotificationChannel.Telegram],
            NotificationTarget.Both => [NotificationChannel.Slack, NotificationChannel.Telegram],
            _ => [NotificationChannel.Slack, NotificationChannel.Telegram]
        };
    }

    private static string ResolveDestination(
        IReadOnlyDictionary<string, string> routing,
        IReadOnlyDictionary<string, Dictionary<string, string>> channels,
        string eventKey,
        string provider)
    {
        if ((!routing.TryGetValue(eventKey, out var routePath) || string.IsNullOrWhiteSpace(routePath))
            && (!routing.TryGetValue("Default", out routePath) || string.IsNullOrWhiteSpace(routePath)))
        {
            throw new InvalidOperationException(
                $"{provider} configuration is invalid: routing for event '{eventKey}' is missing.");
        }

        var segments = routePath.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length is not 1 and not 2)
        {
            throw new InvalidOperationException(
                $"{provider} configuration is invalid: routing path '{routePath}' is not valid.");
        }

        var channelGroup = segments[0];
        var channelKey = segments.Length == 1 ? segments[0] : segments[1];

        if (!channels.TryGetValue(channelGroup, out var group) || group is null)
        {
            throw new InvalidOperationException(
                $"{provider} configuration is invalid: channel group '{channelGroup}' was not found.");
        }

        if (!group.TryGetValue(channelKey, out var destination) || string.IsNullOrWhiteSpace(destination))
        {
            throw new InvalidOperationException(
                $"{provider} configuration is invalid: channel segment '{channelKey}' was not found.");
        }

        return destination.Trim();
    }
}
