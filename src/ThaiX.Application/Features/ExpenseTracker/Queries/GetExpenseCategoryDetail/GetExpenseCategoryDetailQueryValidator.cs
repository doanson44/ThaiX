using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseCategoryDetail;

/// <summary>
/// Validator for GetExpenseCategoryDetailQuery.
/// </summary>
public sealed class GetExpenseCategoryDetailQueryValidator : AbstractValidator<GetExpenseCategoryDetailQuery>
{
    public GetExpenseCategoryDetailQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
