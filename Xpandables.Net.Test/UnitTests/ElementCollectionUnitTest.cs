using FluentAssertions;

namespace Xpandables.Net.Test.UnitTests;
using Xpandables.Net.Collections;

public sealed class ElementCollectionUnitTest
{
    [Fact]
    public void Add_ShouldAddNewEntry_WhenKeyDoesNotExist()
    {
        var collection = new ElementCollection();
        var entry = new ElementEntry("key1", ["value1"]);

        collection.Add(entry);

        collection["key1"].Should().Be(entry);
    }

    [Fact]
    public void Add_ShouldMergeValues_WhenKeyAlreadyExists()
    {
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key1", "value2" }
        };

        var result = collection["key1"];

        result.Should().NotBeNull();
        result.Value.Key.Should().Be("key1");
        result.Value.Values.Should().Contain("value1");
        result.Value.Values.Should().Contain("value2");
    }

    [Fact]
    public void Remove_ShouldRemoveEntry_WhenKeyExists()
    {
        var collection = new ElementCollection
        {
            { "key1", "value1" }
        };

        var removedCount = collection.Remove("key1");

        removedCount.Should().Be(1);
        collection["key1"].Should().BeNull();
    }

    [Fact]
    public void Remove_ShouldReturnZero_WhenKeyDoesNotExist()
    {
        var collection = new ElementCollection();

        var removedCount = collection.Remove("nonexistentKey");

        removedCount.Should().Be(0);
    }

    [Fact]
    public void Merge_ShouldCombineEntriesFromBothCollections()
    {
        var collection1 = new ElementCollection
        {
            { "key1", "value1" }
        };

        var collection2 = new ElementCollection
        {
            { "key2", "value2" }
        };

        collection1.Merge(collection2);

        collection1["key1"]!.Value.Values[0].Should().Be("value1");
        collection1["key2"]!.Value.Values[0].Should().Be("value2");
    }

    [Fact]
    public void Merge_ShouldMergeValues_WhenKeysOverlap()
    {
        var collection1 = new ElementCollection
        {
            { "key1", "value1" }
        };

        var collection2 = new ElementCollection
        {
            { "key1", "value2" }
        };

        collection1.Merge(collection2);

        var result = collection1["key1"];

        result.Should().NotBeNull();
        result.Value.Values.Should().Contain("value1");
        result.Value.Values.Should().Contain("value2");
    }

    [Fact]
    public void ToString_ShouldReturnEmptyString_WhenCollectionIsEmpty()
    {
        var collection = new ElementCollection();

        var result = collection.ToString();

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void ToString_ShouldReturnFormattedString_WhenCollectionHasEntries()
    {
        var collection = new ElementCollection
        {
            { "key1", "value1", "value2" }
        };

        var result = collection.ToString();

        result.Should().Be("key1=value1,value2");
    }
}