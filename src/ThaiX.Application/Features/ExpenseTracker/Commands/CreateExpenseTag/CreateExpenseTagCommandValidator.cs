using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateExpenseTag;

/// <summary>
/// Validator for CreateExpenseTagCommand.
/// </summary>
public sealed class CreateExpenseTagCommandValidator : AbstractValidator<CreateExpenseTagCommand>
{
    public CreateExpenseTagCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(64).WithErrorCode(ErrorCodes.INVALID_LENGTH);
        RuleFor(x => x.ColorHex)
            .MaximumLength(16)
            .When(x => x.ColorHex is not null)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
