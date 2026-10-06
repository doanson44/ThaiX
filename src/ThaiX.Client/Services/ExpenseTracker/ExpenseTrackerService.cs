using System.Globalization;
using System.Net.Http.Json;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.ExpenseTracker;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.ExpenseTracker;

public sealed class ExpenseTrackerService(HttpClient httpClient, IClientCacheService cache) : IExpenseTrackerService
{
    public Task<PagedApiResponse<ExpenseWalletDto>> GetWalletsAsync(ExpenseListRequest request, CancellationToken cancellationToken = default)
        => GetListAsync<ExpenseWalletDto>("api/expense-tracker/wallets", request, cancellationToken);

    public Task<PagedApiResponse<ExpenseCategoryDto>> GetCategoriesAsync(ExpenseListRequest request, CancellationToken cancellationToken = default)
        => GetListAsync<ExpenseCategoryDto>("api/expense-tracker/categories", request, cancellationToken);

    public Task<PagedApiResponse<ExpenseTransactionDto>> GetTransactionsAsync(ExpenseListRequest request, CancellationToken cancellationToken = default)
        => GetListAsync<ExpenseTransactionDto>("api/expense-tracker/transactions", request, cancellationToken);

    public Task<PagedApiResponse<ExpenseTransferDto>> GetTransfersAsync(ExpenseListRequest request, CancellationToken cancellationToken = default)
        => GetListAsync<ExpenseTransferDto>("api/expense-tracker/transfers", request, cancellationToken);

    public Task<PagedApiResponse<ExpenseBudgetDto>> GetBudgetsAsync(ExpenseListRequest request, CancellationToken cancellationToken = default)
        => GetListAsync<ExpenseBudgetDto>("api/expense-tracker/budgets", request, cancellationToken);

    public Task<PagedApiResponse<ExpenseSavingGoalDto>> GetSavingGoalsAsync(ExpenseListRequest request, CancellationToken cancellationToken = default)
        => GetListAsync<ExpenseSavingGoalDto>("api/expense-tracker/saving-goals", request, cancellationToken);

    public Task<PagedApiResponse<ExpenseTagDto>> GetTagsAsync(ExpenseListRequest request, CancellationToken cancellationToken = default)
        => GetListAsync<ExpenseTagDto>("api/expense-tracker/tags", request, cancellationToken);

    public Task<PagedApiResponse<ExpenseTransactionTagDto>> GetTransactionTagsAsync(ExpenseListRequest request, CancellationToken cancellationToken = default)
        => GetListAsync<ExpenseTransactionTagDto>("api/expense-tracker/transaction-tags", request, cancellationToken);

    public Task<PagedApiResponse<ExpenseRecurringTransactionDto>> GetRecurringTransactionsAsync(ExpenseListRequest request, CancellationToken cancellationToken = default)
        => GetListAsync<ExpenseRecurringTransactionDto>("api/expense-tracker/recurring-transactions", request, cancellationToken);

    public Task<ExpenseDashboardDataDto> GetDashboardAsync(CancellationToken cancellationToken = default)
        => cache.GetOrCreateGroupedAsync(
            CacheGroups.ExpenseTracker,
            "api/expense-tracker/dashboard",
            async ct =>
            {
                using var response = await httpClient.GetAsync("api/expense-tracker/dashboard", ct);
                return await ApiResponseReader.ReadSuccessDataAsync<ExpenseDashboardDataDto>(response, ct);
            },
            cancellationToken: cancellationToken);

    public Task<Guid> CreateWalletAsync(ExpenseWalletWriteRequest request, CancellationToken cancellationToken = default)
        => PostAsync("api/expense-tracker/wallets", request, cancellationToken);

    public Task UpdateWalletAsync(Guid id, ExpenseWalletWriteRequest request, CancellationToken cancellationToken = default)
        => PutAsync($"api/expense-tracker/wallets/{id}", request, cancellationToken);

    public Task DeleteWalletAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/expense-tracker/wallets/{id}", cancellationToken);

    public Task<Guid> CreateCategoryAsync(ExpenseCategoryWriteRequest request, CancellationToken cancellationToken = default)
        => PostAsync("api/expense-tracker/categories", request, cancellationToken);

    public Task UpdateCategoryAsync(Guid id, ExpenseCategoryWriteRequest request, CancellationToken cancellationToken = default)
        => PutAsync($"api/expense-tracker/categories/{id}", request, cancellationToken);

    public Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/expense-tracker/categories/{id}", cancellationToken);

    public Task<Guid> CreateTransactionAsync(ExpenseTransactionWriteRequest request, CancellationToken cancellationToken = default)
        => PostAsync("api/expense-tracker/transactions", request, cancellationToken);

    public Task UpdateTransactionAsync(Guid id, ExpenseTransactionWriteRequest request, CancellationToken cancellationToken = default)
        => PutAsync($"api/expense-tracker/transactions/{id}", request, cancellationToken);

    public Task DeleteTransactionAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/expense-tracker/transactions/{id}", cancellationToken);

    public Task<Guid> CreateTransferAsync(ExpenseTransferWriteRequest request, CancellationToken cancellationToken = default)
        => PostAsync("api/expense-tracker/transfers", request, cancellationToken);

    public Task UpdateTransferAsync(Guid id, ExpenseTransferWriteRequest request, CancellationToken cancellationToken = default)
        => PutAsync($"api/expense-tracker/transfers/{id}", request, cancellationToken);

    public Task DeleteTransferAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/expense-tracker/transfers/{id}", cancellationToken);

    public Task<Guid> CreateBudgetAsync(ExpenseBudgetWriteRequest request, CancellationToken cancellationToken = default)
        => PostAsync("api/expense-tracker/budgets", request, cancellationToken);

    public Task UpdateBudgetAsync(Guid id, ExpenseBudgetWriteRequest request, CancellationToken cancellationToken = default)
        => PutAsync($"api/expense-tracker/budgets/{id}", request, cancellationToken);

    public Task DeleteBudgetAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/expense-tracker/budgets/{id}", cancellationToken);

    public Task<Guid> CreateSavingGoalAsync(ExpenseSavingGoalWriteRequest request, CancellationToken cancellationToken = default)
        => PostAsync("api/expense-tracker/saving-goals", request, cancellationToken);

    public Task UpdateSavingGoalAsync(Guid id, ExpenseSavingGoalWriteRequest request, CancellationToken cancellationToken = default)
        => PutAsync($"api/expense-tracker/saving-goals/{id}", request, cancellationToken);

    public Task DeleteSavingGoalAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/expense-tracker/saving-goals/{id}", cancellationToken);

    public Task<Guid> CreateTagAsync(ExpenseTagWriteRequest request, CancellationToken cancellationToken = default)
        => PostAsync("api/expense-tracker/tags", request, cancellationToken);

    public Task UpdateTagAsync(Guid id, ExpenseTagWriteRequest request, CancellationToken cancellationToken = default)
        => PutAsync($"api/expense-tracker/tags/{id}", request, cancellationToken);

    public Task DeleteTagAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/expense-tracker/tags/{id}", cancellationToken);

    public Task<Guid> CreateTransactionTagAsync(ExpenseTransactionTagWriteRequest request, CancellationToken cancellationToken = default)
        => PostAsync("api/expense-tracker/transaction-tags", request, cancellationToken);

    public Task UpdateTransactionTagAsync(Guid id, ExpenseTransactionTagWriteRequest request, CancellationToken cancellationToken = default)
        => PutAsync($"api/expense-tracker/transaction-tags/{id}", request, cancellationToken);

    public Task DeleteTransactionTagAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/expense-tracker/transaction-tags/{id}", cancellationToken);

    public Task<Guid> CreateRecurringTransactionAsync(ExpenseRecurringWriteRequest request, CancellationToken cancellationToken = default)
        => PostAsync("api/expense-tracker/recurring-transactions", request, cancellationToken);

    public Task UpdateRecurringTransactionAsync(Guid id, ExpenseRecurringWriteRequest request, CancellationToken cancellationToken = default)
        => PutAsync($"api/expense-tracker/recurring-transactions/{id}", request, cancellationToken);

    public Task DeleteRecurringTransactionAsync(Guid id, CancellationToken cancellationToken = default)
        => DeleteAsync($"api/expense-tracker/recurring-transactions/{id}", cancellationToken);

    private Task<PagedApiResponse<T>> GetListAsync<T>(string baseUrl, ExpenseListRequest request, CancellationToken cancellationToken)
    {
        var path = BuildListPath(baseUrl, request);
        return cache.GetOrCreateGroupedAsync(
            CacheGroups.ExpenseTracker,
            path,
            async ct =>
            {
                using var response = await httpClient.GetAsync(path, ct);
                return await ApiResponseReader.ReadPagedSuccessAsync<T>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    private async Task<Guid> PostAsync<T>(string url, T body, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync(url, body, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await cache.InvalidateGroupAsync(CacheGroups.ExpenseTracker, cancellationToken);
        return id;
    }

    private async Task PutAsync<T>(string url, T body, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PutAsJsonAsync(url, body, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await cache.InvalidateGroupAsync(CacheGroups.ExpenseTracker, cancellationToken);
    }

    private async Task DeleteAsync(string url, CancellationToken cancellationToken)
    {
        using var response = await httpClient.DeleteAsync(url, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await cache.InvalidateGroupAsync(CacheGroups.ExpenseTracker, cancellationToken);
    }

    private static string BuildListPath(string baseUrl, ExpenseListRequest request)
    {
        var sortBy = string.IsNullOrWhiteSpace(request.SortBy) ? "id" : request.SortBy.Trim();
        var path = string.Create(
            CultureInfo.InvariantCulture,
            $"{baseUrl}?pageNumber={request.PageNumber}&pageSize={request.PageSize}&sortBy={sortBy}&sortDescending={request.SortDescending.ToString().ToLowerInvariant()}");

        if (string.IsNullOrWhiteSpace(request.SearchTerm))
            return path;

        return $"{path}&searchTerm={Uri.EscapeDataString(request.SearchTerm.Trim())}";
    }
}
