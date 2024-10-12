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
/// Represents a query expression that negates the result of another query   
/// expression.  
/// </summary>  
/// <typeparam name="TSource">The type of the source element.</typeparam>  
/// <typeparam name="TResult">The type of the result element.</typeparam>  
public record QueryExpressionNot<TSource, TResult> : QueryExpression<TSource, TResult>
{
    /// <summary>  
    /// Initializes a new instance of the   
    /// <see cref="QueryExpressionNot{TSource, TResult}"/> class.  
    /// </summary>  
    /// <param name="expression">The query expression to negate.</param>  
    public QueryExpressionNot(IQueryExpression<TSource, TResult> expression) :
        base(CacheExpression(expression.Expression))
    { }

    /// <summary>  
    /// Initializes a new instance of the   
    /// <see cref="QueryExpressionNot{TSource, TResult}"/> class.  
    /// </summary>  
    /// <param name="expression">The query expression to negate.</param>  
    public QueryExpressionNot(Expression<Func<TSource, TResult>> expression) :
        base(CacheExpression(expression))
    { }

    private static Expression<Func<TSource, TResult>> CacheExpression(
        Expression<Func<TSource, TResult>> expression)
    {
        UnaryExpression notExpression = System.Linq.Expressions
            .Expression.Not(expression.Body);

        return System.Linq.Expressions.Expression
            .Lambda<Func<TSource, TResult>>(
            notExpression, expression.Parameters);
    }
}
