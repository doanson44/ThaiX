namespace ThaiX.Domain.Aggregates.Notifications;

public enum NotificationKind
{
    MarketOverview = 1,
    PriceUpdate = 2,
    CryptoSpike = 3,
    GoodEntryCrypto = 4,
    GoodEntryStocks = 5,
    SystemAlert = 6,
    DataSync = 7,
    MarketScanner = 8,
    PriceAlert = 9,
    PortfolioUpdate = 10,
    TradeSignal = 11
}
