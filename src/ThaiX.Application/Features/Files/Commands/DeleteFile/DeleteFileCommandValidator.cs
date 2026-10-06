using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Files.Commands.DeleteFile;

public sealed class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
