using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Contacts.Commands.UpdateContactProfile;

/// <summary>
/// Validator for UpdateContactProfileCommand.
/// </summary>
public sealed class UpdateContactProfileCommandValidator : AbstractValidator<UpdateContactProfileCommand>
{
    public UpdateContactProfileCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.FirstName)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(100).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.LastName)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(100).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.Company)
            .MaximumLength(256).WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .When(x => x.Company is not null);

        RuleFor(x => x.JobTitle)
            .MaximumLength(256).WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .When(x => x.JobTitle is not null);

        RuleFor(x => x.AvatarUrl)
            .MaximumLength(2048).WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .When(x => x.AvatarUrl is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(4000).WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .When(x => x.Notes is not null);
    }
}
