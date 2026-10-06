using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseCategoryList;

/// <summary>
/// Validator for GetExpenseCategoryListQuery.
/// </summary>
public sealed class GetExpenseCategoryListQueryValidator : AbstractValidator<GetExpenseCategoryListQuery>
{
    public GetExpenseCategoryListQueryValidator()
    {
        Include(new Common.Validators.PagedRequestValidator());

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithErrorCode(ErrorCodes.INVALID_SEARCH_TERM);
    }
}
