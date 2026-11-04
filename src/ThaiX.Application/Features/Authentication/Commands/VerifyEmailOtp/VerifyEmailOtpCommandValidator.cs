using FluentValidation;

namespace ThaiX.Application.Features.Authentication.Commands.VerifyEmailOtp;

public sealed class VerifyEmailOtpCommandValidator : AbstractValidator<VerifyEmailOtpCommand>
{
    public VerifyEmailOtpCommandValidator()
    {
        RuleFor(x => x.ChallengeId).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(4, 9).Matches("^[0-9]+$");
    }
}
