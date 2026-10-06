using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Blog.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
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
