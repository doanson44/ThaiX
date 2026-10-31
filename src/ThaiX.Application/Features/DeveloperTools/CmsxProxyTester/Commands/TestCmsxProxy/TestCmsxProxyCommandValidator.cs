using FluentValidation;

namespace ThaiX.Application.Features.DeveloperTools.CmsxProxyTester.Commands.TestCmsxProxy;

public sealed class TestCmsxProxyCommandValidator : AbstractValidator<TestCmsxProxyCommand>
{
    private static readonly string[] AllowedMethods =
        ["GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"];

    public TestCmsxProxyCommandValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .MaximumLength(4096);

        RuleFor(x => x.Method)
            .Must(method => !string.IsNullOrWhiteSpace(method)
                && AllowedMethods.Contains(method.Trim().ToUpperInvariant()))
            .WithMessage("Unsupported HTTP method.");

        RuleFor(x => x.TimeoutSeconds)
            .InclusiveBetween(1, 60);

        RuleFor(x => x.Body)
            .Must(body => body is null || body.Length <= 65536)
            .WithMessage("Request body must not exceed 64 KB.");

        RuleFor(x => x.Headers)
            .Must(headers => headers is null || headers.Count <= 50)
            .WithMessage("No more than 50 request headers are allowed.");

        RuleFor(x => x.QueryParameters)
            .Must(parameters => parameters is null || parameters.Count <= 50)
            .WithMessage("No more than 50 query parameters are allowed.");
    }
}
