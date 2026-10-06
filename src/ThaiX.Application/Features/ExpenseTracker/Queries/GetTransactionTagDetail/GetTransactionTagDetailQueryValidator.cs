using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetTransactionTagDetail;

/// <summary>
/// Validator for GetTransactionTagDetailQuery.
/// </summary>
public sealed class GetTransactionTagDetailQueryValidator : AbstractValidator<GetTransactionTagDetailQuery>
{
    public GetTransactionTagDetailQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
