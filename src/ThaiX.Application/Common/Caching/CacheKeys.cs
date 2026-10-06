using ThaiX.Domain.Aggregates.AssetPositions;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Common.Caching;

/// <summary>
/// Centralized cache key builders. Never hardcode cache keys elsewhere.
/// Naming: entity:detail:id, entity:list:v:{version}:params, entity:projection:id.
/// </summary>
public static class CacheKeys
{
    public static class Contacts
    {
        /// <summary>contact:detail:{id}</summary>
        public static string Detail(Guid id) => $"contact:detail:{id:N}";

        /// <summary>contact:projection:{id} - heavy ContactDetailDto projection.</summary>
        public static string Projection(Guid id) => $"contact:projection:{id:N}";

        /// <summary>Version key for list cache; value is int. Increment on contact data change.</summary>
        public static string ListVersionKey => "contacts:list:version";

        /// <summary>contacts:list:v:{version}:{search}:{page}:{size}:{sortBy}:{sortDesc}:{isArchived}:{tag}. Use {version} placeholder; behavior substitutes at runtime.</summary>
        public static string ListVersioned(string? search, int page, int size, string? sortBy, bool sortDesc, bool? isArchived, string? tag)
        {
            var searchPart = string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant();
            var sortPart = string.IsNullOrWhiteSpace(sortBy) ? "lastname" : sortBy.Trim().ToLowerInvariant();
            var archivedPart = isArchived.HasValue ? isArchived.Value ? "1" : "0" : "x";
            var tagPart = string.IsNullOrWhiteSpace(tag) ? "" : tag.Trim().ToLowerInvariant();
            return $"contacts:list:v:{{version}}:{searchPart}:{page}:{size}:{sortPart}:{(sortDesc ? "1" : "0")}:{archivedPart}:{tagPart}";
        }
    }

    public static class UserProfiles
    {
        /// <summary>userprofile:profile:{userId}</summary>
        public static string Profile(Guid userId) => $"userprofile:profile:{userId:N}";

        /// <summary>userprofile:linked:{userId}</summary>
        public static string LinkedContact(Guid userId) => $"userprofile:linked:{userId:N}";
    }

    public static class Suggestions
    {
        /// <summary>suggestions:usersbyphone:{contactId}</summary>
        public static string UsersByPhone(Guid contactId) => $"suggestions:usersbyphone:{contactId:N}";
    }

    public static class ChainBrokerData
    {
        public static string FundsList(string? search, int page, int size) =>
            $"chainbroker:funds:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{page}:{size}";

        public static string ProjectsList(string? search, int page, int size) =>
            $"chainbroker:projects:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{page}:{size}";

        public static string UnlocksList(string? search, int page, int size) =>
            $"chainbroker:unlocks:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{page}:{size}";

        public static string FundsListVersionKey => "chainbroker:funds:list:version";
        public static string ProjectsListVersionKey => "chainbroker:projects:list:version";
        public static string UnlocksListVersionKey => "chainbroker:unlocks:list:version";
    }

    public static class TcbsTop10Data
    {
        public static string PortfoliosList() => "tcbstop10:portfolios:all";
    }

    public static class ExternalData
    {
        public static string VnDirectTopStocks(bool includeEnrichment, int maxEventsLookbackDays) =>
            $"externaldata:vndirect:topstocks:{(includeEnrichment ? "1" : "0")}:{maxEventsLookbackDays}:lq1";

        /// <summary>externaldata:symbol:lookup:{key}</summary>
        public static string SymbolLookup(string key) => $"externaldata:symbol:lookup:{key}";

        /// <summary>externaldata:symbol:search:full:{assetType}</summary>
        public static string SymbolSearchFullList(string assetType)
            => $"externaldata:symbol:search:full:{assetType.ToLowerInvariant()}";
    }

    public static class Trading
    {
        public static string Suggestion(string symbol, string marketType, string timeframe, string marketRegime, string eventRisk)
        {
            var s = string.IsNullOrWhiteSpace(symbol) ? "" : symbol.Trim().ToUpperInvariant();
            var m = string.IsNullOrWhiteSpace(marketType) ? "" : marketType.Trim().ToUpperInvariant();
            var t = string.IsNullOrWhiteSpace(timeframe) ? "" : timeframe.Trim().ToUpperInvariant();
            var r = string.IsNullOrWhiteSpace(marketRegime) ? "" : marketRegime.Trim().ToUpperInvariant();
            var e = string.IsNullOrWhiteSpace(eventRisk) ? "" : eventRisk.Trim().ToUpperInvariant();
            return $"trading:suggestion:{s}:{m}:{t}:{r}:{e}";
        }
    }

    public static class MarketScanner
    {
        public static string RulesListVersionKey => "marketscanner:rules:list:version";

        public static string RulesList(string? search, bool? isEnabled, int page, int size) =>
            $"marketscanner:rules:list:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{(isEnabled.HasValue ? isEnabled.Value.ToString().ToLowerInvariant() : "all")}:{page}:{size}";

        public static string AllRules(bool? isEnabled) =>
            $"marketscanner:rules:all:{(isEnabled.HasValue ? isEnabled.Value.ToString().ToLowerInvariant() : "all")}";
    }

    public static class PriceAlerts
    {
        /// <summary>pricealerts:all:active - all enabled, non-deleted alerts. No expiry; invalidated by CRUD.</summary>
        public static string AllActive() => "pricealerts:all:active";

        public static string PriceAlertsListVersionKey => "pricealerts:list:version";

        public static string PriceAlertsList(string? search, bool? isEnabled, int page, int size) =>
            $"pricealerts:list:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{(isEnabled.HasValue ? isEnabled.Value.ToString().ToLowerInvariant() : "all")}:{page}:{size}";
    }

    public static class Portfolios
    {
        public static string ListVersionKey => "portfolios:list:version";

        public static string List(Guid ownerId, string? searchTerm, int page, int size) =>
            $"portfolios:list:v:{{version}}:{ownerId:N}:{(string.IsNullOrWhiteSpace(searchTerm) ? "" : searchTerm.Trim().ToLowerInvariant())}:{page}:{size}";

        public static string Detail(Guid id) => $"portfolio:detail:{id:N}";
    }

    public static class CryptoPositions
    {
        public static string ListVersionKey => "cryptopositions:list:version";

        public static string List(Guid portfolioId, bool? isClosed, int page, int size) =>
            $"cryptopositions:list:v:{{version}}:{portfolioId:N}:{(isClosed.HasValue ? isClosed.Value ? "1" : "0" : "all")}:{page}:{size}";

        public static string Transactions(Guid positionId, int page, int size) =>
            $"cryptopositions:transactions:{positionId:N}:{page}:{size}";
    }

    public static class StockPositions
    {
        public static string ListVersionKey => "stockpositions:list:version";

        public static string List(Guid portfolioId, bool? isClosed, int page, int size) =>
            $"stockpositions:list:v:{{version}}:{portfolioId:N}:{(isClosed.HasValue ? isClosed.Value ? "1" : "0" : "all")}:{page}:{size}";

        public static string Transactions(Guid positionId, int page, int size) =>
            $"stockpositions:transactions:{positionId:N}:{page}:{size}";
    }

    public static class SavingPositions
    {
        public static string ListVersionKey => "savingpositions:list:version";

        public static string List(Guid portfolioId, SavingStatus? status, int page, int size) =>
            $"savingpositions:list:v:{{version}}:{portfolioId:N}:{(status.HasValue ? status.Value.ToString().ToLowerInvariant() : "all")}:{page}:{size}";
    }

    public static class Notes
    {
        public static string ListVersionKey => "notes:list:version";

        public static string List(Guid ownerId, string? search, bool? isPinned, bool? isArchived, bool? isDeleted, int page, int size) =>
            $"notes:list:v:{{version}}:{ownerId:N}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{(isPinned.HasValue ? isPinned.Value ? "1" : "0" : "x")}:{(isArchived.HasValue ? isArchived.Value ? "1" : "0" : "x")}:{(isDeleted.HasValue ? isDeleted.Value ? "1" : "0" : "x")}:{page}:{size}";

        public static string Detail(Guid id) => $"note:detail:{id:N}";
    }

    public static class CredentialAccounts
    {
        public static string ListVersionKey => "credentialaccounts:list:version";

        public static string List(string? search, bool? isUsed, int page, int size) =>
            $"credentialaccounts:list:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{(isUsed.HasValue ? isUsed.Value ? "1" : "0" : "x")}:{page}:{size}";

        public static string Detail(Guid id) => $"credentialaccount:detail:{id:N}";

        public static string AuditLogs(Guid id, int page, int size) => $"credentialaccount:audit:{id:N}:{page}:{size}";
    }

    public static class NotificationSchedules
    {
        public static string ListVersionKey => "notificationschedules:list:version";

        public static string List(string? search, string? status, int page, int size) =>
            $"notificationschedules:list:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{(string.IsNullOrWhiteSpace(status) ? "" : status.Trim().ToLowerInvariant())}:{page}:{size}";

        public static string Executions(Guid scheduleId, int page, int size) =>
            $"notificationschedules:executions:{scheduleId:N}:{page}:{size}";

        public static string Upcoming(int count) =>
            $"notificationschedules:upcoming:{count}";

        public static string Detail(Guid id) => $"notificationschedule:detail:{id:N}";
    }

    public static class Resumes
    {
        public static string ListVersionKey => "resumes:list:version";

        public static string BySlug(string? slug) =>
            $"resumes:byslug:v:{{version}}:{(string.IsNullOrWhiteSpace(slug) ? "" : slug.Trim().ToLowerInvariant())}";
    }

    public static class Blog
    {
        public static string PostsListVersionKey => "blog:posts:list:version";

        public static string BySlug(string? slug) =>
            $"blog:posts:byslug:v:{{version}}:{(string.IsNullOrWhiteSpace(slug) ? "" : slug.Trim().ToLowerInvariant())}";

        public static string PostsList(string? status, Guid? categoryId, string? search, int page, int size) =>
            $"blog:posts:list:v:{{version}}:{status ?? "all"}:{(categoryId.HasValue ? categoryId.Value.ToString("N") : "all")}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{page}:{size}";

        public static string PublishedPostsList(Guid? categoryId, Guid? tagId, int page, int size) =>
            $"blog:posts:published:v:{{version}}:{(categoryId.HasValue ? categoryId.Value.ToString("N") : "all")}:{(tagId.HasValue ? tagId.Value.ToString("N") : "all")}:{page}:{size}";

        public static string CategoriesListKey => "blog:categories:list";
    }

    public static class Lottery
    {
        /// <summary>lottery:power655:analysis</summary>
        public static string Analysis() => "lottery:power655:analysis";
    }

    public static class JsonBins
    {
        public static string ListVersionKey => "jsonbins:list:version";

        public static string List(
            string? searchTerm,
            JsonBinCategories? category,
            Guid? referenceId,
            bool? includeExpired,
            int pageNumber,
            int pageSize) =>
            $"jsonbins:list:v:{{version}}:{searchTerm ?? ""}:{category}:{referenceId}:{includeExpired}:{pageNumber}:{pageSize}";

        public static string ById(Guid id) => $"jsonbins:byid:{id:N}";
        public static string ByCode(string code) => $"jsonbins:bycode:{code}";
        public static string Content(Guid id) => $"jsonbins:content:{id:N}";
    }

    public static class Wallets
    {
        public static string ListVersionKey => "wallets:list:version";
        public static string List(string? search, int page, int size) =>
            $"wallets:list:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{page}:{size}";
        public static string Detail(Guid id) => $"wallets:detail:{id:N}";
    }

    public static class ExpenseCategories
    {
        public static string ListVersionKey => "expensecategories:list:version";
        public static string List(string? search, int page, int size) =>
            $"expensecategories:list:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{page}:{size}";
        public static string Detail(Guid id) => $"expensecategories:detail:{id:N}";
    }

    public static class ExpenseTransactions
    {
        public static string ListVersionKey => "expensetransactions:list:version";
        public static string List(string? search, int page, int size) =>
            $"expensetransactions:list:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{page}:{size}";
        public static string Detail(Guid id) => $"expensetransactions:detail:{id:N}";
    }

    public static class Transfers
    {
        public static string ListVersionKey => "transfers:list:version";
        public static string List(string? search, int page, int size) =>
            $"transfers:list:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{page}:{size}";
        public static string Detail(Guid id) => $"transfers:detail:{id:N}";
    }

    public static class Budgets
    {
        public static string ListVersionKey => "budgets:list:version";
        public static string List(string? search, int page, int size) =>
            $"budgets:list:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{page}:{size}";
        public static string Detail(Guid id) => $"budgets:detail:{id:N}";
    }

    public static class SavingGoals
    {
        public static string ListVersionKey => "savinggoals:list:version";
        public static string List(string? search, int page, int size) =>
            $"savinggoals:list:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{page}:{size}";
        public static string Detail(Guid id) => $"savinggoals:detail:{id:N}";
    }

    public static class ExpenseTags
    {
        public static string ListVersionKey => "expensetags:list:version";
        public static string List(string? search, int page, int size) =>
            $"expensetags:list:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{page}:{size}";
        public static string Detail(Guid id) => $"expensetags:detail:{id:N}";
    }

    public static class TransactionTags
    {
        public static string ListVersionKey => "transactiontags:list:version";
        public static string List(string? search, int page, int size) =>
            $"transactiontags:list:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{page}:{size}";
        public static string Detail(Guid id) => $"transactiontags:detail:{id:N}";
    }

    public static class RecurringTransactions
    {
        public static string ListVersionKey => "recurringtransactions:list:version";
        public static string List(string? search, int page, int size) =>
            $"recurringtransactions:list:v:{{version}}:{(string.IsNullOrWhiteSpace(search) ? "" : search.Trim().ToLowerInvariant())}:{page}:{size}";
        public static string Detail(Guid id) => $"recurringtransactions:detail:{id:N}";
    }

    public static class ExpenseDashboard
    {
        public static string Summary() => "expensedashboard:summary";
        public static string CashFlow() => "expensedashboard:cashflow";
        public static string ExpenseByCategory() => "expensedashboard:expensebycategory";
        public static string IncomeByCategory() => "expensedashboard:incomebycategory";
        public static string RecentTransactions() => "expensedashboard:recenttransactions";
        public static string MonthlyExpenseChart() => "expensedashboard:monthlyexpensechart";
        public static string MonthlyIncomeChart() => "expensedashboard:monthlyincomechart";
    }

    /// <summary>
    /// Returns the list version key for a group when using versioned list cache; null if group does not use versioned list.
    /// </summary>
    public static string? GetListVersionKeyForGroup(string group)
    {
        return group switch
        {
            CacheGroups.Contacts => Contacts.ListVersionKey,
            CacheGroups.MarketScanner => MarketScanner.RulesListVersionKey,
            CacheGroups.PriceAlerts => PriceAlerts.PriceAlertsListVersionKey,
            CacheGroups.Portfolios => Portfolios.ListVersionKey,
            CacheGroups.CryptoPositions => CryptoPositions.ListVersionKey,
            CacheGroups.StockPositions => StockPositions.ListVersionKey,
            CacheGroups.SavingPositions => SavingPositions.ListVersionKey,
            CacheGroups.Notes => Notes.ListVersionKey,
            CacheGroups.CredentialAccounts => CredentialAccounts.ListVersionKey,
            CacheGroups.NotificationSchedules => NotificationSchedules.ListVersionKey,
            CacheGroups.Resumes => Resumes.ListVersionKey,
            CacheGroups.BlogPosts => Blog.PostsListVersionKey,
            CacheGroups.JsonBins => JsonBins.ListVersionKey,
            CacheGroups.Wallets => Wallets.ListVersionKey,
            CacheGroups.ExpenseCategories => ExpenseCategories.ListVersionKey,
            CacheGroups.ExpenseTransactions => ExpenseTransactions.ListVersionKey,
            CacheGroups.Transfers => Transfers.ListVersionKey,
            CacheGroups.Budgets => Budgets.ListVersionKey,
            CacheGroups.SavingGoals => SavingGoals.ListVersionKey,
            CacheGroups.ExpenseTags => ExpenseTags.ListVersionKey,
            CacheGroups.TransactionTags => TransactionTags.ListVersionKey,
            CacheGroups.RecurringTransactions => RecurringTransactions.ListVersionKey,
            _ => null
        };
    }
}
