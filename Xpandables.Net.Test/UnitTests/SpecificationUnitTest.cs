using FluentAssertions;

using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.Test.UnitTests;

public sealed class SpecificationUnitTest
{
    [Fact]
    public void IsSatisfiedBy_ShouldReturnTrue_WhenExpressionIsTrue()
    {
        // Arrange
        var specification = new Specification<int>() { Expression = x => x > 5 };

        // Act
        var result = specification.IsSatisfiedBy(10);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_ShouldReturnFalse_WhenExpressionIsFalse()
    {
        // Arrange
        var specification = new Specification<int>() { Expression = x => x > 5 };

        // Act
        var result = specification.IsSatisfiedBy(3);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void AndOperator_ShouldCombineSpecificationsWithAnd()
    {
        // Arrange
        var spec1 = new Specification<int>() { Expression = x => x > 5 };
        var spec2 = new Specification<int>() { Expression = x => x < 10 };

        // Act
        var combinedSpec = spec1 & spec2;
        var result1 = combinedSpec.IsSatisfiedBy(7);
        var result2 = combinedSpec.IsSatisfiedBy(3);
        var result3 = combinedSpec.IsSatisfiedBy(12);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
        result3.Should().BeFalse();
    }

    [Fact]
    public void OrOperator_ShouldCombineSpecificationsWithOr()
    {
        // Arrange
        var spec1 = new Specification<int>() { Expression = x => x < 5 };
        var spec2 = new Specification<int>() { Expression = x => x > 10 };

        // Act
        var combinedSpec = spec1 | spec2;
        var result1 = combinedSpec.IsSatisfiedBy(3);
        var result2 = combinedSpec.IsSatisfiedBy(7);
        var result3 = combinedSpec.IsSatisfiedBy(12);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
        result3.Should().BeTrue();
    }

    [Fact]
    public void NotOperator_ShouldNegateSpecification()
    {
        // Arrange
        var spec = new Specification<int>() { Expression = x => x > 5 };

        // Act
        var negatedSpec = !spec;
        var result1 = negatedSpec.IsSatisfiedBy(3);
        var result2 = negatedSpec.IsSatisfiedBy(7);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeFalse();
    }
}
