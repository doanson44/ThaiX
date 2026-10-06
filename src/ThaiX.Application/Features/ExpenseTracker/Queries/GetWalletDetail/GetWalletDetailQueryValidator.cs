using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetWalletDetail;

/// <summary>
/// Validator for GetWalletDetailQuery.
/// </summary>
public sealed class GetWalletDetailQueryValidator : AbstractValidator<GetWalletDetailQuery>
{
    public GetWalletDetailQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
