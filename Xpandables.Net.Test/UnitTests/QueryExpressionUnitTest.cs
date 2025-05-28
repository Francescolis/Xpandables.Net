using System.Linq.Expressions;

using FluentAssertions;

using Xpandables.Net.Expressions;

namespace Xpandables.Net.Test.UnitTests;
public sealed class QueryExpressionUnitTest
{
    private static QueryExpression<int, bool> GreaterThan5 =>
        new(x => x > 5);

    private static QueryExpression<int, bool> LessThan10 =>
        new(x => x < 10);

    private static QueryExpression<int, bool> IsEven =>
        new(x => x % 2 == 0);

    [Fact]
    public void AndAlso_WithTwoExpressions_ReturnsTrueWhenBothAreTrue()
    {
        var expr = GreaterThan5.AndAlso(LessThan10);
        var func = (Func<int, bool>)expr;

        func(7).Should().BeTrue();   // 7 > 5 && 7 < 10
        func(4).Should().BeFalse();  // 4 > 5 && 4 < 10
        func(12).Should().BeFalse(); // 12 > 5 && 12 < 10
    }

    [Fact]
    public void AndAlso_WithExpressionFunc_ReturnsTrueWhenBothAreTrue()
    {
        var expr = GreaterThan5.AndAlso(x => x < 10);
        var func = (Func<int, bool>)expr;

        func(7).Should().BeTrue();
        func(4).Should().BeFalse();
        func(12).Should().BeFalse();
    }

    [Fact]
    public void And_WithTwoExpressions_BitwiseAndBehavior()
    {
        var expr = GreaterThan5.And(IsEven);
        var func = (Func<int, bool>)expr;

        func(8).Should().BeTrue();   // 8 > 5 && 8 % 2 == 0
        func(7).Should().BeFalse();  // 7 > 5 && 7 % 2 == 0
        func(4).Should().BeFalse();  // 4 > 5 && 4 % 2 == 0
    }

    [Fact]
    public void And_WithExpressionFunc_BitwiseAndBehavior()
    {
        var expr = GreaterThan5.And(x => x % 2 == 0);
        var func = (Func<int, bool>)expr;

        func(8).Should().BeTrue();
        func(7).Should().BeFalse();
        func(4).Should().BeFalse();
    }

    [Fact]
    public void OrElse_WithTwoExpressions_ReturnsTrueWhenEitherIsTrue()
    {
        var expr = GreaterThan5.OrElse(LessThan10);
        var func = (Func<int, bool>)expr;

        func(7).Should().BeTrue();   // 7 > 5 || 7 < 10
        func(4).Should().BeTrue();   // 4 > 5 || 4 < 10
        func(12).Should().BeTrue();  // 12 > 5 || 12 < 10
        func(2).Should().BeTrue();   // 2 > 5 || 2 < 10
    }

    [Fact]
    public void OrElse_WithExpressionFunc_ReturnsTrueWhenEitherIsTrue()
    {
        var expr = GreaterThan5.OrElse(x => x < 10);
        var func = (Func<int, bool>)expr;

        func(7).Should().BeTrue();
        func(4).Should().BeTrue();
        func(12).Should().BeTrue();
    }

    [Fact]
    public void Or_WithTwoExpressions_BitwiseOrBehavior()
    {
        var expr = GreaterThan5.Or(IsEven);
        var func = (Func<int, bool>)expr;

        func(8).Should().BeTrue();   // 8 > 5 || 8 % 2 == 0
        func(7).Should().BeTrue();   // 7 > 5 || 7 % 2 == 0
        func(4).Should().BeTrue();   // 4 > 5 || 4 % 2 == 0
        func(1).Should().BeFalse();  // 1 > 5 || 1 % 2 == 0
    }

    [Fact]
    public void Or_WithExpressionFunc_BitwiseOrBehavior()
    {
        var expr = GreaterThan5.Or(x => x % 2 == 0);
        var func = (Func<int, bool>)expr;

        func(8).Should().BeTrue();
        func(7).Should().BeTrue();
        func(4).Should().BeTrue();
        func(1).Should().BeFalse();
    }

    [Fact]
    public void Not_WithQueryExpression_NegatesResult()
    {
        var expr = GreaterThan5.Not();
        var func = (Func<int, bool>)expr;

        func(7).Should().BeFalse();  // !(7 > 5)
        func(4).Should().BeTrue();   // !(4 > 5)
    }

    [Fact]
    public void Not_WithExpressionFunc_NegatesResult()
    {
        var expr = ((Expression<Func<int, bool>>)(x => x > 5)).Not();
        var func = (Func<int, bool>)expr;

        func(7).Should().BeFalse();
        func(4).Should().BeTrue();
    }
}
