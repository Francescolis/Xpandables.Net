using System.Net;
using System.Results;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Systems.Results;

public sealed class ResultConversionTests
{
	[Fact]
	public void NaturalUpcast_FromGenericToNonGeneric_PreservesRuntimeType()
	{
		// Arrange
		Result<int> typedResult = Result.Success(42);

		// Act — natural reference upcast via inheritance
		Result result = typedResult;

		// Assert
		Assert.Equal(HttpStatusCode.OK, result.StatusCode);
		result.IsGeneric.Should().BeTrue();
		result.GetUnderlyingValue().Should().Be(42);
		Assert.Same(typedResult, result);
	}

	[Fact]
	public void ToResult_FromNonGenericToGeneric_CastsValueWhenCompatible()
	{
		// Arrange
		Result source = Result.Success()
			.WithStatusCode(HttpStatusCode.Created)
			.WithValue("payload")
			.Build();

		// Act
		Result<string> typed = source.ToResult<string>();

		// Assert
		Assert.Equal(HttpStatusCode.Created, typed.StatusCode);
		Assert.Equal("payload", typed.Value);
	}

	[Fact]
	public void ToResult_FromNonGenericToGeneric_IncompatibleValueUsesDefault()
	{
		// Arrange
		Result source = Result.Success()
			.WithStatusCode(HttpStatusCode.Accepted)
			.WithValue(123)
			.Build();

		// Act
		Result<string> typed = source.ToResult<string>();

		// Assert
		Assert.Equal(HttpStatusCode.Accepted, typed.StatusCode);
		Assert.Null(typed.Value);
	}

	[Fact]
	public void ToResult_WhenAlreadyCorrectType_ReturnsSameInstance()
	{
		// Arrange
		Result<int> original = Result.Success(42);
		Result asBase = original;

		// Act
		Result<int> converted = asBase.ToResult<int>();

		// Assert — zero-copy: same instance returned
		Assert.Same(original, converted);
	}
}
