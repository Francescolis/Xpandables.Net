namespace Xpandables.Net.Test.UnitTests;
using Xpandables.Net.Collections;

public sealed class ElementCollectionUnitTest
{
    [Fact]
    public void Add_ShouldAddNewEntry_WhenKeyDoesNotExist()
    {
        var collection = new ElementCollection();
        var entry = new ElementEntry("key1", new[] { "value1" });
    
        collection.Add(entry);
    
        Assert.Equal(entry, collection["key1"]);
    }
    
    [Fact]
    public void Add_ShouldMergeValues_WhenKeyAlreadyExists()
    {
        var collection = new ElementCollection();
        collection.Add("key1", "value1");
        collection.Add("key1", "value2");
    
        var result = collection["key1"];
    
        Assert.NotNull(result);
        Assert.Equal("key1", result.Value.Key);
        Assert.Contains("value1", result.Value.Values, StringComparer.Ordinal);
        Assert.Contains("value2", result.Value.Values, StringComparer.Ordinal);
    }
    
    [Fact]
    public void Remove_ShouldRemoveEntry_WhenKeyExists()
    {
        var collection = new ElementCollection();
        collection.Add("key1", "value1");
    
        var removedCount = collection.Remove("key1");
    
        Assert.Equal(1, removedCount);
        Assert.Null(collection["key1"]);
    }
    
    [Fact]
    public void Remove_ShouldReturnZero_WhenKeyDoesNotExist()
    {
        var collection = new ElementCollection();
    
        var removedCount = collection.Remove("nonexistentKey");
    
        Assert.Equal(0, removedCount);
    }
    
    [Fact]
    public void Merge_ShouldCombineEntriesFromBothCollections()
    {
        var collection1 = new ElementCollection();
        collection1.Add("key1", "value1");
    
        var collection2 = new ElementCollection();
        collection2.Add("key2", "value2");
    
        collection1.Merge(collection2);
    
        Assert.Equal("value1", collection1["key1"].Value.Values[0]);
        Assert.Equal("value2", collection1["key2"].Value.Values[0]);
    }
    
    [Fact]
    public void Merge_ShouldMergeValues_WhenKeysOverlap()
    {
        var collection1 = new ElementCollection();
        collection1.Add("key1", "value1");
    
        var collection2 = new ElementCollection();
        collection2.Add("key1", "value2");
    
        collection1.Merge(collection2);
    
        var result = collection1["key1"];
    
        Assert.NotNull(result);
        Assert.Contains("value1", result.Value.Values, StringComparer.Ordinal);
        Assert.Contains("value2", result.Value.Values, StringComparer.Ordinal);
    }
    
    [Fact]
    public void ToString_ShouldReturnEmptyString_WhenCollectionIsEmpty()
    {
        var collection = new ElementCollection();
    
        var result = collection.ToString();
    
        Assert.Equal(string.Empty, result);
    }
    
    [Fact]
    public void ToString_ShouldReturnFormattedString_WhenCollectionHasEntries()
    {
        var collection = new ElementCollection();
        collection.Add("key1", "value1", "value2");
    
        var result = collection.ToString();
    
        Assert.Equal("key1=value1,value2", result);
    }
}