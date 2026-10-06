using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Notes.Commands.ToggleArchiveNote;

public sealed class ToggleArchiveNoteCommandValidator : AbstractValidator<ToggleArchiveNoteCommand>
{
    public ToggleArchiveNoteCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
