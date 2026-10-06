using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactAddress;

public sealed class AddContactAddressCommandValidator : AbstractValidator<AddContactAddressCommand>
{
    public AddContactAddressCommandValidator()
    {
        RuleFor(x => x.ContactId)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);

        RuleFor(x => x.Street)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(500).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .MaximumLength(10).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.CityCode)
            .MaximumLength(20).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.DistrictCode)
            .MaximumLength(20).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.PostalCode)
            .MaximumLength(20).WithErrorCode(ErrorCodes.INVALID_LENGTH);
    }
}
