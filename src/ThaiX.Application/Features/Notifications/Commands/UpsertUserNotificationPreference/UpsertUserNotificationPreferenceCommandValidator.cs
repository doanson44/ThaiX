using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Notifications.Commands.UpsertUserNotificationPreference;

public sealed class UpsertUserNotificationPreferenceCommandValidator
    : AbstractValidator<UpsertUserNotificationPreferenceCommand>
{
    public UpsertUserNotificationPreferenceCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.Kind)
            .IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.Channel)
            .IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.Destination)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(512).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.MinimumSeverity)
            .IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.TimeZoneId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(128).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.BatchingMode)
            .IsInEnum().WithErrorCode(ErrorCodes.INVALID_FORMAT);
    }
}
