using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateExpenseTransaction;

/// <summary>
/// Validator for UpdateExpenseTransactionCommand.
/// </summary>
public sealed class UpdateExpenseTransactionCommandValidator : AbstractValidator<UpdateExpenseTransactionCommand>
{
    public UpdateExpenseTransactionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
        RuleFor(x => x.WalletId).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
        RuleFor(x => x.TransactionType).IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT);
        RuleFor(x => x.Amount).GreaterThan(0).WithErrorCode(ErrorCodes.INVALID_RANGE);
        RuleFor(x => x.Note)
            .MaximumLength(500)
            .When(x => x.Note is not null)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
