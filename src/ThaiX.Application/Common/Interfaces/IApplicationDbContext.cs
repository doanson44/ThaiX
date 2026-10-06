using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
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
using ThaiX.Domain.Aggregates.Outbox;
using ThaiX.Domain.Aggregates.Portfolios;
using ThaiX.Domain.Aggregates.PriceAlerts;
using ThaiX.Domain.Aggregates.Resumes;
using ThaiX.Domain.Aggregates.TcbsTop10;
using ThaiX.Domain.Aggregates.TradingSuggestions;
using ThaiX.Domain.Common.Entities;
using ThaiX.Domain.Identity;

namespace ThaiX.Application.Common.Interfaces;

/// <summary>
/// Interface for ApplicationDbContext.
/// Allows Application layer to query data without depending on Infrastructure.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// API clients for M2M authentication.
    /// </summary>
    DbSet<ApiClient> ApiClients { get; }

    /// <summary>
    /// Outbox messages for domain event processing.
    /// </summary>
    DbSet<OutboxMessage> OutboxMessages { get; }

    /// <summary>
    /// Contacts aggregate.
    /// </summary>
    DbSet<Contact> Contacts { get; }

    /// <summary>
    /// Contact emails (child of Contact). Use for direct query/update without loading Contact.
    /// </summary>
    DbSet<ContactEmail> ContactEmails { get; }

    /// <summary>
    /// Contact phones (child of Contact).
    /// </summary>
    DbSet<ContactPhone> ContactPhones { get; }

    /// <summary>
    /// Contact addresses (child of Contact).
    /// </summary>
    DbSet<ContactAddress> ContactAddresses { get; }

    /// <summary>
    /// Contact social links (child of Contact).
    /// </summary>
    DbSet<ContactSocialLink> ContactSocialLinks { get; }

    /// <summary>
    /// Contact tags (child of Contact).
    /// </summary>
    DbSet<ContactTag> ContactTags { get; }

    /// <summary>
    /// Contact bank accounts (child of Contact).
    /// </summary>
    DbSet<ContactBankAccount> ContactBankAccounts { get; }

    /// <summary>
    /// Contact identity documents (child of Contact).
    /// </summary>
    DbSet<ContactIdentityDocument> ContactIdentityDocuments { get; }

    /// <summary>
    /// Countries master data.
    /// </summary>
    DbSet<Country> Countries { get; }

    /// <summary>
    /// Cities master data.
    /// </summary>
    DbSet<City> Cities { get; }

    /// <summary>
    /// Districts master data.
    /// </summary>
    DbSet<District> Districts { get; }

    /// <summary>
    /// Banks master data.
    /// </summary>
    DbSet<Bank> Banks { get; }

    /// <summary>
    /// Contact import jobs (async bulk import tracking).
    /// </summary>
    DbSet<ContactImportJob> ContactImportJobs { get; }

    /// <summary>
    /// Shared credential accounts managed by admins.
    /// </summary>
    DbSet<CredentialAccount> CredentialAccounts { get; }

    /// <summary>
    /// Immutable audit trail for credential account activity.
    /// </summary>
    DbSet<CredentialAccountAudit> CredentialAccountAudits { get; }

    /// <summary>
    /// JSON binary storage entries.
    /// </summary>
    DbSet<JsonBin> JsonBins { get; }

    /// <summary>
    /// File attachment metadata (file content in storage).
    /// </summary>
    DbSet<FileAttachment> FileAttachments { get; }

    /// <summary>
    /// User-Contact links for profile projection.
    /// </summary>
    DbSet<UserContactLink> UserContactLinks { get; }

    /// <summary>
    /// User profile read model (FullName, AvatarUrl from linked Contact).
    /// </summary>
    DbSet<UserProfile> UserProfiles { get; }

    // ChainBroker snapshot data
    DbSet<ChainBrokerUnlock> ChainBrokerUnlocks { get; }
    DbSet<ChainBrokerProject> ChainBrokerProjects { get; }
    DbSet<ChainBrokerProjectBlockchain> ChainBrokerProjectBlockchains { get; }
    DbSet<ChainBrokerProjectTag> ChainBrokerProjectTags { get; }
    DbSet<ChainBrokerProjectFundRef> ChainBrokerProjectFundRefs { get; }
    DbSet<ChainBrokerProjectLaunchpadRef> ChainBrokerProjectLaunchpadRefs { get; }
    DbSet<ChainBrokerFund> ChainBrokerFunds { get; }

    // TCBS Top 10 portfolio data
    DbSet<TcbsTop10Portfolio> TcbsTop10Portfolios { get; }
    DbSet<TcbsTop10Ticker> TcbsTop10Tickers { get; }
    DbSet<TcbsTop10Image> TcbsTop10Images { get; }
    DbSet<WeeklySuggestionReport> WeeklySuggestionReports { get; }
    DbSet<WeeklySuggestionReportItem> WeeklySuggestionReportItems { get; }

    /// <summary>24HMoney transaction history (synced daily, retained 1 year).</summary>
    DbSet<TwentyFourHMoneyTransaction> TwentyFourHMoneyTransactions { get; }

    /// <summary>Power 6/55 lottery results (synced from ketquadientoan.com).</summary>
    DbSet<Power655Result> Power655Results { get; }
    DbSet<Power655Prediction> Power655Predictions { get; }

    /// <summary>
    /// Market scanner rules for anomaly detection configuration.
    /// </summary>
    DbSet<MarketScannerRule> MarketScannerRules { get; }

    /// <summary>
    /// Price alerts for asset price monitoring.
    /// </summary>
    DbSet<PriceAlert> PriceAlerts { get; }

    /// <summary>Portfolios for grouping asset positions.</summary>
    DbSet<Portfolio> Portfolios { get; }

    /// <summary>Cryptocurrency positions.</summary>
    DbSet<CryptoPosition> CryptoPositions { get; }

    /// <summary>Stock positions.</summary>
    DbSet<StockPosition> StockPositions { get; }

    /// <summary>Saving/deposit positions.</summary>
    DbSet<SavingPosition> SavingPositions { get; }

    /// <summary>Position transactions (shared by Crypto and Stock).</summary>
    DbSet<PositionTransaction> PositionTransactions { get; }

    /// <summary>Notes for the note-taking feature.</summary>
    DbSet<Note> Notes { get; }

    /// <summary>Notifications created by domain/application events.</summary>
    DbSet<Notification> Notifications { get; }

    /// <summary>Per-channel notification delivery records.</summary>
    DbSet<NotificationDelivery> NotificationDeliveries { get; }

    /// <summary>User notification channel preferences.</summary>
    DbSet<UserNotificationPreference> UserNotificationPreferences { get; }

    /// <summary>Notification schedule definitions.</summary>
    DbSet<NotificationSchedule> NotificationSchedules { get; }

    /// <summary>Notification schedule execution history.</summary>
    DbSet<NotificationScheduleExecution> NotificationScheduleExecutions { get; }

    /// <summary>Public resume/portfolio-website profiles.</summary>
    DbSet<ResumeProfile> ResumeProfiles { get; }

    /// <summary>Blog posts.</summary>
    DbSet<Post> Posts { get; }

    /// <summary>Blog categories (flat).</summary>
    DbSet<Domain.Aggregates.Blog.Category> Categories { get; }

    /// <summary>Blog tags.</summary>
    DbSet<Domain.Aggregates.Blog.Tag> Tags { get; }

    /// <summary>Post &lt;-&gt; Tag join rows.</summary>
    DbSet<PostTag> PostTags { get; }

    /// <summary>Expense tracker wallets.</summary>
    DbSet<Wallet> Wallets { get; }

    /// <summary>Expense tracker categories.</summary>
    DbSet<Domain.Aggregates.ExpenseTracker.Category> ExpenseCategories { get; }

    /// <summary>Expense tracker transactions.</summary>
    DbSet<Transaction> ExpenseTransactions { get; }

    /// <summary>Expense tracker transfers.</summary>
    DbSet<Transfer> Transfers { get; }

    /// <summary>Expense tracker budgets.</summary>
    DbSet<Budget> Budgets { get; }

    /// <summary>Expense tracker saving goals.</summary>
    DbSet<SavingGoal> SavingGoals { get; }

    /// <summary>Expense tracker tags.</summary>
    DbSet<Domain.Aggregates.ExpenseTracker.Tag> ExpenseTags { get; }

    /// <summary>Expense tracker transaction-tag links.</summary>
    DbSet<TransactionTag> TransactionTags { get; }

    /// <summary>Expense tracker recurring transactions.</summary>
    DbSet<RecurringTransaction> RecurringTransactions { get; }

    DatabaseFacade Database { get; }

    /// <summary>
    /// Saves all changes made in this context to the database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
