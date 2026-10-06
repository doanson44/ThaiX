using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetBudgetList;

/// <summary>
/// Validator for GetBudgetListQuery.
/// </summary>
public sealed class GetBudgetListQueryValidator : AbstractValidator<GetBudgetListQuery>
{
    public GetBudgetListQueryValidator()
    {
        Include(new Common.Validators.PagedRequestValidator());

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithErrorCode(ErrorCodes.INVALID_SEARCH_TERM);
    }
}
