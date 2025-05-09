using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Executions;

namespace Xpandables.Net.Test.UnitTests;
public sealed class ExecutionResultJsonConverterUnitTest
{
    private readonly JsonSerializerOptions _options;

    public ExecutionResultJsonConverterUnitTest() => _options = new JsonSerializerOptions
    {
        Converters = { new ExecutionResultJsonConverter() }
    };

    [Fact]
    public void Write_ShouldSerializeExecutionResultToJson()
    {
        // Arrange
        var result = new ExecutionResult
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Title = "Test Title",
            Detail = "Test Detail"
        };

        // Act
        var json = JsonSerializer.Serialize(result, _options);

        // Assert
        json.Should().Contain("\"StatusCode\":200");
        json.Should().Contain("\"Title\":\"Test Title\"");
        json.Should().Contain("\"Detail\":\"Test Detail\"");
    }

    [Fact]
    public void Read_ShouldDeserializeJsonToExecutionResult()
    {
        // Arrange
        var json = "{\"StatusCode\":200,\"Title\":\"Test Title\",\"Detail\":\"Test Detail\"}";

        // Act
        var result = JsonSerializer.Deserialize<ExecutionResult>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Title.Should().Be("Test Title");
        result.Detail.Should().Be("Test Detail");
    }

    [Fact]
    public void Read_ShouldThrowNotSupportedException_WhenUsingAspNetCoreCompatibility()
    {
        // Arrange
        var converter = new ExecutionResultJsonConverter { UseAspNetCoreCompatibility = true };
        var options = new JsonSerializerOptions { Converters = { converter } };
        var json = "{\"StatusCode\":200,\"Title\":\"Test Title\"}";

        // Act
        Action act = () => JsonSerializer.Deserialize<ExecutionResult>(json, options);

        // Assert
        act.Should().Throw<NotSupportedException>();
    }
}
