
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

using Xpandables.Net.Events;

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
    /// <typeparam name="TResult">The type of the result model.</typeparam>
    /// <typeparam name="TIntermediate">The type of the intermediate model.
    /// </typeparam>
    /// <typeparam name="TFirstParam">The type of the first parameter.</typeparam>
    /// <param name="first">The first expression for composition.</param>
    /// <param name="second">The second expression for composition.</param>
    /// <returns>A statement matching the composition of target functions.</returns>
    public static Expression<Func<TFirstParam, TResult>> Combine
        <TFirstParam, TIntermediate, TResult>(
        this Expression<Func<TFirstParam, TIntermediate>> first,
        Expression<Func<TFirstParam, TIntermediate, TResult>> second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);

        ParameterExpression param = Expression
            .Parameter(typeof(TFirstParam), "param");

        Expression newFirst = first.Body.Replace(first.Parameters[0], param);
        Expression newSecond = second.Body.Replace(second.Parameters[0], param)
            .Replace(second.Parameters[1], newFirst);

        return Expression.Lambda<Func<TFirstParam, TResult>>(newSecond, param);
    }

    /// <summary>
    /// Combines the first expression to be used as parameter 
    /// for the second expression.
    /// </summary>
    /// <param name="source">The first expression for composition.</param>
    /// <param name="composeSource">The compose expression to apply 
    /// the source expression to.</param>
    /// <typeparam name="TSource">The type of the source model.</typeparam>
    /// <typeparam name="TCompose">The type of the compose model.</typeparam>
    /// <typeparam name="TResult">The type of the result model.</typeparam>
    /// <returns>A statement matching the composition of target functions
    /// .</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="composeSource"/> is null.</exception>
    public static Expression<Func<TSource, TResult>>
        Compose<TSource, TCompose, TResult>(
        this Expression<Func<TSource, TCompose>> source,
        Expression<Func<TCompose, TResult>> composeSource)
        where TCompose : notnull
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(composeSource);

        ParameterExpression param = Expression.Parameter(typeof(TSource), null);
        InvocationExpression invoke = Expression.Invoke(source, param);
        InvocationExpression result = Expression.Invoke(composeSource, invoke);

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

    internal static Expression Replace(
        this Expression expression,
        Expression searchEx,
        Expression replaceEx)
        => new ReplaceVisitor(searchEx, replaceEx).Visit(expression);

    internal class ReplaceVisitor(Expression from, Expression to) :
        ExpressionVisitor
    {
        private readonly Expression from = from, to = to;

        public override Expression Visit(Expression? node)
            => node == from ? to : base.Visit(node)!;
    }

    internal sealed class EventFilterEntityVisitor : ExpressionVisitor
    {
        internal static readonly ParameterExpression EventEntityParameter
            = Expression.Parameter(typeof(IEntityEventDomain));
        internal static readonly EventFilterEntityVisitor EventEntityVisitor
            = new(typeof(IEntityEventDomain), nameof(IEntityEventDomain.Data));

        internal readonly ParameterExpression Parameter;
        private readonly Expression _expression;

        internal EventFilterEntityVisitor(Type parameterType, string member)
        {
            Parameter = Expression.Parameter(parameterType);
            _expression = Expression.PropertyOrField(Parameter, member);
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node.Type == _expression.Type
            ? _expression
            : base.VisitParameter(node);
    }
}