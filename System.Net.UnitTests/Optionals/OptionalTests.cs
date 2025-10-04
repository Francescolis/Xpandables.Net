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
using System.Net.Optionals;
using System.Text.Json;

using FluentAssertions;

namespace System.Net.UnitTests.Optionals;

/// <summary>
/// Tests for the Optional functionality including JSON serialization.
/// </summary>
public class OptionalTests
{
    [Fact]
    public void OptionalExtensions_TryGetValue_ShouldWorkCorrectly()
    {
        // Arrange
        var some = Optional.Some("test");
        var none = Optional.Empty<string>();

        // Act & Assert
        some.TryGetValue(out var value1).Should().BeTrue();
        value1.Should().Be("test");

        none.TryGetValue(out var value2).Should().BeFalse();
        value2.Should().BeNull();
    }

    [Fact]
    public void OptionalExtensions_GetValueOrDefault_ShouldWorkCorrectly()
    {
        // Arrange
        var some = Optional.Some("test");
        var none = Optional.Empty<string>();

        // Act & Assert
        some.GetValueOrDefault("default").Should().Be("test");
        none.GetValueOrDefault("default").Should().Be("default");

        some.GetValueOrDefault(() => "factory").Should().Be("test");
        none.GetValueOrDefault(() => "factory").Should().Be("factory");
    }

    [Fact]
    public void OptionalExtensions_Filter_ShouldFilterBasedOnPredicate()
    {
        // Arrange
        var evenNumber = Optional.Some(4);
        var oddNumber = Optional.Some(5);
        var none = Optional.Empty<int>();

        // Act
        var filteredEven = evenNumber.Where(x => x % 2 == 0);
        var filteredOdd = oddNumber.Where(x => x % 2 == 0);
        var filteredNone = none.Where(x => x % 2 == 0);

        // Assert
        filteredEven.IsNotEmpty.Should().BeTrue();
        filteredEven.Value.Should().Be(4);

        filteredOdd.IsEmpty.Should().BeTrue();
        filteredNone.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void OptionalExtensions_DoIfPresent_And_DoIfEmpty_ShouldExecuteCorrectly()
    {
        // Arrange
        var some = Optional.Some("test");
        var none = Optional.Empty<string>();
        var executed = false;
        var emptyExecuted = false;

        // Act
        some.Map(val => executed = val == "test");
        none.Empty(() => emptyExecuted = true);

        // Assert
        executed.Should().BeTrue();
        emptyExecuted.Should().BeTrue();
    }

    [Fact]
    public void OptionalExtensions_FirstOrEmpty_ShouldWorkCorrectly()
    {
        // Arrange
        var sequence = new[] { 1, 2, 3 };
        var emptySequence = Array.Empty<int>();

        // Act & Assert
        sequence.FirstOrEmpty().IsNotEmpty.Should().BeTrue();
        sequence.FirstOrEmpty().Value.Should().Be(1);

        emptySequence.FirstOrEmpty().IsEmpty.Should().BeTrue();

        sequence.Where(x => x > 2).FirstOrEmpty().Value.Should().Be(3);
        sequence.Where(x => x > 5).FirstOrEmpty().IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void OptionalExtensions_WhereSome_ShouldFilterNonEmptyValues()
    {
        // Arrange
        var optionals = new[]
        {
            Optional.Some(1),
            Optional.Empty<int>(),
            Optional.Some(2),
            Optional.Empty<int>(),
            Optional.Some(3)
        };

        // Act
        var values = optionals.WhereSome().ToArray();

        // Assert
        values.Should().Equal(1, 2, 3);
    }

    [Fact]
    public void OptionalJsonSerialization_BasicTypes_ShouldSerializeCorrectly()
    {
        // Arrange
        var someString = Optional.Some("hello");
        var noneString = Optional.Empty<string>();
        var someInt = Optional.Some(42);
        var noneInt = Optional.Empty<int>();

        // Use default options without source generation to avoid type issues
        var options = new JsonSerializerOptions
        {
            Converters = { new OptionalJsonConverterFactory() }
        };

        // Act
        var someStringJson = JsonSerializer.Serialize(someString, options);
        var noneStringJson = JsonSerializer.Serialize(noneString, options);
        var someIntJson = JsonSerializer.Serialize(someInt, options);
        var noneIntJson = JsonSerializer.Serialize(noneInt, options);

        // Assert
        someStringJson.Should().Be("\"hello\"");
        noneStringJson.Should().Be("null");
        someIntJson.Should().Be("42");
        noneIntJson.Should().Be("null");
    }

    [Fact]
    public void OptionalJsonSerialization_ShouldDeserializeCorrectly()
    {
        // Arrange
        var stringJson = "\"hello\"";
        var nullJson = "null";
        var intJson = "42";

        // Use default JsonSerializer options without source generation for this test
        var options = new JsonSerializerOptions
        {
            Converters = { new OptionalJsonConverterFactory() }
        };

        // Act
        var deserializedString = JsonSerializer.Deserialize<Optional<string>>(stringJson, options);
        var deserializedNull = JsonSerializer.Deserialize<Optional<string>>(nullJson, options);
        var deserializedInt = JsonSerializer.Deserialize<Optional<int>>(intJson, options);

        // Assert
        deserializedString.IsNotEmpty.Should().BeTrue();
        deserializedString.Value.Should().Be("hello");

        deserializedNull.IsEmpty.Should().BeTrue();

        deserializedInt.IsNotEmpty.Should().BeTrue();
        deserializedInt.Value.Should().Be(42);
    }

    [Fact]
    public void OptionalJsonSerialization_WithComplexTypes_ShouldWorkCorrectly()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };
        var optionalPerson = Optional.Some(person);
        var emptyPerson = Optional.Empty<Person>();

        var options = new JsonSerializerOptions
        {
            Converters = { new OptionalJsonConverterFactory() }
        };

        // Act
        var json1 = JsonSerializer.Serialize(optionalPerson, options);
        var json2 = JsonSerializer.Serialize(emptyPerson, options);

        var restored1 = JsonSerializer.Deserialize<Optional<Person>>(json1, options);
        var restored2 = JsonSerializer.Deserialize<Optional<Person>>(json2, options);

        // Assert
        json1.Should().Contain("John").And.Contain("30");
        json2.Should().Be("null");

        restored1.IsNotEmpty.Should().BeTrue();
        restored1.Value.Name.Should().Be("John");
        restored1.Value.Age.Should().Be(30);

        restored2.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void OptionalExtensions_ParameterValidation_ShouldThrowOnNull()
    {
        // Arrange
        var optional = Optional.Some("test");

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => optional.Where(null!));
        Assert.Throws<ArgumentNullException>(() => optional.GetValueOrDefault((Func<string>)null!));
        Assert.Throws<ArgumentNullException>(() => optional.Empty((Func<Optional<string>>)null!));
    }

    [Fact]
    public void OptionalEnhancedExtensions_IntegrationWithExistingApi_ShouldWork()
    {
        // Test that the enhanced extensions work with existing Optional methods
        var optional = Optional.Some(10);

        // Use existing methods
        var mapped = optional.Map(x => x * 2);
        var bound = optional.Bind(x => Optional.Some(x.ToString()));

        // Use enhanced extensions
        var filtered = mapped.Where(x => x > 15);
        var defaulted = filtered.GetValueOrDefault(0);

        // Assert
        mapped.Value.Should().Be(20);
        bound.Value.Should().Be("10");
        filtered.Value.Should().Be(20);
        defaulted.Should().Be(20);
    }

    private record Person
    {
        public string Name { get; init; } = string.Empty;
        public int Age { get; init; }
    }
}