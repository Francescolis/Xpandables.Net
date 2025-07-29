
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
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Represents a base class for creating specifications that can be combined using logical operations.
/// </summary>
/// <remarks>The <see cref="Specification"/> class provides methods to combine specifications using logical AND,
/// OR, and NOT operations. These methods allow for the creation of complex query conditions by combining simpler
/// specifications.</remarks>
public abstract record Specification
{
    /// <summary>
    /// Named alternate for the &amp; operator. Combines two specifications using a logical AND operation.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification that represents the logical AND of the two specifications.</returns>
    public static Specification<TSource> And<TSource>(Specification<TSource> left, Specification<TSource> right) =>
        new(left, right.Expression, ExpressionType.AndAlso);

    /// <summary>
    /// Named alternate for the | operator. Combines two specifications using a logical OR operation.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification that represents the logical OR of the two specifications.</returns>
    public static Specification<TSource> Or<TSource>(Specification<TSource> left, Specification<TSource> right) =>
        new(left, right.Expression, ExpressionType.OrElse);

    /// <summary>
    /// Named alternate for the ! operator. Negates the given specification.
    /// </summary>
    /// <param name="specification">The specification to negate.</param>
    /// <returns>A new specification that represents the negation of the given specification.</returns>
    public static Specification<TSource> Not<TSource>(Specification<TSource> specification) =>
        new(NotExpression(specification.Expression));

    internal static Expression<Func<TSource, bool>> AndExpression<TSource>(
        Expression<Func<TSource, bool>> left,
        Expression<Func<TSource, bool>> right) =>
        CombineExpressions(left, right, Expression.And);

    internal static Expression<Func<TSource, bool>> AndAlsoExpression<TSource>(
        Expression<Func<TSource, bool>> left,
        Expression<Func<TSource, bool>> right) =>
        CombineExpressions(left, right, Expression.AndAlso);

    internal static Expression<Func<TSource, bool>> OrExpression<TSource>(
        Expression<Func<TSource, bool>> left,
        Expression<Func<TSource, bool>> right) =>
        CombineExpressions(left, right, Expression.Or);

    internal static Expression<Func<TSource, bool>> OrElseExpression<TSource>(
        Expression<Func<TSource, bool>> left,
        Expression<Func<TSource, bool>> right) =>
        CombineExpressions(left, right, Expression.OrElse);

    internal static Expression<Func<TSource, bool>> NotExpression<TSource>(
        Expression<Func<TSource, bool>> expression) =>
        CombineExpressions(expression, Expression.Not);

    internal static Expression<Func<TSource, bool>> CombineExpressions<TSource>(
        Expression<Func<TSource, bool>> left,
        Expression<Func<TSource, bool>> right,
        Func<Expression, Expression, BinaryExpression> combiner)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(TSource), "param");
        Expression leftBody = ReplaceParameter(left.Body, left.Parameters[0], parameter);
        Expression rightBody = ReplaceParameter(right.Body, right.Parameters[0], parameter);
        return Expression.Lambda<Func<TSource, bool>>(
            combiner(leftBody, rightBody), parameter);
    }

    internal static Expression<Func<TSource, bool>> CombineExpressions<TSource>(
        Expression<Func<TSource, bool>> expression,
        Func<Expression, UnaryExpression> combiner)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(TSource), "param");
        Expression body = ReplaceParameter(expression.Body, expression.Parameters[0], parameter);
        return Expression.Lambda<Func<TSource, bool>>(
            combiner(body), parameter);
    }

    internal static Expression ReplaceParameter(
        Expression body,
        ParameterExpression toReplace,
        ParameterExpression replaceWith) =>
        new ParameterReplacer(toReplace, replaceWith).Visit(body);

    internal sealed class ParameterReplacer(
        ParameterExpression toReplace,
        ParameterExpression replaceWith) : ExpressionVisitor
    {
        private readonly ParameterExpression _toReplace = toReplace;
        private readonly ParameterExpression _replaceWith = replaceWith;

        protected override Expression VisitParameter(ParameterExpression node) =>
            node == _toReplace ? _replaceWith : base.VisitParameter(node);
    }
}

/// <summary>
/// Represents a specification that can be used to evaluate whether a given 
/// source satisfies certain criteria.
/// </summary>
/// <typeparam name="TSource">The type of the source to be evaluated.</typeparam>
public record Specification<TSource> : Specification, ISpecification<TSource>
{
    /// <summary>
    /// Gets the expression that defines the query.
    /// </summary>
    public required Expression<Func<TSource, bool>> Expression { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Specification{TSource}"/> class
    /// with a default expression that always returns true.
    /// </summary>
    public Specification() => Expression = _ => true;

    /// <summary>
    /// Initializes a new instance of the <see cref="Specification{TSource}"/> class
    /// with the specified expression.
    /// </summary>
    /// <param name="expression">The expression that defines the specification criteria.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="expression"/> is null.</exception>
    [SetsRequiredMembers]
    public Specification(Expression<Func<TSource, bool>> expression)
    {
        ArgumentNullException.ThrowIfNull(expression);
        Expression = expression;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Specification{TSource}"/> class by combining two expressions using
    /// the specified logical operation.
    /// </summary>
    /// <param name="left">The left-hand expression to be combined.</param>
    /// <param name="right">The right-hand expression to be combined.</param>
    /// <param name="expressionType">The type of logical operation to apply. 
    /// Must be one of <see cref="ExpressionType.AndAlso"/>, <see cref="ExpressionType.OrElse"/>, 
    /// <see cref="ExpressionType.And"/>, or <see cref="ExpressionType.Or"/>.</param>
    /// <exception cref="InvalidOperationException">Thrown if <paramref name="expressionType"/> is not a supported logical operation.</exception>
    [SetsRequiredMembers]
    public Specification(
        Expression<Func<TSource, bool>> left,
        Expression<Func<TSource, bool>> right,
        ExpressionType expressionType) =>
        Expression = expressionType switch
        {
            ExpressionType.AndAlso => AndAlsoExpression(left, right),
            ExpressionType.OrElse => OrElseExpression(left, right),
            ExpressionType.And => AndExpression(left, right),
            ExpressionType.Or => OrExpression(left, right),
            _ => throw new InvalidOperationException($"Unsupported expression type : {expressionType}")
        };

    /// <summary>
    /// Initializes a new instance of the <see cref="Specification{TSource}"/> class by combining two specifications
    /// using a specified logical operation.
    /// </summary>
    /// <remarks>This constructor allows for the creation of complex specifications by combining an existing
    /// specification with a new condition using a logical operator.</remarks>
    /// <param name="left">The left-hand specification to be combined.</param>
    /// <param name="right">The right-hand expression representing a condition to be evaluated.</param>
    /// <param name="expressionType">The type of logical operation to apply when combining the specifications, such as <see
    /// cref="ExpressionType.AndAlso"/> or <see cref="ExpressionType.OrElse"/>.</param>
    [SetsRequiredMembers]
    public Specification(
        Specification<TSource> left,
        Expression<Func<TSource, bool>> right,
        ExpressionType expressionType) : this(left.Expression, right, expressionType)
    { }

    /// <inheritdoc/>
    public bool IsSatisfiedBy(TSource source)
    {
        ArgumentNullException.ThrowIfNull(source);
        return Expression.Compile().Invoke(source);
    }

    /// <inheritdoc/>
    public override int GetHashCode() => Expression.GetHashCode();

    /// <summary>
    /// Implicitly converts a <see cref="Specification{TSource}"/> to an <see cref="Expression{TDelegate}"/>.
    /// </summary>
    /// <param name="specification">The specification to convert.</param>
    /// <returns>The expression that defines the specification.</returns>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static implicit operator Expression<Func<TSource, bool>>(Specification<TSource> specification) =>
        specification.Expression;

    /// <summary>
    /// Implicitly converts a <see cref="Specification{TSource}"/> to a <see cref="Func{TSource, TResult}"/>.
    /// </summary>
    /// <param name="specification">The specification to convert.</param>
    /// <returns>A compiled function that represents the specification.</returns>
    public static implicit operator Func<TSource, bool>(Specification<TSource> specification) =>
        specification.Expression.Compile();

    /// <summary>
    /// Implicitly converts an <see cref="Expression{TDelegate}"/> to a <see cref="Specification{TSource}"/>.
    /// </summary>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>A new instance of <see cref="Specification{TSource}"/>.</returns>
    public static implicit operator Specification<TSource>(Expression<Func<TSource, bool>> expression) =>
        new(expression);

    /// <summary>
    /// Implicitly converts a <see cref="Func{TSource, TResult}"/> to a <see cref="Specification{TSource}"/>.
    /// </summary>
    /// <param name="func">The function to convert.</param>
    /// <returns>A new instance of <see cref="Specification{TSource}"/>.</returns>
    public static implicit operator Specification<TSource>(Func<TSource, bool> func) =>
        new(x => func(x));
#pragma warning restore CA2225 // Operator overloads have named alternates
    /// <summary>
    /// Combines two specifications using a logical AND operation.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification that represents the logical AND of the two specifications.</returns>
    public static Specification<TSource> operator &(Specification<TSource> left, Specification<TSource> right) =>
        And(left, right);

    /// <summary>
    /// Combines two specifications using a logical OR operation.
    /// </summary>
    /// <param name="left">The left specification.</param>
    /// <param name="right">The right specification.</param>
    /// <returns>A new specification that represents the logical OR of the two specifications.</returns>
    public static Specification<TSource> operator |(Specification<TSource> left, Specification<TSource> right) =>
        Or(left, right);

    /// <summary>  
    /// Negates the given specification.  
    /// </summary>  
    /// <param name="specification">The specification to negate.</param>  
    /// <returns>A new specification that represents the negation of the given specification.</returns> 
    public static Specification<TSource> operator !(Specification<TSource> specification) =>
        Not(specification);

    /// <summary>
    /// Determines whether the specified <see cref="Specification{TSource}"/> evaluates as true.
    /// </summary>
    /// <remarks>This operator always returns <see langword="false"/> to enforce the use of logical operators 
    /// such as <c>|</c> without short-circuiting. This behavior is standard for expression combinators.</remarks>
    /// <param name="specification">The <see cref="Specification{TSource}"/> to evaluate.</param>
    /// <returns>Always returns <see langword="false"/>.</returns>
    public static bool operator true(Specification<TSource> specification) => specification.IsTrue;

    /// <summary>
    /// Defines the behavior of the conditional `false` operator for the <see cref="Specification{TSource}"/> type.
    /// </summary>
    /// <remarks>This operator always returns <see langword="false"/> to ensure that logical operators such as
    /// `|` are used without short-circuiting.</remarks>
    /// <param name="_">The <see cref="Specification{TSource}"/> instance to evaluate.</param>
    /// <returns>Always returns <see langword="false"/>.</returns>
    public static bool operator false(Specification<TSource> _) => false;

    /// <summary>
    /// Gets a value indicating whether the condition is true.
    /// </summary>
    public bool IsTrue => true;

    /// <summary>
    /// Combines this specification with another using a logical AND operation.
    /// </summary>
    /// <param name="other">The other specification to combine with.</param>
    /// <returns>A new specification representing the logical AND of both specifications.</returns>
    public Specification<TSource> AndAlso(Specification<TSource> other) =>
        new(this, other.Expression, ExpressionType.AndAlso);

    /// <summary>
    /// Combines this specification with an expression using a logical AND operation.
    /// </summary>
    /// <param name="expression">The expression to combine with.</param>
    /// <returns>A new specification representing the logical AND of the specification and expression.</returns>
    public Specification<TSource> AndAlso(Expression<Func<TSource, bool>> expression) =>
        new(this, expression, ExpressionType.AndAlso);

    /// <summary>
    /// Combines this specification with another using a bitwise AND operation.
    /// </summary>
    /// <param name="other">The other specification to combine with.</param>
    /// <returns>A new specification representing the bitwise AND of both specifications.</returns>
    public Specification<TSource> And(Specification<TSource> other) =>
        new(this, other.Expression, ExpressionType.And);

    /// <summary>
    /// Combines this specification with an expression using a bitwise AND operation.
    /// </summary>
    /// <param name="expression">The expression to combine with.</param>
    /// <returns>A new specification representing the bitwise AND of the specification and expression.</returns>
    public Specification<TSource> And(Expression<Func<TSource, bool>> expression) =>
        new(this, expression, ExpressionType.And);

    /// <summary>
    /// Combines this specification with another using a logical OR operation.
    /// </summary>
    /// <param name="other">The other specification to combine with.</param>
    /// <returns>A new specification representing the logical OR of both specifications.</returns>
    public Specification<TSource> OrElse(Specification<TSource> other) =>
        new(this, other.Expression, ExpressionType.OrElse);

    /// <summary>
    /// Combines this specification with an expression using a logical OR operation.
    /// </summary>
    /// <param name="expression">The expression to combine with.</param>
    /// <returns>A new specification representing the logical OR of the specification and expression.</returns>
    public Specification<TSource> OrElse(Expression<Func<TSource, bool>> expression) =>
        new(this, expression, ExpressionType.OrElse);

    /// <summary>
    /// Combines this specification with another using a bitwise OR operation.
    /// </summary>
    /// <param name="other">The other specification to combine with.</param>
    /// <returns>A new specification representing the bitwise OR of both specifications.</returns>
    public Specification<TSource> Or(Specification<TSource> other) =>
        new(this, other.Expression, ExpressionType.Or);

    /// <summary>
    /// Combines this specification with an expression using a bitwise OR operation.
    /// </summary>
    /// <param name="expression">The expression to combine with.</param>
    /// <returns>A new specification representing the bitwise OR of the specification and expression.</returns>
    public Specification<TSource> Or(Expression<Func<TSource, bool>> expression) =>
        new(this, expression, ExpressionType.Or);

    /// <summary>
    /// Negates this specification.
    /// </summary>
    /// <returns>A new specification that represents the negation of this specification.</returns>
    public Specification<TSource> Not() => Not(this);

}