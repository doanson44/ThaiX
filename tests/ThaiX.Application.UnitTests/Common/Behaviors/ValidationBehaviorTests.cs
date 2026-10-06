using FluentValidation;
using FluentValidation.Results;
using MediatR;
using ThaiX.Application.Common.Behaviors;

namespace ThaiX.Application.UnitTests.Common.Behaviors;

public sealed class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, TestResponse>(validators);
        var request = new TestRequest { Value = "test" };
        var expectedResponse = new TestResponse { Result = "success" };

        // Act
        var result = await behavior.Handle(
            request,
            () => Task.FromResult(expectedResponse),
            CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handle_WithPassingValidator_ShouldCallNext()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, TestResponse>(new[] { validatorMock.Object });
        var request = new TestRequest { Value = "valid" };
        var expectedResponse = new TestResponse { Result = "success" };

        // Act
        var result = await behavior.Handle(
            request,
            () => Task.FromResult(expectedResponse),
            CancellationToken.None);

        // Assert
        result.Should().Be(expectedResponse);
    }

    [Fact]
    public async Task Handle_WithFailingValidator_ShouldThrowValidationException()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("Value", "Value is required")
            }));

        var behavior = new ValidationBehavior<TestRequest, TestResponse>(new[] { validatorMock.Object });
        var request = new TestRequest { Value = "" };

        // Act
        var act = () => behavior.Handle(
            request,
            () => Task.FromResult(new TestResponse { Result = "should-not-reach" }),
            CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().ContainSingle(e => e.PropertyName == "Value");
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldAggregateErrors()
    {
        // Arrange
        var validator1 = new Mock<IValidator<TestRequest>>();
        validator1
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("Field1", "Field1 error")
            }));

        var validator2 = new Mock<IValidator<TestRequest>>();
        validator2
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[]
            {
                new ValidationFailure("Field2", "Field2 error")
            }));

        var behavior = new ValidationBehavior<TestRequest, TestResponse>(
            new[] { validator1.Object, validator2.Object });
        var request = new TestRequest { Value = "" };

        // Act
        var act = () => behavior.Handle(
            request,
            () => Task.FromResult(new TestResponse()),
            CancellationToken.None);

        // Assert
        var exception = await act.Should().ThrowAsync<ValidationException>();
        exception.Which.Errors.Should().HaveCount(2);
    }

    // Test request/response types — must be public for Moq's Castle DynamicProxy
    public sealed record TestRequest : IRequest<TestResponse>
    {
        public string Value { get; init; } = string.Empty;
    }

    public sealed record TestResponse
    {
        public string Result { get; init; } = string.Empty;
    }
}
