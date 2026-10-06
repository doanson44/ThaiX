using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class DefaultNotificationTemplateProvider : INotificationTemplateProvider
{
    public NotificationTemplate GetTemplate(NotificationKind kind, string? templateKey)
    {
        var key = string.IsNullOrWhiteSpace(templateKey)
            ? $"{kind}.Default"
            : templateKey.Trim();

        return new NotificationTemplate
        {
            Key = key,
            Kind = kind,
            DefaultSubject = GetDefaultSubject(kind)
        };
    }

    private static string GetDefaultSubject(NotificationKind kind)
    {
        return kind switch
        {
            NotificationKind.MarketOverview => "Market overview",
            NotificationKind.PriceUpdate => "Price update",
            NotificationKind.CryptoSpike => "Crypto spike",
            NotificationKind.GoodEntryCrypto => "Crypto trade signal",
            NotificationKind.GoodEntryStocks => "Stock trade signal",
            NotificationKind.SystemAlert => "System alert",
            NotificationKind.DataSync => "Data sync",
            NotificationKind.MarketScanner => "Market scanner",
            NotificationKind.PriceAlert => "Price alert",
            NotificationKind.PortfolioUpdate => "Portfolio update",
            NotificationKind.TradeSignal => "Trade signal",
            _ => "Notification"
        };
    }
}
