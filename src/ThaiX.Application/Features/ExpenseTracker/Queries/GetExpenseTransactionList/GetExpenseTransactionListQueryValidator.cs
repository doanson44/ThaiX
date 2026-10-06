using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTransactionList;

/// <summary>
/// Validator for GetExpenseTransactionListQuery.
/// </summary>
public sealed class GetExpenseTransactionListQueryValidator : AbstractValidator<GetExpenseTransactionListQuery>
{
    public GetExpenseTransactionListQueryValidator()
    {
        Include(new Common.Validators.PagedRequestValidator());

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithErrorCode(ErrorCodes.INVALID_SEARCH_TERM);
    }
}
