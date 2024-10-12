using System.Net;
using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Collections;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Test.UnitTests;

public sealed class OperationResultUnitTest
{
    [Fact]
    public void Success_ShouldReturnOperationResultWithStatusCodeOk()
    {
        // Act
        var result = OperationResults.Success().Build();

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
        var result = OperationResults.Success(expectedResult).Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        result.IsSuccessStatusCode.Should().BeTrue();
        result.Result.Should().Be(expectedResult);
    }

    [Fact]
    public void Failure_ShouldReturnOperationResultWithStatusCodeBadRequest()
    {
        // Act
        var result = OperationResults.Failure().Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        result.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public void Failure_WithStatusCode_ShouldReturnOperationResultWithSpecifiedStatusCode()
    {
        // Act
        var result = OperationResults.Failure(HttpStatusCode.NotFound).Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public void OperationResult_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var operationResult = new OperationResult
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
        { Converters = { new OperationResultJsonConverterFactory() } };
        var json = JsonSerializer.Serialize(operationResult, options);
        var deserializedResult = JsonSerializer.Deserialize<OperationResult>(json, options);

        // Assert
        deserializedResult.Should().BeEquivalentTo(operationResult);
    }

    [Fact]
    public void OperationResult_WithGenericResult_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var operationResult = new OperationResult<string>
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
        { Converters = { new OperationResultJsonConverterFactory() } };
        var json = JsonSerializer.Serialize(operationResult, options);
        var deserializedResult = JsonSerializer.Deserialize<OperationResult<string>>(json, options);

        // Assert
        deserializedResult.Should().BeEquivalentTo(operationResult);
    }

}
