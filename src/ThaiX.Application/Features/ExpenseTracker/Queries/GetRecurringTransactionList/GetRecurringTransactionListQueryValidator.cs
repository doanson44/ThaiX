using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetRecurringTransactionList;

/// <summary>
/// Validator for GetRecurringTransactionListQuery.
/// </summary>
public sealed class GetRecurringTransactionListQueryValidator : AbstractValidator<GetRecurringTransactionListQuery>
{
    public GetRecurringTransactionListQueryValidator()
    {
        Include(new Common.Validators.PagedRequestValidator());

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithErrorCode(ErrorCodes.INVALID_SEARCH_TERM);
    }
}
