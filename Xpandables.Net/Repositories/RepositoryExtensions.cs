
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

namespace Xpandables.Net.Repositories;

/// <summary>
/// Provides extension methods for repository operations.
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>  
    /// Determines whether the specified entity is active.  
    /// </summary>  
    /// <param name="entity">The entity to check.</param>  
    /// <returns><c>true</c> if the entity is active; otherwise, <c>false</c>.</returns>  
    public static bool IsActive(this IEntity entity) =>
        entity.Status == EntityStatus.ACTIVE;
    /// <summary>  
    /// Determines whether the specified entity is deleted.  
    /// </summary>  
    /// <param name="entity">The entity to check.</param>  
    /// <returns><c>true</c> if the entity is deleted; otherwise, <c>false</c>.</returns>  
    public static bool IsDeleted(this IEntity entity) =>
        entity.Status == EntityStatus.DELETED;

    /// <summary>  
    /// Determines whether the specified entity is pending.  
    /// </summary>  
    /// <param name="entity">The entity to check.</param>  
    /// <returns><c>true</c> if the entity is pending; otherwise, <c>false</c>.</returns>  
    public static bool IsPending(this IEntity entity) =>
        entity.Status == EntityStatus.PENDING;

    /// <summary>  
    /// Determines whether the specified entity is suspended.  
    /// </summary>  
    /// <param name="entity">The entity to check.</param>  
    /// <returns><c>true</c> if the entity is suspended; otherwise, <c>false</c>.</returns>  
    public static bool IsSuspended(this IEntity entity) =>
        entity.Status == EntityStatus.SUSPENDED;

    /// <summary>
    /// Combines two expressions into a single expression.
    /// </summary>
    /// <typeparam name="TFirstParam">The type of the first parameter.</typeparam>
    /// <typeparam name="TIntermediate">The type of the intermediate result.</typeparam>
    /// <typeparam name="TResult">The type of the final result.</typeparam>
    /// <param name="first">The first expression.</param>
    /// <param name="second">The second expression.</param>
    /// <returns>A combined expression.</returns>
    public static Expression<Func<TFirstParam, TResult>> Combine
        <TFirstParam, TIntermediate, TResult>(
        this Expression<Func<TFirstParam, TIntermediate>> first,
        Expression<Func<TFirstParam, TIntermediate, TResult>> second)
    {
        ParameterExpression param = Expression
            .Parameter(typeof(TFirstParam), "param");

        Expression newFirst = first.Body
            .Replace(first.Parameters[0], param);

        Expression newSecond = second.Body
            .Replace(second.Parameters[0], param)
            .Replace(second.Parameters[1], newFirst);

        return Expression
            .Lambda<Func<TFirstParam, TResult>>(newSecond, param);
    }

    /// <summary>
    /// Composes two expressions into a single expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source parameter.</typeparam>
    /// <typeparam name="TCompose">The type of the intermediate result.</typeparam>
    /// <typeparam name="TResult">The type of the final result.</typeparam>
    /// <param name="source">The source expression.</param>
    /// <param name="compose">The compose expression.</param>
    /// <returns>A composed expression.</returns>
    public static Expression<Func<TSource, TResult>> Compose
        <TSource, TCompose, TResult>(
        this Expression<Func<TSource, TCompose>> source,
        Expression<Func<TCompose, TResult>> compose)
        where TCompose : notnull
    {
        ParameterExpression param = Expression.Parameter(typeof(TSource), null);
        InvocationExpression invoke = Expression.Invoke(source, param);
        InvocationExpression result = Expression.Invoke(compose, invoke);

        return Expression.Lambda<Func<TSource, TResult>>(result, param);
    }

    /// <summary>  
    /// Filters a sequence of values based on a predicate applied to a property 
    /// of the elements.  
    /// </summary>  
    /// <typeparam name="TSource">The type of the elements of source.</typeparam>
    /// <typeparam name="TParam">The type of the property to filter.</typeparam>
    /// <param name="source">An <see cref="IQueryable{T}"/> to filter.</param>
    /// <param name="propertyExpression">The property expression to filter.</param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>An <see cref="IQueryable{T}"/> that contains elements from the 
    /// input sequence that satisfy the condition.</returns>
    public static IQueryable<TSource> Where<TSource, TParam>(
       this IQueryable<TSource> source,
       Expression<Func<TSource, TParam>> propertyExpression,
       Expression<Func<TParam, bool>> predicate)
       where TParam : notnull =>
       source.Where(propertyExpression.Compose(predicate));

    private static Expression Replace(
           this Expression expression,
           Expression searchEx,
           Expression replaceEx)
           => new ReplaceVisitor(searchEx, replaceEx).Visit(expression);

    private sealed class ReplaceVisitor(Expression from, Expression to) :
        ExpressionVisitor
    {
        private readonly Expression from = from, to = to;

        public override Expression Visit(Expression? node)
            => node == from ? to : base.Visit(node)!;
    }
}
