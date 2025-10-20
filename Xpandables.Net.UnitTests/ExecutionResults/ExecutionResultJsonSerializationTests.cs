/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

using FluentAssertions;

using Xpandables.Net;
using Xpandables.Net.ExecutionResults;

namespace Xpandables.Net.UnitTests.ExecutionResults;

/// <summary>
/// Tests for ExecutionResult JSON serialization functionality.
/// </summary>
public partial class ExecutionResultJsonSerializationTests
{
    private readonly JsonSerializerOptions _defaultOptions = new()
    {
        TypeInfoResolver = JsonTypeInfoResolver.Combine(ExecutionResultJsonContext.Default, TestModelJsonContext.Default)
    };

    private readonly JsonSerializerOptions _aspNetCoreCompatibilityOptions = new()
    {
        TypeInfoResolver = JsonTypeInfoResolver.Combine(ExecutionResultJsonContext.Default, TestModelJsonContext.Default),
        Converters = { new ExecutionResultJsonConverterFactory { UseAspNetCoreCompatibility = true } }
    };

    [Fact]
    public void ExecutionResultJsonConverterFactory_CanConvert_WithExecutionResult_ShouldReturnTrue()
    {
        // Arrange
        var factory = new ExecutionResultJsonConverterFactory();

        // Act
        var canConvert = factory.CanConvert(typeof(ExecutionResult));

        // Assert
        canConvert.Should().BeTrue();
    }

    [Fact]
    public void ExecutionResultJsonConverterFactory_CanConvert_WithGenericExecutionResult_ShouldReturnTrue()
    {
        // Arrange
        var factory = new ExecutionResultJsonConverterFactory();

        // Act
        var canConvert = factory.CanConvert(typeof(ExecutionResult<string>));

        // Assert
        canConvert.Should().BeTrue();
    }

    [Fact]
    public void ExecutionResultJsonConverterFactory_CanConvert_WithNonExecutionResult_ShouldReturnFalse()
    {
        // Arrange
        var factory = new ExecutionResultJsonConverterFactory();

        // Act
        var canConvert = factory.CanConvert(typeof(string));

        // Assert
        canConvert.Should().BeFalse();
    }

    [Fact]
    public void ExecutionResultJsonConverterFactory_CanConvert_WithNullType_ShouldThrow()
    {
        // Arrange
        var factory = new ExecutionResultJsonConverterFactory();

        // Act & Assert
        factory.Invoking(f => f.CanConvert(null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExecutionResultJsonConverterFactory_CreateConverter_WithExecutionResult_ShouldReturnCorrectConverter()
    {
        // Arrange
        var factory = new ExecutionResultJsonConverterFactory();

        // Act
        var converter = factory.CreateConverter(typeof(ExecutionResult), _defaultOptions);

        // Assert
        converter.Should().BeOfType<ExecutionResultJsonConverter>();
    }

    [Fact]
    public void ExecutionResultJsonConverterFactory_CreateConverter_WithGenericExecutionResult_ShouldReturnCorrectConverter()
    {
        // Arrange
        var factory = new ExecutionResultJsonConverterFactory();

        // Act
        var converter = factory.CreateConverter(typeof(ExecutionResult<string>), _defaultOptions);

        // Assert
        converter.Should().BeOfType<ExecutionResultJsonConverter<string>>();
    }

    [Fact]
    public void ExecutionResultJsonConverterFactory_CreateConverter_WithAspNetCoreCompatibility_ShouldSetProperty()
    {
        // Arrange
        var factory = new ExecutionResultJsonConverterFactory { UseAspNetCoreCompatibility = true };

        // Act
        var converter = factory.CreateConverter(typeof(ExecutionResult), _defaultOptions) as ExecutionResultJsonConverter;

        // Assert
        converter.Should().NotBeNull();
        converter!.UseAspNetCoreCompatibility.Should().BeTrue();
    }

    [Fact]
    public void ExecutionResultJsonConverterFactory_CreateConverter_WithNullType_ShouldThrow()
    {
        // Arrange
        var factory = new ExecutionResultJsonConverterFactory();

        // Act & Assert
        factory.Invoking(f => f.CreateConverter(null!, _defaultOptions))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExecutionResultJsonConverterFactory_CreateConverter_WithNullOptions_ShouldThrow()
    {
        // Arrange
        var factory = new ExecutionResultJsonConverterFactory();

        // Act & Assert
        factory.Invoking(f => f.CreateConverter(typeof(ExecutionResult), null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExecutionResult_Serialize_WithDefaultOptions_ShouldSerializeCorrectly()
    {
        // Arrange
        var result = ExecutionResult
            .Success(HttpStatusCode.OK)
            .Build();

        // Act
        var json = JsonSerializer.Serialize(result, _defaultOptions);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"statusCode\":");
    }

    [Fact]
    public void ExecutionResult_Serialize_WithAspNetCoreCompatibility_ShouldSerializeOnlyValue()
    {
        // Arrange
        var result = ExecutionResult
            .Success(HttpStatusCode.OK, "test value")
            .Build();

        // Act
        var json = JsonSerializer.Serialize(result, _aspNetCoreCompatibilityOptions);

        // Assert
        json.Should().Be("\"test value\"");
    }

    [Fact]
    public void ExecutionResult_Serialize_WithAspNetCoreCompatibilityAndNullValue_ShouldNotSerialize()
    {
        // Arrange
        var result = ExecutionResult
            .Success(HttpStatusCode.NoContent)
            .Build();

        // Act
        var json = JsonSerializer.Serialize(result, _aspNetCoreCompatibilityOptions);

        // Assert
        json.Should().BeNullOrEmpty();
    }

    [Fact]
    public void ExecutionResult_Deserialize_WithDefaultOptions_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
            {
                "StatusCode": 200,
                "Title": "Success",
                "Detail": "Operation completed",
                "Value": "test result",
                "Errors": [],
                "Headers": [],
                "Extensions": []
            }
            """;

        // Act
        var result = JsonSerializer.Deserialize<ExecutionResult<string>>(json, _defaultOptions);

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(HttpStatusCode.OK);
        result.Title.Should().Be("Success");
        result.Detail.Should().Be("Operation completed");
        result.Value.Should().Be("test result");
    }

    [Fact]
    public void ExecutionResult_Deserialize_WithAspNetCoreCompatibility_ShouldThrowNotSupportedException()
    {
        // Arrange
        var json = "\"test value\"";

        // Act & Assert
        FluentActions.Invoking(() => JsonSerializer.Deserialize<ExecutionResult>(json, _aspNetCoreCompatibilityOptions))
            .Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ExecutionResultGeneric_Serialize_WithDefaultOptions_ShouldSerializeCorrectly()
    {
        // Arrange
        var result = ExecutionResult
            .Success(HttpStatusCode.Created, new TestModel { Id = 1, Name = "Test" })
            .Build();

        // Act
        var json = JsonSerializer.Serialize(result, _defaultOptions);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"statusCode\":");
        json.Should().Contain("\"value\":");
        json.Should().Contain("\"Id\":");
        json.Should().Contain("\"Name\":");
    }

    [Fact]
    public void ExecutionResultGeneric_Serialize_WithAspNetCoreCompatibility_ShouldSerializeOnlyValue()
    {
        // Arrange
        var result = ExecutionResult
            .Success(HttpStatusCode.OK, new TestModel { Id = 42, Name = "Answer" })
            .Build();

        // Act
        var json = JsonSerializer.Serialize(result, _aspNetCoreCompatibilityOptions);

        // Assert
        json.Should().Contain("\"Id\":");
        json.Should().Contain("\"Name\":");
        json.Should().NotContain("StatusCode");
        json.Should().NotContain("Title");
    }

    [Fact]
    public void ExecutionResultGeneric_Serialize_WithAspNetCoreCompatibilityAndNullValue_ShouldNotSerialize()
    {
        // Arrange
        var result = ExecutionResult
            .Success<TestModel?>(HttpStatusCode.NoContent, null)
            .Build();

        // Act
        var json = JsonSerializer.Serialize(result, _aspNetCoreCompatibilityOptions);

        // Assert
        json.Should().BeNullOrEmpty();
    }

    [Fact]
    public void ExecutionResultGeneric_Deserialize_WithDefaultOptions_ShouldDeserializeCorrectly()
    {
        // Arrange
        var json = """
            {
                "StatusCode": 201,
                "Title": "Created",
                "Detail": "Resource created",
                "Value": {
                    "Id": 123,
                    "Name": "Test Resource"
                },
                "Errors": [],
                "Headers": [],
                "Extensions": []
            }
            """;

        // Act
        var result = JsonSerializer.Deserialize<ExecutionResult<TestModel>>(json, _defaultOptions);

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(HttpStatusCode.Created);
        result.Title.Should().Be("Created");
        result.Detail.Should().Be("Resource created");
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(123);
        result.Value.Name.Should().Be("Test Resource");
    }

    [Fact]
    public void ExecutionResultGeneric_Deserialize_WithAspNetCoreCompatibility_ShouldThrowNotSupportedException()
    {
        // Arrange
        var json = """
            {
                "id": 1,
                "name": "Test"
            }
            """;

        // Act & Assert
        FluentActions.Invoking(() => JsonSerializer.Deserialize<ExecutionResult<TestModel>>(json, _aspNetCoreCompatibilityOptions))
            .Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void ExecutionResult_SerializeWithErrors_ShouldIncludeErrorsInJson()
    {
        // Arrange
        var result = ExecutionResult
            .Failure(HttpStatusCode.BadRequest)
            .WithTitle("Validation Failed")
            .WithError("field1", "Field is required")
            .WithError("field2", "Invalid format", "Must be numeric")
            .Build();

        // Act
        var json = JsonSerializer.Serialize(result, _defaultOptions);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"errors\":");
        json.Should().Contain("field1");
        json.Should().Contain("field2");
        json.Should().Contain("Field is required");
        json.Should().Contain("Invalid format");
        json.Should().Contain("Must be numeric");
    }

    [Fact]
    public void ExecutionResult_SerializeWithHeaders_ShouldIncludeHeadersInJson()
    {
        // Arrange
        var result = ExecutionResult
            .Success(HttpStatusCode.OK)
            .WithHeader("X-Custom-Header", "custom-value")
            .WithHeader("X-Multi-Header", "value1", "value2")
            .Build();

        // Act
        var json = JsonSerializer.Serialize(result, _defaultOptions);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"headers\":");
        json.Should().Contain("X-Custom-Header");
        json.Should().Contain("X-Multi-Header");
        json.Should().Contain("custom-value");
        json.Should().Contain("value1");
        json.Should().Contain("value2");
    }

    [Fact]
    public void ExecutionResult_SerializeWithExtensions_ShouldIncludeExtensionsInJson()
    {
        // Arrange
        var result = ExecutionResult
            .Success(HttpStatusCode.OK)
            .WithExtension("traceId", "12345")
            .WithExtension("requestId", "abc-def-ghi")
            .Build();

        // Act
        var json = JsonSerializer.Serialize(result, _defaultOptions);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"extensions\":");
        json.Should().Contain("traceId");
        json.Should().Contain("requestId");
        json.Should().Contain("12345");
        json.Should().Contain("abc-def-ghi");
    }

    [Fact]
    public void ExecutionResult_SerializeWithLocation_ShouldIncludeLocationInJson()
    {
        // Arrange
        var location = new Uri("https://api.example.com/resources/123");
        var result = ExecutionResult
            .Success(HttpStatusCode.Created)
            .WithLocation(location)
            .Build();

        // Act
        var json = JsonSerializer.Serialize(result, _defaultOptions);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"location\":");
        json.Should().Contain("https://api.example.com/resources/123");
    }

    [Fact]
    public void ExecutionResult_SerializeWithException_ShouldIncludeExceptionInJson()
    {
        // Arrange
        var exception = new InvalidOperationException("Test exception");
        var result = ExecutionResult
            .Failure(HttpStatusCode.InternalServerError)
            .WithException(exception)
            .Build();

        // Act
        var json = JsonSerializer.Serialize(result, _defaultOptions);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"errors\":");
        json.Should().Contain("Exception");
        json.Should().Contain("Test exception");
    }

    [Fact]
    public void ExecutionResultJsonConverter_Write_WithNullValue_ShouldThrow()
    {
        // Arrange
        var converter = new ExecutionResultJsonConverter();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act & Assert
        converter.Invoking(c => c.Write(writer, null!, _defaultOptions))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExecutionResultJsonConverter_Write_WithNullOptions_ShouldThrow()
    {
        // Arrange
        var converter = new ExecutionResultJsonConverter();
        var result = ExecutionResult.Success();
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act & Assert
        converter.Invoking(c => c.Write(writer, result, null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExecutionResultJsonConverterGeneric_Write_WithNullValue_ShouldThrow()
    {
        // Arrange
        var converter = new ExecutionResultJsonConverter<string>(false);
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act & Assert
        converter.Invoking(c => c.Write(writer, null!, _defaultOptions))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExecutionResultJsonConverterGeneric_Write_WithNullOptions_ShouldThrow()
    {
        // Arrange
        var converter = new ExecutionResultJsonConverter<string>(false);
        var result = ExecutionResult.Success("test");
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);

        // Act & Assert
        converter.Invoking(c => c.Write(writer, result, null!))
            .Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ExecutionResultJsonConverterGeneric_UseAspNetCoreCompatibility_ShouldReturnCorrectValue()
    {
        // Arrange & Act
        var converterWithCompatibility = new ExecutionResultJsonConverter<string>(true);
        var converterWithoutCompatibility = new ExecutionResultJsonConverter<string>(false);

        // Assert
        converterWithCompatibility.UseAspNetCoreCompatibility.Should().BeTrue();
        converterWithoutCompatibility.UseAspNetCoreCompatibility.Should().BeFalse();
    }

    [Fact]
    public void ExecutionResultJsonContext_ShouldSupportCommonTypes()
    {
        // Arrange & Act
        var context = ExecutionResultJsonContext.Default;

        // Assert
        context.Should().NotBeNull();
        context.GetTypeInfo(typeof(ExecutionResult)).Should().NotBeNull();
        context.GetTypeInfo(typeof(ExecutionResult<string>)).Should().NotBeNull();
        context.GetTypeInfo(typeof(ExecutionResult<int>)).Should().NotBeNull();
        context.GetTypeInfo(typeof(ExecutionResult<bool>)).Should().NotBeNull();
    }

    [Theory]
    [InlineData(typeof(ExecutionResult<string>))]
    [InlineData(typeof(ExecutionResult<int>))]
    [InlineData(typeof(ExecutionResult<long>))]
    [InlineData(typeof(ExecutionResult<double>))]
    [InlineData(typeof(ExecutionResult<decimal>))]
    [InlineData(typeof(ExecutionResult<bool>))]
    [InlineData(typeof(ExecutionResult<DateTime>))]
    [InlineData(typeof(ExecutionResult<DateTimeOffset>))]
    [InlineData(typeof(ExecutionResult<Guid>))]
    [InlineData(typeof(ExecutionResult<object>))]
    public void ExecutionResultJsonContext_ShouldSupportBuiltInTypes(Type resultType)
    {
        // Arrange
        var context = ExecutionResultJsonContext.Default;

        // Act
        var typeInfo = context.GetTypeInfo(resultType);

        // Assert
        typeInfo.Should().NotBeNull();
    }

    [Fact]
    public void ExecutionResult_RoundTripSerialization_ShouldPreserveAllProperties()
    {
        // Arrange
        var originalResult = ExecutionResult
            .Failure(HttpStatusCode.BadRequest)
            .WithTitle("Validation Error")
            .WithDetail("Multiple validation errors occurred")
            .WithError("field1", "Required field")
            .WithError("field2", "Invalid format", "Must be email")
            .WithHeader("X-Request-Id", "12345")
            .WithExtension("traceId", "trace-123")
            .WithLocation("https://example.com/errors")
            .Build();

        // Act
        var json = JsonSerializer.Serialize(originalResult, _defaultOptions);
        var deserializedResult = JsonSerializer.Deserialize<ExecutionResult>(json, _defaultOptions);

        // Assert
        deserializedResult.Should().NotBeNull();
        deserializedResult!.StatusCode.Should().Be(originalResult.StatusCode);
        deserializedResult.Title.Should().Be(originalResult.Title);
        deserializedResult.Detail.Should().Be(originalResult.Detail);
        deserializedResult.Location.Should().Be(originalResult.Location);
        deserializedResult.Errors.Count.Should().Be(originalResult.Errors.Count);
        deserializedResult.Headers.Count.Should().Be(originalResult.Headers.Count);
        deserializedResult.Extensions.Count.Should().Be(originalResult.Extensions.Count);
    }

    [Fact]
    public void ExecutionResultGeneric_RoundTripSerialization_ShouldPreserveAllProperties()
    {
        // Arrange
        var testModel = new TestModel { Id = 42, Name = "Test Model" };
        var originalResult = ExecutionResult
            .Success(HttpStatusCode.Created, testModel)
            .WithHeader("Location", "https://example.com/resources/42")
            .WithExtension("createdBy", "system")
            .Build();

        // Act
        var json = JsonSerializer.Serialize(originalResult, _defaultOptions);
        var deserializedResult = JsonSerializer.Deserialize<ExecutionResult<TestModel>>(json, _defaultOptions);

        // Assert
        deserializedResult.Should().NotBeNull();
        deserializedResult!.StatusCode.Should().Be(originalResult.StatusCode);
        deserializedResult.Headers.Count.Should().Be(originalResult.Headers.Count);
        deserializedResult.Extensions.Count.Should().Be(originalResult.Extensions.Count);
        deserializedResult.Value.Should().NotBeNull();
        deserializedResult.Value!.Id.Should().Be(testModel.Id);
        deserializedResult.Value.Name.Should().Be(testModel.Name);
    }

    private record TestModel
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }

    [JsonSerializable(typeof(TestModel))]
    [JsonSerializable(typeof(ExecutionResult<TestModel>))]
    private partial class TestModelJsonContext : JsonSerializerContext { }
}