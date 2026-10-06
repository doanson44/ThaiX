using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .EmailAddress().WithErrorCode(ErrorCodes.INVALID_FORMAT)
            .MaximumLength(256).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Password)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MinimumLength(8).WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .Matches(@"[A-Z]").WithErrorCode(ErrorCodes.INVALID_FORMAT)
            .Matches(@"[a-z]").WithErrorCode(ErrorCodes.INVALID_FORMAT)
            .Matches(@"[0-9]").WithErrorCode(ErrorCodes.INVALID_FORMAT)
            .Matches(@"[@$!%*?&#]").WithErrorCode(ErrorCodes.INVALID_FORMAT);

        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[0-9]{9,15}$")
                .WithErrorCode(ErrorCodes.INVALID_FORMAT);
        });

        When(x => x.Permissions != null && x.Permissions.Any(), () =>
        {
            RuleForEach(x => x.Permissions)
                .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
                .Must(BeValidPermissionFormat)
                .WithErrorCode(ErrorCodes.INVALID_FORMAT);
        });
    }

    private static bool BeValidPermissionFormat(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return false;

        var parts = permission.Split('.');
        return parts.Length == 2 &&
               !string.IsNullOrWhiteSpace(parts[0]) &&
               !string.IsNullOrWhiteSpace(parts[1]);
    }
}
