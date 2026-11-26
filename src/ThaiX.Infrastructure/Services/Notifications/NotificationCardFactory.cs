using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

internal static class NotificationCardFactory
{
    public static NotificationCard FromMessage(NotificationMessage message)
    {
        var kindLabel = NotificationBrand.KindLabel(message.Kind);
        var sections = new List<NotificationSection>();

        if (!string.IsNullOrWhiteSpace(message.Text))
        {
            sections.Add(new NotificationSection
            {
                Title = "Details",
                Text = message.Text.Trim()
            });
        }

        sections.AddRange(message.Sections);

        return new NotificationCard
        {
            Category = kindLabel,
            Title = string.IsNullOrWhiteSpace(message.Title) ? kindLabel : message.Title.Trim(),
            Type = GetCardType(message.Kind, message.Severity),
            Severity = message.Severity,
            Sections = sections,
            Actions = message.Actions.ToArray()
        };
    }

    private static NotificationCardType GetCardType(NotificationKind kind, NotificationSeverity severity)
    {
        return kind switch
        {
            NotificationKind.MarketOverview or NotificationKind.PortfolioUpdate or NotificationKind.DataSync
                => NotificationCardType.Summary,
            NotificationKind.PriceUpdate
                => NotificationCardType.Metric,
            NotificationKind.PriceAlert
                => NotificationCardType.Alert,
            NotificationKind.SystemAlert when severity >= NotificationSeverity.Error
                => NotificationCardType.Error,
            NotificationKind.SystemAlert
                => NotificationCardType.Alert,
            NotificationKind.CryptoSpike or NotificationKind.GoodEntryCrypto or
                NotificationKind.GoodEntryStocks or NotificationKind.MarketScanner or NotificationKind.TradeSignal
                => NotificationCardType.Signal,
            _ => NotificationCardType.Summary
        };
    }
}
