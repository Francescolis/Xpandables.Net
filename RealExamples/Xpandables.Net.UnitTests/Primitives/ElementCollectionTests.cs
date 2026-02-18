/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Collections;
using System.Collections.ObjectModel;
using System.Text.Json;

using FluentAssertions;

using Microsoft.Extensions.Primitives;

namespace Xpandables.Net.UnitTests.Primitives;

public sealed class ElementCollectionTests
{
    #region Constructor and Factory Tests

    [Fact]
    public void WhenCreatingEmptyCollectionThenShouldBeEmpty()
    {
        // Act
        var collection = new ElementCollection();

        // Assert
        collection.IsEmpty.Should().BeTrue();
        collection.Count.Should().Be(0);
    }

    [Fact]
    public void WhenAccessingStaticEmptyThenShouldBeEmpty()
    {
        // Assert
        ElementCollection.Empty.IsEmpty.Should().BeTrue();
        ElementCollection.Empty.Count.Should().Be(0);
    }

    [Fact]
    public void WhenCreatingWithKeyAndValueThenShouldContainEntry()
    {
        // Act
        var collection = ElementCollection.With("name", "John");

        // Assert
        collection.IsEmpty.Should().BeFalse();
        collection.Count.Should().Be(1);
        collection.ContainsKey("name").Should().BeTrue();
    }

    [Fact]
    public void WhenCreatingWithKeyAndMultipleValuesThenShouldContainEntry()
    {
        // Act
        var collection = ElementCollection.With("colors", "red", "green", "blue");

        // Assert
        collection.Count.Should().Be(1);
        var entry = collection["colors"];
        entry.Should().NotBeNull();
        entry!.Value.Values.Count.Should().Be(3);
    }

    [Fact]
    public void WhenCreatingWithElementEntryThenShouldContainEntry()
    {
        // Arrange
        var entry = new ElementEntry("key", "value");

        // Act
        var collection = ElementCollection.With(entry);

        // Assert
        collection.Count.Should().Be(1);
        collection.ContainsKey("key").Should().BeTrue();
    }

    [Fact]
    public void WhenCreatingWithEnumerableThenShouldContainAllEntries()
    {
        // Arrange
        var entries = new[]
        {
            new ElementEntry("key1", "value1"),
            new ElementEntry("key2", "value2")
        };

        // Act
        var collection = ElementCollection.With(entries);

        // Assert
        collection.Count.Should().Be(2);
    }

    [Fact]
    public void WhenCreatingFromDictionaryThenShouldContainAllEntries()
    {
        // Arrange
        var dict = new Dictionary<string, StringValues>
        {
            ["name"] = "John",
            ["age"] = "30"
        };

        // Act
        var collection = ElementCollection.FromDictionary(dict);

        // Assert
        collection.Count.Should().Be(2);
        collection["name"]!.Value.Values.Should().Contain("John");
    }

    #endregion

    #region Add Tests

    [Fact]
    public void WhenAddingEntryThenShouldBeInCollection()
    {
        // Arrange
        var collection = new ElementCollection
        {
            // Act
            { "name", "John" }
        };

        // Assert
        collection.Count.Should().Be(1);
        collection.ContainsKey("name").Should().BeTrue();
    }

    [Fact]
    public void WhenAddingMultipleEntriesThenAllShouldBeInCollection()
    {
        // Arrange
        var collection = new ElementCollection
        {
            // Act
            { "name", "John" },
            { "email", "john@example.com" },
            { "age", "30" }
        };

        // Assert
        collection.Count.Should().Be(3);
    }

    [Fact]
    public void WhenAddingDuplicateKeyThenShouldMergeValues()
    {
        // Arrange
        var collection = new ElementCollection
        {
            // Act
            { "errors", "Error 1" },
            { "errors", "Error 2" }
        };

        // Assert
        collection.Count.Should().Be(1);
        var entry = collection["errors"];
        entry!.Value.Values.Count.Should().Be(2);
        entry.Value.Values.Should().Contain("Error 1");
        entry.Value.Values.Should().Contain("Error 2");
    }

    [Fact]
    public void WhenAddingElementEntryThenShouldBeInCollection()
    {
        // Arrange
        var collection = new ElementCollection();
        var entry = new ElementEntry("key", "value1", "value2");

        // Act
        collection.Add(entry);

        // Assert
        collection.Count.Should().Be(1);
        collection["key"]!.Value.Values.Count.Should().Be(2);
    }

    [Fact]
    public void WhenAddingRangeFromDictionaryThenAllShouldBeAdded()
    {
        // Arrange
        var collection = new ElementCollection();
        var dict = new Dictionary<string, string>
        {
            ["key1"] = "value1",
            ["key2"] = "value2"
        };

        // Act
        collection.AddRange(dict);

        // Assert
        collection.Count.Should().Be(2);
    }

    [Fact]
    public void WhenAddingRangeFromStringValuesDictionaryThenAllShouldBeAdded()
    {
        // Arrange
        var collection = new ElementCollection();
        var dict = new Dictionary<string, StringValues>
        {
            ["key1"] = new StringValues(["a", "b"]),
            ["key2"] = new StringValues(["c", "d"])
        };

        // Act
        collection.AddRange(dict);

        // Assert
        collection.Count.Should().Be(2);
        collection["key1"]!.Value.Values.Count.Should().Be(2);
    }

    [Fact]
    public void WhenAddingRangeOfEntriesThenAllShouldBeAdded()
    {
        // Arrange
        var collection = new ElementCollection();
        var entries = new[]
        {
            new ElementEntry("key1", "value1"),
            new ElementEntry("key2", "value2"),
            new ElementEntry("key3", "value3")
        };

        // Act
        collection.AddRange(entries);

        // Assert
        collection.Count.Should().Be(3);
    }

    #endregion

    #region Remove Tests

    [Fact]
    public void WhenRemovingExistingKeyThenShouldReturnTrueAndRemove()
    {
        // Arrange
        var collection = ElementCollection.With("name", "John");

        // Act
        var result = collection.Remove("name");

        // Assert
        result.Should().BeTrue();
        collection.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void WhenRemovingNonExistingKeyThenShouldReturnFalse()
    {
        // Arrange
        var collection = ElementCollection.With("name", "John");

        // Act
        var result = collection.Remove("nonexistent");

        // Assert
        result.Should().BeFalse();
        collection.Count.Should().Be(1);
    }

    [Fact]
    public void WhenRemovingFromMiddleThenShouldMaintainOtherEntries()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "first", "1" },
            { "second", "2" },
            { "third", "3" }
        };

        // Act
        collection.Remove("second");

        // Assert
        collection.Count.Should().Be(2);
        collection.ContainsKey("first").Should().BeTrue();
        collection.ContainsKey("third").Should().BeTrue();
        collection.ContainsKey("second").Should().BeFalse();
    }

    #endregion

    #region TryGetValue and Indexer Tests

    [Fact]
    public void WhenTryGetValueForExistingKeyThenShouldReturnTrueWithEntry()
    {
        // Arrange
        var collection = ElementCollection.With("name", "John");

        // Act
        var result = collection.TryGetValue("name", out var entry);

        // Assert
        result.Should().BeTrue();
        entry.Key.Should().Be("name");
        entry.Values.Should().Contain("John");
    }

    [Fact]
    public void WhenTryGetValueForNonExistingKeyThenShouldReturnFalse()
    {
        // Arrange
        var collection = ElementCollection.With("name", "John");

        // Act
        var result = collection.TryGetValue("nonexistent", out var entry);

        // Assert
        result.Should().BeFalse();
        entry.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void WhenAccessingByIndexerForExistingKeyThenShouldReturnEntry()
    {
        // Arrange
        var collection = ElementCollection.With("name", "John");

        // Act
        var entry = collection["name"];

        // Assert
        entry.Should().NotBeNull();
        entry!.Value.Key.Should().Be("name");
    }

    [Fact]
    public void WhenAccessingByIndexerForNonExistingKeyThenShouldReturnNull()
    {
        // Arrange
        var collection = ElementCollection.With("name", "John");

        // Act
        var entry = collection["nonexistent"];

        // Assert
        entry.Should().BeNull();
    }

    [Fact]
    public void WhenAccessingByIndexerWithNullKeyThenShouldThrow()
    {
        // Arrange
        var collection = new ElementCollection();

        // Act
        var act = () => _ = collection[null!];

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Merge Tests

    [Fact]
    public void WhenMergingCollectionsThenShouldCombineAll()
    {
        // Arrange
        var collection1 = ElementCollection.With("key1", "value1");
        var collection2 = ElementCollection.With("key2", "value2");

        // Act
        collection1.Merge(collection2);

        // Assert
        collection1.Count.Should().Be(2);
        collection1.ContainsKey("key1").Should().BeTrue();
        collection1.ContainsKey("key2").Should().BeTrue();
    }

    [Fact]
    public void WhenMergingWithOverlappingKeysThenShouldMergeValues()
    {
        // Arrange
        var collection1 = ElementCollection.With("errors", "Error 1");
        var collection2 = ElementCollection.With("errors", "Error 2");

        // Act
        collection1.Merge(collection2);

        // Assert
        collection1.Count.Should().Be(1);
        collection1["errors"]!.Value.Values.Count.Should().Be(2);
    }

    [Fact]
    public void WhenMergingEmptyCollectionThenShouldRemainUnchanged()
    {
        // Arrange
        var collection = ElementCollection.With("key", "value");

        // Act
        collection.Merge(ElementCollection.Empty);

        // Assert
        collection.Count.Should().Be(1);
    }

    #endregion

    #region Clear and Copy Tests

    [Fact]
    public void WhenClearingCollectionThenShouldBeEmpty()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        collection.Clear();

        // Assert
        collection.IsEmpty.Should().BeTrue();
        collection.Count.Should().Be(0);
    }

    [Fact]
    public void WhenCopyingCollectionThenShouldCreateIndependentCopy()
    {
        // Arrange
        var original = ElementCollection.With("key", "value");

        // Act
        var copy = original.Copy();
        copy.Add("newKey", "newValue");

        // Assert
        copy.Count.Should().Be(2);
        original.Count.Should().Be(1);
    }

    [Fact]
    public void WhenCopyingEmptyCollectionThenShouldReturnEmpty()
    {
        // Act
        var copy = ElementCollection.Empty.Copy();

        // Assert
        copy.IsEmpty.Should().BeTrue();
    }

    #endregion

    #region Operator Tests

    [Fact]
    public void WhenUsingAdditionOperatorWithEntryThenShouldAddEntry()
    {
        // Arrange
        var collection = ElementCollection.With("key1", "value1");
        var entry = new ElementEntry("key2", "value2");

        // Act
        var result = collection + entry;

        // Assert
        result.Count.Should().Be(2);
        collection.Count.Should().Be(1);
    }

    [Fact]
    public void WhenUsingAdditionOperatorWithCollectionsThenShouldMerge()
    {
        // Arrange
        var left = ElementCollection.With("key1", "value1");
        var right = ElementCollection.With("key2", "value2");

        // Act
        var result = left + right;

        // Assert
        result.Count.Should().Be(2);
        left.Count.Should().Be(1);
        right.Count.Should().Be(1);
    }

    [Fact]
    public void WhenUsingSubtractionOperatorWithKeyThenShouldRemoveKey()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        var result = collection - "key1";

        // Assert
        result.Count.Should().Be(1);
        result.ContainsKey("key1").Should().BeFalse();
        collection.Count.Should().Be(2);
    }

    [Fact]
    public void WhenUsingSubtractionOperatorWithEntryThenShouldRemoveEntry()
    {
        // Arrange
        var collection = ElementCollection.With("key", "value");
        var entry = new ElementEntry("key", "value");

        // Act
        var result = collection - entry;

        // Assert
        result.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void WhenUsingSubtractionOperatorWithCollectionThenShouldRemoveAll()
    {
        // Arrange
        var left = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" }
        };

        var right = ElementCollection.With("key2", "value2");

        // Act
        var result = left - right;

        // Assert
        result.Count.Should().Be(2);
        result.ContainsKey("key2").Should().BeFalse();
    }

    #endregion

    #region Conversion Tests

    [Fact]
    public void WhenConvertingToReadOnlyDictionaryThenShouldContainAllEntries()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        ReadOnlyDictionary<string, StringValues> dict = collection;

        // Assert
        dict.Count.Should().Be(2);
        dict["key1"].Should().Contain("value1");
    }

    [Fact]
    public void WhenConvertingToDictionaryThenShouldContainAllEntries()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2", "value3" }
        };

        // Act
        var dict = collection.ToDictionary();

        // Assert
        dict.Count.Should().Be(2);
        dict["key2"].Count.Should().Be(2);
    }

    [Fact]
    public void WhenConvertingEmptyCollectionToDictionaryThenShouldReturnEmptyDictionary()
    {
        // Act
        var dict = ElementCollection.Empty.ToDictionary();

        // Assert
        dict.Should().BeEmpty();
    }

    #endregion

    #region Enumeration Tests

    [Fact]
    public void WhenEnumeratingCollectionThenShouldYieldAllEntries()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2" },
            { "key3", "value3" }
        };

        // Act
        var keys = collection.Select(e => e.Key).ToList();

        // Assert
        keys.Should().HaveCount(3);
        keys.Should().Contain("key1");
        keys.Should().Contain("key2");
        keys.Should().Contain("key3");
    }

    [Fact]
    public void WhenAccessingKeysThenShouldReturnAllKeys()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        var keys = collection.Keys.ToList();

        // Assert
        keys.Should().HaveCount(2);
        keys.Should().Contain("key1");
        keys.Should().Contain("key2");
    }

    [Fact]
    public void WhenAccessingValuesThenShouldReturnAllValues()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Act
        var values = collection.Values.ToList();

        // Assert
        values.Should().HaveCount(2);
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void WhenCallingToStringOnEmptyCollectionThenShouldReturnEmptyString()
    {
        // Act
        var result = ElementCollection.Empty.ToString();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void WhenCallingToStringThenShouldFormatCorrectly()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "name", "John" },
            { "email", "john@example.com" }
        };

        // Act
        var result = collection.ToString();

        // Assert
        result.Should().Contain("name=John");
        result.Should().Contain("email=john@example.com");
    }

    [Fact]
    public void WhenCallingToDebugStringThenShouldIncludeDetails()
    {
        // Arrange
        var collection = ElementCollection.With("key", "value");

        // Act
        var result = collection.ToDebugString();

        // Assert
        result.Should().Contain("ElementCollection");
        result.Should().Contain("Count = 1");
        result.Should().Contain("key");
    }

    #endregion

    #region JSON Serialization Tests

    [Fact]
    public void WhenSerializingCollectionThenShouldProduceValidJson()
    {
        // Arrange
        var collection = new ElementCollection
        {
            { "name", "John" },
            { "age", "30" }
        };

        // Act
        var json = JsonSerializer.Serialize(collection, ElementCollectionContext.Default.ElementCollection);

        // Assert
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void WhenDeserializingCollectionThenShouldRecreateCollection()
    {
        // Arrange
        var original = new ElementCollection
        {
            { "field1", "value1" },
            { "field2", "value2", "value3" }
        };

        var json = JsonSerializer.Serialize(original, ElementCollectionContext.Default.ElementCollection);

        // Act
        var deserialized = JsonSerializer.Deserialize(json, ElementCollectionContext.Default.ElementCollection);

        // Assert
        deserialized.Count.Should().Be(2);
        deserialized["field2"]!.Value.Values.Count.Should().Be(2);
    }

    #endregion

    #region Real World Scenario Tests

    [Fact]
    public void WhenBuildingValidationErrorsThenShouldAccumulateErrors()
    {
        // Arrange
        var errors = new ElementCollection
        {
            // Act - Simulating form validation
            { "Email", "Email is required" },
            { "Email", "Invalid email format" },
            { "Password", "Password must be at least 8 characters" },
            { "Password", "Password must contain a number" },
            { "ConfirmPassword", "Passwords do not match" }
        };

        // Assert
        errors.Count.Should().Be(3);
        errors["Email"]!.Value.Values.Count.Should().Be(2);
        errors["Password"]!.Value.Values.Count.Should().Be(2);
        errors["ConfirmPassword"]!.Value.Values.Count.Should().Be(1);
    }

    [Fact]
    public void WhenBuildingHttpHeadersThenShouldSupportMultipleValues()
    {
        // Arrange
        var headers = new ElementCollection
        {
            // Act
            { "Accept", "application/json" },
            { "Accept", "text/plain" },
            { "Content-Type", "application/json" },
            { "Authorization", "Bearer token123" }
        };

        // Assert
        headers["Accept"]!.Value.Values.Count.Should().Be(2);
        headers["Content-Type"]!.Value.Values.Should().Contain("application/json");
    }

    [Fact]
    public void WhenBuildingQueryParametersThenShouldAccumulateValues()
    {
        // Arrange
        var queryParams = new ElementCollection
        {
            // Act - Building: ?category=electronics&category=books&sort=price&page=1
            { "category", "electronics" },
            { "category", "books" },
            { "sort", "price" },
            { "page", "1" }
        };

        // Assert
        queryParams["category"]!.Value.Values.Count.Should().Be(2);
        queryParams.Count.Should().Be(3);
    }

    [Fact]
    public void WhenMergingErrorsFromMultipleSourcesThenShouldCombineAll()
    {
        // Arrange
        var frontendErrors = ElementCollection.With("Email", "Invalid format");
        var backendErrors = ElementCollection.With("Email", "Already exists");
        var securityErrors = ElementCollection.With("Password", "Too weak");

        // Act
        var allErrors = frontendErrors + backendErrors + securityErrors;

        // Assert
        allErrors.Count.Should().Be(2);
        allErrors["Email"]!.Value.Values.Count.Should().Be(2);
    }

    [Fact]
    public void WhenFilteringErrorsThenShouldWorkCorrectly()
    {
        // Arrange
        var errors = new ElementCollection
        {
            { "critical_error", "Database connection failed" },
            { "warning", "Cache miss" },
            { "info", "Request processed" }
        };

        // Act
        var criticalOnly = errors.Where(e => e.Key.StartsWith("critical", StringComparison.Ordinal));

        // Assert
        criticalOnly.Should().HaveCount(1);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void WhenAddingWithNullOrEmptyKeyThenShouldThrow()
    {
        // Arrange
        var collection = new ElementCollection();

        // Act
        var actNull = () => collection.Add(null!, "value");
        var actEmpty = () => collection.Add("", "value");

        // Assert
        actNull.Should().Throw<ArgumentNullException>();
        actEmpty.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WhenRemovingWithNullOrEmptyKeyThenShouldThrow()
    {
        // Arrange
        var collection = new ElementCollection();

        // Act
        var actNull = () => collection.Remove(null!);
        var actEmpty = () => collection.Remove("");

        // Assert
        actNull.Should().Throw<ArgumentException>();
        actEmpty.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WhenCollectionInitializerUsedThenShouldWork()
    {
        // Act
        var collection = new ElementCollection
        {
            { "key1", "value1" },
            { "key2", "value2" }
        };

        // Assert
        collection.Count.Should().Be(2);
    }

    #endregion
}
