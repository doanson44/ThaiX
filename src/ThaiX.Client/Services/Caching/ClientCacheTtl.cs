namespace ThaiX.Client.Services.Caching;

public static class ClientCacheTtl
{
    public static readonly TimeSpan Ticker = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan Depth = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan Funding = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan VnDirect = TimeSpan.FromMinutes(2);
    public static readonly TimeSpan BankRates = TimeSpan.FromMinutes(3);
    public static readonly TimeSpan StockHistory = TimeSpan.FromMinutes(3);
    public static readonly TimeSpan CoinGeckoList = TimeSpan.FromMinutes(20);
    public static readonly TimeSpan Polling = TimeSpan.FromSeconds(45);
    public static readonly TimeSpan AuditLogs = TimeSpan.FromSeconds(30);
    public static readonly TimeSpan TradeSuggestion = TimeSpan.FromSeconds(45);
}
