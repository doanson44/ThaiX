using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateBudget;

/// <summary>
/// Validator for UpdateBudgetCommand.
/// </summary>
public sealed class UpdateBudgetCommandValidator : AbstractValidator<UpdateBudgetCommand>
{
    public UpdateBudgetCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
        RuleFor(x => x.CategoryId).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
        RuleFor(x => x.Period).IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT);
        RuleFor(x => x.Amount).GreaterThan(0).WithErrorCode(ErrorCodes.INVALID_RANGE);
        RuleFor(x => x)
            .Must(x => x.EndDate >= x.StartDate)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);
    }
}
