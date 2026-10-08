using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
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
using ThaiX.Domain.Aggregates.Portfolios;
using ThaiX.Domain.Aggregates.PriceAlerts;
using ThaiX.Domain.Aggregates.Resumes;
using ThaiX.Domain.Aggregates.TcbsTop10;
using ThaiX.Domain.Aggregates.TradingSuggestions;
using ThaiX.Domain.Common.Entities;
using ThaiX.Domain.Identity;
using FileAttachment = ThaiX.Domain.Common.Entities.FileAttachment;

namespace ThaiX.Infrastructure.Persistence;

/// <summary>
/// Application database context.
/// Implements IApplicationDbContext interface for abstraction.
/// </summary>
/// <remarks>
/// Architecture Note:
/// - Query handlers that need DbContext live in Infrastructure layer
/// - This is CORRECT: Infrastructure can reference Application layer
/// - Handlers are resolved via MediatR (no direct dependencies)
/// - IApplicationDbContext exists for cases where Application layer needs to define contracts
/// </remarks>
public class ApplicationDbContext : IdentityDbContext<Identity.ApplicationUser, Identity.ApplicationRole, Guid>, IApplicationDbContext
{
    private readonly ILogger<ApplicationDbContext> _logger;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger)
        : base(options)
    {
        _logger = logger;
    }

    public DbSet<Domain.Aggregates.ApiClient.ApiClient> ApiClients => Set<Domain.Aggregates.ApiClient.ApiClient>();
    public DbSet<Domain.Aggregates.Outbox.OutboxMessage> OutboxMessages => Set<Domain.Aggregates.Outbox.OutboxMessage>();

    // Contacts
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<ContactEmail> ContactEmails => Set<ContactEmail>();
    public DbSet<ContactPhone> ContactPhones => Set<ContactPhone>();
    public DbSet<ContactAddress> ContactAddresses => Set<ContactAddress>();
    public DbSet<ContactSocialLink> ContactSocialLinks => Set<ContactSocialLink>();
    public DbSet<ContactTag> ContactTags => Set<ContactTag>();
    public DbSet<ContactBankAccount> ContactBankAccounts => Set<ContactBankAccount>();
    public DbSet<ContactIdentityDocument> ContactIdentityDocuments => Set<ContactIdentityDocument>();
    public DbSet<ContactImportJob> ContactImportJobs => Set<ContactImportJob>();
    public DbSet<CredentialAccount> CredentialAccounts => Set<CredentialAccount>();
    public DbSet<CredentialAccountAudit> CredentialAccountAudits => Set<CredentialAccountAudit>();
    public DbSet<JsonBin> JsonBins => Set<JsonBin>();
    public DbSet<FileAttachment> FileAttachments => Set<FileAttachment>();

    // User-Contact integration (profile projection)
    public DbSet<UserContactLink> UserContactLinks => Set<UserContactLink>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    // Master Data
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<District> Districts => Set<District>();
    public DbSet<Bank> Banks => Set<Bank>();

    // ChainBroker snapshot data
    public DbSet<ChainBrokerUnlock> ChainBrokerUnlocks => Set<ChainBrokerUnlock>();
    public DbSet<ChainBrokerProject> ChainBrokerProjects => Set<ChainBrokerProject>();
    public DbSet<ChainBrokerProjectBlockchain> ChainBrokerProjectBlockchains => Set<ChainBrokerProjectBlockchain>();
    public DbSet<ChainBrokerProjectTag> ChainBrokerProjectTags => Set<ChainBrokerProjectTag>();
    public DbSet<ChainBrokerProjectFundRef> ChainBrokerProjectFundRefs => Set<ChainBrokerProjectFundRef>();
    public DbSet<ChainBrokerProjectLaunchpadRef> ChainBrokerProjectLaunchpadRefs => Set<ChainBrokerProjectLaunchpadRef>();
    public DbSet<ChainBrokerFund> ChainBrokerFunds => Set<ChainBrokerFund>();

    // TCBS Top 10 portfolio data
    public DbSet<TcbsTop10Portfolio> TcbsTop10Portfolios => Set<TcbsTop10Portfolio>();
    public DbSet<TcbsTop10Ticker> TcbsTop10Tickers => Set<TcbsTop10Ticker>();
    public DbSet<TcbsTop10Image> TcbsTop10Images => Set<TcbsTop10Image>();
    public DbSet<WeeklySuggestionReport> WeeklySuggestionReports => Set<WeeklySuggestionReport>();
    public DbSet<WeeklySuggestionReportItem> WeeklySuggestionReportItems => Set<WeeklySuggestionReportItem>();

    // 24HMoney transaction history (synced daily, retained 1 year)
    public DbSet<TwentyFourHMoneyTransaction> TwentyFourHMoneyTransactions => Set<TwentyFourHMoneyTransaction>();

    // Power 6/55 lottery results (synced from ketquadientoan.com)
    public DbSet<Power655Result> Power655Results => Set<Power655Result>();
    public DbSet<Power655Prediction> Power655Predictions => Set<Power655Prediction>();

    // Market Scanner
    public DbSet<MarketScannerRule> MarketScannerRules => Set<MarketScannerRule>();

    // Price Alerts
    public DbSet<PriceAlert> PriceAlerts => Set<PriceAlert>();

    // Asset Management
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<CryptoPosition> CryptoPositions => Set<CryptoPosition>();
    public DbSet<StockPosition> StockPositions => Set<StockPosition>();
    public DbSet<SavingPosition> SavingPositions => Set<SavingPosition>();
    public DbSet<PositionTransaction> PositionTransactions => Set<PositionTransaction>();

    // Notes
    public DbSet<Note> Notes => Set<Note>();

    // Resumes
    public DbSet<ResumeProfile> ResumeProfiles => Set<ResumeProfile>();

    // Blog
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Domain.Aggregates.Blog.Category> Categories => Set<Domain.Aggregates.Blog.Category>();
    public DbSet<Domain.Aggregates.Blog.Tag> Tags => Set<Domain.Aggregates.Blog.Tag>();
    public DbSet<PostTag> PostTags => Set<PostTag>();

    // Expense Tracker
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Domain.Aggregates.ExpenseTracker.Category> ExpenseCategories => Set<Domain.Aggregates.ExpenseTracker.Category>();
    public DbSet<Transaction> ExpenseTransactions => Set<Transaction>();
    public DbSet<Transfer> Transfers => Set<Transfer>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<SavingGoal> SavingGoals => Set<SavingGoal>();
    public DbSet<Domain.Aggregates.ExpenseTracker.Tag> ExpenseTags => Set<Domain.Aggregates.ExpenseTracker.Tag>();
    public DbSet<TransactionTag> TransactionTags => Set<TransactionTag>();
    public DbSet<RecurringTransaction> RecurringTransactions => Set<RecurringTransaction>();

    // Notifications
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<NotificationDelivery> NotificationDeliveries => Set<NotificationDelivery>();
    public DbSet<UserNotificationPreference> UserNotificationPreferences => Set<UserNotificationPreference>();
    public DbSet<NotificationSchedule> NotificationSchedules => Set<NotificationSchedule>();
    public DbSet<NotificationScheduleExecution> NotificationScheduleExecutions => Set<NotificationScheduleExecution>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder
            .Properties<TimeOnly>()
            .HaveConversion<ThaiX.Infrastructure.Persistence.Converters.TimeOnlyToTimeSpanConverter>();

        configurationBuilder
            .Properties<DateOnly>()
            .HaveConversion<ThaiX.Infrastructure.Persistence.Converters.DateOnlyToDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Ignore<Microsoft.AspNetCore.Identity.IdentityUserPasskey<Guid>>();

        // Configure base entity properties
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            // Configure BaseEntity properties
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // RowVersion for optimistic concurrency: DB-generated timestamp, not app-managed.
                builder.Entity(entityType.ClrType)
                    .Property<DateTime>("RowVersion")
                    .HasColumnType("timestamp(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                    .IsRowVersion();

                // CreatedAt is required
                builder.Entity(entityType.ClrType)
                    .Property<DateTime>("CreatedAt")
                    .IsRequired();

                // UpdatedAt is optional
                builder.Entity(entityType.ClrType)
                    .Property<DateTime?>("UpdatedAt");
            }

            // Configure BaseAuditableEntity properties
            if (typeof(BaseAuditableEntity).IsAssignableFrom(entityType.ClrType))
            {
                // CreatedBy is optional (system-created entities may not have a user)
                builder.Entity(entityType.ClrType)
                    .Property<Guid?>("CreatedBy");

                // UpdatedBy is optional
                builder.Entity(entityType.ClrType)
                    .Property<Guid?>("UpdatedBy");

                // IsDeleted is required (default false)
                builder.Entity(entityType.ClrType)
                    .Property<bool>("IsDeleted")
                    .IsRequired()
                    .HasDefaultValue(false);

                // DeletedAt is optional
                builder.Entity(entityType.ClrType)
                    .Property<DateTime?>("DeletedAt");

                // DeletedBy is optional
                builder.Entity(entityType.ClrType)
                    .Property<Guid?>("DeletedBy");

                // Global query filter for soft delete
                // Automatically filters out soft-deleted entities
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, "IsDeleted");
                var filter = System.Linq.Expressions.Expression.Lambda(
                    System.Linq.Expressions.Expression.Equal(property, System.Linq.Expressions.Expression.Constant(false)),
                    parameter);

                builder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "DbUpdateConcurrencyException detected.");

            foreach (var entry in ex.Entries)
            {
                _logger.LogError(
                    "Entity: {Entity}, Type: {Type}, State: {State}",
                    entry.Metadata.DisplayName(),
                    entry.Entity.GetType().FullName,
                    entry.State);

                _logger.LogDebug(
                    "DebugView:{NewLine}{DebugView}",
                    Environment.NewLine,
                    entry.DebugView.LongView);

                foreach (var property in entry.Properties)
                {
                    _logger.LogDebug(
                        "Property {Property}: Original={Original}, Current={Current}",
                        property.Metadata.Name,
                        property.OriginalValue,
                        property.CurrentValue);
                }

                var databaseValues = await entry.GetDatabaseValuesAsync(cancellationToken);

                if (databaseValues is null)
                {
                    _logger.LogWarning(
                        "Database row no longer exists for entity {Entity}.",
                        entry.Metadata.DisplayName());
                }
                else
                {
                    foreach (var property in databaseValues.Properties)
                    {
                        _logger.LogDebug(
                            "Database {Property}: {Value}",
                            property.Name,
                            databaseValues[property]);
                    }
                }
            }

            throw;
        }
    }
}
