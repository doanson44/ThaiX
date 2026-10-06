using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Notes.Commands.DeleteNote;

public sealed class DeleteNoteCommandValidator : AbstractValidator<DeleteNoteCommand>
{
    public DeleteNoteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
