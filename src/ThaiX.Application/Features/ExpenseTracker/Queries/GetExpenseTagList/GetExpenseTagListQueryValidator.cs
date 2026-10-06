using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTagList;

/// <summary>
/// Validator for GetExpenseTagListQuery.
/// </summary>
public sealed class GetExpenseTagListQueryValidator : AbstractValidator<GetExpenseTagListQuery>
{
    public GetExpenseTagListQueryValidator()
    {
        Include(new Common.Validators.PagedRequestValidator());

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithErrorCode(ErrorCodes.INVALID_SEARCH_TERM);
    }
}
