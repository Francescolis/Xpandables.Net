using FluentAssertions;

namespace Xpandables.Net.Test.UnitTests;
using System.Collections.Generic;
using System.Linq;

using global::Xpandables.Net.Collections;

using Xunit;

public sealed class ElementCollectionExtensionsUnitTest
{
    [Fact]
    public void ToElementCollection_ShouldThrowArgumentNullException_WhenEntriesIsNull()
    {
        // Arrange
        IEnumerable<ElementEntry> entries = []!;

        // Act & Assert
        entries.Invoking(e => e.ToElementCollection())
               .Should().ThrowExactly<ArgumentNullException>();
    }

    [Fact]
    public void ToElementCollection_ShouldReturnEmptyCollection_WhenEntriesIsEmpty()
    {
        // Arrange
        var entries = Enumerable.Empty<ElementEntry>();

        // Act
        var result = entries.ToElementCollection();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToElementCollection_ShouldReturnElementCollection_WhenEntriesAreProvided()
    {
        // Arrange
        var entries = new List<ElementEntry>
            {
                new("Key1", "Value1"),
                new("Key2", "Value2", "Value3")
            };

        // Act
        var result = entries.ToElementCollection();

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().Be(2);
        result["Key1"]!.Value.Values.First().Should().Be("Value1");
        result["Key2"]!.Value.Values.ToArray().Should().Equal(["Value2", "Value3"]);
    }

    [Fact]
    public void ToElementCollection_ShouldHandleDuplicateKeysByMergingValues()
    {
        // Arrange
        var entries = new List<ElementEntry>
            {
                new("Key1", "Value1"),
                new("Key1", "Value2")
            };

        // Act
        var result = entries.ToElementCollection();

        // Assert
        result.Should().NotBeNull();
        result.Should().ContainSingle();
        result["Key1"]!.Value.Values.ToArray().Should().Equal(["Value1", "Value2"]);
    }

    [Fact]
    public void ToElementCollection_ShouldOptimizeForICollection()
    {
        // Arrange
        ICollection<ElementEntry> entries =
            [
                new("Key1", "Value1"),
                new("Key2", "Value2")
            ];

        // Act
        var result = entries.ToElementCollection();

        // Assert
        result.Should().NotBeNull();
        result.Count().Should().Be(2);
        result["Key1"]!.Value.Values.First().Should().Be("Value1");
        result["Key2"]!.Value.Values.First().Should().Be("Value2");
    }
}