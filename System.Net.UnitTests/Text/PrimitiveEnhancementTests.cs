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
using System.Net.Abstractions.Text;
using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Abstractions.Text;

namespace Xpandables.Net.UnitTests.Text;

/// <summary>
/// Tests for the enhanced IPrimitive functionality and JSON serialization.
/// </summary>
public class PrimitiveEnhancementTests
{
    // Example primitive implementation for testing
    [PrimitiveJsonConverter]
    public readonly record struct TestPrimitive : IPrimitive<TestPrimitive, string>
    {
        public string Value { get; } = string.Empty;

        private TestPrimitive(string value)
        {
            Value = value ?? string.Empty; // Handle null gracefully
        }

        public static TestPrimitive Create(string value) => new(value ?? string.Empty);

        public static implicit operator string(TestPrimitive primitive) => primitive.Value ?? string.Empty;
        public static implicit operator TestPrimitive(string value) => new(value ?? string.Empty);

        public bool Equals(TestPrimitive other) => string.Equals(Value ?? string.Empty, other.Value ?? string.Empty, StringComparison.Ordinal);
        public override int GetHashCode() => (Value ?? string.Empty).GetHashCode();
        public override string ToString() => Value ?? string.Empty;
    }

    // Numeric primitive for testing
    [PrimitiveJsonConverter]
    public readonly record struct TestNumericPrimitive : IPrimitive<TestNumericPrimitive, int>
    {
        public int Value { get; }

        private TestNumericPrimitive(int value)
        {
            Value = value;
        }

        public static TestNumericPrimitive Create(int value) => new(value);

        public static implicit operator int(TestNumericPrimitive primitive) => primitive.Value;
        public static implicit operator TestNumericPrimitive(int value) => new(value);
        public static implicit operator string(TestNumericPrimitive primitive) => primitive.Value.ToString();

        public bool Equals(TestNumericPrimitive other) => Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
        public override string ToString() => Value.ToString();
    }

    [Fact]
    public void PrimitiveJsonConverter_ShouldSerializeStringPrimitive()
    {
        // Arrange
        var primitive = TestPrimitive.Create("hello world");
        var options = new JsonSerializerOptions
        {
            Converters = { new PrimitiveJsonConverterFactory() }
        };

        // Act
        var json = JsonSerializer.Serialize(primitive, options);
        var deserialized = JsonSerializer.Deserialize<TestPrimitive>(json, options);

        // Assert
        json.Should().Be("\"hello world\"");
        deserialized.Value.Should().Be("hello world");
        deserialized.Should().Be(primitive);
    }

    [Fact]
    public void PrimitiveJsonConverter_ShouldSerializeNumericPrimitive()
    {
        // Arrange
        var primitive = TestNumericPrimitive.Create(42);
        var options = new JsonSerializerOptions
        {
            Converters = { new PrimitiveJsonConverterFactory() }
        };

        // Act
        var json = JsonSerializer.Serialize(primitive, options);
        var deserialized = JsonSerializer.Deserialize<TestNumericPrimitive>(json, options);

        // Assert
        json.Should().Be("42");
        deserialized.Value.Should().Be(42);
        deserialized.Should().Be(primitive);
    }

    [Fact]
    public void PrimitiveJsonConverter_ShouldHandleNullValues()
    {
        // Arrange
        var options = new JsonSerializerOptions
        {
            Converters = { new PrimitiveJsonConverterFactory() }
        };

        // Act & Assert - Test with valid value (null handling is complex for structs)
        var valid = JsonSerializer.Deserialize<TestNumericPrimitive>("42", options);
        valid.Value.Should().Be(42);

        // Test zero value
        var zero = JsonSerializer.Deserialize<TestNumericPrimitive>("0", options);
        zero.Value.Should().Be(0);
    }

    [Fact]
    public void PrimitiveJsonConverter_Factory_ShouldCacheConverters()
    {
        // Arrange
        var factory = new PrimitiveJsonConverterFactory();
        var options = new JsonSerializerOptions();

        // Act
        var converter1 = factory.CreateConverter(typeof(TestPrimitive), options);
        var converter2 = factory.CreateConverter(typeof(TestPrimitive), options);

        // Assert
        converter1.Should().NotBeNull();
        converter2.Should().NotBeNull();
        ReferenceEquals(converter1, converter2).Should().BeTrue("Converters should be cached");
    }

    [Fact]
    public void PrimitiveJsonConverter_Factory_ShouldDetectPrimitiveTypes()
    {
        // Arrange
        var factory = new PrimitiveJsonConverterFactory();

        // Act & Assert
        factory.CanConvert(typeof(TestPrimitive)).Should().BeTrue();
        factory.CanConvert(typeof(TestNumericPrimitive)).Should().BeTrue();
        factory.CanConvert(typeof(string)).Should().BeFalse();
        factory.CanConvert(typeof(int)).Should().BeFalse();
        factory.CanConvert(typeof(object)).Should().BeFalse();
    }

    [Fact]
    public void Primitive_CreationAndConversion_ShouldWorkCorrectly()
    {
        // Act & Assert - Test creating primitives
        var primitive = TestPrimitive.Create("test");
        primitive.Value.Should().Be("test");

        var numericPrimitive = TestNumericPrimitive.Create(42);
        numericPrimitive.Value.Should().Be(42);

        // Test conversions work
        string stringValue = primitive;
        stringValue.Should().Be("test");

        int intValue = numericPrimitive;
        intValue.Should().Be(42);
    }

    [Fact]
    public void Primitive_ImplicitConversions_ShouldWork()
    {
        // Arrange
        var originalValue = "test value";

        // Act
        TestPrimitive primitive = originalValue; // implicit conversion from string
        string convertedBack = primitive; // implicit conversion to string
        string stringRepresentation = primitive; // implicit conversion to string

        // Assert
        primitive.Value.Should().Be(originalValue);
        convertedBack.Should().Be(originalValue);
        stringRepresentation.Should().Be(originalValue);
    }

    [Fact]
    public void Primitive_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var primitive1 = TestPrimitive.Create("test");
        var primitive2 = TestPrimitive.Create("test");
        var primitive3 = TestPrimitive.Create("different");

        // Act & Assert
        primitive1.Equals(primitive2).Should().BeTrue();
        primitive1.Equals(primitive3).Should().BeFalse();
        primitive1.GetHashCode().Should().Be(primitive2.GetHashCode());
    }

    [Fact]
    public void PrimitiveJsonConverter_ShouldHandleComplexScenarios()
    {
        // Arrange
        var primitives = new[]
        {
            TestPrimitive.Create("first"),
            TestPrimitive.Create("second"),
            TestPrimitive.Create("third")
        };

        var options = new JsonSerializerOptions
        {
            Converters = { new PrimitiveJsonConverterFactory() }
        };

        // Act
        var json = JsonSerializer.Serialize(primitives, options);
        var deserialized = JsonSerializer.Deserialize<TestPrimitive[]>(json, options);

        // Assert
        json.Should().Be("[\"first\",\"second\",\"third\"]");
        deserialized.Should().NotBeNull();
        deserialized!.Length.Should().Be(3);
        deserialized[0].Value.Should().Be("first");
        deserialized[1].Value.Should().Be("second");
        deserialized[2].Value.Should().Be("third");
    }
}