using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Blog.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(100).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Slug)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(100).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Description)
            .MaximumLength(300).When(x => x.Description is not null)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
