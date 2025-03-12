using System.Net;
using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Collections;
using Xpandables.Net.Executions;

namespace Xpandables.Net.Test.UnitTests;

public sealed class OperationResultUnitTest
{
    [Fact]
    public void Success_ShouldReturnOperationResultWithStatusCodeOk()
    {
        // Act
        var result = ExecutionResults.Success();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public void Success_WithResult_ShouldReturnOperationResultWithStatusCodeOkAndResult()
    {
        // Arrange
        var expectedResult = "Success";

        // Act
        var result = ExecutionResults.Success(expectedResult);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccessStatusCode.Should().BeTrue();
        result.Result.Should().Be(expectedResult);
    }

    [Fact]
    public void Failure_ShouldReturnOperationResultWithStatusCodeBadRequest()
    {
        // Act
        var result = ExecutionResults.Failure("key", "error");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public void Failure_WithStatusCode_ShouldReturnOperationResultWithSpecifiedStatusCode()
    {
        // Act
        var result = ExecutionResults.Failure(HttpStatusCode.NotFound).Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public void OperationResult_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var operationResult = new ExecutionResult
        {
            StatusCode = HttpStatusCode.OK,
            Title = "Test Title",
            Detail = "Test Detail",
            Location = new Uri("http://example.com"),
            Errors = ElementCollection.With("ErrorKey", "ErrorValue"),
            Headers = ElementCollection.With("HeaderKey", "HeaderValue"),
            Extensions = ElementCollection.With("ExtensionKey", "ExtensionValue")
        };

        // Act
        JsonSerializerOptions options = new()
        { Converters = { new ExecutionResultJsonConverterFactory() } };
        var json = JsonSerializer.Serialize(operationResult, options);
        var deserializedResult = JsonSerializer.Deserialize<ExecutionResult>(json, options);

        // Assert
        deserializedResult.Should().BeEquivalentTo(operationResult);
    }

    [Fact]
    public void OperationResult_WithGenericResult_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var operationResult = new ExecutionResult<string>
        {
            StatusCode = HttpStatusCode.OK,
            Title = "Test Title",
            Detail = "Test Detail",
            Location = new Uri("http://example.com"),
            Result = "Test Result",
            Errors = ElementCollection.With("ErrorKey", "ErrorValue"),
            Headers = ElementCollection.With("HeaderKey", "HeaderValue"),
            Extensions = ElementCollection.With("ExtensionKey", "ExtensionValue")
        };

        // Act
        JsonSerializerOptions options = new()
        { Converters = { new ExecutionResultJsonConverterFactory() } };
        var json = JsonSerializer.Serialize(operationResult, options);
        var deserializedResult = JsonSerializer.Deserialize<ExecutionResult<string>>(json, options);

        // Assert
        deserializedResult.Should().BeEquivalentTo(operationResult);
    }
}
