namespace ThaiX.Domain.Common.Constants;

/// <summary>
/// Centralized permission definitions for the entire system.
/// Permissions follow the format: Feature.Action
/// </summary>
public static class Permissions
{
    // User Management
    public const string UserRead = "User.Read";
    public const string UserWrite = "User.Write";
    public const string UserDelete = "User.Delete";
    public const string UserManageRoles = "User.ManageRoles";
    public const string UserManagePermissions = "User.ManagePermissions";
    public const string UserManageLockout = "User.ManageLockout";
    public const string UserResetPassword = "User.ResetPassword";

    // Role Management
    public const string RoleRead = "Role.Read";
    public const string RoleWrite = "Role.Write";
    public const string RoleDelete = "Role.Delete";
    public const string RoleManagePermissions = "Role.ManagePermissions";

    // System Administration
    public const string SystemAdmin = "System.Admin";
    public const string SystemViewLogs = "System.ViewLogs";
    public const string SystemViewJobs = "System.ViewJobs";
    public const string SystemManageJobs = "System.ManageJobs";

    // Admin Tools
    public const string HangfireView = "Hangfire.View";
    public const string SwaggerView = "Swagger.View";

    // API Client Management (for M2M)
    public const string ApiClientRead = "ApiClient.Read";
    public const string ApiClientWrite = "ApiClient.Write";
    public const string ApiClientDelete = "ApiClient.Delete";
    public const string ApiClientManageScopes = "ApiClient.ManageScopes";

    // Contact Management
    public const string ContactRead = "Contact.Read";
    public const string ContactWrite = "Contact.Write";
    public const string ContactDelete = "Contact.Delete";

    // File Management
    public const string FileRead = "File.Read";
    public const string FileWrite = "File.Write";
    public const string FileDelete = "File.Delete";

    // Master Data Management
    public const string MasterDataRead = "MasterData.Read";
    public const string MasterDataWrite = "MasterData.Write";
    public const string MasterDataDelete = "MasterData.Delete";

    // ChainBroker Data
    public const string ChainBrokerDataRead = "ChainBrokerData.Read";
    public const string ChainBrokerDataSync = "ChainBrokerData.Sync";

    // TCBS Top 10 Data
    public const string TcbsTop10DataSync = "TcbsTop10Data.Sync";

    // Market Scanner Rules
    public const string MarketScannerRuleRead = "MarketScannerRule.Read";
    public const string MarketScannerRuleWrite = "MarketScannerRule.Write";
    public const string MarketScannerRuleDelete = "MarketScannerRule.Delete";

    // Price Alerts
    public const string PriceAlertRead = "PriceAlert.Read";
    public const string PriceAlertWrite = "PriceAlert.Write";
    public const string PriceAlertDelete = "PriceAlert.Delete";

    // Trading
    public const string TradingView = "Trading.View";

    // Portfolio Management
    public const string PortfolioRead = "Portfolio.Read";
    public const string PortfolioWrite = "Portfolio.Write";
    public const string PortfolioDelete = "Portfolio.Delete";

    // Asset Positions
    public const string PositionRead = "Position.Read";
    public const string PositionWrite = "Position.Write";
    public const string PositionDelete = "Position.Delete";

    // Bot Commands (M2M)
    public const string BotCommandExecute = "BotCommand.Execute";

    // Notes
    public const string NoteRead = "Note.Read";
    public const string NoteWrite = "Note.Write";
    public const string NoteDelete = "Note.Delete";

    // Credential Accounts
    public const string CredentialAccountRead = "CredentialAccount.Read";
    public const string CredentialAccountWrite = "CredentialAccount.Write";
    public const string CredentialAccountDelete = "CredentialAccount.Delete";
    public const string CredentialAccountViewPassword = "CredentialAccount.ViewPassword";

    // Notification Schedules
    public const string NotificationScheduleRead = "NotificationSchedule.Read";
    public const string NotificationScheduleWrite = "NotificationSchedule.Write";
    public const string NotificationScheduleDelete = "NotificationSchedule.Delete";
    public const string NotificationScheduleRun = "NotificationSchedule.Run";
    public const string NotificationScheduleManage = "NotificationSchedule.Manage";

    // Notification Delivery
    public const string NotificationDeliveryRead = "NotificationDelivery.Read";
    public const string NotificationDeliveryRetry = "NotificationDelivery.Retry";

    // Notification Payload & Audit
    public const string NotificationPayloadView = "NotificationPayload.View";
    public const string NotificationAuditRead = "NotificationAudit.Read";

    // Resume / Portfolio Website
    public const string ResumeRead = "Resume.Read";
    public const string ResumeWrite = "Resume.Write";

    // Blog
    public const string BlogRead = "Blog.Read";
    public const string BlogWrite = "Blog.Write";
    public const string BlogDelete = "Blog.Delete";
    public const string BlogPublish = "Blog.Publish";

    // JsonBin
    public const string JsonBinRead = "JsonBin.Read";
    public const string JsonBinWrite = "JsonBin.Write";

    // Expense Tracker - Wallet
    public const string WalletRead = "Wallet.Read";
    public const string WalletWrite = "Wallet.Write";
    public const string WalletDelete = "Wallet.Delete";

    // Expense Tracker - Category
    public const string ExpenseCategoryRead = "ExpenseCategory.Read";
    public const string ExpenseCategoryWrite = "ExpenseCategory.Write";
    public const string ExpenseCategoryDelete = "ExpenseCategory.Delete";

    // Expense Tracker - Transaction
    public const string ExpenseTransactionRead = "ExpenseTransaction.Read";
    public const string ExpenseTransactionWrite = "ExpenseTransaction.Write";
    public const string ExpenseTransactionDelete = "ExpenseTransaction.Delete";

    // Expense Tracker - Transfer
    public const string TransferRead = "Transfer.Read";
    public const string TransferWrite = "Transfer.Write";
    public const string TransferDelete = "Transfer.Delete";

    // Expense Tracker - Budget
    public const string BudgetRead = "Budget.Read";
    public const string BudgetWrite = "Budget.Write";
    public const string BudgetDelete = "Budget.Delete";

    // Expense Tracker - Saving Goal
    public const string SavingGoalRead = "SavingGoal.Read";
    public const string SavingGoalWrite = "SavingGoal.Write";
    public const string SavingGoalDelete = "SavingGoal.Delete";

    // Expense Tracker - Tag
    public const string ExpenseTagRead = "ExpenseTag.Read";
    public const string ExpenseTagWrite = "ExpenseTag.Write";
    public const string ExpenseTagDelete = "ExpenseTag.Delete";

    // Expense Tracker - Transaction Tag
    public const string TransactionTagRead = "TransactionTag.Read";
    public const string TransactionTagWrite = "TransactionTag.Write";
    public const string TransactionTagDelete = "TransactionTag.Delete";

    // Expense Tracker - Recurring Transaction
    public const string RecurringTransactionRead = "RecurringTransaction.Read";
    public const string RecurringTransactionWrite = "RecurringTransaction.Write";
    public const string RecurringTransactionDelete = "RecurringTransaction.Delete";

    /// <summary>
    /// Returns all permissions defined in this class.
    /// </summary>
    public static IReadOnlyCollection<string> GetAll()
    {
        return typeof(Permissions)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => (string)f.GetValue(null)!)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Returns default admin permissions.
    /// </summary>
    public static IReadOnlyCollection<string> GetAdminPermissions()
    {
        return GetAll();
    }
}
