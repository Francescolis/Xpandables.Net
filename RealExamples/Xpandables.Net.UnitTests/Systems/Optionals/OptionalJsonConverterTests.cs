using System.Optionals;
using System.Text.Json;
using Xunit;

namespace Xpandables.Net.UnitTests.Systems.Optionals;

public sealed class OptionalJsonConverterTests
{
    [Fact]
    public void Serialize_EmptyOptional_WritesNull()
    {
		// Arrange
		JsonSerializerOptions options = CreateOptions();
        var optional = Optional.Empty<int>();

		// Act
		string json = JsonSerializer.Serialize(optional, options);

        // Assert
        Assert.Equal("null", json);
    }

    [Fact]
    public void Serialize_SomeOptional_WritesPrimitiveValue()
    {
		// Arrange
		JsonSerializerOptions options = CreateOptions();
        var optional = Optional.Some(7);

		// Act
		string json = JsonSerializer.Serialize(optional, options);

        // Assert
        Assert.Equal("7", json);
    }

    [Fact]
    public void CanConvert_ReturnsFalseForNonOptional()
    {
        // Arrange
        var factory = new OptionalJsonConverterFactory();

		// Act
		bool result = factory.CanConvert(typeof(int));

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanConvert_ReturnsTrueForOptional()
    {
        // Arrange
        var factory = new OptionalJsonConverterFactory();

		// Act
		bool result = factory.CanConvert(typeof(Optional<string>));

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CreateConverter_AddsOptionalJsonContextWhenMissing()
    {
        // Arrange
        var factory = new OptionalJsonConverterFactory();
        var options = new JsonSerializerOptions();

        // Act
        factory.CreateConverter(typeof(Optional<int>), options);

        // Assert
        Assert.Contains(options.TypeInfoResolverChain, resolver => resolver is OptionalJsonContext);
    }

    [Fact]
    public void Deserialize_NullToken_ReturnsEmptyOptional()
    {
		// Arrange
		JsonSerializerOptions options = CreateOptions();

		// Act
		Optional<int> result = JsonSerializer.Deserialize<Optional<int>>("null", options);

        // Assert
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void Deserialize_PrimitiveValue_ReturnsOptionalWithValue()
    {
		// Arrange
		JsonSerializerOptions options = CreateOptions();

		// Act
		Optional<int> result = JsonSerializer.Deserialize<Optional<int>>("5", options);

        // Assert
        Assert.True(result.IsNotEmpty);
        Assert.Equal(5, result.Value);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new OptionalJsonConverterFactory());
        options.TypeInfoResolverChain.Add(OptionalJsonContext.Default);
        return options;
    }
}
