using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.ExpenseTracker;

namespace ThaiX.Client.Services.ExpenseTracker;

public interface IExpenseTrackerService
{
    Task<PagedApiResponse<ExpenseWalletDto>> GetWalletsAsync(ExpenseListRequest request, CancellationToken cancellationToken = default);
    Task<PagedApiResponse<ExpenseCategoryDto>> GetCategoriesAsync(ExpenseListRequest request, CancellationToken cancellationToken = default);
    Task<PagedApiResponse<ExpenseTransactionDto>> GetTransactionsAsync(ExpenseListRequest request, CancellationToken cancellationToken = default);
    Task<PagedApiResponse<ExpenseTransferDto>> GetTransfersAsync(ExpenseListRequest request, CancellationToken cancellationToken = default);
    Task<PagedApiResponse<ExpenseBudgetDto>> GetBudgetsAsync(ExpenseListRequest request, CancellationToken cancellationToken = default);
    Task<PagedApiResponse<ExpenseSavingGoalDto>> GetSavingGoalsAsync(ExpenseListRequest request, CancellationToken cancellationToken = default);
    Task<PagedApiResponse<ExpenseTagDto>> GetTagsAsync(ExpenseListRequest request, CancellationToken cancellationToken = default);
    Task<PagedApiResponse<ExpenseTransactionTagDto>> GetTransactionTagsAsync(ExpenseListRequest request, CancellationToken cancellationToken = default);
    Task<PagedApiResponse<ExpenseRecurringTransactionDto>> GetRecurringTransactionsAsync(ExpenseListRequest request, CancellationToken cancellationToken = default);
    Task<ExpenseDashboardDataDto> GetDashboardAsync(CancellationToken cancellationToken = default);

    Task<Guid> CreateWalletAsync(ExpenseWalletWriteRequest request, CancellationToken cancellationToken = default);
    Task UpdateWalletAsync(Guid id, ExpenseWalletWriteRequest request, CancellationToken cancellationToken = default);
    Task DeleteWalletAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Guid> CreateCategoryAsync(ExpenseCategoryWriteRequest request, CancellationToken cancellationToken = default);
    Task UpdateCategoryAsync(Guid id, ExpenseCategoryWriteRequest request, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Guid> CreateTransactionAsync(ExpenseTransactionWriteRequest request, CancellationToken cancellationToken = default);
    Task UpdateTransactionAsync(Guid id, ExpenseTransactionWriteRequest request, CancellationToken cancellationToken = default);
    Task DeleteTransactionAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Guid> CreateTransferAsync(ExpenseTransferWriteRequest request, CancellationToken cancellationToken = default);
    Task UpdateTransferAsync(Guid id, ExpenseTransferWriteRequest request, CancellationToken cancellationToken = default);
    Task DeleteTransferAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Guid> CreateBudgetAsync(ExpenseBudgetWriteRequest request, CancellationToken cancellationToken = default);
    Task UpdateBudgetAsync(Guid id, ExpenseBudgetWriteRequest request, CancellationToken cancellationToken = default);
    Task DeleteBudgetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Guid> CreateSavingGoalAsync(ExpenseSavingGoalWriteRequest request, CancellationToken cancellationToken = default);
    Task UpdateSavingGoalAsync(Guid id, ExpenseSavingGoalWriteRequest request, CancellationToken cancellationToken = default);
    Task DeleteSavingGoalAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Guid> CreateTagAsync(ExpenseTagWriteRequest request, CancellationToken cancellationToken = default);
    Task UpdateTagAsync(Guid id, ExpenseTagWriteRequest request, CancellationToken cancellationToken = default);
    Task DeleteTagAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Guid> CreateTransactionTagAsync(ExpenseTransactionTagWriteRequest request, CancellationToken cancellationToken = default);
    Task UpdateTransactionTagAsync(Guid id, ExpenseTransactionTagWriteRequest request, CancellationToken cancellationToken = default);
    Task DeleteTransactionTagAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Guid> CreateRecurringTransactionAsync(ExpenseRecurringWriteRequest request, CancellationToken cancellationToken = default);
    Task UpdateRecurringTransactionAsync(Guid id, ExpenseRecurringWriteRequest request, CancellationToken cancellationToken = default);
    Task DeleteRecurringTransactionAsync(Guid id, CancellationToken cancellationToken = default);
}
