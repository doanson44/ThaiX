using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateTransactionTag;

/// <summary>
/// Validator for CreateTransactionTagCommand.
/// </summary>
public sealed class CreateTransactionTagCommandValidator : AbstractValidator<CreateTransactionTagCommand>
{
    public CreateTransactionTagCommandValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
        RuleFor(x => x.TagId).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
