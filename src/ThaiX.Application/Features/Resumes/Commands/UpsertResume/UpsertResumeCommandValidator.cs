using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Resumes.Commands.UpsertResume;

public sealed class UpsertResumeCommandValidator : AbstractValidator<UpsertResumeCommand>
{
    public UpsertResumeCommandValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .Matches("^[a-zA-Z0-9-]+$")
            .WithErrorCode(ErrorCodes.INVALID_FORMAT)
            .WithMessage("Slug may only contain letters, numbers, and hyphens.")
            .MaximumLength(100)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(200)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Headline)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(200)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.MetaDescription)
            .MaximumLength(300)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
