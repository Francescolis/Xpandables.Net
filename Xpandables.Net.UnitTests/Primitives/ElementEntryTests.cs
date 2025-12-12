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
using System.Text.Json;

using FluentAssertions;

using Microsoft.Extensions.Primitives;

namespace Xpandables.Net.UnitTests.Primitives;

public sealed class ElementEntryTests
{
    #region Constructor Tests

    [Fact]
    public void WhenCreatingWithKeyAndValuesThenShouldSetProperties()
    {
        // Arrange & Act
        var entry = new ElementEntry("name", "John", "Jane");

        // Assert
        entry.Key.Should().Be("name");
        entry.Values.Count.Should().Be(2);
        entry.Values.Should().Contain("John");
        entry.Values.Should().Contain("Jane");
    }

    [Fact]
    public void WhenCreatingWithKeyAndSingleValueThenShouldSetProperties()
    {
        // Arrange & Act
        var entry = new ElementEntry("email", "test@example.com");

        // Assert
        entry.Key.Should().Be("email");
        entry.Values.Count.Should().Be(1);
        entry.Values.Should().Contain("test@example.com");
    }

    [Fact]
    public void WhenCreatingWithKeyAndStringValuesThenShouldSetProperties()
    {
        // Arrange
        StringValues values = new(["value1", "value2", "value3"]);

        // Act
        var entry = new ElementEntry("key", values);

        // Assert
        entry.Key.Should().Be("key");
        entry.Values.Count.Should().Be(3);
    }

    [Fact]
    public void WhenCreatingWithNullKeyThenShouldThrowArgumentNullException()
    {
        // Act
        var act = () => new ElementEntry(null!, "value");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void WhenCreatingWithNullValuesThenShouldThrowArgumentException()
    {
        // Act
        var act = () => new ElementEntry("key", (string[])null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void WhenCreatingWithEmptyValueArrayThenShouldThrowArgumentException()
    {
        // Arrange
        string[] emptyArray = [];
        
        // Act
        var act = () => new ElementEntry("key", emptyArray);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public void WhenCreatingWithEmptyStringValuesThenShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        StringValues values = new(Array.Empty<string>());

        // Act
        var act = () => new ElementEntry("key", values);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void WhenComparingEqualEntriesThenShouldBeEqual()
    {
        // Arrange
        var entry1 = new ElementEntry("key", "value1", "value2");
        var entry2 = new ElementEntry("key", "value1", "value2");

        // Assert
        entry1.Should().Be(entry2);
        (entry1 == entry2).Should().BeTrue();
    }

    [Fact]
    public void WhenComparingEntriesWithDifferentKeysThenShouldNotBeEqual()
    {
        // Arrange
        var entry1 = new ElementEntry("key1", "value");
        var entry2 = new ElementEntry("key2", "value");

        // Assert
        entry1.Should().NotBe(entry2);
        (entry1 != entry2).Should().BeTrue();
    }

    [Fact]
    public void WhenComparingEntriesWithDifferentValuesThenShouldNotBeEqual()
    {
        // Arrange
        var entry1 = new ElementEntry("key", "value1");
        var entry2 = new ElementEntry("key", "value2");

        // Assert
        entry1.Should().NotBe(entry2);
    }

    [Fact]
    public void WhenGettingHashCodeForEqualEntriesThenShouldBeEqual()
    {
        // Arrange
        var entry1 = new ElementEntry("key", "value");
        var entry2 = new ElementEntry("key", "value");

        // Assert
        entry1.GetHashCode().Should().Be(entry2.GetHashCode());
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void WhenCallingToStringWithSingleValueThenShouldFormatCorrectly()
    {
        // Arrange
        var entry = new ElementEntry("name", "John");

        // Act
        var result = entry.ToString();

        // Assert
        result.Should().Be("name: John");
    }

    [Fact]
    public void WhenCallingToStringWithMultipleValuesThenShouldJoinWithComma()
    {
        // Arrange
        var entry = new ElementEntry("colors", "red", "green", "blue");

        // Act
        var result = entry.ToString();

        // Assert
        result.Should().Be("colors: red,green,blue");
    }

    #endregion

    #region JSON Serialization Tests

    [Fact]
    public void WhenSerializingElementEntryThenShouldProduceValidJson()
    {
        // Arrange
        var entry = new ElementEntry("name", "John");

        // Act
        var json = JsonSerializer.Serialize(entry, ElementEntryContext.Default.ElementEntry);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("name");
        json.Should().Contain("John");
    }

    [Fact]
    public void WhenDeserializingElementEntryThenShouldRecreateEntry()
    {
        // Arrange
        var original = new ElementEntry("email", "test@example.com");
        var json = JsonSerializer.Serialize(original, ElementEntryContext.Default.ElementEntry);

        // Act
        var deserialized = JsonSerializer.Deserialize(json, ElementEntryContext.Default.ElementEntry);

        // Assert
        deserialized.Key.Should().Be(original.Key);
        deserialized.Values.Should().BeEquivalentTo(original.Values);
    }

    [Fact]
    public void WhenSerializingElementEntryArrayThenShouldProduceValidJson()
    {
        // Arrange
        var entries = new[]
        {
            new ElementEntry("name", "John"),
            new ElementEntry("age", "30")
        };

        // Act
        var json = JsonSerializer.Serialize(entries, ElementEntryContext.Default.ElementEntryArray);

        // Assert
        json.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void WhenDeserializingElementEntryArrayThenShouldRecreateAll()
    {
        // Arrange
        var original = new[]
        {
            new ElementEntry("field1", "value1"),
            new ElementEntry("field2", "value2", "value3")
        };
        var json = JsonSerializer.Serialize(original, ElementEntryContext.Default.ElementEntryArray);

        // Act
        var deserialized = JsonSerializer.Deserialize(json, ElementEntryContext.Default.ElementEntryArray);

        // Assert
        deserialized.Should().HaveCount(2);
        deserialized![0].Key.Should().Be("field1");
        deserialized[1].Values.Count.Should().Be(2);
    }

    [Fact]
    public void WhenRoundTrippingElementEntryWithMultipleValuesThenShouldPreserveAll()
    {
        // Arrange
        var original = new ElementEntry("errors", "Error 1", "Error 2", "Error 3");

        // Act
        var json = JsonSerializer.Serialize(original, ElementEntryContext.Default.ElementEntry);
        var deserialized = JsonSerializer.Deserialize(json, ElementEntryContext.Default.ElementEntry);

        // Assert
        deserialized.Key.Should().Be("errors");
        deserialized.Values.Count.Should().Be(3);
        deserialized.Values.Should().Contain("Error 1");
        deserialized.Values.Should().Contain("Error 2");
        deserialized.Values.Should().Contain("Error 3");
    }

    #endregion

    #region StringValues Integration Tests

    [Fact]
    public void WhenAccessingValuesAsStringValuesThenShouldWorkCorrectly()
    {
        // Arrange
        var entry = new ElementEntry("tags", "tag1", "tag2", "tag3");

        // Act
        StringValues values = entry.Values;

        // Assert
        values.Count.Should().Be(3);
        values[0].Should().Be("tag1");
        values[1].Should().Be("tag2");
        values[2].Should().Be("tag3");
    }

    [Fact]
    public void WhenIteratingOverValuesThenShouldEnumerateAll()
    {
        // Arrange
        var entry = new ElementEntry("items", "a", "b", "c");
        var collected = new List<string?>();

        // Act
        foreach (var value in entry.Values)
        {
            collected.Add(value);
        }

        // Assert
        collected.Should().BeEquivalentTo(["a", "b", "c"]);
    }

    [Fact]
    public void WhenConvertingValuesToArrayThenShouldReturnArray()
    {
        // Arrange
        var entry = new ElementEntry("numbers", "1", "2", "3");

        // Act
        var array = entry.Values.ToArray();

        // Assert
        array.Should().BeEquivalentTo(["1", "2", "3"]);
    }

    #endregion

    #region Record Struct Tests

    [Fact]
    public void WhenUsingWithExpressionThenShouldCreateNewInstance()
    {
        // Arrange
        var original = new ElementEntry("key", "value1");

        // Act
        var modified = original with { Key = "newKey" };

        // Assert
        modified.Key.Should().Be("newKey");
        modified.Values.Should().BeEquivalentTo(original.Values);
        original.Key.Should().Be("key");
    }

    [Fact]
    public void WhenAccessingKeyAndValuesThenShouldReturnCorrectData()
    {
        // Arrange
        var entry = new ElementEntry("name", "John");

        // Act & Assert
        entry.Key.Should().Be("name");
        entry.Values.Should().Contain("John");
    }

    #endregion

    #region Real World Scenario Tests

    [Fact]
    public void WhenRepresentingValidationErrorsThenShouldStoreMultipleMessages()
    {
        // Arrange & Act
        var entry = new ElementEntry(
            "Password",
            "Must be at least 8 characters",
            "Must contain a number",
            "Must contain a special character");

        // Assert
        entry.Key.Should().Be("Password");
        entry.Values.Count.Should().Be(3);
    }

    [Fact]
    public void WhenRepresentingHttpHeaderThenShouldStoreMultipleValues()
    {
        // Arrange & Act
        var entry = new ElementEntry(
            "Accept",
            "application/json",
            "text/plain",
            "text/html");

        // Assert
        entry.Key.Should().Be("Accept");
        entry.Values.Count.Should().Be(3);
        entry.ToString().Should().Be("Accept: application/json,text/plain,text/html");
    }

    [Fact]
    public void WhenRepresentingFormFieldThenShouldStoreSingleValue()
    {
        // Arrange & Act
        var entry = new ElementEntry("username", "john_doe");

        // Assert
        entry.Key.Should().Be("username");
        entry.Values.Count.Should().Be(1);
        ((string)entry.Values).Should().Be("john_doe");
    }

    [Fact]
    public void WhenRepresentingQueryParameterWithMultipleValuesThenShouldStoreAll()
    {
        // Arrange - Query string like ?category=electronics&category=books&category=games
        var entry = new ElementEntry("category", "electronics", "books", "games");

        // Act
        var values = entry.Values.ToArray();
        var queryPart = $"{entry.Key}={string.Join($"&{entry.Key}=", values!)}";

        // Assert
        queryPart.Should().Be("category=electronics&category=books&category=games");
    }

    [Fact]
    public void WhenBuildingErrorResponseThenShouldFormatCorrectly()
    {
        // Arrange
        var errors = new[]
        {
            new ElementEntry("Email", "Invalid email format"),
            new ElementEntry("Password", "Too short", "Missing number"),
            new ElementEntry("Username", "Already taken")
        };

        // Act
        var errorDict = errors.ToDictionary(
            e => e.Key,
            e => e.Values.ToArray());

        // Assert
        errorDict.Should().ContainKey("Email");
        errorDict.Should().ContainKey("Password");
        errorDict.Should().ContainKey("Username");
        errorDict["Password"].Should().HaveCount(2);
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public void WhenValueContainsSpecialCharactersThenShouldPreserve()
    {
        // Arrange
        var entry = new ElementEntry("data", "value with spaces", "value,with,commas", "value\"with\"quotes");

        // Assert
        entry.Values.Count.Should().Be(3);
        entry.Values.Should().Contain("value with spaces");
        entry.Values.Should().Contain("value,with,commas");
        entry.Values.Should().Contain("value\"with\"quotes");
    }

    [Fact]
    public void WhenKeyContainsSpecialCharactersThenShouldPreserve()
    {
        // Arrange
        var entry = new ElementEntry("special-key_name.test", "value");

        // Assert
        entry.Key.Should().Be("special-key_name.test");
    }

    [Fact]
    public void WhenValueIsEmptyStringThenShouldBeValid()
    {
        // Arrange & Act
        var entry = new ElementEntry("emptyValue", "");

        // Assert
        entry.Values.Count.Should().Be(1);
        ((string)entry.Values).Should().BeEmpty();
    }

    [Fact]
    public void WhenCreatingManyEntriesThenShouldAllBeIndependent()
    {
        // Arrange
        var entries = Enumerable.Range(1, 100)
            .Select(i => new ElementEntry($"key{i}", $"value{i}"))
            .ToArray();

        // Assert
        entries.Should().HaveCount(100);
        entries.Select(e => e.Key).Should().OnlyHaveUniqueItems();
    }

    #endregion
}
