using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateSavingGoal;

/// <summary>
/// Validator for UpdateSavingGoalCommand.
/// </summary>
public sealed class UpdateSavingGoalCommandValidator : AbstractValidator<UpdateSavingGoalCommand>
{
    public UpdateSavingGoalCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
        RuleFor(x => x.WalletId).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(128).WithErrorCode(ErrorCodes.INVALID_LENGTH);
        RuleFor(x => x.TargetAmount).GreaterThan(0).WithErrorCode(ErrorCodes.INVALID_RANGE);
        RuleFor(x => x.CurrentAmount).GreaterThanOrEqualTo(0).WithErrorCode(ErrorCodes.INVALID_RANGE);
        RuleFor(x => x)
            .Must(x => x.CurrentAmount <= x.TargetAmount)
            .WithErrorCode(ErrorCodes.BUSINESS_RULE_VIOLATION);
    }
}
