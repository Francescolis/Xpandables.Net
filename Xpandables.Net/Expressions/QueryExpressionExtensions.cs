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
/// Provides with extensions methods for <see cref="IQueryExpression{TSource, TResult}"/>.
/// </summary>
public static class QueryExpressionExtensions
{
    /// <summary>
    /// Applies the AndAlso operator to both query expressions and returns a new one.
    /// </summary>
    /// <param name="left">The expression left side.</param>
    /// <param name="right">The expression right side.</param>
    /// <returns><see cref="QueryExpression{TSource, TResult}"/> object</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    public static QueryExpression<TSource, TResult> AndAlso<TSource, TResult>(
         this QueryExpression<TSource, TResult> left, QueryExpression<TSource, TResult> right) =>
        new(left, right, ExpressionType.AndAlso);

    /// <summary>
    /// Applies the AndAlso operator to both query expressions and returns a new one.
    /// </summary>
    /// <param name="left">The expression left side.</param>
    /// <param name="right">The expression right side.</param>
    /// <returns><see cref="QueryExpression{TSource, TResult}"/> object</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    public static QueryExpression<TSource, TResult> AndAlso<TSource, TResult>(
         this QueryExpression<TSource, TResult> left, Expression<Func<TSource, TResult>> right) =>
        new(left, right, ExpressionType.AndAlso);

    /// <summary>
    /// Applies the bitwise AND operator to both query expressions and returns a new one.
    /// </summary>
    /// <param name="left">The expression left side.</param>
    /// <param name="right">The expression right side.</param>
    /// <returns><see cref="QueryExpression{TSource, TResult}"/> object</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    public static QueryExpression<TSource, TResult> And<TSource, TResult>(
         this QueryExpression<TSource, TResult> left, QueryExpression<TSource, TResult> right) =>
        new(left, right, ExpressionType.And);

    /// <summary>
    /// Applies the bitwise AND operator to both query expressions and returns a new one.
    /// </summary>
    /// <param name="left">The expression left side.</param>
    /// <param name="right">The expression right side.</param>
    /// <returns><see cref="QueryExpression{TSource, TResult}"/> object</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    public static QueryExpression<TSource, TResult> And<TSource, TResult>(
         this QueryExpression<TSource, TResult> left, Expression<Func<TSource, TResult>> right) =>
        new(left, right, ExpressionType.And);

    /// <summary>
    /// Applies the OrElse operator to both query expressions and returns a new one.
    /// </summary>
    /// <param name="left">The expression left side.</param>
    /// <param name="right">The expression right side.</param>
    /// <returns><see cref="QueryExpression{TSource, TResult}"/> object</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    public static QueryExpression<TSource, TResult> OrElse<TSource, TResult>(
         this QueryExpression<TSource, TResult> left, QueryExpression<TSource, TResult> right) =>
        new(left, right, ExpressionType.OrElse);

    /// <summary>
    /// Applies the OrElse operator to both query expressions and returns a new one.
    /// </summary>
    /// <param name="left">The expression left side.</param>
    /// <param name="right">The expression right side.</param>
    /// <returns><see cref="QueryExpression{TSource, TResult}"/> object</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    public static QueryExpression<TSource, TResult> OrElse<TSource, TResult>(
         this QueryExpression<TSource, TResult> left, Expression<Func<TSource, TResult>> right) =>
        new(left, right, ExpressionType.OrElse);

    /// <summary>
    /// Applies the bitwise OR operator to both query expressions and returns a new one.
    /// </summary>
    /// <param name="left">The expression left side.</param>
    /// <param name="right">The expression right side.</param>
    /// <returns><see cref="QueryExpression{TSource, TResult}"/> object</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    public static QueryExpression<TSource, TResult> Or<TSource, TResult>(
         this QueryExpression<TSource, TResult> left, QueryExpression<TSource, TResult> right) =>
        new(left, right, ExpressionType.Or);

    /// <summary>
    /// Applies the bitwise OR operator to both query expressions and returns a new one.
    /// </summary>
    /// <param name="left">The expression left side.</param>
    /// <param name="right">The expression right side.</param>
    /// <returns><see cref="QueryExpression{TSource, TResult}"/> object</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The <paramref name="right"/> is null.</exception>
    public static QueryExpression<TSource, TResult> Or<TSource, TResult>(
         this QueryExpression<TSource, TResult> left, Expression<Func<TSource, TResult>> right) =>
        new(left, right, ExpressionType.Or);

    /// <summary>
    /// Applies the NOT operator to the query expression and returns a new one.
    /// </summary>
    /// <param name="queryExpression">The expression left side.</param>
    /// <returns><see cref="QueryExpression{TSource, TResult}"/> object</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static QueryExpression<TSource, TResult> Not<TSource, TResult>(
         this QueryExpression<TSource, TResult> queryExpression) =>
        !(QueryExpression<TSource, TResult>)new(queryExpression);

    /// <summary>
    /// Applies the NOT operator to the query expression and returns a new one.
    /// </summary>
    /// <param name="queryExpression">The expression left side.</param>
    /// <returns><see cref="QueryExpression{TSource, TResult}"/> object</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static QueryExpression<TSource, TResult> Not<TSource, TResult>(
         this Expression<Func<TSource, TResult>> queryExpression) =>
        !(QueryExpression<TSource, TResult>)new(queryExpression);
}