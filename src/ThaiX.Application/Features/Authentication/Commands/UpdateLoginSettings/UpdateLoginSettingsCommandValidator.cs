using FluentValidation;

namespace ThaiX.Application.Features.Authentication.Commands.UpdateLoginSettings;

public sealed class UpdateLoginSettingsCommandValidator : AbstractValidator<UpdateLoginSettingsCommand>
{
    public UpdateLoginSettingsCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
