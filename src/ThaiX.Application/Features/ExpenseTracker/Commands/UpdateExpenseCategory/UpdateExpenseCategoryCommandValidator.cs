using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateExpenseCategory;

/// <summary>
/// Validator for UpdateExpenseCategoryCommand.
/// </summary>
public sealed class UpdateExpenseCategoryCommandValidator : AbstractValidator<UpdateExpenseCategoryCommand>
{
    public UpdateExpenseCategoryCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(128).WithErrorCode(ErrorCodes.INVALID_LENGTH);
        RuleFor(x => x.CategoryType).IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT);
        RuleFor(x => x)
            .Must(x => !x.ParentCategoryId.HasValue || x.ParentCategoryId.Value != x.Id)
            .WithErrorCode(ErrorCodes.BUSINESS_RULE_VIOLATION)
            .WithMessage("Category parent cannot self-reference.");
    }
}
