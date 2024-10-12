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
    /// <param name="expression">The expression that defines the query.</param>
    public QueryExpression(Expression<Func<TSource, TResult>> expression)
        => Expression = expression
            ?? throw new ArgumentNullException(nameof(expression));

    /// <summary>
    /// Gets the expression that defines the query.
    /// </summary>
    public Expression<Func<TSource, TResult>> Expression { get; }

    /// <inheritdoc/>
    public override int GetHashCode() => Expression.GetHashCode();

    /// <summary>
    /// Implicitly converts a <see cref="QueryExpression{TSource, TResult}"/> 
    /// to an <see cref="Expression{TSource}"/>.
    /// </summary>
    /// <param name="queryExpression">The query expression to convert.</param>
    /// <returns>The expression that defines the query.</returns>
    public static implicit operator Expression<Func<TSource, TResult>>(
        QueryExpression<TSource, TResult> queryExpression) =>
        queryExpression.Expression;

    /// <summary>
    /// Implicitly converts a <see cref="QueryExpression{TSource, TResult}"/> 
    /// to a <see cref="Func{TSource, TResult}"/>.
    /// </summary>
    /// <param name="queryExpression">The query expression to convert.</param>
    /// <returns>A compiled function that represents the query.</returns>
    public static implicit operator Func<TSource, TResult>(
        QueryExpression<TSource, TResult> queryExpression) =>
        queryExpression.Expression.Compile();

    /// <summary>
    /// Implicitly converts an <see cref="Expression{TSource}"/> 
    /// to a <see cref="QueryExpression{TSource, TResult}"/>.
    /// </summary>
    /// <param name="expression">The expression to convert.</param>
    /// <returns>A new instance of <see cref="QueryExpression{TSource, TResult}"/>.</returns>
    public static implicit operator QueryExpression<TSource, TResult>(
        Expression<Func<TSource, TResult>> expression) => new(expression);

    /// <summary>
    /// Implicitly converts a <see cref="Func{TSource, TResult}"/> 
    /// to a <see cref="QueryExpression{TSource, TResult}"/>.
    /// </summary>
    /// <param name="func">The function to convert.</param>
    /// <returns>A new instance of 
    /// <see cref="QueryExpression{TSource, TResult}"/>.</returns>
    public static implicit operator QueryExpression<TSource, TResult>(
        Func<TSource, TResult> func) => new(x => func(x));

    /// <summary>
    /// Combines two <see cref="QueryExpression{TSource, TResult}"/> instances 
    /// using a logical AND operation.
    /// </summary>
    /// <param name="left">The left query expression.</param>
    /// <param name="right">The right query expression.</param>
    /// <returns>A new instance of <see cref="QueryExpression{TSource, TResult}"/> 
    /// that represents the logical AND of the two expressions.</returns>
    public static QueryExpression<TSource, TResult> operator &(
        QueryExpression<TSource, TResult> left,
        IQueryExpression<TSource, TResult> right) =>
        new QueryExpressionAnd<TSource, TResult>(left, right);

    /// <summary>
    /// Combines two <see cref="QueryExpression{TSource, TResult}"/> instances 
    /// using a logical AND operation.
    /// </summary>
    /// <param name="left">The left query expression.</param>
    /// <param name="right">The right query expression.</param>
    /// <returns>A new instance of <see cref="QueryExpression{TSource, TResult}"/> 
    /// that represents the logical AND of the two expressions.</returns>
    public static QueryExpression<TSource, TResult> operator &(
        QueryExpression<TSource, TResult> left,
        Expression<Func<TSource, TResult>> right) =>
        new QueryExpressionAnd<TSource, TResult>(left, right);

    /// <summary>
    /// Combines two <see cref="QueryExpression{TSource, TResult}"/> instances 
    /// using a logical OR operation.
    /// </summary>
    /// <param name="left">The left query expression.</param>
    /// <param name="right">The right query expression.</param>
    /// <returns>A new instance of <see cref="QueryExpression{TSource, TResult}"/> 
    /// that represents the logical OR of the two expressions.</returns>
    public static QueryExpression<TSource, TResult> operator |(
        QueryExpression<TSource, TResult> left,
        IQueryExpression<TSource, TResult> right) =>
        new QueryExpressionOr<TSource, TResult>(left, right);

    /// <summary>
    /// Combines two <see cref="QueryExpression{TSource, TResult}"/> instances 
    /// using a logical OR operation.
    /// </summary>
    /// <param name="left">The left query expression.</param>
    /// <param name="right">The right query expression.</param>
    /// <returns>A new instance of <see cref="QueryExpression{TSource, TResult}"/> 
    /// that represents the logical OR of the two expressions.</returns>
    public static QueryExpression<TSource, TResult> operator |(
        QueryExpression<TSource, TResult> left,
        Expression<Func<TSource, TResult>> right) =>
        new QueryExpressionOr<TSource, TResult>(left, right);

    /// <summary>
    /// Negates the given <see cref="QueryExpression{TSource, TResult}"/>.
    /// </summary>
    /// <param name="expression">The query expression to negate.</param>
    /// <returns>A new instance of 
    /// <see cref="QueryExpression{TSource, TResult}"/>.</returns>
    public static QueryExpression<TSource, TResult> operator !(
        QueryExpression<TSource, TResult> expression) =>
        new QueryExpressionNot<TSource, TResult>(expression.Expression);

}

/// <summary>
/// Represents a query expression that returns a boolean result.
/// </summary>
/// <typeparam name="TSource">The type of the source.</typeparam>
public record QueryExpression<TSource> :
    QueryExpression<TSource, bool>,
    IQueryExpression<TSource>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryExpression{TSource}"/> class.
    /// </summary>
    /// <param name="expression">The expression that defines the query.</param>
    public QueryExpression(Expression<Func<TSource, bool>> expression)
        : base(expression)
    {
    }
}
