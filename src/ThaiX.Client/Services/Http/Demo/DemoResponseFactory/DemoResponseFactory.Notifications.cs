using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ThaiX.Client.Models.Ai;
using ThaiX.Client.Models.ApiClients;
using ThaiX.Client.Models.AssetPositions;
using ThaiX.Client.Models.Auth;
using ThaiX.Client.Models.Blog;
using ThaiX.Client.Models.Contacts;
using ThaiX.Client.Models.CredentialAccounts;
using ThaiX.Client.Models.ExpenseTracker;
using ThaiX.Client.Models.ExternalData;
using ThaiX.Client.Models.JsonBins;
using ThaiX.Client.Models.Lottery;
using ThaiX.Client.Models.MasterData;
using ThaiX.Client.Models.MarketScanner;
using ThaiX.Client.Models.Notes;
using ThaiX.Client.Models.Notifications;
using ThaiX.Client.Models.Portfolios;
using ThaiX.Client.Models.PriceAlerts;
using ThaiX.Client.Models.Resumes;
using ThaiX.Client.Models.Slack;
using ThaiX.Client.Models.Trading;
using ThaiX.Client.Models.Users;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Notifications;


namespace ThaiX.Client.Services.Http.Demo;

public sealed partial class DemoResponseFactory
{
    private async Task<HttpResponseMessage> HandleNotificationSchedulesAsync(
        string method,
        string path,
        string query,
        int pageNumber,
        int pageSize,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        Guid entityId = default;
        var hasId = segments.Length >= 3 && Guid.TryParse(segments[2], out entityId);
        var action = segments.Length >= 4 ? segments[3].ToLowerInvariant() : string.Empty;

        if (method == "GET" && path.EndsWith("/upcoming", StringComparison.OrdinalIgnoreCase))
        {
            var countText = GetQueryValue(query, "count");
            var count = int.TryParse(countText, out var c) && c > 0 ? Math.Min(c, 50) : 10;
            return _store.WithLock(() =>
            {
                var upcoming = _store.NotificationSchedules
                    .Where(x => x.NextExecuteAtUtc.HasValue)
                    .OrderBy(x => x.NextExecuteAtUtc)
                    .Take(count)
                    .ToList();
                return DemoEnvelope.SuccessData(upcoming);
            });
        }

        if (method == "GET" && !hasId)
        {
            return _store.WithLock(() =>
            {
                var search = GetSearchTerm(query);
                var status = GetQueryValue(query, "status");
                IEnumerable<NotificationScheduleListItemDto> rows = _store.NotificationSchedules;
                if (!string.IsNullOrWhiteSpace(search))
                    rows = rows.Where(x => x.Name.Contains(search, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(status))
                    rows = rows.Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
                return DemoEnvelope.Paged(rows.ToList(), pageNumber, pageSize);
            });
        }

        if (hasId && action.Equals("executions", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            var executions = Enumerable.Range(1, 12).Select(i => new ScheduleExecutionDto
            {
                Id = Guid.NewGuid(),
                ScheduleId = entityId,
                Status = i % 5 == 0 ? "Failed" : "Succeeded",
                OccurrenceTimeUtc = DateTime.UtcNow.AddDays(-i),
                OccurrenceOrdinal = i,
                TriggeredAtUtc = DateTime.UtcNow.AddDays(-i).AddMinutes(1),
                CompletedAtUtc = DateTime.UtcNow.AddDays(-i).AddMinutes(2),
                ErrorMessage = i % 5 == 0 ? "Demo failure" : null
            }).ToList();
            return DemoEnvelope.Paged(executions, pageNumber, pageSize);
        }

        if (hasId && action.Equals("preview", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            var countText = GetQueryValue(query, "count");
            var count = int.TryParse(countText, out var c) && c > 0 ? Math.Min(c, 30) : 10;
            var preview = Enumerable.Range(1, count)
                .Select(i => DateTime.UtcNow.Date.AddDays(i).AddHours(1))
                .ToList();
            return DemoEnvelope.SuccessData(preview);
        }

        if (hasId && action.Equals("run-now", StringComparison.OrdinalIgnoreCase) && method == "POST")
        {
            return DemoEnvelope.SuccessData(new ScheduleExecutionDto
            {
                Id = Guid.NewGuid(),
                ScheduleId = entityId,
                Status = "Succeeded",
                OccurrenceTimeUtc = DateTime.UtcNow,
                OccurrenceOrdinal = 1,
                TriggeredAtUtc = DateTime.UtcNow,
                CompletedAtUtc = DateTime.UtcNow
            });
        }

        if (hasId && method == "POST" && action is "activate" or "pause" or "resume" or "disable")
        {
            if (action == "resume")
            {
                return _store.WithLock(() =>
                {
                    var item = _store.NotificationSchedules.FirstOrDefault(x => x.Id == entityId);
                    if (item is not null)
                    {
                        _store.NotificationSchedules.Remove(item);
                        _store.NotificationSchedules.Insert(0, item with { Status = "Active" });
                    }

                    return DemoEnvelope.Success();
                });
            }

            return DemoEnvelope.Success();
        }

        if (method == "GET" && hasId)
        {
            return _store.WithLock(() =>
            {
                var item = _store.NotificationSchedules.FirstOrDefault(x => x.Id == entityId)
                           ?? _store.NotificationSchedules[0];
                return DemoEnvelope.SuccessData(ToScheduleDto(item));
            });
        }

        if (method == "POST" && !hasId)
        {
            var body = await ReadJsonAsync<CreateNotificationScheduleRequest>(request, cancellationToken);
            return _store.WithLock(() =>
            {
                var id = Guid.NewGuid();
                var listItem = new NotificationScheduleListItemDto
                {
                    Id = id,
                    Name = body?.Name ?? "Demo Schedule",
                    Type = body?.Type ?? "EveryXDays",
                    Status = "Draft",
                    NextExecuteAtUtc = DateTime.UtcNow.AddDays(1),
                    FailureCount = 0,
                    ExecutionCount = 0
                };
                _store.NotificationSchedules.Insert(0, listItem);
                return DemoEnvelope.SuccessData(ToScheduleDto(listItem, body));
            });
        }

        return DemoEnvelope.Success();
    }

    private static NotificationScheduleDto ToScheduleDto(
        NotificationScheduleListItemDto item,
        CreateNotificationScheduleRequest? create = null)
    {
        Enum.TryParse<ScheduleStatus>(item.Status, ignoreCase: true, out var status);
        if (status == 0) status = ScheduleStatus.Draft;
        Enum.TryParse<ScheduleType>(item.Type, ignoreCase: true, out var type);
        if (type == 0) type = ScheduleType.EveryXDays;

        return new NotificationScheduleDto
        {
            Id = item.Id,
            Name = item.Name,
            Description = "Demo notification schedule",
            TemplateKey = create?.TemplateKey ?? "demo.template",
            Subject = create?.Subject ?? item.Name,
            Body = create?.Body ?? "Hello {{name}}, this is a demo notification.",
            DataJson = create?.DataJson,
            Recurrence = new ScheduleRecurrenceRequest
            {
                Type = type,
                IntervalDays = create?.IntervalDays ?? 1,
                MonthlyOverflowPolicy = MonthlyOverflowPolicy.SkipMonth,
                MisfirePolicy = MisfirePolicy.Skip
            },
            Status = status,
            TimeZoneId = create?.TimeZoneId ?? "Asia/Ho_Chi_Minh",
            ExecuteTimeLocal = TimeOnly.Parse(create?.ExecuteTimeLocal ?? "08:00:00"),
            NextExecuteAtUtc = item.NextExecuteAtUtc,
            LastTriggeredAtUtc = item.LastTriggeredAtUtc,
            FailureCount = item.FailureCount,
            ExecutionCount = item.ExecutionCount,
            CreatedAt = DateTime.UtcNow.AddDays(-14),
            UpdatedAt = DateTime.UtcNow
        };
    }

    private async Task<HttpResponseMessage> HandleNotificationsAsync(
        string method,
        string path,
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (path.EndsWith("/send", StringComparison.OrdinalIgnoreCase) && method == "POST")
            return DemoEnvelope.Success();

        if (path.Contains("/preferences/", StringComparison.OrdinalIgnoreCase) && method == "GET")
        {
            var userIdText = path.Split('/', StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            _ = Guid.TryParse(userIdText, out var userId);
            return _store.WithLock(() =>
            {
                var prefs = _store.NotificationPreferences
                    .Where(p => userId == Guid.Empty || p.UserId == userId)
                    .ToList();
                return DemoEnvelope.SuccessData(prefs);
            });
        }

        if (path.EndsWith("/preferences", StringComparison.OrdinalIgnoreCase) && method == "PUT")
        {
            var body = await ReadJsonAsync<UpsertUserNotificationPreferenceRequest>(request, cancellationToken);
            if (body is null)
                return DemoEnvelope.Success();

            return _store.WithLock(() =>
            {
                var existing = _store.NotificationPreferences.FirstOrDefault(p =>
                    p.UserId == body.UserId && p.Kind == body.Kind && p.Channel == body.Channel);
                if (existing is not null)
                    _store.NotificationPreferences.Remove(existing);

                _store.NotificationPreferences.Add(new UserNotificationPreferenceDto
                {
                    Id = existing?.Id ?? Guid.NewGuid(),
                    UserId = body.UserId,
                    Kind = body.Kind,
                    Channel = body.Channel,
                    Enabled = body.Enabled,
                    Destination = body.Destination,
                    MinimumSeverity = body.MinimumSeverity,
                    QuietHoursStart = body.QuietHoursStart,
                    QuietHoursEnd = body.QuietHoursEnd,
                    TimeZoneId = body.TimeZoneId,
                    BatchingMode = body.BatchingMode
                });
                return DemoEnvelope.Success();
            });
        }

        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (method == "GET" && segments.Length >= 3 && Guid.TryParse(segments[2], out var notificationId))
        {
            return DemoEnvelope.SuccessData(new NotificationDetailDto
            {
                Id = notificationId,
                EventType = NotificationEventType.SystemAlert,
                Title = "Demo notification",
                Text = "This is a demo notification lifecycle payload.",
                Severity = "Info",
                Status = "Delivered",
                CreatedAtUtc = DateTime.UtcNow.AddMinutes(-5),
                DeliveredAtUtc = DateTime.UtcNow.AddMinutes(-4),
                Channels = ["Telegram", "Email"]
            });
        }

        return DemoEnvelope.Success();
    }

}
