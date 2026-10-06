namespace ThaiX.Client.Constants;

/// <summary>
/// Route path segments used for navigation and breadcrumb. Values must match @page routes.
/// </summary>
public static class RouteSegments
{
    // Account
    public const string Profile = "account/profile";
    public const string ChangePassword = "account/change-password";
    public const string Account = "account";
    public const string TwoFa = "account/2fa";
    public const string PersonalData = "account/personal-data";
    public const string EnableAuthenticator = "account/enable-authenticator";

    // Auth
    public const string Login = "auth/login";
    public const string Register = "auth/register";
    public const string Logout = "auth/logout";
    public const string ForgotPassword = "auth/forgot-password";
    public const string ResetPassword = "auth/reset-password";
    public const string ConfirmEmail = "auth/confirm-email";
    public const string ResendConfirmation = "auth/resend-confirmation";
    public const string Lockout = "auth/lockout";
    public const string Login2Fa = "auth/login-2fa";
    public const string ExternalLoginCallback = "auth/external-login-callback";
    public const string Health = "auth/health";

    // Market Data
    public const string MarketData = "market-data";
    public const string BankInterestRates = "market-data/bank-interest-rates";
    public const string BankDepositRates = "market-data/bank-deposit-rates";
    public const string SacombankExchangeRates = "market-data/sacombank-exchange-rates";
    public const string TcbsTop10 = "market-data/tcbs-top10";
    public const string MexcContractTickerSocket = "market-data/mexc/contract-ticker-socket";
    public const string MexcSpotTickerSocket = "market-data/mexc/spot-ticker-socket";
    public const string Power655Analysis = "market-data/power-655-analysis";

    // Market Research
    public const string MarketResearch = "market-research";
    public const string CoinGecko = "market-research/coingecko";
    public const string VnDirectTopStocks = "market-research/vndirect/top-stocks";
    public const string MexcContractTicker = "market-research/mexc/contract-ticker";
    public const string MexcSpotTicker = "market-research/mexc/spot-ticker";
    public const string DragonCapitalFunds = "market-research/dragon-capital";
    public const string ChainBroker = "market-research/chainbroker";
    public const string ChainBrokerFunds = "market-research/chainbroker/funds";
    public const string ChainBrokerProjects = "market-research/chainbroker/projects";
    public const string ChainBrokerUnlocks = "market-research/chainbroker/unlocks";

    // Portfolio
    public const string Portfolio = "portfolio";
    public const string Portfolios = "portfolio/list";
    public const string PortfolioDetail = "portfolio/{Id:guid}";
    public const string PriceAlerts = "portfolio/price-alerts";
    public const string MarketScannerRules = "portfolio/market-scanner-rules";
    public const string ExpenseTracker = "portfolio/expense-tracker";

    // CRM
    public const string Crm = "crm";
    public const string Contacts = "crm/contacts";
    public const string Notes = "crm/notes";

    // Master Data
    public const string MasterData = "master-data";
    public const string Countries = "master-data/countries";
    public const string Cities = "master-data/cities";
    public const string Districts = "master-data/districts";
    public const string Banks = "master-data/banks";

    // Notifications
    public const string Notifications = "notifications";
    public const string NotificationSender = "notifications/sender";
    public const string NotificationSchedules = "notifications/schedules";

    // Security
    public const string Security = "security";
    public const string ApiClients = "security/api-clients";
    public const string CredentialAccounts = "security/credential-accounts";

    // Administration
    public const string Admin = "admin";
    public const string Users = "admin/users";
    public const string SlackCommands = "admin/command-tester";
    public const string AiTester = "admin/ai-tester";
    public const string JsonBins = "admin/json-bins";
    public const string JsonBinCreate = "admin/json-bins/create";
    public const string JsonBinEdit = "admin/json-bins/{Id:guid}";
    public const string JsonBinSharePublic = "public/json-bins/share/{Token}";

    // Legacy compatibility (keep old constants for existing code references)
    public const string Mexc = "market-research/mexc";
}
