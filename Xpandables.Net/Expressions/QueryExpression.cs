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

namespace Xpandables.Net.Expressions;

/// <summary>
/// Represents a query expression that can be used to filter or project data
/// </summary>
/// <typeparam name="TSource">The type of the source.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public record QueryExpression<TSource, TResult> : IQueryExpression<TSource, TResult>
{
    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="QueryExpression{TSource, TResult}"/> class.
    /// </summary>
    public QueryExpression() { }

    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="QueryExpression{TSource, TResult}"/> class with the specified 
    /// expressions and expression type to produce a new expression of type
    /// <see cref="ExpressionType.AndAlso"/>,<see cref="ExpressionType.OrElse"/>,
    /// <see cref="ExpressionType.And"/> and <see cref="ExpressionType.Or"/>.
    /// </summary>
    /// <param name="left">The left expression.</param>
    /// <param name="right">The right expression.</param>
    /// <param name="expressionType">The type of the expression.</param>
    /// <exception cref="InvalidOperationException">Thrown when the expression 
    /// type is not supported.</exception>
    [SetsRequiredMembers]
    public QueryExpression(
        Expression<Func<TSource, TResult>> left,
        Expression<Func<TSource, TResult>> right,
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
    /// Initializes a new instance of the 
    /// <see cref="QueryExpression{TSource, TResult}"/> class with the specified expression.
    /// </summary>
    /// <param name="expression">The expression.</param>
    [SetsRequiredMembers]
    public QueryExpression(Expression<Func<TSource, TResult>> expression) =>
        Expression = expression;

    /// <summary>
    /// Gets the expression that defines the query.
    /// </summary>
    public required Expression<Func<TSource, TResult>> Expression { get; init; }

    /// <inheritdoc/>
    public override int GetHashCode() => Expression.GetHashCode();

    /// <summary>
    /// Implicitly converts a <see cref="QueryExpression{TSource, TResult}"/> 
    /// to an <see cref="Expression{TSource}"/>.
    /// </summary>
    /// <param name="queryExpression">The query expression to convert.</param>
    /// <returns>The expression that defines the query.</returns>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static implicit operator Expression<Func<TSource, TResult>>(
#pragma warning restore CA2225 // Operator overloads have named alternates
        QueryExpression<TSource, TResult> queryExpression) =>
        queryExpression.Expression;

    /// <summary>
    /// Implicitly converts a <see cref="QueryExpression{TSource, TResult}"/> 
    /// to a <see cref="Func{TSource, TResult}"/>.
    /// </summary>
    /// <param name="queryExpression">The query expression to convert.</param>
    /// <returns>A compiled function that represents the query.</returns>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static implicit operator Func<TSource, TResult>(
#pragma warning restore CA2225 // Operator overloads have named alternates
        QueryExpression<TSource, TResult> queryExpression) =>
        queryExpression.Expression.Compile();

    /// <summary>
    /// Implicitly converts an <see cref="Expression{TSource}"/> 
    /// to a <see cref="QueryExpression{TSource, TResult}"/>.
    /// </summary>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>A new instance of <see cref="QueryExpression{TSource, TResult}"/>.</returns>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static implicit operator QueryExpression<TSource, TResult>(
#pragma warning restore CA2225 // Operator overloads have named alternates
        Expression<Func<TSource, TResult>> expression) =>
        new() { Expression = expression };

    /// <summary>
    /// Implicitly converts a <see cref="Func{TSource, TResult}"/> 
    /// to a <see cref="QueryExpression{TSource, TResult}"/>.
    /// </summary>
    /// <param name="func">The function to convert.</param>
    /// <returns>A new instance of 
    /// <see cref="QueryExpression{TSource, TResult}"/>.</returns>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static implicit operator QueryExpression<TSource, TResult>(
#pragma warning restore CA2225 // Operator overloads have named alternates
        Func<TSource, TResult> func) => new() { Expression = x => func(x) };

    /// <summary>
    /// Combines two <see cref="QueryExpression{TSource, TResult}"/> instances 
    /// using a logical AND operation.
    /// </summary>
    /// <param name="left">The left query expression.</param>
    /// <param name="right">The right query expression.</param>
    /// <returns>A new instance of <see cref="QueryExpression{TSource, TResult}"/> 
    /// that represents the logical AND of the two expressions.</returns>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static QueryExpression<TSource, TResult> operator &(
#pragma warning restore CA2225 // Operator overloads have named alternates
        QueryExpression<TSource, TResult> left,
        QueryExpression<TSource, TResult> right) =>
        new(left, right.Expression, ExpressionType.AndAlso);

    /// <summary>
    /// Combines two <see cref="QueryExpression{TSource, TResult}"/> instances 
    /// using a logical OR operation.
    /// </summary>
    /// <param name="left">The left query expression.</param>
    /// <param name="right">The right query expression.</param>
    /// <returns>A new instance of <see cref="QueryExpression{TSource, TResult}"/> 
    /// that represents the logical OR of the two expressions.</returns>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static QueryExpression<TSource, TResult> operator |(
#pragma warning restore CA2225 // Operator overloads have named alternates
        QueryExpression<TSource, TResult> left,
        QueryExpression<TSource, TResult> right) =>
        new(left, right.Expression, ExpressionType.OrElse);

    /// <summary>
    /// Negates the given <see cref="QueryExpression{TSource, TResult}"/>.
    /// </summary>
    /// <param name="expression">The query expression to negate.</param>
    /// <returns>A new instance of 
    /// <see cref="QueryExpression{TSource, TResult}"/>.</returns>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static QueryExpression<TSource, TResult> operator !(
#pragma warning restore CA2225 // Operator overloads have named alternates
        QueryExpression<TSource, TResult> expression) =>
        new(NotExpression(expression.Expression));

    /// <summary>
    /// Determines whether the specified <see cref="QueryExpression{TSource, TResult}"/> evaluates as true.
    /// </summary>
    /// <remarks>This operator always returns <see langword="false"/> to enforce the use of logical operators 
    /// such as <c>|</c> without short-circuiting. This behavior is standard for expression
    /// combinators.</remarks>
    /// <param name="_">The <see cref="QueryExpression{TSource, TResult}"/> to evaluate.</param>
    /// <returns>Always returns <see langword="false"/>.</returns>
#pragma warning disable CA2225 // Operator overloads have named alternates
    public static bool operator true(QueryExpression<TSource, TResult> _) => false;

    /// <summary>
    /// Defines the behavior of the conditional `false` operator for the <see cref="QueryExpression{TSource, TResult}"/>
    /// type.
    /// </summary>
    /// <remarks>This operator always returns <see langword="false"/> to ensure that logical operators such as
    /// `|` are used without short-circuiting.</remarks>
    /// <param name="_">The <see cref="QueryExpression{TSource, TResult}"/> instance to evaluate.</param>
    /// <returns>Always returns <see langword="false"/>.</returns>
    public static bool operator false(QueryExpression<TSource, TResult> _) => false;

    internal static Expression<Func<TSource, TResult>> AndExpression(
        Expression<Func<TSource, TResult>> left,
        Expression<Func<TSource, TResult>> right) =>
        CombineExpressions(left, right, System.Linq.Expressions.Expression.And);
    internal static Expression<Func<TSource, TResult>> AndAlsoExpression(
        Expression<Func<TSource, TResult>> left,
        Expression<Func<TSource, TResult>> right) =>
        CombineExpressions(left, right, System.Linq.Expressions.Expression.AndAlso);

    internal static Expression<Func<TSource, TResult>> OrExpression(
        Expression<Func<TSource, TResult>> left,
        Expression<Func<TSource, TResult>> right) =>
        CombineExpressions(left, right, System.Linq.Expressions.Expression.Or);

    internal static Expression<Func<TSource, TResult>> OrElseExpression(
        Expression<Func<TSource, TResult>> left,
        Expression<Func<TSource, TResult>> right) =>
        CombineExpressions(left, right, System.Linq.Expressions.Expression.OrElse);

    internal static Expression<Func<TSource, TResult>> NotExpression(
        Expression<Func<TSource, TResult>> expression) =>
        CombineExpressions(expression, System.Linq.Expressions.Expression.Not);

    private static Expression<Func<TSource, TResult>> CombineExpressions(
        Expression<Func<TSource, TResult>> left,
        Expression<Func<TSource, TResult>> right,
        Func<Expression, Expression, BinaryExpression> combiner)
    {
        ParameterExpression parameter = System.Linq.Expressions.Expression.Parameter(typeof(TSource), "param");
        Expression leftBody = ReplaceParameter(left.Body, left.Parameters[0], parameter);
        Expression rightBody = ReplaceParameter(right.Body, right.Parameters[0], parameter);
        return System.Linq.Expressions.Expression.Lambda<Func<TSource, TResult>>(
            combiner(leftBody, rightBody), parameter);
    }

    private static Expression<Func<TSource, TResult>> CombineExpressions(
        Expression<Func<TSource, TResult>> expression,
        Func<Expression, UnaryExpression> combiner)
    {
        ParameterExpression parameter = System.Linq.Expressions.Expression.Parameter(typeof(TSource), "param");
        Expression body = ReplaceParameter(expression.Body, expression.Parameters[0], parameter);
        return System.Linq.Expressions.Expression.Lambda<Func<TSource, TResult>>(
            combiner(body), parameter);
    }
    private static Expression ReplaceParameter(
        Expression body,
        ParameterExpression toReplace,
        ParameterExpression replaceWith) =>
        new ParameterReplacer(toReplace, replaceWith).Visit(body);

    private sealed class ParameterReplacer(
        ParameterExpression toReplace,
        ParameterExpression replaceWith) : ExpressionVisitor
    {
        private readonly ParameterExpression _toReplace = toReplace;
        private readonly ParameterExpression _replaceWith = replaceWith;

        protected override Expression VisitParameter(ParameterExpression node) =>
            node == _toReplace ? _replaceWith : base.VisitParameter(node);
    }
}