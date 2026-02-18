using System.Net;
using System.Results;

using FluentAssertions;

namespace Xpandables.Net.UnitTests.Systems.Results;

public sealed class ResultConversionTests
{
	[Fact]
	public void ImplicitConversion_FromGenericToNonGeneric_PreservesStatusAndValue()
	{
		// Arrange
		Result<int> typedResult = Result.Success(42);

		// Act
		Result result = typedResult;

		// Assert
		Assert.Equal(HttpStatusCode.OK, result.StatusCode);
		result.IsGeneric.Should().BeFalse();
	}

	[Fact]
	public void ImplicitConversion_FromNonGenericToGeneric_CastsValueWhenCompatible()
	{
		// Arrange
		Result source = Result.Success()
			.WithStatusCode(HttpStatusCode.Created)
			.WithValue("payload")
			.Build();

		// Act
		Result<string> typed = source;

		// Assert
		Assert.Equal(HttpStatusCode.Created, typed.StatusCode);
		Assert.Equal("payload", typed.Value);
	}

	[Fact]
	public void ImplicitConversion_FromNonGenericToGeneric_IncompatibleValueUsesDefault()
	{
		// Arrange
		Result source = Result.Success()
			.WithStatusCode(HttpStatusCode.Accepted)
			.WithValue(123)
			.Build();

		// Act
		Result<string> typed = source;

		// Assert
		Assert.Equal(HttpStatusCode.Accepted, typed.StatusCode);
		Assert.Null(typed.Value);
	}

	[Fact]
	public void ImplicitConversion_FromGeneric_NullSource_Throws()
	{
		// Arrange
		Result<int>? source = null;

		// Act
		void Act() => _ = (Result)source!;

		// Assert
		Assert.Throws<ArgumentNullException>(Act);
	}
}
