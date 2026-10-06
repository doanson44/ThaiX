using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Users.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        When(x => !string.IsNullOrWhiteSpace(x.NewPassword), () =>
        {
            RuleFor(x => x.NewPassword)
                .MinimumLength(8).WithErrorCode(ErrorCodes.INVALID_LENGTH)
                .Matches(@"[A-Z]").WithErrorCode(ErrorCodes.INVALID_FORMAT)
                .Matches(@"[a-z]").WithErrorCode(ErrorCodes.INVALID_FORMAT)
                .Matches(@"[0-9]").WithErrorCode(ErrorCodes.INVALID_FORMAT)
                .Matches(@"[@$!%*?&#]").WithErrorCode(ErrorCodes.INVALID_FORMAT);
        });
    }
}
