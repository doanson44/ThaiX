using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateExpenseCategory;

/// <summary>
/// Validator for CreateExpenseCategoryCommand.
/// </summary>
public sealed class CreateExpenseCategoryCommandValidator : AbstractValidator<CreateExpenseCategoryCommand>
{
    public CreateExpenseCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(128).WithErrorCode(ErrorCodes.INVALID_LENGTH);
        RuleFor(x => x.CategoryType).IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT);
    }
}
