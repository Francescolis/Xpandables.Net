﻿
/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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
/// Provides the <see cref="QueryExpression{TSource, TResult}"/> "And" profile.
/// </summary>
/// <typeparam name="TSource">The data type to apply expression to.</typeparam>
/// <typeparam name="TResult">The type of the result of expression.</typeparam>
public record class QueryExpressionAnd<TSource, TResult>
    : QueryExpression<TSource, TResult>
{
    private readonly IQueryExpression<TSource, TResult> _left;
    private readonly IQueryExpression<TSource, TResult> _right;
    private Expression<Func<TSource, TResult>>? _cache;

    /// <summary>
    /// Returns a new instance of 
    /// <see cref="QueryExpressionAnd{TSource, TResult}"/> class with the request 
    /// expressions for composition.
    /// </summary>
    /// <param name="left">The request expression for the left side.</param>
    /// <param name="right">The request expression for the right side.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="right"/> is null.</exception>
    public QueryExpressionAnd(
        IQueryExpression<TSource, TResult> left,
        IQueryExpression<TSource, TResult> right)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
    }

    /// <summary>
    /// Returns a new instance of 
    /// <see cref="QueryExpressionAnd{TSource, TResult}"/> class with the 
    /// expressions for composition.
    /// </summary>
    /// <param name="left">The request expression  for the left side.</param>
    /// <param name="rightExpression">The expression for the right side.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="rightExpression"/> is null.</exception>
    public QueryExpressionAnd(
        IQueryExpression<TSource, TResult> left,
        Expression<Func<TSource, TResult>> rightExpression)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = new QueryExpressionBuilder<TSource, TResult>(
            rightExpression
                ?? throw new ArgumentNullException(nameof(rightExpression)));
    }

    /// <summary>
    /// Returns the expression to be used for the 
    /// clause <see langword="Where"/> in a request.
    /// </summary>
    public override Expression<Func<TSource, TResult>> GetExpression()
        => _cache ??= QueryExpressionFactory
            .And(_left.GetExpression(), _right.GetExpression());
}

/// <summary>
/// Provides the <see cref="QueryExpression{TSource}"/> "And" profile
/// for TResult as <see cref="bool"/>.
/// </summary>
/// <typeparam name="TSource">The data type to apply expression to.</typeparam>
public sealed record class QueryExpressionAnd<TSource>
    : QueryExpression<TSource>, IQueryExpression<TSource>
{
    private readonly IQueryExpression<TSource> _left;
    private readonly IQueryExpression<TSource> _right;
    private Expression<Func<TSource, bool>>? _cache;

    /// <summary>
    /// Returns a new instance of 
    /// <see cref="QueryExpressionAnd{TSource}"/> class with the request 
    /// expressions for composition.
    /// </summary>
    /// <param name="left">The request expression for the left side.</param>
    /// <param name="right">The request expression for the right side.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="right"/> is null.</exception>
    public QueryExpressionAnd(
        IQueryExpression<TSource> left,
        IQueryExpression<TSource> right)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = right ?? throw new ArgumentNullException(nameof(right));
    }

    /// <summary>
    /// Returns a new instance of 
    /// <see cref="QueryExpressionAnd{TSource}"/> class with the expressions 
    /// for composition.
    /// </summary>
    /// <param name="left">The request expression  for the left side.</param>
    /// <param name="rightExpression">The expression for the right side.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="left"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="rightExpression"/> is null.</exception>
    public QueryExpressionAnd(
        IQueryExpression<TSource> left,
        Expression<Func<TSource, bool>> rightExpression)
    {
        _left = left ?? throw new ArgumentNullException(nameof(left));
        _right = new QueryExpressionBuilder<TSource>(
            rightExpression
                ?? throw new ArgumentNullException(nameof(rightExpression)));
    }

    /// <summary>
    /// Returns the expression to be used for the 
    /// clause <see langword="Where"/> in a request.
    /// </summary>
    public override Expression<Func<TSource, bool>> GetExpression()
        => _cache ??= QueryExpressionFactory
            .And(_left.GetExpression(), _right.GetExpression());
}