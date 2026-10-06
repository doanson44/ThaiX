using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetSavingGoalList;

/// <summary>
/// Validator for GetSavingGoalListQuery.
/// </summary>
public sealed class GetSavingGoalListQueryValidator : AbstractValidator<GetSavingGoalListQuery>
{
    public GetSavingGoalListQueryValidator()
    {
        Include(new Common.Validators.PagedRequestValidator());

        RuleFor(x => x.Search)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Search))
            .WithErrorCode(ErrorCodes.INVALID_SEARCH_TERM);
    }
}
