using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteSavingGoal;

/// <summary>
/// Validator for DeleteSavingGoalCommand.
/// </summary>
public sealed class DeleteSavingGoalCommandValidator : AbstractValidator<DeleteSavingGoalCommand>
{
    public DeleteSavingGoalCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
