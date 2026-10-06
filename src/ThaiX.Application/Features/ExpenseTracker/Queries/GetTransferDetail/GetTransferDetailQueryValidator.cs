using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetTransferDetail;

/// <summary>
/// Validator for GetTransferDetailQuery.
/// </summary>
public sealed class GetTransferDetailQueryValidator : AbstractValidator<GetTransferDetailQuery>
{
    public GetTransferDetailQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
