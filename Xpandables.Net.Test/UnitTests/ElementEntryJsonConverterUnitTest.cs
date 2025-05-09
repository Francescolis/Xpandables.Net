using System.Text.Json;

using FluentAssertions;

using Microsoft.Extensions.Primitives;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Test.UnitTests;
public sealed class ElementEntryJsonConverterUnitTest
{
    private readonly JsonSerializerOptions _options;

    public ElementEntryJsonConverterUnitTest() => _options = new JsonSerializerOptions
    {
        Converters = { new ElementEntryJsonConverter() }
    };

    [Fact]
    public void Read_ShouldDeserializeValidJsonToElementEntry()
    {
        // Arrange
        string json = "{\"Key\":\"TestKey\",\"Values\":[\"Value1\",\"Value2\"]}";

        // Act
        var result = JsonSerializer.Deserialize<ElementEntry>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result.Key.Should().Be("TestKey");
        result.Values.Should().BeEquivalentTo(new StringValues(["Value1", "Value2"]));
    }

    [Fact]
    public void Read_ShouldThrowJsonException_WhenJsonIsInvalid()
    {
        // Arrange
        string json = "{\"Invalid\":\"Data\"}";

        // Act
        Action act = () => JsonSerializer.Deserialize<ElementEntry>(json, _options);

        // Assert
        act.Should().Throw<JsonException>();
    }

    [Fact]
    public void Write_ShouldSerializeElementEntryToJson()
    {
        // Arrange
        var entry = new ElementEntry("TestKey", "Value1", "Value2");

        // Act
        string json = JsonSerializer.Serialize(entry, _options);

        // Assert
        json.Should().Be("{\"Key\":\"TestKey\",\"Values\":[\"Value1\",\"Value2\"]}");
    }
}
