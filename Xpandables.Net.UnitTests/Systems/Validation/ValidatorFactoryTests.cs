using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Xpandables.Net.UnitTests.Systems.Validation;

public sealed class ValidatorFactoryTests
{
    [Fact]
    public void CreateValidator_UsesResolverForExactType()
    {
        // Arrange
        var validator = new TestValidator();
        var resolver = new ResolverStub(typeof(TestValidatable), validator);
        var factory = new ValidatorFactory(new ServiceCollection().BuildServiceProvider(), new[] { resolver });

        // Act
        var result = factory.CreateValidator(typeof(TestValidatable));

        // Assert
        Assert.Same(validator, result);
    }

    [Fact]
    public void CreateValidator_GenericUsesServiceProviderRegistration()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IValidator<TestValidatable>, TestValidator>();
        var provider = services.BuildServiceProvider();
        var factory = (IValidatorFactory)new ValidatorFactory(provider, Array.Empty<IValidatorResolver>());

        // Act
        var result = factory.CreateValidator<TestValidatable>();

        // Assert
        Assert.IsType<TestValidator>(result);
    }

    [Fact]
    public void CreateValidator_WithNullType_Throws()
    {
        // Arrange
        var factory = new ValidatorFactory(new ServiceCollection().BuildServiceProvider(), Array.Empty<IValidatorResolver>());

        // Act
        var action = () => factory.CreateValidator(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(action);
    }

    [Fact]
    public void CreateValidator_NoMatchingResolver_ReturnsNull()
    {
        // Arrange
        var factory = new ValidatorFactory(new ServiceCollection().BuildServiceProvider(), Array.Empty<IValidatorResolver>());

        // Act
        var result = factory.CreateValidator(typeof(TestValidatable));

        // Assert
        Assert.Null(result);
    }

    private sealed class TestValidatable : IRequiresValidation;

    private sealed class TestValidator : IValidator<TestValidatable>
    {
        public IReadOnlyCollection<ValidationResult> Validate(TestValidatable instance) => Array.Empty<ValidationResult>();

        public ValueTask<IReadOnlyCollection<ValidationResult>> ValidateAsync(TestValidatable instance) =>
            ValueTask.FromResult<IReadOnlyCollection<ValidationResult>>(Array.Empty<ValidationResult>());

        ValueTask<IReadOnlyCollection<ValidationResult>> IValidator<TestValidatable>.ValidateAsync(TestValidatable instance) =>
            ValidateAsync(instance);

        IReadOnlyCollection<ValidationResult> IValidator.Validate(object instance) => Validate((TestValidatable)instance);
    }

    private sealed class ResolverStub(Type targetType, IValidator instance) : IValidatorResolver
    {
        public Type TargetType { get; } = targetType;

        public IValidator? Resolve(IServiceProvider serviceProvider) => instance;
    }
}
