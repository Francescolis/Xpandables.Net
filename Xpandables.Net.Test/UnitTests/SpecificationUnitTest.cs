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
    
    [Fact]
    public void IsSatisfiedBy_ReturnsTrue_WhenConditionIsMet()
    {
        var specification = new Specification<int>(){Expression = x => x > 10};
    
        var result = specification.IsSatisfiedBy(15);
    
        Assert.True(result);
    }
    
    [Fact]
    public void IsSatisfiedBy_ReturnsFalse_WhenConditionIsNotMet()
    {
        var specification = new Specification<int>(){Expression = x => x > 10};
    
        var result = specification.IsSatisfiedBy(5);
    
        Assert.False(result);
    }
    
    [Fact]
    public void AndOperator_ReturnsTrue_WhenBothSpecificationsAreSatisfied()
    {
        var spec1 = new Specification<int>(){Expression = x => x > 10};
        var spec2 = new Specification<int>(){Expression = x => x < 20};
    
        var combinedSpec = spec1 & spec2;
    
        var result = combinedSpec.IsSatisfiedBy(15);
    
        Assert.True(result);
    }
    
    [Fact]
    public void AndOperator_ReturnsFalse_WhenOneSpecificationIsNotSatisfied()
    {
        var spec1 = new Specification<int>(){Expression = x => x > 10};
        var spec2 = new Specification<int>(){Expression = x => x < 20};
    
        var combinedSpec = spec1 & spec2;
    
        var result = combinedSpec.IsSatisfiedBy(25);
    
        Assert.False(result);
    }
    
    [Fact]
    public void OrOperator_ReturnsTrue_WhenAtLeastOneSpecificationIsSatisfied()
    {
        var spec1 = new Specification<int>(){Expression = x => x > 10};
        var spec2 = new Specification<int>(){Expression = x => x < 5};
    
        var combinedSpec = spec1 | spec2;
    
        var result = combinedSpec.IsSatisfiedBy(15);
    
        Assert.True(result);
    }
    
    [Fact]
    public void OrOperator_ReturnsFalse_WhenNeitherSpecificationIsSatisfied()
    {
        var spec1 = new Specification<int>(){Expression = x => x > 10};
        var spec2 = new Specification<int>(){Expression = x => x < 5};
    
        var combinedSpec = spec1 | spec2;
    
        var result = combinedSpec.IsSatisfiedBy(7);
    
        Assert.False(result);
    }
    
    [Fact]
    public void NotOperator_ReturnsTrue_WhenSpecificationIsNotSatisfied()
    {
        var specification = new Specification<int>(){Expression = x => x > 10};
    
        var negatedSpec = !specification;
    
        var result = negatedSpec.IsSatisfiedBy(5);
    
        Assert.True(result);
    }
    
    [Fact]
    public void NotOperator_ReturnsFalse_WhenSpecificationIsSatisfied()
    {
        var specification = new Specification<int>(){Expression = x => x > 10};
    
        var negatedSpec = !specification;
    
        var result = negatedSpec.IsSatisfiedBy(15);
    
        Assert.False(result);
    }
}
