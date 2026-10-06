using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Files.Commands.UploadFile;

public sealed class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator()
    {
        RuleFor(x => x.FileStream)
            .NotNull().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.FileName)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(512).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.ContentType)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(128).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.StorageKey)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(1024).WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
