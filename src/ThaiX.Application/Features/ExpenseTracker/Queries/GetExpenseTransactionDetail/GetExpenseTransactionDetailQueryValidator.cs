using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTransactionDetail;

/// <summary>
/// Validator for GetExpenseTransactionDetailQuery.
/// </summary>
public sealed class GetExpenseTransactionDetailQueryValidator : AbstractValidator<GetExpenseTransactionDetailQuery>
{
    public GetExpenseTransactionDetailQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
