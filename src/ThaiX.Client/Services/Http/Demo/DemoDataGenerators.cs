using Bogus;
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
using ThaiX.Client.Models.Portfolios;
using ThaiX.Client.Models.PriceAlerts;
using ThaiX.Client.Models.Resumes;
using ThaiX.Client.Models.Trading;
using ThaiX.Client.Models.Users;
using ThaiX.Client.Services.Notifications;

namespace ThaiX.Client.Services.Http.Demo;

/// <summary>
/// Bogus factories for demo list DTOs. Fixed seed for stable UI checks.
/// </summary>
internal static class DemoDataGenerators
{
    /// <summary>
    /// Rows per list. Kept modest so Blazor WASM demo mode does not freeze on first seed.
    /// Still enough for multi-page grids (pageSize 10–20).
    /// </summary>
    public const int DefaultListCount = 80;

    static DemoDataGenerators()
    {
        Randomizer.Seed = new Random(42);
    }

    public static List<T> GenerateList<T>(Faker<T> faker, int count = DefaultListCount)
        where T : class =>
        faker.Generate(count);

    public static CountryListItemDto VietnamCountry() =>
        new()
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Code = "VN",
            Name = "Vietnam",
            DisplayText = "VN - Vietnam"
        };

    public static CityListItemDto HoChiMinhCity() =>
        new()
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            Code = "HCM",
            Name = "Ho Chi Minh",
            CountryCode = "VN",
            CountryName = "Vietnam",
            DisplayText = "HCM - Ho Chi Minh"
        };

    public static Faker<CountryListItemDto> Countries() =>
        new Faker<CountryListItemDto>("en")
            .CustomInstantiator(f =>
            {
                var code = f.Address.CountryCode();
                var name = f.Address.Country();
                return new CountryListItemDto
                {
                    Id = f.Random.Guid(),
                    Code = code.Length > 3 ? code[..3] : code,
                    Name = name,
                    DisplayText = $"{code} - {name}"
                };
            });

    public static Faker<CityListItemDto> Cities(IReadOnlyList<CountryListItemDto> countries) =>
        new Faker<CityListItemDto>("en")
            .CustomInstantiator(f =>
            {
                var parent = countries.Count > 0
                    ? countries[f.Random.Int(0, countries.Count - 1)]
                    : VietnamCountry();
                var code = f.Random.AlphaNumeric(3).ToUpperInvariant();
                var name = f.Address.City();
                return new CityListItemDto
                {
                    Id = f.Random.Guid(),
                    Code = code,
                    Name = name,
                    CountryCode = parent.Code,
                    CountryName = parent.Name,
                    DisplayText = $"{code} - {name}"
                };
            });

    public static Faker<DistrictListItemDto> Districts(IReadOnlyList<CityListItemDto> cities) =>
        new Faker<DistrictListItemDto>("en")
            .CustomInstantiator(f =>
            {
                var parent = cities.Count > 0
                    ? cities[f.Random.Int(0, cities.Count - 1)]
                    : HoChiMinhCity();
                var code = f.Random.AlphaNumeric(4).ToUpperInvariant();
                var name = f.Address.County();
                return new DistrictListItemDto
                {
                    Id = f.Random.Guid(),
                    Code = code,
                    Name = name,
                    CityCode = parent.Code,
                    CityName = parent.Name,
                    DisplayText = $"{code} - {name}"
                };
            });

    public static Faker<BankListItemDto> Banks(IReadOnlyList<CountryListItemDto> countries) =>
        new Faker<BankListItemDto>("en")
            .CustomInstantiator(f =>
            {
                var parent = countries.Count > 0
                    ? countries[f.Random.Int(0, countries.Count - 1)]
                    : VietnamCountry();
                var code = f.Random.AlphaNumeric(3).ToUpperInvariant();
                var name = f.Company.CompanyName() + " Bank";
                return new BankListItemDto
                {
                    Id = f.Random.Guid(),
                    Code = code,
                    Name = name,
                    CountryCode = parent.Code,
                    CountryName = parent.Name,
                    DisplayText = $"{code} - {name}"
                };
            });

    public static Faker<UserListItemDto> Users() =>
        new Faker<UserListItemDto>("en")
            .CustomInstantiator(f => new UserListItemDto
            {
                Id = f.Random.Guid(),
                Email = f.Internet.Email().ToLowerInvariant(),
                EmailConfirmed = f.Random.Bool(0.9f),
                LockoutEnabled = f.Random.Bool(0.1f),
                LockoutEnd = null
            });

    public static Faker<ContactListItemDto> Contacts() =>
        new Faker<ContactListItemDto>("en")
            .CustomInstantiator(f =>
            {
                var first = f.Name.FirstName();
                var last = f.Name.LastName();
                return new ContactListItemDto
                {
                    Id = f.Random.Guid(),
                    FirstName = first,
                    LastName = last,
                    DisplayName = $"{first} {last}",
                    PrimaryEmail = f.Internet.Email(),
                    PrimaryPhone = f.Phone.PhoneNumber(),
                    Company = f.Company.CompanyName(),
                    JobTitle = f.Name.JobTitle(),
                    AvatarUrl = DemoImageUrls.Avatar($"{first} {last}"),
                    IsArchived = f.Random.Bool(0.05f),
                    CreatedAt = f.Date.Past(2),
                    LastUpdated = f.Date.Recent()
                };
            });

    public static Faker<PortfolioListItemDto> Portfolios() =>
        new Faker<PortfolioListItemDto>("en")
            .CustomInstantiator(f =>
            {
                var type = f.PickRandom("Trading", "LongTerm", "Retirement", "Savings");
                var name = type switch
                {
                    "Trading" => $"{f.PickRandom("VN30", "Alpha", "Momentum", "Swing")} Trading",
                    "LongTerm" => $"{f.PickRandom("Blue Chip", "Dividend", "Core", "Value")} Long Term",
                    "Retirement" => $"{f.PickRandom("Pension", "Nest Egg", "FIRE", "Legacy")} Retirement",
                    _ => $"{f.PickRandom("VND", "USD", "Emergency", "Term")} Savings"
                };
                return new PortfolioListItemDto
                {
                    Id = f.Random.Guid(),
                    Name = name,
                    Description = f.Lorem.Sentence(),
                    PortfolioType = type,
                    CreatedAt = f.Date.Past(2),
                    UpdatedAt = f.Date.Recent()
                };
            });

    public static Faker<StockPositionListItemDto> StockPositions(Guid? portfolioId = null) =>
        new Faker<StockPositionListItemDto>("en")
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.PortfolioId, f => portfolioId ?? f.Random.Guid())
            .RuleFor(x => x.Symbol, f => f.PickRandom("VNM", "FPT", "HPG", "VCB", "MWG", "MSN", "VIC", "GAS"))
            .RuleFor(x => x.Exchange, f => f.PickRandom("HOSE", "HNX", "UPCOM"))
            .RuleFor(x => x.Quantity, f => f.Random.Decimal(10, 5000))
            .RuleFor(x => x.AverageEntryPrice, f => f.Random.Decimal(10, 200))
            .RuleFor(x => x.TotalInvested, (f, x) => x.Quantity * x.AverageEntryPrice)
            .RuleFor(x => x.RealizedPnl, f => f.Random.Decimal(-5000, 15000))
            .RuleFor(x => x.TargetPrice, f => f.Random.Decimal(20, 300))
            .RuleFor(x => x.StopLoss, f => f.Random.Decimal(5, 100))
            .RuleFor(x => x.IsClosed, f => f.Random.Bool(0.15f))
            .RuleFor(x => x.Note, f => f.Lorem.Sentence())
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(1));

    public static Faker<CryptoPositionListItemDto> CryptoPositions(Guid? portfolioId = null) =>
        new Faker<CryptoPositionListItemDto>("en")
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.PortfolioId, f => portfolioId ?? f.Random.Guid())
            .RuleFor(x => x.Symbol, f => f.PickRandom("BTC", "ETH", "BNB", "SOL", "XRP", "ADA", "DOGE"))
            .RuleFor(x => x.Quantity, f => f.Random.Decimal(0.001m, 50))
            .RuleFor(x => x.AverageEntryPrice, f => f.Random.Decimal(1, 70000))
            .RuleFor(x => x.TotalInvested, (f, x) => x.Quantity * x.AverageEntryPrice)
            .RuleFor(x => x.RealizedPnl, f => f.Random.Decimal(-2000, 20000))
            .RuleFor(x => x.TargetPrice, f => f.Random.Decimal(1, 100000))
            .RuleFor(x => x.StopLoss, f => f.Random.Decimal(1, 50000))
            .RuleFor(x => x.IsClosed, f => f.Random.Bool(0.1f))
            .RuleFor(x => x.Note, f => f.Lorem.Sentence())
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(1));

    public static Faker<SavingPositionListItemDto> SavingPositions(Guid? portfolioId = null) =>
        new Faker<SavingPositionListItemDto>("en")
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.PortfolioId, f => portfolioId ?? f.Random.Guid())
            .RuleFor(x => x.BankName, f => f.Company.CompanyName())
            .RuleFor(x => x.AccountNumber, f => f.Finance.Account())
            .RuleFor(x => x.PrincipalAmount, f => f.Random.Decimal(1_000_000, 500_000_000))
            .RuleFor(x => x.InterestRate, f => f.Random.Decimal(3, 9))
            .RuleFor(x => x.InterestType, f => f.PickRandom("Simple", "Compound"))
            .RuleFor(x => x.DepositDate, f => DateOnly.FromDateTime(f.Date.Past(2)))
            .RuleFor(x => x.MaturityDate, f => DateOnly.FromDateTime(f.Date.Future(2)))
            .RuleFor(x => x.Status, f => f.PickRandom("Active", "Matured", "Closed"))
            .RuleFor(x => x.Note, f => f.Lorem.Sentence())
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(2));

    public static Faker<NoteDto> Notes() =>
        new Faker<NoteDto>("en")
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.OwnerId, _ => Guid.Parse(DemoJwtFactory.DemoUserId))
            .RuleFor(x => x.Title, f => f.Lorem.Sentence(3))
            .RuleFor(x => x.Content, f => f.Lorem.Paragraphs(2))
            .RuleFor(x => x.Color, f => f.PickRandom("Default", "Red", "Blue", "Green", "Yellow"))
            .RuleFor(x => x.IsPinned, f => f.Random.Bool(0.1f))
            .RuleFor(x => x.IsArchived, f => f.Random.Bool(0.05f))
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(1))
            .RuleFor(x => x.UpdatedAt, f => f.Date.Recent());

    public static Faker<CredentialAccountDto> CredentialAccounts() =>
        new Faker<CredentialAccountDto>("en")
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Username, f => f.Internet.UserName())
            .RuleFor(x => x.Description, f => f.Lorem.Sentence())
            .RuleFor(x => x.IsUsed, f => f.Random.Bool(0.4f))
            .RuleFor(x => x.UsedAt, f => f.Date.Recent())
            .RuleFor(x => x.UsedBy, f => f.Internet.UserName())
            .RuleFor(x => x.UsageCount, f => f.Random.Int(0, 50))
            .RuleFor(x => x.LastUsedAgo, f => f.Date.Recent().ToString("g"));

    public static Faker<PostListItemDto> BlogPosts() =>
        new Faker<PostListItemDto>("en")
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Title, f => f.Lorem.Sentence(4))
            .RuleFor(x => x.Slug, f => f.Lorem.Slug())
            .RuleFor(x => x.Status, f => f.PickRandom("Draft", "Published", "Scheduled"))
            .RuleFor(x => x.CategoryName, f => f.Commerce.Categories(1)[0])
            .RuleFor(x => x.PublishedAt, f => f.Date.Past())
            .RuleFor(x => x.ScheduledAt, f => null)
            .RuleFor(x => x.ReadTimeMinutes, f => f.Random.Int(1, 20))
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(2))
            .RuleFor(x => x.UpdatedAt, f => f.Date.Recent());

    public static Faker<PublishedPostListItemDto> PublishedPosts() =>
        new Faker<PublishedPostListItemDto>("en")
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Title, f => f.Lorem.Sentence(4))
            .RuleFor(x => x.Slug, f => f.Lorem.Slug())
            .RuleFor(x => x.Summary, f => f.Lorem.Paragraph())
            .RuleFor(x => x.FeaturedImageUrl, f => DemoImageUrls.Featured(f.Lorem.Slug()))
            .RuleFor(x => x.CategoryName, f => f.Commerce.Categories(1)[0])
            .RuleFor(x => x.ReadTimeMinutes, f => f.Random.Int(1, 20))
            .RuleFor(x => x.PublishedAt, f => f.Date.Past());

    public static Faker<TagDto> BlogTags() =>
        new Faker<TagDto>("en")
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Name, f => f.Lorem.Word())
            .RuleFor(x => x.Slug, f => f.Lorem.Slug());

    public static Faker<CategoryDto> BlogCategories() =>
        new Faker<CategoryDto>("en")
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Name, f => f.Commerce.Categories(1)[0])
            .RuleFor(x => x.Slug, f => f.Lorem.Slug())
            .RuleFor(x => x.Description, f => f.Lorem.Sentence())
            .RuleFor(x => x.Icon, f => "article")
            .RuleFor(x => x.Color, f => f.Internet.Color());

    public static Faker<NotificationScheduleListItemDto> NotificationSchedules() =>
        new Faker<NotificationScheduleListItemDto>("en")
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Name, f => f.Commerce.ProductName())
            .RuleFor(x => x.Type, f => f.PickRandom("OneTime", "Weekly", "Monthly", "EveryXDays"))
            .RuleFor(x => x.Status, f => f.PickRandom("Draft", "Active", "Paused", "Completed"))
            .RuleFor(x => x.NextExecuteAtUtc, f => f.Date.Future())
            .RuleFor(x => x.FailureCount, f => f.Random.Int(0, 5))
            .RuleFor(x => x.ExecutionCount, f => f.Random.Int(0, 100))
            .RuleFor(x => x.LastTriggeredAtUtc, f => f.Date.Recent());

    public static Faker<PriceAlertListItemDto> PriceAlerts() =>
        new Faker<PriceAlertListItemDto>("en")
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Symbol, f => f.PickRandom("BTC", "ETH", "VNM", "FPT", "SOL"))
            .RuleFor(x => x.AssetType, f => f.PickRandom("Crypto", "Stock"))
            .RuleFor(x => x.Condition, f => f.PickRandom("Above", "Below"))
            .RuleFor(x => x.TargetPrice, f => f.Random.Decimal(1, 100000))
            .RuleFor(x => x.Note, f => f.Lorem.Sentence())
            .RuleFor(x => x.IsEnabled, f => f.Random.Bool(0.8f))
            .RuleFor(x => x.IsOneTime, f => f.Random.Bool())
            .RuleFor(x => x.LastTriggeredAt, f => f.Date.Recent())
            .RuleFor(x => x.TriggerCount, f => f.Random.Int(0, 20))
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(1))
            .RuleFor(x => x.UpdatedAt, f => f.Date.Recent());

    public static Faker<MarketScannerRuleListItemDto> MarketScannerRules() =>
        new Faker<MarketScannerRuleListItemDto>("en")
            .RuleFor(x => x.Id, f => f.Random.Guid())
            .RuleFor(x => x.Name, f => f.Commerce.ProductName())
            .RuleFor(x => x.SignalType, f => f.PickRandom("Breakout", "VolumeSpike", "Rsi"))
            .RuleFor(x => x.Window, f => f.PickRandom("1h", "4h", "1d"))
            .RuleFor(x => x.Threshold, f => f.Random.Decimal(0.5m, 10))
            .RuleFor(x => x.IsEnabled, f => f.Random.Bool(0.85f))
            .RuleFor(x => x.CreatedAt, f => f.Date.Past(1))
            .RuleFor(x => x.LastUpdated, f => f.Date.Recent());

    public static Faker<ApiClientDto> ApiClients() =>
        new Faker<ApiClientDto>("en")
            .CustomInstantiator(f => new ApiClientDto
            {
                Id = f.Random.Guid(),
                ClientId = f.Random.AlphaNumeric(16),
                Name = f.Company.CompanyName(),
                Description = f.Lorem.Sentence(),
                IsActive = f.Random.Bool(0.9f),
                Scopes = ["api.read", "api.write"],
                CreatedAt = f.Date.Past(2),
                UpdatedAt = f.Date.Recent()
            });

    public static Faker<ChainBrokerFundDto> ChainBrokerFunds() =>
        new Faker<ChainBrokerFundDto>("en")
            .CustomInstantiator(f => new ChainBrokerFundDto
            {
                Id = f.Random.Guid(),
                Slug = f.Lorem.Slug(),
                Name = f.Company.CompanyName(),
                Logo = DemoImageUrls.Logo(f.Company.CompanyName()),
                FundTypeName = f.PickRandom("Venture", "Hedge", "Angel"),
                FundTypeSlug = f.Lorem.Slug(),
                LastInvestmentDate = DateOnly.FromDateTime(f.Date.Past()),
                YearFounded = f.Date.Past(20).Year,
                Status = f.PickRandom("Active", "Inactive"),
                AverageCurrentRoi = f.Random.Decimal(-50, 500),
                ProjectCount = f.Random.Int(1, 40),
                GainersCount = f.Random.Int(0, 20),
                LosersCount = f.Random.Int(0, 20)
            });

    public static Faker<ChainBrokerProjectDto> ChainBrokerProjects() =>
        new Faker<ChainBrokerProjectDto>("en")
            .CustomInstantiator(f => new ChainBrokerProjectDto
            {
                Id = f.Random.Guid(),
                Slug = f.Lorem.Slug(),
                Name = f.Company.CompanyName(),
                Logo = DemoImageUrls.Logo(f.Company.CompanyName()),
                Ticker = f.Random.AlphaNumeric(4).ToUpperInvariant(),
                CurrentPriceUsd = f.Random.Decimal(0.01m, 100),
                Rank = f.Random.Int(1, 1000),
                MarketCapUsd = f.Random.Decimal(1_000_000, 5_000_000_000),
                Volume24hUsd = f.Random.Decimal(10_000, 50_000_000),
                PriceChange24h = f.Random.Decimal(-20, 20),
                Blockchains = ["Ethereum", "Solana"],
                Tags = [],
                Funds = [],
                Launchpads = []
            });

    public static Faker<ChainBrokerUnlockDto> ChainBrokerUnlocks() =>
        new Faker<ChainBrokerUnlockDto>("en")
            .CustomInstantiator(f => new ChainBrokerUnlockDto
            {
                Id = f.Random.Guid(),
                Slug = f.Lorem.Slug(),
                Name = f.Company.CompanyName(),
                Logo = DemoImageUrls.Logo(f.Company.CompanyName()),
                Ticker = f.Random.AlphaNumeric(4).ToUpperInvariant(),
                NextUnlockDate = DateOnly.FromDateTime(f.Date.Future()),
                UnlockAmount = f.Random.Long(1000, 10_000_000).ToString(),
                UnlockValueUsd = f.Random.Decimal(10_000, 50_000_000),
                RoundName = f.PickRandom("Seed", "Private", "Public"),
                CirculationPercent = f.Random.Decimal(1, 80),
                UnlockPercent = f.Random.Decimal(0.1m, 10),
                Volume24hUsd = f.Random.Decimal(10_000, 5_000_000),
                PriceChange24h = f.Random.Decimal(-15, 15)
            });

    public static Faker<CommodityDto> Commodities() =>
        new Faker<CommodityDto>("en")
            .CustomInstantiator(f =>
            {
                var last = f.Random.Decimal(10, 5000);
                var change = f.Random.Decimal(-50, 50);
                return new CommodityDto
                {
                    Goods = f.Commerce.ProductName(),
                    Last = last,
                    High = last + f.Random.Decimal(0, 20),
                    Low = Math.Max(0.01m, last - f.Random.Decimal(0, 20)),
                    Change = change,
                    ChangePercent = f.Random.Decimal(-5, 5),
                    LastUpdate = f.Date.Recent().ToString("g")
                };
            });

    public static Faker<CurrencyDto> Currencies() =>
        new Faker<CurrencyDto>("en")
            .CustomInstantiator(f =>
            {
                var price = f.Random.Decimal(0.5m, 30000);
                return new CurrencyDto
                {
                    ProductName = $"{f.Finance.Currency().Code}/VND",
                    CurrentPrice = price,
                    OtherPrice = price * f.Random.Decimal(0.98m, 1.02m),
                    PrevPrice = price * f.Random.Decimal(0.97m, 1.03m),
                    Change24H = f.Random.Decimal(-3, 3),
                    Change7D = f.Random.Decimal(-8, 8),
                    UpdateDate = f.Date.Recent().ToString("g")
                };
            });

    public static Faker<CryptocurrencyDto> Cryptocurrencies() =>
        new Faker<CryptocurrencyDto>("en")
            .CustomInstantiator(f =>
            {
                var symbol = f.PickRandom("BTC", "ETH", "BNB", "SOL", "XRP", "ADA", "DOGE", "AVAX", "DOT", "LINK");
                return new CryptocurrencyDto
                {
                    Name = f.Commerce.ProductName() + " Coin",
                    Symbol = $"{symbol}{f.Random.Int(1, 99)}",
                    Price = f.Random.Decimal(0.01m, 70000),
                    MarketCap = f.Random.Decimal(1_000_000, 1_000_000_000_000),
                    Vol24H = f.Random.Decimal(100_000, 50_000_000_000),
                    Change24H = f.Random.Decimal(-15, 15),
                    Change7D = f.Random.Decimal(-30, 30),
                    LastUpdate = f.Date.Recent().ToString("g")
                };
            });

    public static Faker<BankInterestRateGridItemDto> BankInterestRates() =>
        new Faker<BankInterestRateGridItemDto>("en")
            .CustomInstantiator(f => new BankInterestRateGridItemDto
            {
                BankName = f.Company.CompanyName(),
                Symbol = f.Random.AlphaNumeric(3).ToUpperInvariant(),
                IconUrl = DemoImageUrls.Logo(f.Company.CompanyName(), size: 48),
                Month1 = f.Random.Decimal(2, 6),
                Month3 = f.Random.Decimal(3, 7),
                Month6 = f.Random.Decimal(4, 8),
                Month9 = f.Random.Decimal(4, 8),
                Month12 = f.Random.Decimal(5, 9),
                Month18 = f.Random.Decimal(5, 9),
                Month24 = f.Random.Decimal(5, 10)
            });

    public static Faker<BankDepositRateItemDto> BankDepositRatesOnline() =>
        new Faker<BankDepositRateItemDto>("en")
            .CustomInstantiator(f => new BankDepositRateItemDto
            {
                BankName = f.Company.CompanyName(),
                LogoUrl = DemoImageUrls.Logo(f.Company.CompanyName(), size: 48),
                Note = f.Random.Bool(0.2f) ? "Conditional product" : null,
                UpdatedAt = DateTimeOffset.UtcNow,
                Month1 = f.Random.Decimal(2, 5),
                Month3 = f.Random.Decimal(2.5m, 5.5m),
                Month6 = f.Random.Decimal(3, 6),
                Month9 = f.Random.Decimal(3, 6.5m),
                Month12 = f.Random.Decimal(4, 7)
            });

    public static Faker<BankDepositRateItemDto> BankDepositRatesOffline() =>
        new Faker<BankDepositRateItemDto>("en")
            .CustomInstantiator(f => new BankDepositRateItemDto
            {
                BankName = f.Company.CompanyName(),
                LogoUrl = DemoImageUrls.Logo(f.Company.CompanyName(), size: 48),
                UpdatedAt = DateTimeOffset.UtcNow,
                Month1 = f.Random.Decimal(0.5m, 3),
                Month3 = f.Random.Decimal(1, 3.5m),
                Month6 = f.Random.Decimal(1.5m, 4),
                Month9 = f.Random.Decimal(1.5m, 4.5m),
                Month12 = f.Random.Decimal(2, 5.5m)
            });

    public static Faker<SacombankExchangeRateItemDto> SacombankExchangeRates() =>
        new Faker<SacombankExchangeRateItemDto>("en")
            .CustomInstantiator(f =>
            {
                var bid = f.Random.Decimal(1, 30000);
                return new SacombankExchangeRateItemDto
                {
                    CurrencyCode = f.Finance.Currency().Code,
                    BidInCash = bid,
                    BidInTransfer = bid * 1.001m,
                    OfferInCash = bid * 1.01m,
                    OfferInTransfer = bid * 1.011m,
                    CreatedDate = f.Date.Recent().ToString("g")
                };
            });

    public static Faker<CoinGeckoCoinDto> CoinGeckoCoins() =>
        new Faker<CoinGeckoCoinDto>("en")
            .CustomInstantiator(f =>
            {
                var name = f.Commerce.ProductName();
                return new CoinGeckoCoinDto
                {
                    Id = f.Lorem.Slug(),
                    Symbol = f.Random.AlphaNumeric(3).ToLowerInvariant(),
                    Name = name
                };
            });

    public static Faker<VnDirectChangePriceItemDto> VnDirectChangePrices() =>
        new Faker<VnDirectChangePriceItemDto>("en")
            .CustomInstantiator(f =>
            {
                var price = f.Random.Decimal(5, 200);
                var change = f.Random.Decimal(-10, 10);
                return new VnDirectChangePriceItemDto
                {
                    Code = f.PickRandom("VNM", "FPT", "HPG", "VCB", "MWG", "MSN", "VIC", "GAS", "TCB", "ACB"),
                    Name = f.Company.CompanyName(),
                    Type = f.PickRandom("STOCK", "ETF"),
                    Period = f.PickRandom("1D", "1W", "1M"),
                    Price = price,
                    BopPrice = price - change,
                    Change = change,
                    ChangePct = f.Random.Decimal(-8, 8),
                    LastUpdated = f.Date.Recent().ToString("o")
                };
            });

    public static Faker<VnDirectTopStockDto> VnDirectTopStocks() =>
        new Faker<VnDirectTopStockDto>("en")
            .CustomInstantiator(f => new VnDirectTopStockDto
            {
                Code = f.PickRandom("VNM", "FPT", "HPG", "VCB", "MWG", "MSN", "VIC", "GAS", "TCB", "ACB"),
                Index = f.PickRandom("VN30", "VNINDEX", "HNX30"),
                LastPrice = f.Random.Decimal(5, 200),
                LastUpdated = f.Date.Recent().ToString("o"),
                PriceChgCr1D = f.Random.Decimal(-5, 5),
                PriceChgPctCr1D = f.Random.Decimal(-8, 8),
                AccumulatedVal = f.Random.Decimal(1_000_000, 500_000_000),
                NmVolumeAvgCr20D = f.Random.Decimal(100_000, 10_000_000),
                NmVolNmVolAvg20DPctCr = f.Random.Decimal(-50, 200),
                TotalVolumeAvgCr20D = f.Random.Decimal(100_000, 20_000_000),
                PtVolTotalVolAvg20DPctCr = f.Random.Decimal(-30, 100),
                PtVolAvg5DTotalVolAvg20DPctCr = f.Random.Decimal(-30, 100),
                PtVolSumCr5D = f.Random.Decimal(10_000, 5_000_000),
                PtValAvgCr5D = f.Random.Decimal(1_000_000, 100_000_000),
                PtVolAvgCr5D = f.Random.Decimal(10_000, 2_000_000),
                LongSignal = f.PickRandom("Buy", "Sell", "Hold"),
                ShortSignal = f.PickRandom("Buy", "Sell", "Hold"),
                LongBuyCount = f.Random.Int(0, 10),
                LongSellCount = f.Random.Int(0, 10),
                ShortBuyCount = f.Random.Int(0, 10),
                ShortSellCount = f.Random.Int(0, 10),
                InTcbsCurrentHoldings = f.Random.Bool(0.2f),
                InTcbsAllTimeHoldings = f.Random.Bool(0.4f),
                DragonFundCount = f.Random.Int(0, 5),
                DragonFunds = ["DCDS", "DCDE"],
                DragonTotalWeight = f.Random.Decimal(0, 15),
                EventRiskLevel = f.PickRandom("Low", "Medium", "High"),
                RecentEventCount = f.Random.Int(0, 5),
                MostSevereEventType = f.PickRandom("None", "Earnings", "Dividend"),
                LatestEventEffectiveDate = f.Date.Recent().ToString("yyyy-MM-dd"),
                CompositeScore = f.Random.Int(0, 100),
                TechnicalScore = f.Random.Int(0, 100),
                PortfolioScore = f.Random.Int(0, 100),
                EventPenalty = f.Random.Int(0, 20),
                LiquidityPenalty = f.Random.Int(0, 20),
                ScoreBreakdown =
                [
                    new ScoreBreakdownItemDto { Name = "Momentum", Value = f.Random.Int(0, 30) },
                    new ScoreBreakdownItemDto { Name = "Volume", Value = f.Random.Int(0, 30) }
                ],
                DataCompleteness = true
            });

    public static Faker<MexcContractTickerDto> MexcContractTickers() =>
        new Faker<MexcContractTickerDto>("en")
            .CustomInstantiator(f =>
            {
                var last = f.Random.Decimal(0.01m, 100_000);
                return new MexcContractTickerDto
                {
                    ContractId = f.Random.Int(1, 9999),
                    Symbol = f.PickRandom("BTC_USDT", "ETH_USDT", "SOL_USDT", "XRP_USDT", "BNB_USDT"),
                    LastPrice = last,
                    Bid1 = last * 0.999m,
                    Ask1 = last * 1.001m,
                    High24Price = last * 1.05m,
                    Low24Price = last * 0.95m,
                    Volume24 = f.Random.Decimal(1000, 50_000_000),
                    Amount24 = f.Random.Decimal(10_000, 500_000_000),
                    HoldVol = f.Random.Decimal(1000, 10_000_000),
                    RiseFallRate = f.Random.Decimal(-0.2m, 0.2m),
                    RiseFallValue = f.Random.Decimal(-1000, 1000),
                    IndexPrice = last,
                    FairPrice = last,
                    FundingRate = f.Random.Decimal(-0.01m, 0.01m),
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    CompositeScore = f.Random.Decimal(0, 100),
                    ScoreBreakdown = new MexcScoreBreakdownDto
                    {
                        VolumeScore = f.Random.Decimal(0, 20),
                        OpenInterestScore = f.Random.Decimal(0, 20),
                        FundingScore = f.Random.Decimal(0, 10),
                        MomentumScore = f.Random.Decimal(0, 20),
                        BrokerQualityScore = f.Random.Decimal(0, 10),
                        SocialScore = f.Random.Decimal(0, 10),
                        FundraisingScore = f.Random.Decimal(0, 10),
                        SupplyHealthScore = f.Random.Decimal(0, 10),
                        UnlockPenalty = f.Random.Decimal(0, 5),
                        FdvOverhangPenalty = f.Random.Decimal(0, 5)
                    }
                };
            });

    public static Faker<MexcSpotTicker24HrDto> MexcSpotTickers() =>
        new Faker<MexcSpotTicker24HrDto>("en")
            .CustomInstantiator(f =>
            {
                var last = f.Random.Decimal(0.01m, 100_000);
                return new MexcSpotTicker24HrDto
                {
                    Symbol = f.PickRandom("BTCUSDT", "ETHUSDT", "SOLUSDT", "XRPUSDT", "BNBUSDT"),
                    PriceChange = f.Random.Decimal(-1000, 1000).ToString("F4"),
                    PriceChangePercent = f.Random.Decimal(-15, 15).ToString("F2"),
                    PrevClosePrice = (last * 0.99m).ToString("F4"),
                    LastPrice = last.ToString("F4"),
                    BidPrice = (last * 0.999m).ToString("F4"),
                    AskPrice = (last * 1.001m).ToString("F4"),
                    OpenPrice = (last * 0.98m).ToString("F4"),
                    HighPrice = (last * 1.05m).ToString("F4"),
                    LowPrice = (last * 0.95m).ToString("F4"),
                    Volume = f.Random.Decimal(1000, 50_000_000).ToString("F2"),
                    QuoteVolume = f.Random.Decimal(10_000, 500_000_000).ToString("F2"),
                    OpenTime = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds(),
                    CloseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    Count = f.Random.Long(100, 1_000_000),
                    CompositeScore = f.Random.Decimal(0, 100),
                    ScoreBreakdown = new MexcSpotScoreBreakdownDto
                    {
                        VolumeScore = f.Random.Decimal(0, 25),
                        MomentumScore = f.Random.Decimal(0, 25),
                        BrokerQualityScore = f.Random.Decimal(0, 15),
                        SocialScore = f.Random.Decimal(0, 15),
                        FundraisingScore = f.Random.Decimal(0, 10),
                        SupplyHealthScore = f.Random.Decimal(0, 10),
                        UnlockPenalty = f.Random.Decimal(0, 5),
                        FdvOverhangPenalty = f.Random.Decimal(0, 5)
                    }
                };
            });

    public static Faker<TcbsTop10PortfolioDto> TcbsTop10Portfolios() =>
        new Faker<TcbsTop10PortfolioDto>("en")
            .CustomInstantiator(f => new TcbsTop10PortfolioDto
            {
                Id = f.Random.Guid(),
                SourceContentId = f.Random.Long(1, 1_000_000),
                PostedAt = DateOnly.FromDateTime(f.Date.Past(2)),
                EffectiveDate = DateOnly.FromDateTime(f.Date.Past(1)),
                AddedTickers = [f.PickRandom("VNM", "FPT", "HPG"), f.PickRandom("VCB", "MWG")],
                RemovedTickers = [f.PickRandom("GAS", "MSN")],
                Images =
                [
                    new TcbsTop10ImageDto { ImageType = 1, FileGuid = f.Random.Guid() }
                ]
            });

    public static Faker<WeeklySuggestionHistoryReportDto> WeeklySuggestionReports() =>
        new Faker<WeeklySuggestionHistoryReportDto>("en")
            .CustomInstantiator(f =>
            {
                var picks = Enumerable.Range(1, 5)
                    .Select(rank => new WeeklySuggestionHistoryItemDto
                    {
                        Timeframe = f.PickRandom("1h", "4h", "1d"),
                        Rank = rank,
                        Symbol = f.PickRandom("BTC", "ETH", "VNM", "FPT", "SOL"),
                        MarketType = f.PickRandom<SuggestionAssetClass>(),
                        EntryPrice = f.Random.Decimal(1, 100000),
                        Signal = f.PickRandom("Long", "Short", "Neutral"),
                        Confidence = f.Random.Decimal(0.4m, 0.95m)
                    })
                    .ToList();

                return new WeeklySuggestionHistoryReportDto
                {
                    ReportId = f.Random.Guid(),
                    ReportKey = $"demo-{f.Random.AlphaNumeric(8)}",
                    RunAtUtc = f.Date.Recent(60),
                    AssetClass = f.PickRandom<SuggestionAssetClass>(),
                    ReportType = SuggestionReportType.TopSuggestionsWeekly,
                    Status = WeeklySuggestionReportStatus.Succeeded,
                    PickedCount = picks.Count,
                    Picks = picks
                };
            });

    public static Faker<Power655DrawDto> Power655Draws() =>
        new Faker<Power655DrawDto>("en")
            .CustomInstantiator(f =>
            {
                var numbers = Enumerable.Range(1, 6).Select(_ => f.Random.Int(1, 55)).Distinct().Take(6).ToList();
                while (numbers.Count < 6)
                {
                    numbers.Add(f.Random.Int(1, 55));
                    numbers = numbers.Distinct().ToList();
                }

                return new Power655DrawDto
                {
                    Id = f.Random.Guid(),
                    DrawDate = DateOnly.FromDateTime(f.Date.Past(3)),
                    DayOfWeek = f.Date.Past().DayOfWeek.ToString(),
                    Numbers = numbers,
                    BonusNum = f.Random.Int(1, 55),
                    Jackpot1Value = f.Random.Long(10_000_000_000, 100_000_000_000),
                    Jackpot2Value = f.Random.Long(1_000_000_000, 20_000_000_000)
                };
            });

    public static ResumeDto DemoResume() =>
        new()
        {
            Id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
            Slug = "demo-admin",
            FullName = "Demo Candidate",
            Headline = "Senior Backend Developer",
            MetaDescription =
                "Senior Backend Developer with approximately 11 years of experience delivering enterprise software across Finance, Retail, FMCG, and more.",
            IsPublished = true,
            Content = new ResumeContentDto
            {
                Summary =
                    "Senior Backend Developer with approximately 11 years of professional experience delivering enterprise software across Finance, Retail, FMCG, Employee Management, Project Management, Promotion Platforms, Virtual Commerce, and Embedded Software domains. Specialized in designing and developing scalable backend applications using C#, ASP.NET Core, .NET Core, Entity Framework Core, SQL Server, PostgreSQL, Azure, and RESTful APIs. Strong background in enterprise application modernization, system integration, database optimization, software architecture, and production support. Passionate about clean, maintainable, and high-quality software with a strong focus on performance, reliability, and engineering best practices.",
                Skills =
                [
                    "C#, ASP.NET Core, .NET Core, Entity Framework Core",
                    "SQL Server, PostgreSQL, MySQL",
                    "RESTful API Design & Development",
                    "Software Architecture (Clean Architecture, SOLID, CQRS)",
                    "Cloud-based Solutions (Microsoft Azure)",
                    "Azure DevOps, Docker, CI/CD, Git",
                    "Performance Optimization & Query Tuning",
                    "System Integration",
                    "Authentication & Authorization",
                    "Angular, HTML, CSS, JavaScript",
                    "Agile & Scrum",
                    "Production Support & Debugging"
                ],
                Experience =
                [
                    new ResumeExperienceDto
                    {
                        Company = "Niteco Vietnam",
                        Position = "Software Developer",
                        StartDate = "2022-04-01",
                        EndDate = null,
                        Description =
                            "Enterprise software development for Heineken APAC platforms and project management systems. Designed and implemented backend features for enterprise applications. Developed RESTful APIs and business services using .NET Core and NestJS. Optimized SQL queries and improved application performance. Collaborated with business stakeholders to refine requirements. Performed production troubleshooting, debugging, code reviews, and feature delivery. Technologies: C#, ASP.NET Core, .NET Core, NestJS, PostgreSQL, SQL Server, Azure"
                    },
                    new ResumeExperienceDto
                    {
                        Company = "Titan Technology",
                        Position = "Senior Software Developer",
                        StartDate = "2020-06-01",
                        EndDate = "2022-03-01",
                        Description =
                            "Developed enterprise financial software and third-party integrations. Built backend services and automation features. Integrated enterprise systems with external services. Optimized SQL Server queries and data processing. Maintained production systems and resolved complex issues. Technologies: .NET Core, SQL Server"
                    },
                    new ResumeExperienceDto
                    {
                        Company = "Hybrid Technologies",
                        Position = "Software Developer",
                        StartDate = "2019-12-01",
                        EndDate = "2020-06-01",
                        Description =
                            "Developed backend features for Domestic Tour platform. Implemented enhancements and production bug fixes."
                    },
                    new ResumeExperienceDto
                    {
                        Company = "MTI Technology",
                        Position = "Software Developer",
                        StartDate = "2017-07-01",
                        EndDate = "2019-12-01",
                        Description =
                            "Migrated legacy applications to .NET Core. Developed enterprise management systems. Participated in requirement clarification and system enhancement. Worked with Angular, Vue.js, and Entity Framework Core."
                    },
                    new ResumeExperienceDto
                    {
                        Company = "Renesas Design Vietnam",
                        Position = "Software Engineer",
                        StartDate = "2015-03-01",
                        EndDate = "2017-07-01",
                        Description =
                            "Designed and developed engineering desktop applications. Analyzed customer requirements. Delivered maintenance, debugging, and feature implementation."
                    }
                ],
                Projects =
                [
                    new ResumeProjectDto
                    {
                        Name = "Ignite - Heineken ThaiX",
                        Description = "Enterprise platform for Heineken APAC.",
                        TechStack = "C#, ASP.NET Core, .NET Core, Azure"
                    },
                    new ResumeProjectDto
                    {
                        Name = "Heineken EKOIN & APAC Virtual Commerce",
                        Description = "Virtual commerce platform for Heineken APAC.",
                        TechStack = "NestJS, PostgreSQL, Azure"
                    },
                    new ResumeProjectDto
                    {
                        Name = "QuickBooks Integration & Notification",
                        Description = "Enterprise financial software with third-party service integration.",
                        TechStack = ".NET Core, SQL Server"
                    },
                    new ResumeProjectDto
                    {
                        Name = "Device Management Tool & Seasar Migration",
                        Description = "Legacy application migration to .NET Core and enterprise management systems.",
                        TechStack = ".NET Core, Angular, Vue.js, Entity Framework Core"
                    },
                    new ResumeProjectDto
                    {
                        Name = "MISRA-C Checker & Smart Manual",
                        Description = "Engineering desktop applications for embedded systems.",
                        TechStack = "C#, Desktop Development"
                    }
                ],
                Education =
                [
                    new ResumeEducationDto
                    {
                        School = "University of Science - Vietnam National University Ho Chi Minh City",
                        Degree = "Bachelor of Engineering in Information Technology",
                        StartDate = "",
                        EndDate = null,
                        Description = "Classification: Good"
                    }
                ],
                Links = []
            }
        };

    public static VixResponseDto DemoVix() =>
        new()
        {
            Symbol = "^VIX",
            CurrentPrice = 18.5m,
            PreviousClose = 17.8m,
            DayHigh = 19.2m,
            DayLow = 17.1m,
            FiftyTwoWeekHigh = 35m,
            FiftyTwoWeekLow = 12m,
            LastUpdateTime = DateTime.UtcNow,
            CurrentValue = 18.5m,
            Ema10 = 17.2m,
            Ema20 = 16.8m,
            Momentum3D = 0.8m,
            Momentum5D = 1.2m,
            Percentile = 62.5,
            IsSpike = false,
            Regime = "Neutral",
            Success = true,
            Message = "Demo mode"
        };

    public static DragonCapitalFundPortfolioDto DemoDragonCapital(string fundCode) =>
        new()
        {
            FundCode = string.IsNullOrWhiteSpace(fundCode) ? "DCDS" : fundCode,
            TradingDate = DateTime.UtcNow.Date,
            AllocationByAssetTypes =
            [
                new DragonCapitalAssetTypeAllocationDto { SourceName = "Equity", ValueAssetType = 70 },
                new DragonCapitalAssetTypeAllocationDto { SourceName = "Cash", ValueAssetType = 30 }
            ],
            AllocationBySectors =
            [
                new DragonCapitalSectorAllocationDto { IndustryLevel2 = "Banks", FundWeight = 25 },
                new DragonCapitalSectorAllocationDto { IndustryLevel2 = "Technology", FundWeight = 15 }
            ],
            Top10Holdings = Enumerable.Range(1, 10)
                .Select(i => new DragonCapitalTopHoldingDto
                {
                    AssetId = i % 2 == 0 ? "FPT" : "VNM",
                    Weight = 10m - i * 0.5m,
                    Exchange = "HOSE",
                    IndustryLevel = "Financials",
                    SectorLevel = "Banks",
                    HoldingVolume = 1_000_000 * i,
                    MarketValue = 50_000_000m * i
                })
                .ToList(),
            Success = true,
            Message = "Demo mode"
        };

    public static CoinGeckoCoinMarketResponseDto DemoCoinMarket(string coinId) =>
        new()
        {
            CoinId = string.IsNullOrWhiteSpace(coinId) ? "bitcoin" : coinId,
            Success = true,
            Message = "Demo mode",
            Data = new CoinGeckoCoinMarketDto
            {
                Id = string.IsNullOrWhiteSpace(coinId) ? "bitcoin" : coinId,
                Symbol = "btc",
                Name = "Bitcoin",
                Image = DemoImageUrls.Logo(string.IsNullOrWhiteSpace(coinId) ? "bitcoin" : coinId),
                CurrentPrice = 65_000m,
                MarketCap = 1_200_000_000_000m,
                MarketCapRank = 1,
                High24H = 66_000m,
                Low24H = 64_000m,
                PriceChangePercentage24H = 1.25m
            }
        };

    public static ExchangeTicker24HrResponseDto DemoExchangeTickers(string? symbolFilter = null)
    {
        var symbols = new[] { "BTCUSDT", "ETHUSDT", "SOLUSDT", "XRPUSDT", "BNBUSDT" };
        var data = symbols
            .Where(s => symbolFilter is null || s.Equals(symbolFilter, StringComparison.OrdinalIgnoreCase))
            .Select((s, i) =>
            {
                var last = 1000m + i * 250m;
                return new ExchangeTicker24HrDto
                {
                    Symbol = s,
                    LastPrice = last.ToString("F4"),
                    PriceChange = (last * 0.01m).ToString("F4"),
                    PriceChangePercent = "1.25",
                    HighPrice = (last * 1.05m).ToString("F4"),
                    LowPrice = (last * 0.95m).ToString("F4"),
                    Volume = (1_000_000m * (i + 1)).ToString("F2"),
                    QuoteVolume = (50_000_000m * (i + 1)).ToString("F2"),
                    BidPrice = (last * 0.999m).ToString("F4"),
                    AskPrice = (last * 1.001m).ToString("F4"),
                    OpenTime = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeMilliseconds(),
                    CloseTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };
            })
            .ToList();

        return new ExchangeTicker24HrResponseDto
        {
            Success = true,
            Message = "Demo mode",
            Data = data
        };
    }

    public static OrderBookDepthResponseDto DemoOrderBook(string symbol)
    {
        var mid = 65_000m;
        var bids = Enumerable.Range(1, 20)
            .Select(i => new OrderBookLevelDto { Price = mid - i, Quantity = 0.1m * i })
            .ToList();
        var asks = Enumerable.Range(1, 20)
            .Select(i => new OrderBookLevelDto { Price = mid + i, Quantity = 0.1m * i })
            .ToList();

        return new OrderBookDepthResponseDto
        {
            Symbol = symbol,
            LastUpdateId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Bids = bids,
            Asks = asks,
            Success = true,
            Message = "Demo mode"
        };
    }

    public static FundingRatesResponseDto DemoFundingRates() =>
        new()
        {
            Success = true,
            Message = "Demo mode",
            Data = new[] { "BTCUSDT", "ETHUSDT", "SOLUSDT", "XRPUSDT", "BNBUSDT" }
                .Select((s, i) => new FundingRateDto
                {
                    Symbol = s,
                    FundingRate = 0.0001m * (i + 1),
                    FundingTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    MarkPrice = 1000m + i * 100m
                })
                .ToList()
        };

    public static KlineSeriesResponseDto DemoKlines(string symbol, string interval)
    {
        var now = DateTimeOffset.UtcNow;
        var bars = Enumerable.Range(0, 48)
            .Select(i =>
            {
                var open = 1000m + i;
                return new KlineBarDto
                {
                    OpenTime = now.AddHours(-(48 - i)).ToUnixTimeMilliseconds(),
                    Open = open,
                    High = open + 10,
                    Low = open - 10,
                    Close = open + 2,
                    Volume = 1000 + i * 10,
                    CloseTime = now.AddHours(-(47 - i)).ToUnixTimeMilliseconds()
                };
            })
            .ToList();

        return new KlineSeriesResponseDto
        {
            Symbol = symbol,
            Interval = interval,
            Data = bars,
            Success = true,
            Message = "Demo mode"
        };
    }

    public static StockPriceHistoryResponseDto DemoStockPriceHistory(string symbol) =>
        new()
        {
            Symbol = symbol,
            Success = true,
            Message = "Demo mode",
            Data = Enumerable.Range(0, 60)
                .Select(i =>
                {
                    var close = 40m + i * 0.1m;
                    return new StockPriceHistoryPointDto
                    {
                        Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-(60 - i))),
                        Open = close - 0.2m,
                        High = close + 0.5m,
                        Low = close - 0.5m,
                        Close = close,
                        Volume = 100_000 + i * 1_000
                    };
                })
                .ToList()
        };

    public static WatchlistPriceResponseDto DemoWatchlistPrice(string symbol) =>
        new()
        {
            Symbol = symbol,
            LastPrice = 42.5m,
            Change = 0.8m,
            ChangePercent = 1.92m,
            High = 43.1m,
            Low = 41.7m,
            Volume = 1_250_000,
            UpdatedAt = DateTime.UtcNow,
            Success = true,
            Message = "Demo mode"
        };
}
