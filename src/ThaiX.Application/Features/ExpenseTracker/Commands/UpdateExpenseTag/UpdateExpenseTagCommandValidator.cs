using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateExpenseTag;

/// <summary>
/// Validator for UpdateExpenseTagCommand.
/// </summary>
public sealed class UpdateExpenseTagCommandValidator : AbstractValidator<UpdateExpenseTagCommand>
{
    public UpdateExpenseTagCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(64).WithErrorCode(ErrorCodes.INVALID_LENGTH);
        RuleFor(x => x.ColorHex)
            .MaximumLength(16)
            .When(x => x.ColorHex is not null)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
