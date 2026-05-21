using System.ComponentModel.DataAnnotations;
using System.Results;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.UnitTests.Systems.Validation;

public sealed class ValidatorFactoryTests
{
	[Fact]
	public void CreateValidator_UsesResolverForExactType()
	{
		// Arrange
		var validator = new TestValidator();
		var resolver = new ResolverStub(typeof(TestValidatable), validator);
		var factory = new ValidatorFactory(new ServiceCollection().BuildServiceProvider(), [resolver]);

		// Act
		IValidator? result = factory.CreateValidator(typeof(TestValidatable));

		// Assert
		Assert.Same(validator, result);
	}

	[Fact]
	public void CreateValidator_GenericUsesServiceProviderRegistration()
	{
		// Arrange
		var services = new ServiceCollection();
		services.AddSingleton<IValidator<TestValidatable>, TestValidator>();
		ServiceProvider provider = services.BuildServiceProvider();
		var factory = (IValidatorFactory)new ValidatorFactory(provider, []);

		// Act
		IValidator<TestValidatable>? result = factory.CreateValidator<TestValidatable>();

		// Assert
		Assert.IsType<TestValidator>(result);
	}

	[Fact]
	public void CreateValidator_WithNullType_Throws()
	{
		// Arrange
		var factory = new ValidatorFactory(new ServiceCollection().BuildServiceProvider(), []);

		// Act
		Func<IValidator> action = () => factory.CreateValidator(null!)!;

		// Assert
		Assert.Throws<ArgumentNullException>(action);
	}

	[Fact]
	public void CreateValidator_NoMatchingResolver_ReturnsNull()
	{
		// Arrange
		var factory = new ValidatorFactory(new ServiceCollection().BuildServiceProvider(), []);

		// Act
		IValidator? result = factory.CreateValidator(typeof(TestValidatable));

		// Assert
		Assert.Null(result);
	}

	private sealed class TestValidatable : IRequiresValidation;

	private sealed class TestValidator : IValidator<TestValidatable>
	{
		public int Order { get; } = 0;
		public Result Validate(TestValidatable instance) => ResultWith.Success();

		public ValueTask<Result> ValidateAsync(TestValidatable instance) =>
			ValueTask.FromResult<Result>(ResultWith.Success());

		ValueTask<Result> IValidator<TestValidatable>.ValidateAsync(TestValidatable instance) =>
			ValidateAsync(instance);

		Result IValidator.Validate(object instance) => Validate((TestValidatable)instance);
	}

	private sealed class ResolverStub(Type targetType, IValidator instance) : IValidatorResolver
	{
		public Type TargetType { get; } = targetType;

		public IValidator? Resolve(IServiceProvider serviceProvider) => instance;
	}
}
