using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Collections;

namespace Xpandables.Net.Test.UnitTests;
public sealed class ElementCollectionJsonConverterUnitTest
{
    private readonly JsonSerializerOptions _options;

    public ElementCollectionJsonConverterUnitTest() => _options = new JsonSerializerOptions
    {
        Converters = { new ElementCollectionJsonConverter() }
    };

    [Fact]
    public void Read_ShouldDeserializeValidJsonToElementCollection()
    {
        // Arrange
        string json = "[{\"Key\":\"Key1\",\"Values\":[\"Value1\"]},{\"Key\":\"Key2\",\"Values\":[\"Value2\",\"Value3\"]}]";

        // Act
        var result = JsonSerializer.Deserialize<ElementCollection>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().Be(2);
        result["Key1"]!.Value.Values.Should().BeEquivalentTo(["Value1"]);
        result["Key2"]!.Value.Values.Should().BeEquivalentTo(["Value2", "Value3"]);
    }

    [Fact]
    public void Read_ShouldReturnEmptyCollection_WhenJsonIsEmptyArray()
    {
        // Arrange
        string json = "[]";

        // Act
        var result = JsonSerializer.Deserialize<ElementCollection>(json, _options);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void Write_ShouldSerializeElementCollectionToJson()
    {
        // Arrange
        var collection = ElementCollection.With(
        [
            new ElementEntry("Key1", "Value1"),
            new ElementEntry("Key2", "Value2", "Value3")
        ]);

        // Act
        string json = JsonSerializer.Serialize(collection, _options);

        // Assert
        json.Should().Be("[{\"Key\":\"Key1\",\"Values\":[\"Value1\"]},{\"Key\":\"Key2\",\"Values\":[\"Value2\",\"Value3\"]}]");
    }
}
