using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Notifications.Commands.SendNotification;

public sealed class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
{
    public SendNotificationCommandValidator()
    {
        RuleFor(x => x.EventType)
            .IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.Text)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(20000).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Title)
            .MaximumLength(256).WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .When(x => x.Title is not null);

        RuleFor(x => x.Severity)
            .IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT)
            .When(x => x.Severity is not null);

        RuleFor(x => x.Target)
            .IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT);
    }
}
