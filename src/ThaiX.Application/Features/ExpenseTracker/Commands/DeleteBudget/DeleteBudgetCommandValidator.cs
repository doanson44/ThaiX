using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteBudget;

/// <summary>
/// Validator for DeleteBudgetCommand.
/// </summary>
public sealed class DeleteBudgetCommandValidator : AbstractValidator<DeleteBudgetCommand>
{
    public DeleteBudgetCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
