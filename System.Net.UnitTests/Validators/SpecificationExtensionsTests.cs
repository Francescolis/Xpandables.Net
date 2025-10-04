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
using System.Linq.Expressions;
using System.Net.Validators;

using FluentAssertions;

namespace System.Net.UnitTests.Validators;

public class SpecificationExtensionsTests
{
    private readonly List<TestModel> _testData = [
        new() { Name = "Alice", Age = 25, Email = "alice@example.com" },
        new() { Name = "Bob", Age = 17, Email = "bob@example.com" },
        new() { Name = "Charlie", Age = 30, Email = "charlie@example.com" },
        new() { Name = "Diana", Age = 16, Email = "diana@example.com" },
        new() { Name = "Eve", Age = 22, Email = "eve@example.com" }
    ];

    [Fact]
    public void ToSpecification_ShouldConvertExpressionToSpecification()
    {
        // Arrange
        Expression<Func<TestModel, bool>> expression = x => x.Age >= 18;

        // Act
        var specification = expression.ToSpecification();

        // Assert
        specification.Should().NotBeNull();
        specification.Should().BeAssignableTo<ISpecification<TestModel>>();
        specification.Expression.Should().Be(expression);
    }

    [Fact]
    public void ToSpecification_WithNullExpression_ShouldThrowArgumentNullException()
    {
        // Arrange
        Expression<Func<TestModel, bool>> expression = null!;

        // Act & Assert
        var act = () => expression.ToSpecification();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_And_Extension_ShouldCombineSpecifications()
    {
        // Arrange
        Specification<TestModel> spec1 = new(x => x.Age >= 18);
        Specification<TestModel> spec2 = new(x => x.Name.Length > 3);

        // Act
        var combined = spec1.And(spec2);

        // Assert
        combined.Should().NotBeNull();
        combined.IsSatisfiedBy(new TestModel { Name = "Alice", Age = 25 }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestModel { Name = "Bob", Age = 17 }).Should().BeFalse();
        combined.IsSatisfiedBy(new TestModel { Name = "Al", Age = 25 }).Should().BeFalse();
    }

    [Fact]
    public void Specification_Or_Extension_ShouldCombineSpecifications()
    {
        // Arrange
        Specification<TestModel> spec1 = new(x => x.Age >= 65);
        Specification<TestModel> spec2 = new(x => x.Age <= 12);

        // Act
        var combined = spec1.Or(spec2);

        // Assert
        combined.Should().NotBeNull();
        combined.IsSatisfiedBy(new TestModel { Name = "Senior", Age = 70 }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestModel { Name = "Child", Age = 10 }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestModel { Name = "Adult", Age = 30 }).Should().BeFalse();
    }

    [Fact]
    public void Specification_OrElse_Extension_ShouldCombineSpecifications()
    {
        // Arrange
        Specification<TestModel> spec1 = new(x => x.Age >= 65);
        Specification<TestModel> spec2 = new(x => x.Age <= 12);

        // Act
        var combined = spec1.OrElse(spec2);

        // Assert
        combined.Should().NotBeNull();
        combined.IsSatisfiedBy(new TestModel { Name = "Senior", Age = 70 }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestModel { Name = "Child", Age = 10 }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestModel { Name = "Adult", Age = 30 }).Should().BeFalse();
    }

    [Fact]
    public void Specification_Not_Extension_ShouldNegateSpecification()
    {
        // Arrange
        Specification<TestModel> spec = new(x => x.Age >= 18);

        // Act
        var negated = spec.Not();

        // Assert
        negated.Should().NotBeNull();
        negated.IsSatisfiedBy(new TestModel { Name = "Adult", Age = 25 }).Should().BeFalse();
        negated.IsSatisfiedBy(new TestModel { Name = "Minor", Age = 16 }).Should().BeTrue();
    }

    [Fact]
    public void IQueryable_Where_WithSpecification_ShouldFilterCorrectly()
    {
        // Arrange
        var queryable = _testData.AsQueryable();
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 18);

        // Act
        var results = queryable.Where(specification).ToList();

        // Assert
        results.Should().HaveCount(3);
        results.Should().Contain(x => x.Name == "Alice");
        results.Should().Contain(x => x.Name == "Charlie");
        results.Should().Contain(x => x.Name == "Eve");
    }

    [Fact]
    public void IEnumerable_Where_WithSpecification_ShouldFilterCorrectly()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 18);

        // Act
        var results = _testData.Where(specification).ToList();

        // Assert
        results.Should().HaveCount(3);
        results.Should().Contain(x => x.Name == "Alice");
        results.Should().Contain(x => x.Name == "Charlie");
        results.Should().Contain(x => x.Name == "Eve");
    }

    [Fact]
    public void IEnumerable_Any_WithSpecification_ShouldReturnTrueWhenElementsMatch()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 30);

        // Act
        var result = _testData.Any(specification);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IEnumerable_Any_WithSpecification_ShouldReturnFalseWhenNoElementsMatch()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 50);

        // Act
        var result = _testData.Any(specification);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IEnumerable_All_WithSpecification_ShouldReturnTrueWhenAllElementsMatch()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Name.Length > 0);

        // Act
        var result = _testData.All(specification);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IEnumerable_All_WithSpecification_ShouldReturnFalseWhenNotAllElementsMatch()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 18);

        // Act
        var result = _testData.All(specification);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IEnumerable_First_WithSpecification_ShouldReturnFirstMatchingElement()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 18);

        // Act
        var result = _testData.First(specification);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Alice"); // Alice is the first adult in the list
    }

    [Fact]
    public void IEnumerable_First_WithSpecification_WhenNoMatch_ShouldThrowInvalidOperationException()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 50);

        // Act & Assert
        var act = () => _testData.First(specification);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IEnumerable_FirstOrDefault_WithSpecification_ShouldReturnFirstMatchingElement()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 18);

        // Act
        var result = _testData.FirstOrDefault(specification);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Alice");
    }

    [Fact]
    public void IEnumerable_FirstOrDefault_WithSpecification_WhenNoMatch_ShouldReturnNull()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 50);

        // Act
        var result = _testData.FirstOrDefault(specification);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void IEnumerable_Single_WithSpecification_ShouldReturnSingleMatchingElement()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Name == "Charlie");

        // Act
        var result = _testData.Single(specification);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Charlie");
    }

    [Fact]
    public void IEnumerable_Single_WithSpecification_WhenNoMatch_ShouldThrowInvalidOperationException()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 50);

        // Act & Assert
        var act = () => _testData.Single(specification);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IEnumerable_Single_WithSpecification_WhenMultipleMatches_ShouldThrowInvalidOperationException()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 18);

        // Act & Assert
        var act = () => _testData.Single(specification);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IEnumerable_SingleOrDefault_WithSpecification_ShouldReturnSingleMatchingElement()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Name == "Charlie");

        // Act
        var result = _testData.SingleOrDefault(specification);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Charlie");
    }

    [Fact]
    public void IEnumerable_SingleOrDefault_WithSpecification_WhenNoMatch_ShouldReturnNull()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 50);

        // Act
        var result = _testData.SingleOrDefault(specification);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void IEnumerable_SingleOrDefault_WithSpecification_WhenMultipleMatches_ShouldThrowInvalidOperationException()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 18);

        // Act & Assert
        var act = () => _testData.SingleOrDefault(specification);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void IEnumerable_Count_WithSpecification_ShouldReturnCorrectCount()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 18);

        // Act
        var result = _testData.Count(specification);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public void IEnumerable_Count_WithSpecification_WhenNoMatches_ShouldReturnZero()
    {
        // Arrange
        ISpecification<TestModel> specification = new Specification<TestModel>(x => x.Age >= 50);

        // Act
        var result = _testData.Count(specification);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public void AllOf_ShouldCombineSpecificationsWithAndLogic()
    {
        // Arrange
        var specifications = new[]
        {
            new Specification<TestModel>(x => x.Age >= 18),
            new Specification<TestModel>(x => x.Name.Length > 3),
            new Specification<TestModel>(x => !string.IsNullOrEmpty(x.Email))
        };

        // Act
        var combined = specifications.AllOf();

        // Assert
        combined.Should().NotBeNull();
        combined.IsSatisfiedBy(new TestModel { Name = "Alice", Age = 25, Email = "alice@example.com" }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestModel { Name = "Bob", Age = 17, Email = "bob@example.com" }).Should().BeFalse();
        combined.IsSatisfiedBy(new TestModel { Name = "Al", Age = 25, Email = "al@example.com" }).Should().BeFalse();
    }

    [Fact]
    public void AnyOf_ShouldCombineSpecificationsWithOrLogic()
    {
        // Arrange
        var specifications = new[]
        {
            new Specification<TestModel>(x => x.Age >= 65),
            new Specification<TestModel>(x => x.Age <= 12),
            new Specification<TestModel>(x => x.Name.StartsWith('V'))
        };

        // Act
        var combined = specifications.AnyOf();

        // Assert
        combined.Should().NotBeNull();
        combined.IsSatisfiedBy(new TestModel { Name = "Senior", Age = 70 }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestModel { Name = "Child", Age = 10 }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestModel { Name = "Victoria", Age = 30 }).Should().BeTrue();
        combined.IsSatisfiedBy(new TestModel { Name = "Adult", Age = 30 }).Should().BeFalse();
    }

    [Fact]
    public void ExtensionMethods_WithNullArguments_ShouldThrowArgumentNullException()
    {
        // Arrange
        Specification<TestModel> specification = new(x => x.Age >= 18);
        IEnumerable<TestModel> enumerable = _testData;
        IQueryable<TestModel> queryable = _testData.AsQueryable();

        // Act & Assert
        var act1 = () => ((Specification<TestModel>)null!).And(specification);
        act1.Should().Throw<NullReferenceException>();

        var act2 = () => specification.And((Expression<Func<TestModel, bool>>)null!);
        act2.Should().Throw<ArgumentNullException>();

        var act3 = () => ((IEnumerable<TestModel>)null!).Where((ISpecification<TestModel>)specification);
        act3.Should().Throw<ArgumentNullException>();

        var act4 = () => enumerable.Where((ISpecification<TestModel>)null!);
        act4.Should().Throw<ArgumentNullException>();

        var act5 = () => ((IQueryable<TestModel>)null!).Where(specification);
        act5.Should().Throw<ArgumentNullException>();

        var act6 = () => queryable.Where((ISpecification<TestModel>)null!);
        act6.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AllOf_WithNullSpecifications_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => ((IEnumerable<ISpecification<TestModel>>)null!).AllOf();
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AnyOf_WithNullSpecifications_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => ((IEnumerable<ISpecification<TestModel>>)null!).AnyOf();
        act.Should().Throw<ArgumentNullException>();
    }
}