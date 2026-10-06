using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateTransactionTag;

/// <summary>
/// Validator for UpdateTransactionTagCommand.
/// </summary>
public sealed class UpdateTransactionTagCommandValidator : AbstractValidator<UpdateTransactionTagCommand>
{
    public UpdateTransactionTagCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
        RuleFor(x => x.TransactionId).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
        RuleFor(x => x.TagId).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
