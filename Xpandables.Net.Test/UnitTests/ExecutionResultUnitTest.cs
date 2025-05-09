using System.Net;

using FluentAssertions;

using Xpandables.Net.Executions;

namespace Xpandables.Net.Test.UnitTests;
public sealed class ExecutionResultUnitTest
{
    [Fact]
    public void IsSuccessStatusCode_ShouldReturnTrue_ForSuccessStatusCode()
    {
        // Arrange
        var result = new ExecutionResult
        {
            StatusCode = HttpStatusCode.OK
        };

        // Act
        var isSuccess = result.IsSuccessStatusCode;

        // Assert
        isSuccess.Should().BeTrue();
    }

    [Fact]
    public void IsSuccessStatusCode_ShouldReturnFalse_ForFailureStatusCode()
    {
        // Arrange
        var result = new ExecutionResult
        {
            StatusCode = HttpStatusCode.BadRequest
        };

        // Act
        var isSuccess = result.IsSuccessStatusCode;

        // Assert
        isSuccess.Should().BeFalse();
    }

    [Fact]
    public void EnsureSuccessStatusCode_ShouldNotThrow_ForSuccessStatusCode()
    {
        // Arrange
        var result = new ExecutionResult
        {
            StatusCode = HttpStatusCode.OK
        };

        // Act
        Action act = () => result.EnsureSuccessStatusCode();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureSuccessStatusCode_ShouldThrow_ForFailureStatusCode()
    {
        // Arrange
        var result = new ExecutionResult
        {
            StatusCode = HttpStatusCode.BadRequest
        };

        // Act
        Action act = () => result.EnsureSuccessStatusCode();

        // Assert
        act.Should().Throw<ExecutionResultException>();
    }

    [Fact]
    public void ToExecutionResult_ShouldConvertToGenericExecutionResult()
    {
        // Arrange
        var result = new ExecutionResult
        {
            StatusCode = HttpStatusCode.OK,
            Title = "Test Title",
            Detail = "Test Detail"
        };

        // Act
        var genericResult = result.ToExecutionResult();

        // Assert
        genericResult.Should().NotBeNull();
        genericResult.StatusCode.Should().Be(HttpStatusCode.OK);
        genericResult.Title.Should().Be("Test Title");
        genericResult.Detail.Should().Be("Test Detail");
    }
}
