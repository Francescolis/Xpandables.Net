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
/// Represents a query expression with a source and result type.
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
    /// <see cref="ExpressionType.AndAlso"/> or <see cref="ExpressionType.OrElse"/>.
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
#pragma warning disable IDE0072 // Add missing cases
        ExpressionType expressionType) =>
        Expression = expressionType switch
        {
            ExpressionType.AndAlso => AndExpression(left, right),
            ExpressionType.OrElse => OrExpression(left, right),
            _ => throw new InvalidOperationException(
                $"Only {ExpressionType.AndAlso} and " +
                $"{ExpressionType.Or} are supported")
        };
#pragma warning restore IDE0072 // Add missing cases

    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="QueryExpression{TSource, TResult}"/> class with the specified 
    /// expression to produce a new negated expression.
    /// </summary>
    /// <param name="expression">The expression to negate.</param>
    [SetsRequiredMembers]
    public QueryExpression(Expression<Func<TSource, TResult>> expression) =>
        Expression = NotExpression(expression);

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
        new(expression.Expression);

    private static Expression<Func<TSource, TResult>> AndExpression(
        Expression<Func<TSource, TResult>> left,
        Expression<Func<TSource, TResult>> right)
    {
        ParameterExpression parameter = System.Linq.Expressions
            .Expression.Parameter(typeof(TSource), "param");

        Expression leftBody = ReplaceParameter(
            left.Body, left.Parameters[0], parameter);
        Expression rightBody = ReplaceParameter(
            right.Body, right.Parameters[0], parameter);

        return System.Linq.Expressions.Expression
            .Lambda<Func<TSource, TResult>>(
                System.Linq.Expressions.Expression
                    .AndAlso(leftBody, rightBody), parameter);
    }

    private static Expression<Func<TSource, TResult>> OrExpression(
        Expression<Func<TSource, TResult>> left,
        Expression<Func<TSource, TResult>> right)
    {
        ParameterExpression parameter = System.Linq.Expressions
            .Expression.Parameter(typeof(TSource), "param");

        Expression leftBody = ReplaceParameter(
            left.Body, left.Parameters[0], parameter);
        Expression rightBody = ReplaceParameter(
            right.Body, right.Parameters[0], parameter);

        return System.Linq.Expressions.Expression
            .Lambda<Func<TSource, TResult>>(
                System.Linq.Expressions.Expression
                    .OrElse(leftBody, rightBody), parameter);
    }

    private static Expression<Func<TSource, TResult>> NotExpression(
        Expression<Func<TSource, TResult>> expression)
    {
        ParameterExpression parameter = System.Linq.Expressions
            .Expression.Parameter(typeof(TSource), "param");

        Expression expressionBody = ReplaceParameter(
            expression.Body, expression.Parameters[0], parameter);

        return System.Linq.Expressions.Expression
            .Lambda<Func<TSource, TResult>>(
                System.Linq.Expressions.Expression
                    .Not(expressionBody), parameter);
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