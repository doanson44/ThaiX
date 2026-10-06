using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Users.Commands.SetUserLockout;

public sealed class SetUserLockoutCommandValidator : AbstractValidator<SetUserLockoutCommand>
{
    public SetUserLockoutCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        When(x => x.IsLocked, () =>
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
                .MaximumLength(500).WithErrorCode(ErrorCodes.INVALID_LENGTH);

            RuleFor(x => x.LockoutDurationMinutes)
                .GreaterThan(0)
                .When(x => x.LockoutDurationMinutes.HasValue)
                .WithErrorCode(ErrorCodes.INVALID_RANGE);
        });
    }
}
