using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetSavingGoalDetail;

/// <summary>
/// Validator for GetSavingGoalDetailQuery.
/// </summary>
public sealed class GetSavingGoalDetailQueryValidator : AbstractValidator<GetSavingGoalDetailQuery>
{
    public GetSavingGoalDetailQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
