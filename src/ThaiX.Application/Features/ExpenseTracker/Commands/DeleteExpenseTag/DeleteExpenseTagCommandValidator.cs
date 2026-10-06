using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteExpenseTag;

/// <summary>
/// Validator for DeleteExpenseTagCommand.
/// </summary>
public sealed class DeleteExpenseTagCommandValidator : AbstractValidator<DeleteExpenseTagCommand>
{
    public DeleteExpenseTagCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
