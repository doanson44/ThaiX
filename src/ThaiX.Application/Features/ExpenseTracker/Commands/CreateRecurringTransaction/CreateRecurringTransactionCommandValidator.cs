using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateRecurringTransaction;

/// <summary>
/// Validator for CreateRecurringTransactionCommand.
/// </summary>
public sealed class CreateRecurringTransactionCommandValidator : AbstractValidator<CreateRecurringTransactionCommand>
{
    public CreateRecurringTransactionCommandValidator()
    {
        RuleFor(x => x.WalletId).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
        RuleFor(x => x.TransactionType).IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT);
        RuleFor(x => x.Frequency).IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT);
        RuleFor(x => x.Amount).GreaterThan(0).WithErrorCode(ErrorCodes.INVALID_RANGE);
        RuleFor(x => x)
            .Must(x => !x.EndDate.HasValue || x.EndDate.Value > x.NextRun)
            .WithErrorCode(ErrorCodes.BUSINESS_RULE_VIOLATION);
        RuleFor(x => x.Note)
            .MaximumLength(500)
            .When(x => x.Note is not null)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
