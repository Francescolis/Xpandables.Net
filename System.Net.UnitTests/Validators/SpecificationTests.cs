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

public class SpecificationTests
{
    [Fact]
    public void Specification_FromExpression_ShouldCreateSpecification()
    {
        // Arrange
        Expression<Func<TestModel, bool>> expression = x => x.Age > 18;

        // Act
        var spec = Specification.FromExpression(expression);

        // Assert
        spec.Should().NotBeNull();
        spec.Expression.Should().Be(expression);
    }

    [Fact]
    public void Specification_FromExpression_WithNullExpression_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.FromExpression<TestModel>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_ToExpression_ShouldReturnExpression()
    {
        // Arrange
        Expression<Func<TestModel, bool>> originalExpression = x => x.Age > 18;
        var spec = new Specification<TestModel>(originalExpression);

        // Act
        var expression = Specification.ToExpression(spec);

        // Assert
        expression.Should().Be(originalExpression);
    }

    [Fact]
    public void Specification_ToExpression_WithNullSpecification_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => Specification.ToExpression<TestModel>(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_DefaultConstructor_ShouldCreateAlwaysTrueSpecification()
    {
        // Arrange & Act
        var spec = new Specification<TestModel>();
        var testModel = new TestModel { Name = "John", Age = 25 };

        // Assert
        spec.IsSatisfiedBy(testModel).Should().BeTrue();
    }

    [Fact]
    public void Specification_WithExpression_ShouldEvaluateCorrectly()
    {
        // Arrange
        Expression<Func<TestModel, bool>> expression = x => x.Age >= 18;
        var spec = new Specification<TestModel>(expression);
        var adult = new TestModel { Name = "John", Age = 25 };
        var minor = new TestModel { Name = "Jane", Age = 16 };

        // Act & Assert
        spec.IsSatisfiedBy(adult).Should().BeTrue();
        spec.IsSatisfiedBy(minor).Should().BeFalse();
    }

    [Fact]
    public void Specification_IsSatisfiedBy_WithNullSource_ShouldThrowArgumentNullException()
    {
        // Arrange
        var spec = new Specification<TestModel>(x => x.Age > 18);

        // Act & Assert
        var act = () => spec.IsSatisfiedBy(null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Specification_And_StaticMethod_ShouldCombineSpecifications()
    {
        // Arrange
        var spec1 = new Specification<TestModel>(x => x.Age >= 18);
        var spec2 = new Specification<TestModel>(x => x.Name.Length > 0);
        var testModel = new TestModel { Name = "John", Age = 25 };

        // Act
        var combined = Specification.And(spec1, spec2);

        // Assert
        combined.IsSatisfiedBy(testModel).Should().BeTrue();
    }

    [Fact]
    public void Specification_And_InstanceMethod_ShouldCombineSpecifications()
    {
        // Arrange
        var spec1 = new Specification<TestModel>(x => x.Age >= 18);
        var spec2 = new Specification<TestModel>(x => x.Name.Length > 0);
        var testModel = new TestModel { Name = "John", Age = 25 };

        // Act
        var combined = spec1.And(spec2);

        // Assert
        combined.IsSatisfiedBy(testModel).Should().BeTrue();
    }

    [Fact]
    public void Specification_And_WithExpression_ShouldCombineWithExpression()
    {
        // Arrange
        var spec = new Specification<TestModel>(x => x.Age >= 18);
        Expression<Func<TestModel, bool>> expression = x => x.Name.Length > 0;
        var testModel = new TestModel { Name = "John", Age = 25 };

        // Act
        var combined = spec.And(expression);

        // Assert
        combined.IsSatisfiedBy(testModel).Should().BeTrue();
    }

    [Fact]
    public void Specification_Or_ShouldCombineWithOrLogic()
    {
        // Arrange
        var spec1 = new Specification<TestModel>(x => x.Age >= 65); // Senior
        var spec2 = new Specification<TestModel>(x => x.Age <= 12); // Child
        var senior = new TestModel { Name = "Bob", Age = 70 };
        var child = new TestModel { Name = "Alice", Age = 10 };
        var adult = new TestModel { Name = "John", Age = 35 };

        // Act
        var combined = spec1.Or(spec2);

        // Assert
        combined.IsSatisfiedBy(senior).Should().BeTrue();
        combined.IsSatisfiedBy(child).Should().BeTrue();
        combined.IsSatisfiedBy(adult).Should().BeFalse();
    }

    [Fact]
    public void Specification_Not_ShouldNegateSpecification()
    {
        // Arrange
        var spec = new Specification<TestModel>(x => x.Age >= 18);
        var adult = new TestModel { Name = "John", Age = 25 };
        var minor = new TestModel { Name = "Jane", Age = 16 };

        // Act
        var negated = spec.Not();

        // Assert
        negated.IsSatisfiedBy(adult).Should().BeFalse();
        negated.IsSatisfiedBy(minor).Should().BeTrue();
    }

    [Fact]
    public void Specification_AndAlso_ShouldUseShortCircuitEvaluation()
    {
        // Arrange
        var spec1 = new Specification<TestModel>(x => x.Age >= 18);
        var spec2 = new Specification<TestModel>(x => x.Name.Length > 0);
        var testModel = new TestModel { Name = "John", Age = 25 };

        // Act
        var combined = spec1.AndAlso(spec2);

        // Assert
        combined.IsSatisfiedBy(testModel).Should().BeTrue();
    }

    [Fact]
    public void Specification_OrElse_ShouldUseShortCircuitEvaluation()
    {
        // Arrange
        var spec1 = new Specification<TestModel>(x => x.Age >= 65);
        var spec2 = new Specification<TestModel>(x => x.Age <= 12);
        var senior = new TestModel { Name = "Bob", Age = 70 };

        // Act
        var combined = spec1.OrElse(spec2);

        // Assert
        combined.IsSatisfiedBy(senior).Should().BeTrue();
    }

    [Fact]
    public void Specification_CombiningWithTwoSpecifications_ShouldWork()
    {
        // Arrange
        var spec1 = new Specification<TestModel>(x => x.Age >= 18);
        var spec2 = new Specification<TestModel>(x => x.Name.StartsWith('J'));

        // Act
        var combined = new Specification<TestModel>(spec1, spec2, System.Linq.Expressions.ExpressionType.AndAlso);
        var testModel = new TestModel { Name = "John", Age = 25 };

        // Assert
        combined.IsSatisfiedBy(testModel).Should().BeTrue();
    }

    [Fact]
    public void Specification_CombiningWithTwoExpressions_ShouldWork()
    {
        // Arrange
        Expression<Func<TestModel, bool>> expr1 = x => x.Age >= 18;
        Expression<Func<TestModel, bool>> expr2 = x => x.Name.StartsWith('J');

        // Act
        var combined = new Specification<TestModel>(expr1, expr2, System.Linq.Expressions.ExpressionType.AndAlso);
        var testModel = new TestModel { Name = "John", Age = 25 };

        // Assert
        combined.IsSatisfiedBy(testModel).Should().BeTrue();
    }

    [Fact]
    public void Specification_CombiningSpecificationWithExpression_ShouldWork()
    {
        // Arrange
        var spec = new Specification<TestModel>(x => x.Age >= 18);
        Expression<Func<TestModel, bool>> expr = x => x.Name.StartsWith('J');

        // Act
        var combined = new Specification<TestModel>(spec, expr, System.Linq.Expressions.ExpressionType.AndAlso);
        var testModel = new TestModel { Name = "John", Age = 25 };

        // Assert
        combined.IsSatisfiedBy(testModel).Should().BeTrue();
    }

    [Fact]
    public void Specification_WithUnsupportedExpressionType_ShouldThrowInvalidOperationException()
    {
        // Arrange
        Expression<Func<TestModel, bool>> expr1 = x => x.Age >= 18;
        Expression<Func<TestModel, bool>> expr2 = x => x.Name.StartsWith('J');

        // Act & Assert
        var act = () => new Specification<TestModel>(expr1, expr2, System.Linq.Expressions.ExpressionType.Add);
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("Unsupported expression type : Add");
    }

    [Fact]
    public void Specification_GetHashCode_ShouldReturnExpressionHashCode()
    {
        // Arrange
        Expression<Func<TestModel, bool>> expression = x => x.Age > 18;
        var spec = new Specification<TestModel>(expression);

        // Act
        var hashCode = spec.GetHashCode();

        // Assert
        hashCode.Should().Be(expression.GetHashCode());
    }

    [Fact]
    public void Specification_ExplicitOperatorToExpression_ShouldReturnExpression()
    {
        // Arrange
        Expression<Func<TestModel, bool>> originalExpression = x => x.Age > 18;
        var spec = new Specification<TestModel>(originalExpression);

        // Act
        var expression = (Expression<Func<TestModel, bool>>)spec;

        // Assert
        expression.Should().Be(originalExpression);
    }

    [Fact]
    public void Specification_ImplicitOperatorToFunc_ShouldReturnCompiledExpression()
    {
        // Arrange
        var spec = new Specification<TestModel>(x => x.Age > 18);
        var testModel = new TestModel { Name = "John", Age = 25 };

        // Act
        Func<TestModel, bool> func = spec;

        // Assert
        func(testModel).Should().BeTrue();
    }

    [Fact]
    public void Specification_ImplicitOperatorFromExpression_ShouldCreateSpecification()
    {
        // Arrange
        Expression<Func<TestModel, bool>> expression = x => x.Age > 18;

        // Act
        Specification<TestModel> spec = expression;

        // Assert
        spec.Expression.Should().Be(expression);
    }

    [Fact]
    public void Specification_ImplicitOperatorFromFunc_ShouldCreateSpecification()
    {
        // Arrange
        Func<TestModel, bool> func = x => x.Age > 18;

        // Act
        Specification<TestModel> spec = func;

        // Assert
        spec.Should().NotBeNull();
        spec.IsSatisfiedBy(new TestModel { Name = "John", Age = 25 }).Should().BeTrue();
    }

    [Fact]
    public void Specification_BitwiseAndOperator_ShouldCombineSpecifications()
    {
        // Arrange
        var spec1 = new Specification<TestModel>(x => x.Age >= 18);
        var spec2 = new Specification<TestModel>(x => x.Name.Length > 0);
        var testModel = new TestModel { Name = "John", Age = 25 };

        // Act
        var combined = spec1 & spec2;

        // Assert
        combined.IsSatisfiedBy(testModel).Should().BeTrue();
    }

    [Fact]
    public void Specification_BitwiseOrOperator_ShouldCombineSpecifications()
    {
        // Arrange
        var spec1 = new Specification<TestModel>(x => x.Age >= 65);
        var spec2 = new Specification<TestModel>(x => x.Age <= 12);
        var senior = new TestModel { Name = "Bob", Age = 70 };

        // Act
        var combined = spec1 | spec2;

        // Assert
        combined.IsSatisfiedBy(senior).Should().BeTrue();
    }

    [Fact]
    public void Specification_NotOperator_ShouldNegateSpecification()
    {
        // Arrange
        var spec = new Specification<TestModel>(x => x.Age >= 18);
        var minor = new TestModel { Name = "Jane", Age = 16 };

        // Act
        var negated = !spec;

        // Assert
        negated.IsSatisfiedBy(minor).Should().BeTrue();
    }

    [Theory]
    [InlineData(17, false)]
    [InlineData(18, true)]
    [InlineData(25, true)]
    [InlineData(65, true)]
    public void Specification_WithAgeRestriction_ShouldEvaluateCorrectly(int age, bool expected)
    {
        // Arrange
        var spec = new Specification<TestModel>(x => x.Age >= 18);
        var testModel = new TestModel { Name = "Test", Age = age };

        // Act
        var result = spec.IsSatisfiedBy(testModel);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Specification_ComplexCombination_ShouldWorkCorrectly()
    {
        // Arrange
        var ageSpec = new Specification<TestModel>(x => x.Age >= 18 && x.Age <= 65);
        var nameSpec = new Specification<TestModel>(x => !string.IsNullOrEmpty(x.Name));
        var emailSpec = new Specification<TestModel>(x => !string.IsNullOrEmpty(x.Email));

        var validAdult = new TestModel { Name = "John", Age = 30, Email = "john@example.com" };
        var invalidAge = new TestModel { Name = "Bob", Age = 70, Email = "bob@example.com" };
        var invalidName = new TestModel { Name = "", Age = 25, Email = "test@example.com" };

        // Act
        var combined = ageSpec.And(nameSpec).And(emailSpec);

        // Assert
        combined.IsSatisfiedBy(validAdult).Should().BeTrue();
        combined.IsSatisfiedBy(invalidAge).Should().BeFalse();
        combined.IsSatisfiedBy(invalidName).Should().BeFalse();
    }
}