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
        var result = ExecutionResult.Success();

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
        var result = ExecutionResult.Success(expectedResult);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccessStatusCode.Should().BeTrue();
        result.Value.Should().Be(expectedResult);
    }

    [Fact]
    public void Failure_ShouldReturnOperationResultWithStatusCodeBadRequest()
    {
        // Act
        var result = ExecutionResult.Failure("key", "error");

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public void Failure_WithStatusCode_ShouldReturnOperationResultWithSpecifiedStatusCode()
    {
        // Act
        var result = ExecutionResult.Failure(HttpStatusCode.NotFound).Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public void OperationResult_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var successResult = ExecutionResult.Ok()
            .WithLocation(new Uri("http://example.com"))
            .WithHeaders(ElementCollection.With("HeaderKey", "HeaderValue"))
            .WithExtensions(ElementCollection.With("ExtensionKey", "ExtensionValue"))
            .Build();
        var failureResult = ExecutionResult.BadRequest()
            .WithTitle("Test Title")
            .WithDetail("Test Detail")
            .WithErrors(ElementCollection.With("ErrorKey", "ErrorValue"))
            .Build();

        // Act
        JsonSerializerOptions options = new()
        { Converters = { new ExecutionResultJsonConverterFactory() } };
        var succesJson = JsonSerializer.Serialize(successResult, options);
        var deserializedSuccess = JsonSerializer.Deserialize<ExecutionResult>(succesJson, options);
        var failureJson = JsonSerializer.Serialize(failureResult, options);
        var deserializedFailure = JsonSerializer.Deserialize<ExecutionResult>(failureJson, options);

        // Assert
        deserializedSuccess.Should().BeEquivalentTo(successResult);
        deserializedFailure.Should().BeEquivalentTo(failureResult);
    }

    [Fact]
    public void OperationResult_WithGenericResult_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var successResult = ExecutionResult.Ok("Test Result")
            .WithLocation(new Uri("http://example.com"))
            .WithHeaders(ElementCollection.With("HeaderKey", "HeaderValue"))
            .WithExtensions(ElementCollection.With("ExtensionKey", "ExtensionValue"))
            .Build();
        var failureResult = ExecutionResult.BadRequest<string>()
            .WithTitle("Test Title")
            .WithDetail("Test Detail")
            .WithErrors(ElementCollection.With("ErrorKey", "ErrorValue"))
            .Build();

        // Act
        JsonSerializerOptions options = new()
        { Converters = { new ExecutionResultJsonConverterFactory() } };
        var successJson = JsonSerializer.Serialize(successResult, options);
        var deserializedSuccess = JsonSerializer.Deserialize<ExecutionResult<string>>(successJson, options);
        var failureJson = JsonSerializer.Serialize(failureResult, options);
        var deserializedFailure = JsonSerializer.Deserialize<ExecutionResult<string>>(failureJson, options);

        // Assert
        deserializedSuccess.Should().BeEquivalentTo(successResult);
        deserializedFailure.Should().BeEquivalentTo(failureResult);
    }
}
