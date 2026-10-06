using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateBudget;

/// <summary>
/// Validator for CreateBudgetCommand.
/// </summary>
public sealed class CreateBudgetCommandValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetCommandValidator()
    {
        RuleFor(x => x.CategoryId).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
        RuleFor(x => x.Period).IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT);
        RuleFor(x => x.Amount).GreaterThan(0).WithErrorCode(ErrorCodes.INVALID_RANGE);
        RuleFor(x => x)
            .Must(x => x.EndDate >= x.StartDate)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);
    }
}
