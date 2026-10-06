using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteTransactionTag;

/// <summary>
/// Validator for DeleteTransactionTagCommand.
/// </summary>
public sealed class DeleteTransactionTagCommandValidator : AbstractValidator<DeleteTransactionTagCommand>
{
    public DeleteTransactionTagCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
