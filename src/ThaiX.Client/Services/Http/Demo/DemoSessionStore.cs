using ThaiX.Client.Constants;
using ThaiX.Client.Models.ApiClients;
using ThaiX.Client.Models.AssetPositions;
using ThaiX.Client.Models.Blog;
using ThaiX.Client.Models.Contacts;
using ThaiX.Client.Models.CredentialAccounts;
using ThaiX.Client.Models.ExternalData;
using ThaiX.Client.Models.Lottery;
using ThaiX.Client.Models.MarketScanner;
using ThaiX.Client.Models.MasterData;
using ThaiX.Client.Models.Notes;
using ThaiX.Client.Models.Notifications;
using ThaiX.Client.Models.Portfolios;
using ThaiX.Client.Models.PriceAlerts;
using ThaiX.Client.Models.Resumes;
using ThaiX.Client.Models.Trading;
using ThaiX.Client.Models.Users;
using ThaiX.Client.Services.Notifications;

namespace ThaiX.Client.Services.Http.Demo;

/// <summary>
/// In-memory demo data for the browser session (singleton).
/// Lists are Bogus-generated and seeded lazily by area to keep first page loads fast.
/// </summary>
public sealed class DemoSessionStore
{
    private static readonly IReadOnlyList<string> CachedPermissions = PermissionNames.GetAll().ToList();

    private readonly object _gate = new();
    private bool _coreSeeded;
    private bool _marketSeeded;

    public List<CountryListItemDto> Countries { get; } = [];
    public List<CityListItemDto> Cities { get; } = [];
    public List<DistrictListItemDto> Districts { get; } = [];
    public List<BankListItemDto> Banks { get; } = [];
    public List<UserListItemDto> Users { get; } = [];
    public List<ContactListItemDto> Contacts { get; } = [];
    public List<PortfolioListItemDto> Portfolios { get; } = [];
    public List<StockPositionListItemDto> StockPositions { get; } = [];
    public List<CryptoPositionListItemDto> CryptoPositions { get; } = [];
    public List<SavingPositionListItemDto> SavingPositions { get; } = [];
    public List<NoteDto> Notes { get; } = [];
    public List<CredentialAccountDto> CredentialAccounts { get; } = [];
    public List<CredentialAccountAuditDto> CredentialAudits { get; } = [];
    public List<PostListItemDto> BlogPosts { get; } = [];
    public List<PublishedPostListItemDto> PublishedPosts { get; } = [];
    public List<TagDto> BlogTags { get; } = [];
    public List<CategoryDto> BlogCategories { get; } = [];
    public List<NotificationScheduleListItemDto> NotificationSchedules { get; } = [];
    public List<PriceAlertListItemDto> PriceAlerts { get; } = [];
    public List<MarketScannerRuleListItemDto> MarketScannerRules { get; } = [];
    public List<ApiClientDto> ApiClients { get; } = [];
    public List<ChainBrokerFundDto> ChainBrokerFunds { get; } = [];
    public List<ChainBrokerProjectDto> ChainBrokerProjects { get; } = [];
    public List<ChainBrokerUnlockDto> ChainBrokerUnlocks { get; } = [];
    public List<CommodityDto> Commodities { get; } = [];
    public List<CurrencyDto> Currencies { get; } = [];
    public List<CryptocurrencyDto> Cryptocurrencies { get; } = [];
    public List<BankInterestRateGridItemDto> BankInterestRates { get; } = [];
    public List<BankDepositRateItemDto> BankDepositRatesOnline { get; } = [];
    public List<BankDepositRateItemDto> BankDepositRatesOffline { get; } = [];
    public List<SacombankExchangeRateItemDto> SacombankExchangeRates { get; } = [];
    public List<CoinGeckoCoinDto> CoinGeckoCoins { get; } = [];
    public List<VnDirectChangePriceItemDto> VnDirectChangePrices { get; } = [];
    public List<VnDirectTopStockDto> VnDirectTopStocks { get; } = [];
    public List<MexcContractTickerDto> MexcContractTickers { get; } = [];
    public List<MexcSpotTicker24HrDto> MexcSpotTickers { get; } = [];
    public List<TcbsTop10PortfolioDto> TcbsTop10Portfolios { get; } = [];
    public List<WeeklySuggestionHistoryReportDto> WeeklySuggestionReports { get; } = [];
    public List<Power655DrawDto> Power655Draws { get; } = [];
    public Dictionary<Guid, ContactImportJobDto> ContactImportJobs { get; } = new();
    public List<UserNotificationPreferenceDto> NotificationPreferences { get; } = [];
    public Dictionary<Guid, UploadedFileDto> Files { get; } = new();
    public ResumeDto Resume { get; private set; } = DemoDataGenerators.DemoResume();

    /// <summary>Admin/CRM/blog/portfolio lists used by most pages.</summary>
    public void EnsureCoreSeeded()
    {
        if (_coreSeeded)
        {
            return;
        }

        lock (_gate)
        {
            if (_coreSeeded)
            {
                return;
            }

            SeedCoreUnlocked();
            _coreSeeded = true;
        }
    }

    /// <summary>Market / external-data / lottery / trading history — generated only when those pages are hit.</summary>
    public void EnsureMarketSeeded()
    {
        if (_marketSeeded)
        {
            return;
        }

        lock (_gate)
        {
            if (_marketSeeded)
            {
                return;
            }

            SeedMarketUnlocked();
            _marketSeeded = true;
        }
    }

    public void Seed()
    {
        lock (_gate)
        {
            ClearAllUnlocked();
            Resume = DemoDataGenerators.DemoResume();
            SeedCoreUnlocked();
            SeedMarketUnlocked();
            _coreSeeded = true;
            _marketSeeded = true;
        }
    }

    public T WithLock<T>(Func<T> action)
    {
        lock (_gate)
        {
            return action();
        }
    }

    public void WithLock(Action action)
    {
        lock (_gate)
        {
            action();
        }
    }

    public static IReadOnlyList<string> AllPermissions => CachedPermissions;

    private void ClearAllUnlocked()
    {
        Countries.Clear();
        Cities.Clear();
        Districts.Clear();
        Banks.Clear();
        Users.Clear();
        Contacts.Clear();
        Portfolios.Clear();
        StockPositions.Clear();
        CryptoPositions.Clear();
        SavingPositions.Clear();
        Notes.Clear();
        CredentialAccounts.Clear();
        CredentialAudits.Clear();
        BlogPosts.Clear();
        PublishedPosts.Clear();
        BlogTags.Clear();
        BlogCategories.Clear();
        NotificationSchedules.Clear();
        PriceAlerts.Clear();
        MarketScannerRules.Clear();
        ApiClients.Clear();
        ChainBrokerFunds.Clear();
        ChainBrokerProjects.Clear();
        ChainBrokerUnlocks.Clear();
        Commodities.Clear();
        Currencies.Clear();
        Cryptocurrencies.Clear();
        BankInterestRates.Clear();
        BankDepositRatesOnline.Clear();
        BankDepositRatesOffline.Clear();
        SacombankExchangeRates.Clear();
        CoinGeckoCoins.Clear();
        VnDirectChangePrices.Clear();
        VnDirectTopStocks.Clear();
        MexcContractTickers.Clear();
        MexcSpotTickers.Clear();
        TcbsTop10Portfolios.Clear();
        WeeklySuggestionReports.Clear();
        Power655Draws.Clear();
        ContactImportJobs.Clear();
        NotificationPreferences.Clear();
        Files.Clear();
        _coreSeeded = false;
        _marketSeeded = false;
    }

    private void SeedCoreUnlocked()
    {
        // Stable anchors so parent filters (VN / HCM) always return child rows.
        Countries.Add(DemoDataGenerators.VietnamCountry());
        Countries.AddRange(DemoDataGenerators.GenerateList(
            DemoDataGenerators.Countries(),
            DemoDataGenerators.DefaultListCount - 1));

        Cities.Add(DemoDataGenerators.HoChiMinhCity());
        // Bias enough rows under VN / HCM so parent filters are never empty in the UI.
        var vietnamOnly = new[] { DemoDataGenerators.VietnamCountry() };
        var vnCityCount = Math.Max(20, DemoDataGenerators.DefaultListCount / 3);
        Cities.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.Cities(vietnamOnly), vnCityCount));
        Cities.AddRange(DemoDataGenerators.GenerateList(
            DemoDataGenerators.Cities(Countries),
            Math.Max(0, DemoDataGenerators.DefaultListCount - 1 - vnCityCount)));

        var hcmOnly = new[] { DemoDataGenerators.HoChiMinhCity() };
        var hcmDistrictCount = Math.Max(20, DemoDataGenerators.DefaultListCount / 3);
        Districts.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.Districts(hcmOnly), hcmDistrictCount));
        Districts.AddRange(DemoDataGenerators.GenerateList(
            DemoDataGenerators.Districts(Cities),
            Math.Max(0, DemoDataGenerators.DefaultListCount - hcmDistrictCount)));

        Banks.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.Banks(vietnamOnly), vnCityCount));
        Banks.AddRange(DemoDataGenerators.GenerateList(
            DemoDataGenerators.Banks(Countries),
            Math.Max(0, DemoDataGenerators.DefaultListCount - vnCityCount)));

        Users.Add(new UserListItemDto
        {
            Id = Guid.Parse(DemoJwtFactory.DemoUserId),
            Email = DemoJwtFactory.DemoEmail,
            EmailConfirmed = true,
            LockoutEnabled = false
        });
        Users.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.Users(), DemoDataGenerators.DefaultListCount - 1));

        Contacts.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.Contacts()));
        Portfolios.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.Portfolios()));

        // Spread positions across a few portfolios so detail tabs are not empty.
        var seedPortfolioIds = Portfolios.Take(5).Select(p => p.Id).ToArray();
        var perPortfolio = Math.Max(1, DemoDataGenerators.DefaultListCount / Math.Max(1, seedPortfolioIds.Length));
        foreach (var portfolioId in seedPortfolioIds)
        {
            StockPositions.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.StockPositions(portfolioId), perPortfolio));
            CryptoPositions.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.CryptoPositions(portfolioId), perPortfolio));
            SavingPositions.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.SavingPositions(portfolioId), perPortfolio));
        }

        Notes.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.Notes()));
        CredentialAccounts.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.CredentialAccounts()));
        foreach (var account in CredentialAccounts.Take(10))
        {
            for (var i = 0; i < 3; i++)
            {
                CredentialAudits.Add(new CredentialAccountAuditDto
                {
                    Id = Guid.NewGuid(),
                    CredentialAccountId = account.Id,
                    Action = i % 2 == 0 ? "ViewPassword" : "CopyPassword",
                    UserId = Guid.Parse(DemoJwtFactory.DemoUserId),
                    UserName = DemoJwtFactory.DemoEmail,
                    CreatedAt = DateTime.UtcNow.AddDays(-i).AddHours(-i)
                });
            }
        }

        BlogPosts.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.BlogPosts()));
        PublishedPosts.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.PublishedPosts()));
        BlogTags.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.BlogTags()));
        BlogCategories.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.BlogCategories()));
        NotificationSchedules.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.NotificationSchedules()));
        PriceAlerts.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.PriceAlerts()));
        MarketScannerRules.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.MarketScannerRules()));
        ApiClients.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.ApiClients()));

        var demoUserId = Guid.Parse(DemoJwtFactory.DemoUserId);
        NotificationPreferences.AddRange(
        [
            new UserNotificationPreferenceDto
            {
                Id = Guid.NewGuid(),
                UserId = demoUserId,
                Kind = NotificationKind.PriceAlert,
                Channel = NotificationChannel.Telegram,
                Enabled = true,
                Destination = "@demo_alerts",
                MinimumSeverity = NotificationPreferenceSeverity.Info,
                TimeZoneId = "Asia/Ho_Chi_Minh",
                BatchingMode = NotificationBatchingMode.Immediate
            },
            new UserNotificationPreferenceDto
            {
                Id = Guid.NewGuid(),
                UserId = demoUserId,
                Kind = NotificationKind.SystemAlert,
                Channel = NotificationChannel.Email,
                Enabled = true,
                Destination = DemoJwtFactory.DemoEmail,
                MinimumSeverity = NotificationPreferenceSeverity.Warning,
                QuietHoursStart = new TimeOnly(22, 0),
                QuietHoursEnd = new TimeOnly(7, 0),
                TimeZoneId = "Asia/Ho_Chi_Minh",
                BatchingMode = NotificationBatchingMode.Digest
            }
        ]);
    }

    private void SeedMarketUnlocked()
    {
        ChainBrokerFunds.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.ChainBrokerFunds()));
        ChainBrokerProjects.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.ChainBrokerProjects()));
        ChainBrokerUnlocks.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.ChainBrokerUnlocks()));
        Commodities.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.Commodities()));
        Currencies.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.Currencies()));
        Cryptocurrencies.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.Cryptocurrencies()));
        BankInterestRates.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.BankInterestRates()));
        BankDepositRatesOnline.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.BankDepositRatesOnline()));
        BankDepositRatesOffline.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.BankDepositRatesOffline()));
        SacombankExchangeRates.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.SacombankExchangeRates()));
        CoinGeckoCoins.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.CoinGeckoCoins()));
        VnDirectChangePrices.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.VnDirectChangePrices()));
        VnDirectTopStocks.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.VnDirectTopStocks()));
        MexcContractTickers.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.MexcContractTickers()));
        MexcSpotTickers.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.MexcSpotTickers()));
        TcbsTop10Portfolios.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.TcbsTop10Portfolios()));
        WeeklySuggestionReports.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.WeeklySuggestionReports()));
        Power655Draws.AddRange(DemoDataGenerators.GenerateList(DemoDataGenerators.Power655Draws()));
    }
}
