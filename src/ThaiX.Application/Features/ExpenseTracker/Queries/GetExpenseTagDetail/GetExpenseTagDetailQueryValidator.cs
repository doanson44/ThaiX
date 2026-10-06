using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTagDetail;

/// <summary>
/// Validator for GetExpenseTagDetailQuery.
/// </summary>
public sealed class GetExpenseTagDetailQueryValidator : AbstractValidator<GetExpenseTagDetailQuery>
{
    public GetExpenseTagDetailQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
