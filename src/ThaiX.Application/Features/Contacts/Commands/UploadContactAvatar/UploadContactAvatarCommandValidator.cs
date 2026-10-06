using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Contacts.Commands.UploadContactAvatar;

public sealed class UploadContactAvatarCommandValidator : AbstractValidator<UploadContactAvatarCommand>
{
    public UploadContactAvatarCommandValidator()
    {
        RuleFor(x => x.ContactId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.FileStream)
            .NotNull().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.FileName)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.ContentType)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
