using System.Net;
using System.Results;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Systems.Results;

public sealed class ResultConversionTests
{
	[Fact]
	public void NaturalUpcast_FromGenericToBase_PreservesRuntimeType()
	{
		// Arrange
		SuccessResult<int> typedResult = ResultWith.Success(42);

		// Act — natural reference upcast via inheritance
		Result result = typedResult;

		// Assert
		SuccessResult<int> success = Assert.IsType<SuccessResult<int>>(result);
		Assert.Equal(HttpStatusCode.OK, success.StatusCode);
		success.Value.Should().Be(42);
		Assert.Same(typedResult, result);
	}

	[Fact]
	public void Cast_FromNonGenericSuccessToGeneric_PreservesCompatibleValue()
	{
		// Arrange
		SuccessResult source = ResultWith.Success("payload");

		// Act
		SuccessResult<string> typed = source;

		// Assert
		Assert.Equal(HttpStatusCode.OK, typed.StatusCode);
		Assert.Equal("payload", typed.Value);
	}

	[Fact]
	public void Cast_FromNonGenericSuccessToGeneric_IncompatibleValueUsesDefault()
	{
		// Arrange
		SuccessResult source = ResultWith.Success(123);

		// Act
		SuccessResult<string> typed = source;

		// Assert
		Assert.Equal(HttpStatusCode.OK, typed.StatusCode);
		Assert.Null(typed.Value);
	}

	[Fact]
	public void Cast_FromGenericToNonGeneric_PreservesMetadataAndValue()
	{
		// Arrange
		SuccessResult<int> original = ResultWith.Success(42);

		// Act
		SuccessResult converted = original;

		// Assert
		Assert.Equal(HttpStatusCode.OK, converted.StatusCode);
		converted.GetValue().Should().Be(42);
	}
}
