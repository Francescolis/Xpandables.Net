
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

namespace Xpandables.Net.Repositories;

/// <summary>
/// Provides with extension methods for <see cref="Expression"/>.
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// Combines the first expression to be used as parameter 
    /// for the second expression.
    /// </summary>
    /// <param name="source">The first expression for composition.</param>
    /// <param name="composeResult">The compose expression to apply 
    /// the source expression to.</param>
    /// <typeparam name="TSource">The type of the source model.</typeparam>
    /// <typeparam name="TCompose">The type of the compose model.</typeparam>
    /// <typeparam name="TResult">The type of the result model.</typeparam>
    /// <returns>A statement matching the composition of target functions.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="composeResult"/> is null.</exception>
    public static Expression<Func<TSource, TResult>>
        Compose<TSource, TCompose, TResult>(
        this Expression<Func<TSource, TCompose>> source,
        Expression<Func<TCompose, TResult>> composeResult)
        where TCompose : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(composeResult);

        ParameterExpression param = Expression.Parameter(typeof(TSource), null);
        InvocationExpression invoke = Expression.Invoke(source, param);
        InvocationExpression result = Expression.Invoke(composeResult, invoke);

        return Expression.Lambda<Func<TSource, TResult>>(result, param);
    }

    /// <summary>
    ///  Filters a sequence of values based on a predicate to 
    ///  be applied on properties of <typeparamref name="TParam"/> type.
    /// </summary>
    /// <param name="source"> An <see cref="IQueryable{T}"/> to filter.</param>
    /// <param name="propertyExpression">The expression that contains 
    /// the member name for composition.</param>
    /// <param name="whereClause">A function to test each element 
    /// of the source for a condition.</param>
    /// <typeparam name="TSource">The type of the model source.</typeparam>
    /// <typeparam name="TParam">The type of the model parameter.</typeparam>
    /// <returns> An <see cref="IQueryable{T}"/> that contains 
    /// elements of <typeparamref name="TParam"/> type
    /// from the source sequence that satisfy the condition 
    /// specified by the <paramref name="whereClause"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="propertyExpression"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="whereClause"/> is null.</exception>
    public static IQueryable<TSource> Where<TSource, TParam>(
        this IQueryable<TSource> source,
        Expression<Func<TSource, TParam>> propertyExpression,
        Expression<Func<TParam, bool>> whereClause)
        where TParam : notnull =>
        source.Where(propertyExpression.Compose(whereClause));
}