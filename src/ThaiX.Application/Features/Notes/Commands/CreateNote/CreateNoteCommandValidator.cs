using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Notes.Commands.CreateNote;

public sealed class CreateNoteCommandValidator : AbstractValidator<CreateNoteCommand>
{
    public CreateNoteCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(200)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(10000)
            .WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Color)
            .IsInEnum()
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);
    }
}
