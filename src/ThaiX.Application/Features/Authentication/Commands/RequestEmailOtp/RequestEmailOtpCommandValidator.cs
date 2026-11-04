using FluentValidation;

namespace ThaiX.Application.Features.Authentication.Commands.RequestEmailOtp;

public sealed class RequestEmailOtpCommandValidator : AbstractValidator<RequestEmailOtpCommand>
{
    public RequestEmailOtpCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}
