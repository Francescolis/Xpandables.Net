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
using FluentAssertions;

using Xpandables.Net.Validators;

namespace Xpandables.Net.UnitTests.Validators;

public class SpecificationStaticMethodsTests
{
    [Fact]
    public void Specification_True_ShouldAlwaysReturnTrue()
    {
        // Act
        var spec = Specification.True<TestModel>();
        var testModel = new TestModel { Name = "Test", Age = -100 }; // Even invalid items

        // Assert
        spec.IsSatisfiedBy(testModel).Should().BeTrue();
    }

    [Fact]
    public void Specification_False_ShouldAlwaysReturnFalse()
    {
        // Act
        var spec = Specification.False<TestModel>();
        var testModel = new TestModel { Name = "Valid", Age = 25 }; // Even valid items

        // Assert
        spec.IsSatisfiedBy(testModel).Should().BeFalse();
    }

    [Fact]
    public void Specification_All_WithEmptyArray_ShouldReturnTrue()
    {
        // Act
        var spec = Specification.All<TestModel>();

        // Assert
        var testModel = new TestModel { Name = "Test", Age = 25 };
        spec.IsSatisfiedBy(testModel).Should().BeTrue();
    }

    [Fact]
    public void Specification_Any_WithEmptyArray_ShouldReturnFalse()
    {
        // Act
        var spec = Specification.Any<TestModel>();

        // Assert
        var testModel = new TestModel { Name = "Test", Age = 25 };
        spec.IsSatisfiedBy(testModel).Should().BeFalse();
    }

    [Fact]
    public void Specification_All_WithMultipleSpecs_ShouldCombineWithAnd()
    {
        // Arrange
        var spec1 = new Specification<TestModel>(x => x.Age >= 18);
        var spec2 = new Specification<TestModel>(x => x.Name.Length > 0);
        var spec3 = new Specification<TestModel>(x => !string.IsNullOrEmpty(x.Email));

        // Act
        var combined = Specification.All(spec1, spec2, spec3);

        // Assert
        var validModel = new TestModel { Name = "John", Age = 25, Email = "john@example.com" };
        var invalidModel = new TestModel { Name = "Bob", Age = 16, Email = "bob@example.com" }; // Age < 18

        combined.IsSatisfiedBy(validModel).Should().BeTrue();
        combined.IsSatisfiedBy(invalidModel).Should().BeFalse();
    }

    [Fact]
    public void Specification_Any_WithMultipleSpecs_ShouldCombineWithOr()
    {
        // Arrange
        var spec1 = new Specification<TestModel>(x => x.Age >= 65); // Senior
        var spec2 = new Specification<TestModel>(x => x.Age <= 12); // Child
        var spec3 = new Specification<TestModel>(x => x.Name.StartsWith('V')); // Special name

        // Act
        var combined = Specification.Any(spec1, spec2, spec3);

        // Assert
        var senior = new TestModel { Name = "Bob", Age = 70, Email = "bob@example.com" };
        var child = new TestModel { Name = "Alice", Age = 10, Email = "alice@example.com" };
        var specialName = new TestModel { Name = "Victoria", Age = 30, Email = "victoria@example.com" };
        var regularAdult = new TestModel { Name = "John", Age = 30, Email = "john@example.com" };

        combined.IsSatisfiedBy(senior).Should().BeTrue();
        combined.IsSatisfiedBy(child).Should().BeTrue();
        combined.IsSatisfiedBy(specialName).Should().BeTrue();
        combined.IsSatisfiedBy(regularAdult).Should().BeFalse();
    }

    [Fact]
    public void Specification_All_WithNullArray_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.All<TestModel>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_Any_WithNullArray_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.Any<TestModel>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_Equal_ShouldCheckEquality()
    {
        // Act
        var spec = Specification.Equal<TestModel, int>(x => x.Age, 25);

        // Assert
        var matchingModel = new TestModel { Name = "John", Age = 25 };
        var nonMatchingModel = new TestModel { Name = "Jane", Age = 30 };

        spec.IsSatisfiedBy(matchingModel).Should().BeTrue();
        spec.IsSatisfiedBy(nonMatchingModel).Should().BeFalse();
    }

    [Fact]
    public void Specification_Equal_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.Equal<TestModel, int>(null!, 25);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_NotEqual_ShouldCheckInequality()
    {
        // Act
        var spec = Specification.NotEqual<TestModel, int>(x => x.Age, 25);

        // Assert
        var matchingModel = new TestModel { Name = "John", Age = 30 };
        var nonMatchingModel = new TestModel { Name = "Jane", Age = 25 };

        spec.IsSatisfiedBy(matchingModel).Should().BeTrue();
        spec.IsSatisfiedBy(nonMatchingModel).Should().BeFalse();
    }

    [Fact]
    public void Specification_NotEqual_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.NotEqual<TestModel, int>(null!, 25);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_IsNull_ShouldCheckForNull()
    {
        // Act
        var spec = Specification.IsNull<TestModel, string>(x => x.Email);

        // Assert
        var nullEmailModel = new TestModel { Name = "John", Age = 25, Email = null };
        var nonNullEmailModel = new TestModel { Name = "Jane", Age = 30, Email = "jane@example.com" };

        spec.IsSatisfiedBy(nullEmailModel).Should().BeTrue();
        spec.IsSatisfiedBy(nonNullEmailModel).Should().BeFalse();
    }

    [Fact]
    public void Specification_IsNull_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.IsNull<TestModel, string>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_IsNotNull_ShouldCheckForNotNull()
    {
        // Act
        var spec = Specification.IsNotNull<TestModel, string>(x => x.Email);

        // Assert
        var nullEmailModel = new TestModel { Name = "John", Age = 25, Email = null };
        var nonNullEmailModel = new TestModel { Name = "Jane", Age = 30, Email = "jane@example.com" };

        spec.IsSatisfiedBy(nullEmailModel).Should().BeFalse();
        spec.IsSatisfiedBy(nonNullEmailModel).Should().BeTrue();
    }

    [Fact]
    public void Specification_IsNotNull_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.IsNotNull<TestModel, string>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_Contains_ShouldCheckStringContainment()
    {
        // Act
        var spec = Specification.Contains<TestModel>(x => x.Name, "oh");

        // Assert
        var matchingModel = new TestModel { Name = "John" };
        var nonMatchingModel = new TestModel { Name = "Alice" };

        spec.IsSatisfiedBy(matchingModel).Should().BeTrue();
        spec.IsSatisfiedBy(nonMatchingModel).Should().BeFalse();
    }

    [Fact]
    public void Specification_Contains_WithStringComparison_ShouldUseComparison()
    {
        // Act
        var spec = Specification.Contains<TestModel>(x => x.Name, "JOHN", StringComparison.OrdinalIgnoreCase);

        // Assert
        var matchingModel = new TestModel { Name = "john" };
        var nonMatchingModel = new TestModel { Name = "Alice" };

        spec.IsSatisfiedBy(matchingModel).Should().BeTrue();
        spec.IsSatisfiedBy(nonMatchingModel).Should().BeFalse();
    }

    [Fact]
    public void Specification_Contains_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.Contains<TestModel>(null!, "test");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_Contains_WithNullValue_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.Contains<TestModel>(x => x.Name, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_StartsWith_ShouldCheckStringPrefix()
    {
        // Act
        var spec = Specification.StartsWith<TestModel>(x => x.Name, "Jo");

        // Assert
        var matchingModel = new TestModel { Name = "John" };
        var nonMatchingModel = new TestModel { Name = "Alice" };

        spec.IsSatisfiedBy(matchingModel).Should().BeTrue();
        spec.IsSatisfiedBy(nonMatchingModel).Should().BeFalse();
    }

    [Fact]
    public void Specification_StartsWith_WithStringComparison_ShouldUseComparison()
    {
        // Act
        var spec = Specification.StartsWith<TestModel>(x => x.Name, "JO", StringComparison.OrdinalIgnoreCase);

        // Assert
        var matchingModel = new TestModel { Name = "john" };
        var nonMatchingModel = new TestModel { Name = "Alice" };

        spec.IsSatisfiedBy(matchingModel).Should().BeTrue();
        spec.IsSatisfiedBy(nonMatchingModel).Should().BeFalse();
    }

    [Fact]
    public void Specification_StartsWith_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.StartsWith<TestModel>(null!, "test");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_StartsWith_WithNullValue_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.StartsWith<TestModel>(x => x.Name, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_EndsWith_ShouldCheckStringSuffix()
    {
        // Act
        var spec = Specification.EndsWith<TestModel>(x => x.Name, "hn");

        // Assert
        var matchingModel = new TestModel { Name = "John" };
        var nonMatchingModel = new TestModel { Name = "Alice" };

        spec.IsSatisfiedBy(matchingModel).Should().BeTrue();
        spec.IsSatisfiedBy(nonMatchingModel).Should().BeFalse();
    }

    [Fact]
    public void Specification_EndsWith_WithStringComparison_ShouldUseComparison()
    {
        // Act
        var spec = Specification.EndsWith<TestModel>(x => x.Name, "HN", StringComparison.OrdinalIgnoreCase);

        // Assert
        var matchingModel = new TestModel { Name = "john" };
        var nonMatchingModel = new TestModel { Name = "Alice" };

        spec.IsSatisfiedBy(matchingModel).Should().BeTrue();
        spec.IsSatisfiedBy(nonMatchingModel).Should().BeFalse();
    }

    [Fact]
    public void Specification_EndsWith_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.EndsWith<TestModel>(null!, "test");
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_EndsWith_WithNullValue_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.EndsWith<TestModel>(x => x.Name, null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_GreaterThan_ShouldCheckComparison()
    {
        // Act
        var spec = Specification.GreaterThan<TestModel, int>(x => x.Age, 18);

        // Assert
        var olderModel = new TestModel { Name = "John", Age = 25 };
        var youngerModel = new TestModel { Name = "Alice", Age = 16 };
        var exactAgeModel = new TestModel { Name = "Bob", Age = 18 };

        spec.IsSatisfiedBy(olderModel).Should().BeTrue();
        spec.IsSatisfiedBy(youngerModel).Should().BeFalse();
        spec.IsSatisfiedBy(exactAgeModel).Should().BeFalse();
    }

    [Fact]
    public void Specification_GreaterThan_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.GreaterThan<TestModel, int>(null!, 18);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_LessThan_ShouldCheckComparison()
    {
        // Act
        var spec = Specification.LessThan<TestModel, int>(x => x.Age, 18);

        // Assert
        var youngerModel = new TestModel { Name = "Alice", Age = 16 };
        var olderModel = new TestModel { Name = "John", Age = 25 };
        var exactAgeModel = new TestModel { Name = "Bob", Age = 18 };

        spec.IsSatisfiedBy(youngerModel).Should().BeTrue();
        spec.IsSatisfiedBy(olderModel).Should().BeFalse();
        spec.IsSatisfiedBy(exactAgeModel).Should().BeFalse();
    }

    [Fact]
    public void Specification_LessThan_WithNullSelector_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.LessThan<TestModel, int>(null!, 18);
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("John", "Jo", true)]
    [InlineData("Alice", "Jo", false)]
    [InlineData("John", "john", false)]
    [InlineData("John", "JOHN", false)]
    public void Specification_Contains_WithOrdinalComparison_ShouldBeCaseSensitive(string name, string search, bool expected)
    {
        // Arrange
        var spec = Specification.Contains<TestModel>(x => x.Name, search, StringComparison.Ordinal);
        var model = new TestModel { Name = name };

        // Act
        var result = spec.IsSatisfiedBy(model);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Specification_ComplexCombinationWithStaticMethods_ShouldWorkCorrectly()
    {
        // Arrange
        var ageSpec = Specification.GreaterThan<TestModel, int>(x => x.Age, 18);
        var nameSpec = Specification.IsNotNull<TestModel, string>(x => x.Name);
        var emailSpec = Specification.Contains<TestModel>(x => x.Email ?? "", "@");

        var validAdult = new TestModel { Name = "John", Age = 30, Email = "john@example.com" };
        var invalidAge = new TestModel { Name = "Bob", Age = 16, Email = "bob@example.com" };
        var nullName = new TestModel { Name = null!, Age = 25, Email = "test@example.com" };
        var invalidEmail = new TestModel { Name = "Alice", Age = 25, Email = "invalid" };

        // Act
        var combined = Specification.All(ageSpec, nameSpec, emailSpec);

        // Assert
        combined.IsSatisfiedBy(validAdult).Should().BeTrue();
        combined.IsSatisfiedBy(invalidAge).Should().BeFalse();
        combined.IsSatisfiedBy(nullName).Should().BeFalse();
        combined.IsSatisfiedBy(invalidEmail).Should().BeFalse();
    }
}