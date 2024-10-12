
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
/// Represents a query expression that combines two query expressions using a 
/// logical AND operation.
/// </summary>
/// <typeparam name="TSource">The type of the source.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public record QueryExpressionAnd<TSource, TResult> : QueryExpression<TSource, TResult>
{
    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="QueryExpressionAnd{TSource, TResult}"/> class.
    /// </summary>
    /// <param name="left">The left query expression.</param>
    /// <param name="right">The right query expression.</param>
    public QueryExpressionAnd(
        IQueryExpression<TSource, TResult> left,
        IQueryExpression<TSource, TResult> right) :
        base(CacheExpression(left.Expression, right.Expression))
    { }

    /// <summary>
    /// Initializes a new instance of the 
    /// <see cref="QueryExpressionAnd{TSource, TResult}"/> class.
    /// </summary>
    /// <param name="left">The left query expression.</param>
    /// <param name="right">The right expression.</param>
    public QueryExpressionAnd(
        IQueryExpression<TSource, TResult> left,
        Expression<Func<TSource, TResult>> right) :
        base(CacheExpression(left.Expression, right))
    { }

    private static Expression<Func<TSource, TResult>> CacheExpression(
        Expression<Func<TSource, TResult>> left,
        Expression<Func<TSource, TResult>> right)
    {
        InvocationExpression invocation = System.Linq.Expressions
            .Expression.Invoke(right, left.Parameters);

        return System.Linq.Expressions.Expression
            .Lambda<Func<TSource, TResult>>(
            System.Linq.Expressions.Expression
            .AndAlso(left.Body, invocation), left.Parameters);
    }
}
