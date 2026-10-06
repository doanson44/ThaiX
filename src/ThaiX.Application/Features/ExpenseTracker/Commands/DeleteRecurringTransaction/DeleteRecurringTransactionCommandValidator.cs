using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteRecurringTransaction;

/// <summary>
/// Validator for DeleteRecurringTransactionCommand.
/// </summary>
public sealed class DeleteRecurringTransactionCommandValidator : AbstractValidator<DeleteRecurringTransactionCommand>
{
    public DeleteRecurringTransactionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
