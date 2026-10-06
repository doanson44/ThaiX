using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetRecurringTransactionDetail;

/// <summary>
/// Validator for GetRecurringTransactionDetailQuery.
/// </summary>
public sealed class GetRecurringTransactionDetailQueryValidator : AbstractValidator<GetRecurringTransactionDetailQuery>
{
    public GetRecurringTransactionDetailQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
