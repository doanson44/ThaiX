namespace ThaiX.Client.Services.Caching;

/// <summary>
/// Client-side cache group identifiers for invalidation (WASM in-memory cache only).
/// </summary>
public static class CacheGroups
{
    // Master data
    public const string Country = "master-data:country";
    public const string City = "master-data:city";
    public const string District = "master-data:district";
    public const string Bank = "master-data:bank";
    public const string MasterDataSymbol = "master-data:symbol";

    // Reference lookups
    public const string UserPermissionCatalog = "users:permission-catalog";
    public const string ApiClientScopes = "api-clients:scopes";
    public const string SlackChannels = "slack:channels";
    public const string TelegramChannels = "telegram:channels";

    // CRM & admin
    public const string Contact = "contacts";
    public const string User = "users";
    public const string CurrentUserProfile = "users:me-profile";
    public const string UserPermissions = "users:permissions";
    public const string UserLinkedContact = "users:linked-contact";
    public const string Note = "notes";
    public const string CredentialAccount = "credential-accounts";
    public const string ApiClient = "api-clients";
    public const string ContactSuggestedUsers = "contacts:suggested-users";
    public const string CredentialAccountAudit = "credential-accounts:audit";

    // Content & config
    public const string BlogPost = "blog:posts";
    public const string BlogPostPublic = "blog:posts:public";
    public const string BlogCategory = "blog:categories";
    public const string BlogTag = "blog:tags";
    public const string NotificationSchedule = "notification-schedules";
    public const string NotificationScheduleUpcoming = "notification-schedules:upcoming";
    public const string NotificationScheduleExecutions = "notification-schedules:executions";
    public const string MarketScannerRule = "market-scanner:rules";
    public const string PriceAlert = "price-alerts";
    public const string LotteryAnalysis = "lottery:power655:analysis";
    public const string ResumePublic = "resumes:public";

    // Portfolios & positions
    public const string Portfolio = "portfolios";
    public const string CryptoPosition = "positions:crypto";
    public const string StockPosition = "positions:stock";
    public const string SavingPosition = "positions:saving";
    public const string PositionTransaction = "positions:transactions";

    // External synced data
    public const string ChainBrokerFunds = "external:chainbroker:funds";
    public const string ChainBrokerProjects = "external:chainbroker:projects";
    public const string ChainBrokerUnlocks = "external:chainbroker:unlocks";
    public const string TcbsTop10 = "external:tcbs-top10";
    public const string DragonCapitalPortfolio = "external:dragon-capital:portfolio";
    public const string ExternalLotteryPower655 = "external:lottery:power655";
    public const string WeeklySuggestionHistory = "trading:weekly-suggestion-history";
    public const string CoinGeckoCoinsList = "external:coingecko:coins-list";

    // External market (short TTL)
    public const string ExternalBankRates = "external:bank-rates";
    public const string ExternalVnDirect = "external:vndirect";
    public const string ExternalExchangeMexc = "external:exchange:mexc";
    public const string ExternalExchangeBinance = "external:exchange:binance";
    public const string ExternalExchangeBybit = "external:exchange:bybit";
    public const string ExternalMarket = "external:market";
    public const string ExternalTradeSuggestion = "trading:suggestion";
    public const string ExpenseTracker = "expense-tracker";
}
