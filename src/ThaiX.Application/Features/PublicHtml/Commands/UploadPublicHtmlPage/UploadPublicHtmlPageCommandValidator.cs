using FluentValidation;
using ThaiX.Application.Common.Constants;

namespace ThaiX.Application.Features.PublicHtml.Commands.UploadPublicHtmlPage;

public sealed class UploadPublicHtmlPageCommandValidator : AbstractValidator<UploadPublicHtmlPageCommand>
{
    public UploadPublicHtmlPageCommandValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithErrorCode(ErrorCodes.REQUIRED_FIELD)
            .Length(1, 80).WithErrorCode(ErrorCodes.INVALID_LENGTH)
            .Matches("^[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*$").WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.Title)
            .MaximumLength(200).WithErrorCode(ErrorCodes.INVALID_LENGTH);

        RuleFor(x => x.FileName)
            .Must(x => x.EndsWith(".html", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".htm", StringComparison.OrdinalIgnoreCase))
            .WithErrorCode(ErrorCodes.INVALID_FORMAT);

        RuleFor(x => x.SizeBytes)
            .InclusiveBetween(1, 5 * 1024 * 1024)
            .WithErrorCode(ErrorCodes.INVALID_RANGE);
    }
}
