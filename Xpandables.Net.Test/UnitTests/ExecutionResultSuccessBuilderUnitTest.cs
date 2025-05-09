using System.Net;

using FluentAssertions;

using Xpandables.Net.Collections;
using Xpandables.Net.Executions;

namespace Xpandables.Net.Test.UnitTests;
public sealed class ExecutionResultSuccessBuilderUnitTest
{
    [Fact]
    public void Build_ShouldReturnExecutionResult_WithAllPropertiesSet()
    {
        // Arrange
        var builder = new TestExecutionResultBuilder(HttpStatusCode.BadRequest)
            .WithTitle("Test Title")
            .WithDetail("Test Detail")
            .WithStatusCode(HttpStatusCode.NotFound)
            .WithResult("Test Result")
            .WithLocation("http://example.com")
            .WithHeader("HeaderKey", "HeaderValue")
            .WithExtension("ExtensionKey", "ExtensionValue")
            .WithError("ErrorKey", "ErrorMessage");

        // Act
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
        result.Title.Should().Be("Test Title");
        result.Detail.Should().Be("Test Detail");
        result.Result.Should().Be("Test Result");
        result.Location.Should().Be(new Uri("http://example.com"));
        result.Headers["HeaderKey"]!.Value.Values.Should().Contain("HeaderValue");
        result.Extensions["ExtensionKey"]!.Value.Values.Should().Contain("ExtensionValue");
        result.Errors["ErrorKey"]!.Value.Values.Should().Contain("ErrorMessage");
    }

    [Fact]
    public void ClearAll_ShouldResetAllProperties()
    {
        // Arrange
        var builder = new TestExecutionResultBuilder(HttpStatusCode.BadRequest)
            .WithTitle("Test Title")
            .WithDetail("Test Detail")
            .WithStatusCode(HttpStatusCode.NotFound)
            .WithResult("Test Result")
            .WithLocation("http://example.com")
            .WithHeader("HeaderKey", "HeaderValue")
            .WithExtension("ExtensionKey", "ExtensionValue")
            .WithError("ErrorKey", "ErrorMessage");

        // Act
        builder.ClearAll();
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(default);
        result.Title.Should().BeNull();
        result.Detail.Should().BeNull();
        result.Result.Should().BeNull();
        result.Location.Should().BeNull();
        result.Headers.Should().BeEmpty();
        result.Extensions.Should().BeEmpty();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Merge_ShouldCombinePropertiesFromAnotherExecutionResult()
    {
        // Arrange
        var builder = new TestExecutionResultBuilder(HttpStatusCode.BadRequest)
            .WithTitle("Original Title")
            .WithDetail("Original Detail")
            .WithError("OriginalErrorKey", "OriginalErrorMessage");

        var otherResult = new ExecutionResult
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Title = "New Title",
            Detail = "New Detail",
            Errors = ElementCollection.With("NewErrorKey", "NewErrorMessage")
        };

        // Act
        builder.Merge(otherResult);
        var result = builder.Build();

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        result.Title.Should().Be("New Title");
        result.Detail.Should().Be("New Detail");
        result.Errors["OriginalErrorKey"]!.Value.Values.Should().Contain("OriginalErrorMessage");
        result.Errors["NewErrorKey"]!.Value.Values.Should().Contain("NewErrorMessage");
    }

    private class TestExecutionResultBuilder(HttpStatusCode statusCode) :
        ExecutionResultBuilder<TestExecutionResultBuilder>(statusCode)
    {
    }
}
