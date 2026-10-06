using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Blog.Posts.Commands.UpdatePost;

public sealed class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
{
    public UpdatePostCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(200).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Slug)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(200).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Summary)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(500).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.ContentHtml)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.MetaTitle)
            .MaximumLength(70).When(x => x.MetaTitle is not null)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.MetaDescription)
            .MaximumLength(160).When(x => x.MetaDescription is not null)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
