
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
/// Provides the <see cref="QueryExpression{TSource, TResult}"/> "Not" profile.
/// </summary>
/// <typeparam name="TSource">The data type to apply expression to.</typeparam>
/// <typeparam name="TResult">The type of the result of expression.</typeparam>
public record class QueryExpressionNot<TSource, TResult>
    : QueryExpression<TSource, TResult>
{
    private readonly IQueryExpression<TSource, TResult> _expression;
    private Expression<Func<TSource, TResult>>? _cache;

    /// <summary>
    /// Returns a new instance of 
    /// <see cref="QueryExpressionNot{TSource, TResult}"/> class with the request 
    /// expression.
    /// </summary>
    /// <param name="expression">The request expression  for the left side.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="expression"/> is null.</exception>
    public QueryExpressionNot(IQueryExpression<TSource, TResult> expression)
        => _expression = expression
        ?? throw new ArgumentNullException(nameof(expression));

    /// <summary>
    /// Returns a new instance of 
    /// <see cref="QueryExpressionNot{TSource, TResult}"/> class with the 
    /// expression.
    /// </summary>
    /// <param name="leftExpression">The request expression 
    /// for the left side.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="leftExpression"/> is null.</exception>
    public QueryExpressionNot(Expression<Func<TSource, TResult>> leftExpression)
        => _expression = new QueryExpressionBuilder<TSource, TResult>(
            leftExpression
                ?? throw new ArgumentNullException(nameof(leftExpression)));

    /// <summary>
    /// Returns the expression to be used for the clause <see langword="Where"/>
    /// in a request.
    /// </summary>
    public override Expression<Func<TSource, TResult>> GetExpression()
        => _cache ??= QueryExpressionFactory
            .Not(_expression.GetExpression());
}

/// <summary>
/// Provides the <see cref="QueryExpression{TSource}"/> "Not" profile for 
/// TResult as <see cref="bool"/>.
/// </summary>
/// <typeparam name="TSource">The data type to apply expression to.</typeparam>
public sealed record class QueryExpressionNot<TSource>
    : QueryExpression<TSource>, IQueryExpression<TSource>
{

    private readonly IQueryExpression<TSource> _expression;
    private Expression<Func<TSource, bool>>? _cache;

    /// <summary>
    /// Returns a new instance of 
    /// <see cref="QueryExpressionNot{TSource}"/> class with the request 
    /// expression.
    /// </summary>
    /// <param name="expression">The request expression 
    /// for the left side.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="expression"/> is null.</exception>
    public QueryExpressionNot(IQueryExpression<TSource> expression)
        => _expression = expression
        ?? throw new ArgumentNullException(nameof(expression));

    /// <summary>
    /// Returns a new instance of 
    /// <see cref="QueryExpressionNot{TSource}"/> class with the expression.
    /// </summary>
    /// <param name="leftExpression">The request expression 
    /// for the left side.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="leftExpression"/> is null.</exception>
    public QueryExpressionNot(Expression<Func<TSource, bool>> leftExpression)
        => _expression = new QueryExpressionBuilder<TSource>(
            leftExpression
                ?? throw new ArgumentNullException(nameof(leftExpression)));

    /// <summary>
    /// Returns the expression to be used for the clause <see langword="Where"/> 
    /// in a request.
    /// </summary>
    public override Expression<Func<TSource, bool>> GetExpression()
        => _cache ??= QueryExpressionFactory
            .Not(_expression.GetExpression());
}