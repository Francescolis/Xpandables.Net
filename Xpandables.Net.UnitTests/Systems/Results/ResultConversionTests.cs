using System.Net;
using System.Results;
using Xunit;

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
        Assert.Equal(42, result.Value);
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
    public void ImplicitConversion_FromGenericToObjectResult_PreservesValue()
    {
        // Arrange
        Result<int> typedResult = Result.Success(10);

        // Act
        Result<object> result = typedResult;

        // Assert
        Assert.Equal(10, result.Value);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
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
