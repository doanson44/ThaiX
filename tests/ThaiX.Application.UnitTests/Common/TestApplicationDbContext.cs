using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.ApiClient;
using ThaiX.Domain.Aggregates.AssetPositions;
using ThaiX.Domain.Aggregates.Blog;
using ThaiX.Domain.Aggregates.ChainBroker;
using ThaiX.Domain.Aggregates.ContactImport;
using ThaiX.Domain.Aggregates.Contacts;
using ThaiX.Domain.Aggregates.CredentialAccounts;
using ThaiX.Domain.Aggregates.ExpenseTracker;
using ThaiX.Domain.Aggregates.JsonBin;
using ThaiX.Domain.Aggregates.Lottery;
using ThaiX.Domain.Aggregates.MarketData;
using ThaiX.Domain.Aggregates.MarketScanner;
using ThaiX.Domain.Aggregates.MasterData;
using ThaiX.Domain.Aggregates.Notes;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Domain.Aggregates.Notifications.ValueObjects;
using ThaiX.Domain.Aggregates.Outbox;
using ThaiX.Domain.Aggregates.Portfolios;
using ThaiX.Domain.Aggregates.PriceAlerts;
using ThaiX.Domain.Aggregates.Resumes;
using ThaiX.Domain.Aggregates.TcbsTop10;
using ThaiX.Domain.Aggregates.TradingSuggestions;
using ThaiX.Domain.Common.Entities;
using ThaiX.Domain.Identity;

namespace ThaiX.Application.UnitTests.Common;

public sealed class TestApplicationDbContext : DbContext, IApplicationDbContext
{
    public TestApplicationDbContext(DbContextOptions<TestApplicationDbContext> options)
        : base(options)
    {
    }

    DbSet<ApiClient> IApplicationDbContext.ApiClients => Set<ApiClient>();
    DbSet<OutboxMessage> IApplicationDbContext.OutboxMessages => Set<OutboxMessage>();
    DbSet<Contact> IApplicationDbContext.Contacts => Set<Contact>();
    DbSet<ContactEmail> IApplicationDbContext.ContactEmails => Set<ContactEmail>();
    DbSet<ContactPhone> IApplicationDbContext.ContactPhones => Set<ContactPhone>();
    DbSet<ContactAddress> IApplicationDbContext.ContactAddresses => Set<ContactAddress>();
    DbSet<ContactSocialLink> IApplicationDbContext.ContactSocialLinks => Set<ContactSocialLink>();
    DbSet<ContactTag> IApplicationDbContext.ContactTags => Set<ContactTag>();
    DbSet<ContactBankAccount> IApplicationDbContext.ContactBankAccounts => Set<ContactBankAccount>();
    DbSet<ContactIdentityDocument> IApplicationDbContext.ContactIdentityDocuments => Set<ContactIdentityDocument>();
    DbSet<Country> IApplicationDbContext.Countries => Set<Country>();
    DbSet<City> IApplicationDbContext.Cities => Set<City>();
    DbSet<District> IApplicationDbContext.Districts => Set<District>();
    DbSet<Bank> IApplicationDbContext.Banks => Set<Bank>();
    DbSet<ContactImportJob> IApplicationDbContext.ContactImportJobs => Set<ContactImportJob>();
    DbSet<CredentialAccount> IApplicationDbContext.CredentialAccounts => Set<CredentialAccount>();
    DbSet<CredentialAccountAudit> IApplicationDbContext.CredentialAccountAudits => Set<CredentialAccountAudit>();
    DbSet<JsonBin> IApplicationDbContext.JsonBins => Set<JsonBin>();
    DbSet<FileAttachment> IApplicationDbContext.FileAttachments => Set<FileAttachment>();
    DbSet<UserContactLink> IApplicationDbContext.UserContactLinks => Set<UserContactLink>();
    DbSet<UserProfile> IApplicationDbContext.UserProfiles => Set<UserProfile>();
    DbSet<ChainBrokerUnlock> IApplicationDbContext.ChainBrokerUnlocks => Set<ChainBrokerUnlock>();
    DbSet<ChainBrokerProject> IApplicationDbContext.ChainBrokerProjects => Set<ChainBrokerProject>();
    DbSet<ChainBrokerProjectBlockchain> IApplicationDbContext.ChainBrokerProjectBlockchains => Set<ChainBrokerProjectBlockchain>();
    DbSet<ChainBrokerProjectTag> IApplicationDbContext.ChainBrokerProjectTags => Set<ChainBrokerProjectTag>();
    DbSet<ChainBrokerProjectFundRef> IApplicationDbContext.ChainBrokerProjectFundRefs => Set<ChainBrokerProjectFundRef>();
    DbSet<ChainBrokerProjectLaunchpadRef> IApplicationDbContext.ChainBrokerProjectLaunchpadRefs => Set<ChainBrokerProjectLaunchpadRef>();
    DbSet<ChainBrokerFund> IApplicationDbContext.ChainBrokerFunds => Set<ChainBrokerFund>();
    DbSet<TcbsTop10Portfolio> IApplicationDbContext.TcbsTop10Portfolios => Set<TcbsTop10Portfolio>();
    DbSet<TcbsTop10Ticker> IApplicationDbContext.TcbsTop10Tickers => Set<TcbsTop10Ticker>();
    DbSet<TcbsTop10Image> IApplicationDbContext.TcbsTop10Images => Set<TcbsTop10Image>();
    DbSet<WeeklySuggestionReport> IApplicationDbContext.WeeklySuggestionReports => Set<WeeklySuggestionReport>();
    DbSet<WeeklySuggestionReportItem> IApplicationDbContext.WeeklySuggestionReportItems => Set<WeeklySuggestionReportItem>();
    DbSet<TwentyFourHMoneyTransaction> IApplicationDbContext.TwentyFourHMoneyTransactions => Set<TwentyFourHMoneyTransaction>();
    DbSet<Power655Result> IApplicationDbContext.Power655Results => Set<Power655Result>();
    DbSet<Power655Prediction> IApplicationDbContext.Power655Predictions => Set<Power655Prediction>();
    DbSet<MarketScannerRule> IApplicationDbContext.MarketScannerRules => Set<MarketScannerRule>();
    DbSet<PriceAlert> IApplicationDbContext.PriceAlerts => Set<PriceAlert>();
    DbSet<Portfolio> IApplicationDbContext.Portfolios => Set<Portfolio>();
    DbSet<CryptoPosition> IApplicationDbContext.CryptoPositions => Set<CryptoPosition>();
    DbSet<StockPosition> IApplicationDbContext.StockPositions => Set<StockPosition>();
    DbSet<SavingPosition> IApplicationDbContext.SavingPositions => Set<SavingPosition>();
    DbSet<PositionTransaction> IApplicationDbContext.PositionTransactions => Set<PositionTransaction>();
    DbSet<Note> IApplicationDbContext.Notes => Set<Note>();
    DbSet<ResumeProfile> IApplicationDbContext.ResumeProfiles => Set<ResumeProfile>();
    DbSet<Post> IApplicationDbContext.Posts => Set<Post>();
    DbSet<ThaiX.Domain.Aggregates.Blog.Category> IApplicationDbContext.Categories => Set<ThaiX.Domain.Aggregates.Blog.Category>();
    DbSet<ThaiX.Domain.Aggregates.Blog.Tag> IApplicationDbContext.Tags => Set<ThaiX.Domain.Aggregates.Blog.Tag>();
    DbSet<PostTag> IApplicationDbContext.PostTags => Set<PostTag>();
    DbSet<Wallet> IApplicationDbContext.Wallets => Set<Wallet>();
    DbSet<ThaiX.Domain.Aggregates.ExpenseTracker.Category> IApplicationDbContext.ExpenseCategories => Set<ThaiX.Domain.Aggregates.ExpenseTracker.Category>();
    DbSet<Transaction> IApplicationDbContext.ExpenseTransactions => Set<Transaction>();
    DbSet<Transfer> IApplicationDbContext.Transfers => Set<Transfer>();
    DbSet<Budget> IApplicationDbContext.Budgets => Set<Budget>();
    DbSet<SavingGoal> IApplicationDbContext.SavingGoals => Set<SavingGoal>();
    DbSet<ThaiX.Domain.Aggregates.ExpenseTracker.Tag> IApplicationDbContext.ExpenseTags => Set<ThaiX.Domain.Aggregates.ExpenseTracker.Tag>();
    DbSet<TransactionTag> IApplicationDbContext.TransactionTags => Set<TransactionTag>();
    DbSet<RecurringTransaction> IApplicationDbContext.RecurringTransactions => Set<RecurringTransaction>();
    DbSet<Notification> IApplicationDbContext.Notifications => Set<Notification>();
    DbSet<NotificationDelivery> IApplicationDbContext.NotificationDeliveries => Set<NotificationDelivery>();
    DbSet<NotificationSchedule> IApplicationDbContext.NotificationSchedules => Set<NotificationSchedule>();
    DbSet<NotificationScheduleExecution> IApplicationDbContext.NotificationScheduleExecutions => Set<NotificationScheduleExecution>();
    DbSet<UserNotificationPreference> IApplicationDbContext.UserNotificationPreferences => Set<UserNotificationPreference>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Contact>();
        modelBuilder.Ignore<UserProfile>();
        modelBuilder.Ignore<ScheduleRecurrence>();
        modelBuilder.Entity<PostTag>().HasKey(x => new { x.PostId, x.TagId });
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}
