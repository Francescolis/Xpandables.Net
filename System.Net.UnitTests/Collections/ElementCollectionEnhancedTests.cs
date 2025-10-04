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
using System.Collections.ObjectModel;
using System.Net.Abstractions.Collections;

using FluentAssertions;

using Microsoft.Extensions.Primitives;

namespace System.Net.UnitTests.Collections;

/// <summary>
/// Performance and functionality tests for the enhanced ElementCollection.
/// </summary>
public class ElementCollectionEnhancedTests
{
    [Fact]
    public void ElementCollection_WithIndexing_ShouldProvideO1Lookup()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2", "value2b" },
            { "key3", "value3" }
        };

        // Act & Assert
        collection.TryGetValue("key2", out var entry).Should().BeTrue();
        entry.Values.Count.Should().Be(2);
        entry.Values[0].Should().Be("value2");
        entry.Values[1].Should().Be("value2b");

        collection.ContainsKey("key1").Should().BeTrue();
        collection.ContainsKey("nonexistent").Should().BeFalse();
    }

    [Fact]
    public void ElementCollection_Count_ShouldReturnCorrectValue()
    {
        // Arrange
        var collection = new ElementCollection();

        // Act & Assert
        collection.Count.Should().Be(0);
        collection.IsEmpty.Should().BeTrue();

        collection.Add("key1", "value1");
        collection.Count.Should().Be(1);
        collection.IsEmpty.Should().BeFalse();

        collection.Add("key2", "value2");
        collection.Count.Should().Be(2);
    }

    [Fact]
    public void ElementCollection_Copy_ShouldCreateIndependentCopy()
    {
        // Arrange
        var original = ElementCollection.With("key1", "value1");

        // Act
        var copy = original.Copy();
        copy.Add("key2", "value2");

        // Assert
        original.Count.Should().Be(1);
        copy.Count.Should().Be(2);
        original.ContainsKey("key2").Should().BeFalse();
        copy.ContainsKey("key2").Should().BeTrue();
    }

    [Fact]
    public void ElementCollection_AddRange_WithDictionary_ShouldAddAllEntries()
    {
        // Arrange
        var collection = new ElementCollection();
        var dictionary = new Dictionary<string, StringValues>
        {
            ["key1"] = new StringValues("value1"),
            ["key2"] = new StringValues(["value2a", "value2b"]),
            ["key3"] = new StringValues("value3")
        };

        // Act
        collection.AddRange(dictionary);

        // Assert
        collection.Count.Should().Be(3);
        collection.ContainsKey("key1").Should().BeTrue();
        collection.ContainsKey("key2").Should().BeTrue();
        collection.ContainsKey("key3").Should().BeTrue();

        // Use TryGetValue instead of indexer for cleaner access
        collection.TryGetValue("key2", out var entry).Should().BeTrue();
        entry.Values.Count.Should().Be(2);
    }

    [Fact]
    public void ElementCollection_FromDictionary_ShouldCreateCorrectCollection()
    {
        // Arrange
        var dictionary = new Dictionary<string, StringValues>
        {
            ["Authorization"] = new StringValues("Bearer token123"),
            ["Content-Type"] = new StringValues("application/json"),
            ["Accept"] = new StringValues(["application/json", "text/plain"])
        };

        // Act
        var collection = ElementCollection.FromDictionary(dictionary);

        // Assert
        collection.Count.Should().Be(3);

        collection.TryGetValue("Authorization", out var authEntry).Should().BeTrue();
        authEntry.Values[0].Should().Be("Bearer token123");

        collection.TryGetValue("Accept", out var acceptEntry).Should().BeTrue();
        acceptEntry.Values.Count.Should().Be(2);
    }

    [Fact]
    public void ElementCollection_Keys_ShouldReturnAllKeys()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" }
        };

        // Act
        var keys = collection.Keys.ToList();

        // Assert
        keys.Should().HaveCount(3);
        keys.Should().Contain(["key1", "key2", "key3"]);
    }

    [Fact]
    public void ElementCollection_Values_ShouldReturnAllValues()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2a", "value2b" }
        };

        // Act
        var values = collection.Values.ToArray();

        // Assert
        values.Should().HaveCount(2);
        values[0][0].Should().Be("value1");
        values[1].Count.Should().Be(2);
    }

    [Fact]
    public void ElementCollection_Remove_ShouldUpdateIndexCorrectly()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" }
        };

        // Act
        var removed = collection.Remove("key2");

        // Assert
        removed.Should().BeTrue();
        collection.Count.Should().Be(2);
        collection.ContainsKey("key2").Should().BeFalse();
        collection.ContainsKey("key1").Should().BeTrue();
        collection.ContainsKey("key3").Should().BeTrue();
    }

    [Fact]
    public void ElementCollection_OperatorOverloads_ShouldWorkCorrectly()
    {
        // Arrange
        var collection1 = ElementCollection.With("key1", "value1");
        var collection2 = ElementCollection.With("key2", "value2");
        var entry = new ElementEntry("key3", "value3");

        // Act
        var merged = collection1 + collection2;
        var withEntry = collection1 + entry;
        var removed = collection1 - "key1";

        // Assert
        merged.Count.Should().Be(2);
        withEntry.Count.Should().Be(2);
        removed.Count.Should().Be(0);
    }

    [Fact]
    public void ElementCollection_ImplicitConversion_ShouldCreateReadOnlyDictionary()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2a", "value2b" }
        };

        // Act
        ReadOnlyDictionary<string, StringValues> dictionary = collection;

        // Assert
        dictionary.Count.Should().Be(2);
        dictionary["key1"][0].Should().Be("value1");
        dictionary["key2"].Count.Should().Be(2);
    }

    [Fact]
    public void ElementCollection_ToString_ShouldFormatCorrectly()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2a", "value2b" }
        };

        // Act
        var result = collection.ToString();

        // Assert
        result.Should().Contain("key1=value1");
        result.Should().Contain("key2=value2a,value2b");
        result.Should().Contain(";");
    }

    [Fact]
    public void ElementCollection_Empty_ShouldReturnCorrectState()
    {
        // Arrange & Act
        var empty = ElementCollection.Empty;

        // Assert
        empty.Count.Should().Be(0);
        empty.IsEmpty.Should().BeTrue();
        empty.Keys.Should().BeEmpty();
        empty.Values.Should().BeEmpty();
        empty.ToString().Should().BeEmpty();
        empty.ToDebugString().Should().Contain("Empty");
    }

    [Fact]
    public void ElementCollection_BasicSerialization_ShouldSerializeCorrectly()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "Content-Type", "application/json" }
        };

        // Act
        var json = collection.ToJsonString();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("Content-Type");
        json.Should().Contain("application/json");
    }


    [Fact]
    public void ElementCollection_ToDebugString_ShouldProvideDetailedInfo()
    {
        // Arrange
        var collection = ElementCollection.With("key1", "value1", "value2");

        // Act
        var debugString = collection.ToDebugString();

        // Assert
        debugString.Should().Contain("ElementCollection");
        debugString.Should().Contain("Count = 1");
        debugString.Should().Contain("key1");
        debugString.Should().Contain("value1");
        debugString.Should().Contain("value2");
    }
}