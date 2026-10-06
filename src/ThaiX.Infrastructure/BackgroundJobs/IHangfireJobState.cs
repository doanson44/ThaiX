namespace ThaiX.Infrastructure.BackgroundJobs;

public static class HangfireJobIds
{
    public const string ProcessOutbox = nameof(ProcessOutbox);
    public const string CleanupOldOutboxMessages = nameof(CleanupOldOutboxMessages);
    public const string ChainBrokerFundsWeeklySyncJob = nameof(ChainBrokerFundsWeeklySyncJob);
    public const string ChainBrokerProjectsWeeklySyncJob = nameof(ChainBrokerProjectsWeeklySyncJob);
    public const string ChainBrokerUnlocksWeeklySyncJob = nameof(ChainBrokerUnlocksWeeklySyncJob);
    public const string IWealthClubTop10DailySyncJob = nameof(IWealthClubTop10DailySyncJob);
    public const string Power655SyncJob = nameof(Power655SyncJob);
    public const string Power655PredictJob = nameof(Power655PredictJob);
    public const string TwentyFourHMoneyTransactionDailySyncJob = nameof(TwentyFourHMoneyTransactionDailySyncJob);
    public const string TopStocksWeeklySuggestionJob = nameof(TopStocksWeeklySuggestionJob);
    public const string MexcSpotWeeklySuggestionJob = nameof(MexcSpotWeeklySuggestionJob);
    public const string MexcMarketScannerJob = nameof(MexcMarketScannerJob);
    public const string PriceAlertCheckerJob = nameof(PriceAlertCheckerJob);
    public const string ScheduledPostPublisherJob = nameof(ScheduledPostPublisherJob);
    public const string PollDueNotificationSchedules = nameof(PollDueNotificationSchedules);
    public const string JsonBinExpiredCleanupJob = nameof(JsonBinExpiredCleanupJob);
}

public interface IHangfireJobState
{
    bool IsEnabled(string jobId);
}
