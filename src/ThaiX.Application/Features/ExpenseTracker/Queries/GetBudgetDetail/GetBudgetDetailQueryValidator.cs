using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetBudgetDetail;

/// <summary>
/// Validator for GetBudgetDetailQuery.
/// </summary>
public sealed class GetBudgetDetailQueryValidator : AbstractValidator<GetBudgetDetailQuery>
{
    public GetBudgetDetailQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
