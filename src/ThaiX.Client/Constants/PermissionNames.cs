namespace ThaiX.Client.Constants;

/// <summary>
/// Client-side permission constants mirrored from API contracts.
/// </summary>
public static class PermissionNames
{
    public const string UserRead = "User.Read";
    public const string UserWrite = "User.Write";
    public const string UserManagePermissions = "User.ManagePermissions";
    public const string UserManageLockout = "User.ManageLockout";
    public const string UserResetPassword = "User.ResetPassword";

    public const string MasterDataRead = "MasterData.Read";
    public const string MasterDataWrite = "MasterData.Write";
    public const string MasterDataDelete = "MasterData.Delete";

    public const string ContactRead = "Contact.Read";
    public const string ContactWrite = "Contact.Write";
    public const string ContactDelete = "Contact.Delete";
    public const string SystemAdmin = "System.Admin";

    public const string ChainBrokerDataRead = "ChainBrokerData.Read";
    public const string ChainBrokerDataSync = "ChainBrokerData.Sync";

    public const string TcbsTop10DataSync = "TcbsTop10Data.Sync";

    public const string MarketScannerRuleRead = "MarketScannerRule.Read";
    public const string MarketScannerRuleWrite = "MarketScannerRule.Write";
    public const string MarketScannerRuleDelete = "MarketScannerRule.Delete";

    public const string PriceAlertRead = "PriceAlert.Read";
    public const string PriceAlertWrite = "PriceAlert.Write";
    public const string PriceAlertDelete = "PriceAlert.Delete";

    public const string PortfolioRead = "Portfolio.Read";
    public const string PortfolioWrite = "Portfolio.Write";
    public const string PortfolioDelete = "Portfolio.Delete";
    public const string PositionRead = "Position.Read";
    public const string PositionWrite = "Position.Write";
    public const string PositionDelete = "Position.Delete";

    public const string ApiClientRead = "ApiClient.Read";
    public const string ApiClientWrite = "ApiClient.Write";
    public const string ApiClientDelete = "ApiClient.Delete";
    public const string ApiClientManageScopes = "ApiClient.ManageScopes";

    public const string NoteRead = "Note.Read";
    public const string NoteWrite = "Note.Write";
    public const string NoteDelete = "Note.Delete";

    public const string NotificationScheduleRead = "NotificationSchedule.Read";
    public const string NotificationScheduleWrite = "NotificationSchedule.Write";
    public const string NotificationScheduleDelete = "NotificationSchedule.Delete";
    public const string NotificationScheduleRun = "NotificationSchedule.Run";
    public const string NotificationScheduleManage = "NotificationSchedule.Manage";

    public const string NotificationDeliveryRead = "NotificationDelivery.Read";
    public const string NotificationDeliveryRetry = "NotificationDelivery.Retry";

    public const string NotificationPayloadView = "NotificationPayload.View";
    public const string NotificationAuditRead = "NotificationAudit.Read";
    public const string CredentialAccountRead = "CredentialAccount.Read";
    public const string CredentialAccountWrite = "CredentialAccount.Write";
    public const string CredentialAccountDelete = "CredentialAccount.Delete";
    public const string CredentialAccountViewPassword = "CredentialAccount.ViewPassword";

    public const string ResumeRead = "Resume.Read";
    public const string ResumeWrite = "Resume.Write";

    public const string BlogRead = "Blog.Read";
    public const string BlogWrite = "Blog.Write";
    public const string BlogDelete = "Blog.Delete";
    public const string BlogPublish = "Blog.Publish";

    public const string JsonBinRead = "JsonBin.Read";
    public const string JsonBinWrite = "JsonBin.Write";

    public const string WalletRead = "Wallet.Read";
    public const string WalletWrite = "Wallet.Write";
    public const string WalletDelete = "Wallet.Delete";
    public const string ExpenseCategoryRead = "ExpenseCategory.Read";
    public const string ExpenseCategoryWrite = "ExpenseCategory.Write";
    public const string ExpenseCategoryDelete = "ExpenseCategory.Delete";
    public const string ExpenseTransactionRead = "ExpenseTransaction.Read";
    public const string ExpenseTransactionWrite = "ExpenseTransaction.Write";
    public const string ExpenseTransactionDelete = "ExpenseTransaction.Delete";
    public const string TransferRead = "Transfer.Read";
    public const string TransferWrite = "Transfer.Write";
    public const string TransferDelete = "Transfer.Delete";
    public const string BudgetRead = "Budget.Read";
    public const string BudgetWrite = "Budget.Write";
    public const string BudgetDelete = "Budget.Delete";
    public const string SavingGoalRead = "SavingGoal.Read";
    public const string SavingGoalWrite = "SavingGoal.Write";
    public const string SavingGoalDelete = "SavingGoal.Delete";
    public const string ExpenseTagRead = "ExpenseTag.Read";
    public const string ExpenseTagWrite = "ExpenseTag.Write";
    public const string ExpenseTagDelete = "ExpenseTag.Delete";
    public const string TransactionTagRead = "TransactionTag.Read";
    public const string TransactionTagWrite = "TransactionTag.Write";
    public const string TransactionTagDelete = "TransactionTag.Delete";
    public const string RecurringTransactionRead = "RecurringTransaction.Read";
    public const string RecurringTransactionWrite = "RecurringTransaction.Write";
    public const string RecurringTransactionDelete = "RecurringTransaction.Delete";

    public static IReadOnlyCollection<string> GetAll()
    {
        return
        [
            UserRead,
            UserWrite,
            UserManagePermissions,
            UserManageLockout,
            UserResetPassword,
            MasterDataRead,
            MasterDataWrite,
            MasterDataDelete,
            ContactRead,
            ContactWrite,
            ContactDelete,
            SystemAdmin,
            ChainBrokerDataRead,
            ChainBrokerDataSync,
            TcbsTop10DataSync,
            MarketScannerRuleRead,
            MarketScannerRuleWrite,
            MarketScannerRuleDelete,
            PriceAlertRead,
            PriceAlertWrite,
            PriceAlertDelete,
            PortfolioRead,
            PortfolioWrite,
            PortfolioDelete,
            PositionRead,
            PositionWrite,
            PositionDelete,
            ApiClientRead,
            ApiClientWrite,
            ApiClientDelete,
            ApiClientManageScopes,
            NoteRead,
            NoteWrite,
            NoteDelete,
            NotificationScheduleRead,
            NotificationScheduleWrite,
            NotificationScheduleDelete,
            NotificationScheduleRun,
            NotificationScheduleManage,
            NotificationDeliveryRead,
            NotificationDeliveryRetry,
            NotificationPayloadView,
            NotificationAuditRead,
            CredentialAccountRead,
            CredentialAccountWrite,
            CredentialAccountDelete,
            CredentialAccountViewPassword,
            ResumeRead,
            ResumeWrite,
            BlogRead,
            BlogWrite,
            BlogDelete,
            BlogPublish,
            JsonBinRead,
            JsonBinWrite,
            WalletRead,
            WalletWrite,
            WalletDelete,
            ExpenseCategoryRead,
            ExpenseCategoryWrite,
            ExpenseCategoryDelete,
            ExpenseTransactionRead,
            ExpenseTransactionWrite,
            ExpenseTransactionDelete,
            TransferRead,
            TransferWrite,
            TransferDelete,
            BudgetRead,
            BudgetWrite,
            BudgetDelete,
            SavingGoalRead,
            SavingGoalWrite,
            SavingGoalDelete,
            ExpenseTagRead,
            ExpenseTagWrite,
            ExpenseTagDelete,
            TransactionTagRead,
            TransactionTagWrite,
            TransactionTagDelete,
            RecurringTransactionRead,
            RecurringTransactionWrite,
            RecurringTransactionDelete
        ];
    }
}
