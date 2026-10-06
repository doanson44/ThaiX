using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteExpenseCategory;

/// <summary>
/// Validator for DeleteExpenseCategoryCommand.
/// </summary>
public sealed class DeleteExpenseCategoryCommandValidator : AbstractValidator<DeleteExpenseCategoryCommand>
{
    public DeleteExpenseCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
