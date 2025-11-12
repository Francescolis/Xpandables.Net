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
using FluentAssertions;

using Xpandables.Net.Optionals;

namespace Xpandables.Net.UnitTests.Optionals;

/// <summary>
/// Unit tests for Optional&lt;T&gt; core functionality including
/// creation, value access, and basic operations.
/// </summary>
public class OptionalCoreTests
{
    #region Creation Tests

    [Fact]
    public void Some_WithValue_ShouldCreateNonEmptyOptional()
    {
        // Arrange & Act
        var optional = Optional.Some("test");

        // Assert
        optional.IsNotEmpty.Should().BeTrue();
        optional.IsEmpty.Should().BeFalse();
        optional.Value.Should().Be("test");
    }

    [Fact]
    public void Some_WithNull_ShouldCreateEmptyOptional()
    {
        // Arrange & Act
        var optional = Optional.Some<string>(null!);

        // Assert
        optional.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Empty_ShouldCreateEmptyOptional()
    {
        // Arrange & Act
        var optional = Optional.Empty<int>();

        // Assert
        optional.IsEmpty.Should().BeTrue();
        optional.IsNotEmpty.Should().BeFalse();
    }

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateOptional()
    {
        // Arrange & Act
        Optional<int> optional = 42;

        // Assert
        optional.IsNotEmpty.Should().BeTrue();
        optional.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromNull_ShouldCreateEmptyOptional()
    {
        // Arrange & Act
        Optional<string> optional = null!;

        // Assert
        optional.IsEmpty.Should().BeTrue();
    }

    #endregion

    #region Value Access Tests

    [Fact]
    public void Value_WhenNotEmpty_ShouldReturnValue()
    {
        // Arrange
        var optional = Optional.Some(123);

        // Act
        int value = optional.Value;

        // Assert
        value.Should().Be(123);
    }

    [Fact]
    public void Value_WhenEmpty_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var optional = Optional.Empty<string>();

        // Act
        Action act = () => _ = optional.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Value is not present.");
    }

    [Fact]
    public void GetValueOrDefault_WithDefaultValue_WhenNotEmpty_ShouldReturnValue()
    {
        // Arrange
        var optional = Optional.Some(10);

        // Act
        int result = optional.GetValueOrDefault(999);

        // Assert
        result.Should().Be(10);
    }

    [Fact]
    public void GetValueOrDefault_WithDefaultValue_WhenEmpty_ShouldReturnDefault()
    {
        // Arrange
        var optional = Optional.Empty<int>();

        // Act
        int result = optional.GetValueOrDefault(999);

        // Assert
        result.Should().Be(999);
    }

    [Fact]
    public void GetValueOrDefault_WithFactory_WhenNotEmpty_ShouldReturnValue()
    {
        // Arrange
        var optional = Optional.Some("actual");
        bool factoryCalled = false;

        // Act
        string result = optional.GetValueOrDefault(() =>
        {
            factoryCalled = true;
            return "default";
        });

        // Assert
        result.Should().Be("actual");
        factoryCalled.Should().BeFalse();
    }

    [Fact]
    public void GetValueOrDefault_WithFactory_WhenEmpty_ShouldInvokeFactory()
    {
        // Arrange
        var optional = Optional.Empty<string>();
        bool factoryCalled = false;

        // Act
        string result = optional.GetValueOrDefault(() =>
        {
            factoryCalled = true;
            return "from factory";
        });

        // Assert
        result.Should().Be("from factory");
        factoryCalled.Should().BeTrue();
    }

    [Fact]
    public void GetValueOrDefault_WithNullFactory_ShouldThrowArgumentNullException()
    {
        // Arrange
        var optional = Optional.Some(42);

        // Act
        Action act = () => optional.GetValueOrDefault(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Enumerable Tests

    [Fact]
    public void GetEnumerator_WhenNotEmpty_ShouldReturnSingleElement()
    {
        // Arrange
        var optional = Optional.Some(42);

        // Act
        var list = optional.ToList();

        // Assert
        list.Should().HaveCount(1);
        list[0].Should().Be(42);
    }

    [Fact]
    public void GetEnumerator_WhenEmpty_ShouldReturnEmptySequence()
    {
        // Arrange
        var optional = Optional.Empty<int>();

        // Act
        var list = optional.ToList();

        // Assert
        list.Should().BeEmpty();
    }

    [Fact]
    public void Foreach_WhenNotEmpty_ShouldIterateOnce()
    {
        // Arrange
        var optional = Optional.Some("value");
        int count = 0;
        string? capturedValue = null;

        // Act
        foreach (var value in optional)
        {
            count++;
            capturedValue = value;
        }

        // Assert
        count.Should().Be(1);
        capturedValue.Should().Be("value");
    }

    [Fact]
    public void Foreach_WhenEmpty_ShouldNotIterate()
    {
        // Arrange
        var optional = Optional.Empty<string>();
        int count = 0;

        // Act
        foreach (var _ in optional)
        {
            count++;
        }

        // Assert
        count.Should().Be(0);
    }

    #endregion

    #region Equality and Comparison Tests

    [Fact]
    public void Equals_TwoEmptyOptionals_ShouldBeEqual()
    {
        // Arrange
        var optional1 = Optional.Empty<int>();
        var optional2 = Optional.Empty<int>();

        // Act & Assert
        optional1.Equals(optional2).Should().BeTrue();
        (optional1 == optional2).Should().BeTrue();
    }

    [Fact]
    public void Equals_TwoOptionalsWithSameValue_ShouldBeEqual()
    {
        // Arrange
        var optional1 = Optional.Some(42);
        var optional2 = Optional.Some(42);

        // Act & Assert
        optional1.Equals(optional2).Should().BeTrue();
        (optional1 == optional2).Should().BeTrue();
    }

    [Fact]
    public void Equals_TwoOptionalsWithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var optional1 = Optional.Some(42);
        var optional2 = Optional.Some(43);

        // Act & Assert
        optional1.Equals(optional2).Should().BeFalse();
        (optional1 != optional2).Should().BeTrue();
    }

    [Fact]
    public void Equals_EmptyAndNonEmpty_ShouldNotBeEqual()
    {
        // Arrange
        var optional1 = Optional.Empty<int>();
        var optional2 = Optional.Some(42);

        // Act & Assert
        optional1.Equals(optional2).Should().BeFalse();
    }

    [Fact]
    public void Equals_OptionalWithValue_ShouldEqualValue()
    {
        // Arrange
        var optional = Optional.Some("test");

        // Act & Assert
        optional.Equals("test").Should().BeTrue();
    }

    [Fact]
    public void Equals_OptionalEmpty_ShouldNotEqualValue()
    {
        // Arrange
        var optional = Optional.Empty<string>();

        // Act & Assert
        optional.Equals("test").Should().BeFalse();
    }

    [Fact]
    public void CompareTo_EmptyOptionals_ShouldBeEqual()
    {
        // Arrange
        var optional1 = Optional.Empty<int>();
        var optional2 = Optional.Empty<int>();

        // Act
        int result = optional1.CompareTo(optional2);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void CompareTo_EmptyVsNonEmpty_EmptyShouldBeLess()
    {
        // Arrange
        var empty = Optional.Empty<int>();
        var nonEmpty = Optional.Some(5);

        // Act
        int result = empty.CompareTo(nonEmpty);

        // Assert
        result.Should().BeLessThan(0);
    }

    [Fact]
    public void CompareTo_NonEmptyVsEmpty_NonEmptyShouldBeGreater()
    {
        // Arrange
        var nonEmpty = Optional.Some(5);
        var empty = Optional.Empty<int>();

        // Act
        int result = nonEmpty.CompareTo(empty);

        // Assert
        result.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ComparisonOperators_ShouldWorkCorrectly()
    {
        // Arrange
        var opt1 = Optional.Some(5);
        var opt2 = Optional.Some(10);
        var empty = Optional.Empty<int>();

        // Act & Assert
        (opt1 < opt2).Should().BeTrue();
        (opt1 <= opt2).Should().BeTrue();
        (opt2 > opt1).Should().BeTrue();
        (opt2 >= opt1).Should().BeTrue();
        (empty < opt1).Should().BeTrue();
        (opt1 > empty).Should().BeTrue();
    }

    #endregion

    #region ToString Tests

    [Fact]
    public void ToString_WhenNotEmpty_ShouldReturnValueString()
    {
        // Arrange
        var optional = Optional.Some(42);

        // Act
        string result = optional.ToString();

        // Assert
        result.Should().Be("42");
    }

    [Fact]
    public void ToString_WhenEmpty_ShouldReturnEmptyString()
    {
        // Arrange
        var optional = Optional.Empty<int>();

        // Act
        string result = optional.ToString();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void ToString_WithFormat_WhenNotEmpty_ShouldFormatValue()
    {
        // Arrange
        var optional = Optional.Some(123.456);
        var format = "F2";

        // Act
        string result = optional.ToString(format, null);

        // Assert
        result.Should().Be("123.46");
    }

    [Fact]
    public void ToString_WithFormat_WhenEmpty_ShouldReturnEmptyString()
    {
        // Arrange
        var optional = Optional.Empty<double>();

        // Act
        string result = optional.ToString("F2", null);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetHashCode Tests

    [Fact]
    public void GetHashCode_ForEmptyOptional_ShouldBeConsistent()
    {
        // Arrange
        var optional1 = Optional.Empty<int>();
        var optional2 = Optional.Empty<int>();

        // Act
        int hash1 = optional1.GetHashCode();
        int hash2 = optional2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_ForOptionalsWithSameValue_ShouldBeEqual()
    {
        // Arrange
        var optional1 = Optional.Some("test");
        var optional2 = Optional.Some("test");

        // Act
        int hash1 = optional1.GetHashCode();
        int hash2 = optional2.GetHashCode();

        // Assert
        hash1.Should().Be(hash2);
    }

    [Fact]
    public void GetHashCode_ForOptionalsWithDifferentValues_ShouldBeDifferent()
    {
        // Arrange
        var optional1 = Optional.Some("test1");
        var optional2 = Optional.Some("test2");

        // Act
        int hash1 = optional1.GetHashCode();
        int hash2 = optional2.GetHashCode();

        // Assert
        hash1.Should().NotBe(hash2);
    }

    #endregion

    #region Complex Type Tests

    [Fact]
    public void Optional_WithReferenceType_ShouldWorkCorrectly()
    {
        // Arrange
        var person = new Person { Name = "John", Age = 30 };
        var optional = Optional.Some(person);

        // Act & Assert
        optional.IsNotEmpty.Should().BeTrue();
        optional.Value.Should().BeSameAs(person);
        optional.Value.Name.Should().Be("John");
    }

    [Fact]
    public void Optional_WithValueType_ShouldWorkCorrectly()
    {
        // Arrange
        var point = new Point { X = 10, Y = 20 };
        var optional = Optional.Some(point);

        // Act & Assert
        optional.IsNotEmpty.Should().BeTrue();
        optional.Value.X.Should().Be(10);
        optional.Value.Y.Should().Be(20);
    }

    [Fact]
    public void Optional_NestedOptional_ShouldHandleCorrectly()
    {
        // Arrange
        var inner = Optional.Some(42);
        var nested = Optional.Some(inner);

        // Act - Access inner optional
        var hasValue = nested.IsNotEmpty && nested.Value.IsNotEmpty;
        var innerValue = nested.IsNotEmpty ? nested.Value.Value : 0;

        // Assert
        hasValue.Should().BeTrue();
        innerValue.Should().Be(42);
    }

    #endregion

    #region Test Support Types

    private class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    #endregion
}
