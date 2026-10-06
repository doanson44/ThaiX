using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Users.Commands.SetUserPermissions;

public sealed class SetUserPermissionsCommandValidator : AbstractValidator<SetUserPermissionsCommand>
{
    public SetUserPermissionsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.Permissions)
            .NotNull().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleForEach(x => x.Permissions)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .Must(BeValidPermissionFormat)
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);
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
