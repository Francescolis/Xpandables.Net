using FluentAssertions;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions;

namespace Xpandables.Net.Test.UnitTests;
public sealed class CompositeValidatorUnitTest
{
    [Fact]
    public void Validate_ShouldReturnSuccess_WhenAllValidatorsPass()
    {
        // Arrange
        var validators = new List<IValidator<TestClass>>
            {
                new TestValidator(true),
                new TestValidator(true)
            };
        var compositeValidator = new CompositeValidator<TestClass>(validators);

        // Act
        var result = compositeValidator.Validate(new TestClass());

        // Assert
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldReturnFailure_WhenAnyValidatorFails()
    {
        // Arrange
        var validators = new List<IValidator<TestClass>>
            {
                new TestValidator(true),
                new TestValidator(false)
            };
        var compositeValidator = new CompositeValidator<TestClass>(validators);

        // Act
        var result = compositeValidator.Validate(new TestClass());

        // Assert
        result.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnSuccess_WhenAllValidatorsPass()
    {
        // Arrange
        var validators = new List<IValidator<TestClass>>
            {
                new TestValidator(true),
                new TestValidator(true)
            };
        var compositeValidator = new CompositeValidator<TestClass>(validators);

        // Act
        var result = await compositeValidator.ValidateAsync(new TestClass());

        // Assert
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_ShouldReturnFailure_WhenAnyValidatorFails()
    {
        // Arrange
        var validators = new List<IValidator<TestClass>>
            {
                new TestValidator(true),
                new TestValidator(false)
            };
        var compositeValidator = new CompositeValidator<TestClass>(validators);

        // Act
        var result = await compositeValidator.ValidateAsync(new TestClass());

        // Assert
        result.IsSuccessStatusCode.Should().BeFalse();
    }

    private class TestClass : IRequiresValidation { }
    private class TestValidator(bool shouldPass) : Validator<TestClass>
    {
        private readonly bool _shouldPass = shouldPass;

        public override ExecutionResult Validate(TestClass instance) =>
            _shouldPass ? ExecutionResult.Success() : ExecutionResult.Failure("key", "message");
    }
}
