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
using System.Text.Json;

using FluentAssertions;

using Microsoft.Extensions.Primitives;

using Xpandables.Net.Collections;

namespace Xpandables.Net.UnitTests.Collections;

/// <summary>
/// Tests for the ElementEntry and ElementCollection JSON serialization functionality.
/// </summary>
public class ElementJsonSerializationTests
{
    [Fact]
    public void ElementEntry_JsonSerialization_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var originalEntry = new ElementEntry { Key = "test-key", Values = new StringValues(["value1", "value2", "value3"]) };

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = ElementEntryContext.Default,
            Converters = { new ElementEntryJsonConverterFactory() }
        };

        // Act
        var json = JsonSerializer.Serialize(originalEntry, options);
        var deserializedEntry = JsonSerializer.Deserialize<ElementEntry>(json, options);

        // Assert
        json.Should().NotBeNullOrEmpty();
        deserializedEntry.Key.Should().Be("test-key");
        deserializedEntry.Values.Count.Should().Be(3);
        deserializedEntry.Values.ToArray().Should().Equal("value1", "value2", "value3");
    }

    [Fact]
    public void ElementEntry_JsonSerialization_WithSingleValue_ShouldWork()
    {
        // Arrange
        var originalEntry = new ElementEntry { Key = "single-key", Values = new StringValues("single-value") };

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = ElementEntryContext.Default,
            Converters = { new ElementEntryJsonConverterFactory() }
        };

        // Act
        var json = JsonSerializer.Serialize(originalEntry, options);
        var deserializedEntry = JsonSerializer.Deserialize<ElementEntry>(json, options);

        // Assert
        deserializedEntry.Key.Should().Be("single-key");
        deserializedEntry.Values.Count.Should().Be(1);
        deserializedEntry.Values[0].Should().Be("single-value");
    }

    [Fact]
    public void ElementCollection_JsonSerialization_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        var originalCollection = new ElementCollection
        {
            { "key1", "value1", "value2" },
            { "key2", "value3" },
            { "key3", "value4", "value5", "value6" }
        };

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = ElementCollectionContext.Default,
            Converters = { new ElementCollectionJsonConverterFactory(), new ElementEntryJsonConverterFactory() }
        };

        // Act
        var json = JsonSerializer.Serialize(originalCollection, options);
        var deserializedCollection = JsonSerializer.Deserialize<ElementCollection>(json, options);

        // Assert
        json.Should().NotBeNullOrEmpty();
        deserializedCollection.Count.Should().Be(3);

        deserializedCollection.TryGetValue("key1", out var entry1).Should().BeTrue();
        entry1.Values.ToArray().Should().Equal("value1", "value2");

        deserializedCollection.TryGetValue("key2", out var entry2).Should().BeTrue();
        entry2.Values.ToArray().Should().Equal("value3");

        deserializedCollection.TryGetValue("key3", out var entry3).Should().BeTrue();
        entry3.Values.ToArray().Should().Equal("value4", "value5", "value6");
    }

    [Fact]
    public void ElementCollection_JsonSerialization_EmptyCollection_ShouldWork()
    {
        // Arrange
        var originalCollection = ElementCollection.Empty;

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = ElementCollectionContext.Default,
            Converters = { new ElementCollectionJsonConverterFactory() }
        };

        // Act
        var json = JsonSerializer.Serialize(originalCollection, options);
        var deserializedCollection = JsonSerializer.Deserialize<ElementCollection>(json, options);

        // Assert
        json.Should().Be("[]");
        deserializedCollection.IsEmpty.Should().BeTrue();
        deserializedCollection.Count.Should().Be(0);
    }

    [Fact]
    public void ElementEntry_JsonSerialization_WithoutSourceGeneration_ShouldWork()
    {
        // Arrange
        var originalEntry = new ElementEntry { Key = "test-key", Values = new StringValues(["value1", "value2"]) };

        var options = new JsonSerializerOptions
        {
            Converters = { new ElementEntryJsonConverterFactory() }
        };

        // Act
        var json = JsonSerializer.Serialize(originalEntry, options);
        var deserializedEntry = JsonSerializer.Deserialize<ElementEntry>(json, options);

        // Assert
        deserializedEntry.Key.Should().Be("test-key");
        deserializedEntry.Values.ToArray().Should().Equal("value1", "value2");
    }

    [Fact]
    public void ElementCollection_JsonSerialization_SimpleTest_ShouldWork()
    {
        // Arrange
        var originalCollection = new ElementCollection
        {
            { "key1", "value1", "value2" }
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new ElementCollectionJsonConverterFactory(), new ElementEntryJsonConverterFactory() }
        };

        // Act
        var json = JsonSerializer.Serialize(originalCollection, options);
        var deserializedCollection = JsonSerializer.Deserialize<ElementCollection>(json, options);

        // Assert
        deserializedCollection.Count.Should().Be(1);
        deserializedCollection.TryGetValue("key1", out var entry1).Should().BeTrue();
        var actualValues = entry1.Values.ToArray();
        actualValues.Should().Equal("value1", "value2");
    }

    [Fact]
    public void ElementCollection_JsonSerialization_SimpleTest_WithMultipleValues_ShouldWork()
    {
        // Arrange
        var originalCollection = new ElementCollection
        {
            { "key1", "value1", "value2" },
            { "key2", "value3", "value4" }
        };

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new ElementCollectionJsonConverterFactory(), new ElementEntryJsonConverterFactory() }
        };

        // Act
        var json = JsonSerializer.Serialize(originalCollection, options);
        var deserializedCollection = JsonSerializer.Deserialize<ElementCollection>(json, options);

        // Assert
        deserializedCollection.Count.Should().Be(2);
        deserializedCollection.TryGetValue("key1", out var entry1).Should().BeTrue();
        var actualValues1 = entry1.Values.ToArray();
        actualValues1.Should().Equal("value1", "value2");

        deserializedCollection.TryGetValue("key2", out var entry2).Should().BeTrue();
        var actualValues2 = entry2.Values.ToArray();
        actualValues2.Should().Equal("value3", "value4");
    }
}