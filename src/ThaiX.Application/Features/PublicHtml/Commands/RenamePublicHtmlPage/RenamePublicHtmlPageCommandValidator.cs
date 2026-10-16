using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.PublicHtml.Commands.RenamePublicHtmlPage;

public sealed class RenamePublicHtmlPageCommandValidator : AbstractValidator<RenamePublicHtmlPageCommand>
{
    public RenamePublicHtmlPageCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD);
        RuleFor(x => x.Slug)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .Length(1, 80).WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .Matches("^[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*$").WithErrorCode(ErrorCodes.INVALID_FORMAT);
    }
}
