using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.Test.UnitTests;
public sealed class ValidatorProviderUnitTest
{
    private readonly IServiceProvider _serviceProvider;

    public ValidatorProviderUnitTest()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IValidator<TestClass>, TestValidator>();
        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void GetValidator_ShouldReturnValidator_WhenValidatorIsRegistered()
    {
        // Arrange
        var provider = new ValidatorProvider(_serviceProvider);

        // Act
        var validator = provider.TryGetValidator<TestClass>();

        // Assert
        validator.Should().NotBeNull();
        validator.Should().BeOfType<TestValidator>();
    }

    [Fact]
    public void GetValidator_ShouldReturnNull_WhenValidatorIsNotRegistered()
    {
        // Arrange
        var provider = new ValidatorProvider(_serviceProvider);

        // Act
        var validator = provider.TryGetValidator<UnregisteredClass>();

        // Assert
        validator.Should().BeNull();
    }

    private class TestClass : IValidationEnabled { }
    private class UnregisteredClass : IValidationEnabled { }
    private class TestValidator : Validator<TestClass> { }
}
