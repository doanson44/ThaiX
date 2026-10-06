using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Notes.Commands.TogglePinNote;

public sealed class TogglePinNoteCommandValidator : AbstractValidator<TogglePinNoteCommand>
{
    public TogglePinNoteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
