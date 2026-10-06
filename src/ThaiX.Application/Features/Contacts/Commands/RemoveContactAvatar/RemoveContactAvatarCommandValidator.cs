using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactAvatar;

public sealed class RemoveContactAvatarCommandValidator : AbstractValidator<RemoveContactAvatarCommand>
{
    public RemoveContactAvatarCommandValidator()
    {
        RuleFor(x => x.ContactId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
    }
}
