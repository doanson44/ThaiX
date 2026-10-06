using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteExpenseTransaction;

/// <summary>
/// Validator for DeleteExpenseTransactionCommand.
/// </summary>
public sealed class DeleteExpenseTransactionCommandValidator : AbstractValidator<DeleteExpenseTransactionCommand>
{
    public DeleteExpenseTransactionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
