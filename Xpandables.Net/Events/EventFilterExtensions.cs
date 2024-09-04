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

using Xpandables.Net.Primitives.Collections;

namespace Xpandables.Net.Events;

/// <summary>
/// Provides a set of static methods for event filter.
/// </summary>
public static class EventFilterExtensions
{
    /// <summary>
    /// Applies the event filter to the queryable.
    /// </summary>
    /// <param name="eventFilter">The event filter to apply.</param>
    /// <param name="queryable">The queryable to apply the filter.</param>
    /// <exception cref="ArgumentNullException">The event filter or 
    /// queryable is null.</exception>
    public static IAsyncEnumerable<IEntityEvent> ApplyFilter(
        this IEventFilter eventFilter,
        IQueryable queryable)
    {
        ArgumentNullException.ThrowIfNull(eventFilter);
        ArgumentNullException.ThrowIfNull(queryable);

        return eventFilter.Type switch
        {
            Type type when type == typeof(IEventDomain)
                => DoFetchAsync(eventFilter, queryable.OfType<EntityEventDomain>()),
            Type type when type == typeof(IEventIntegration)
                => DoFetchAsync(eventFilter, queryable.OfType<EntityEventIntegration>()),
            Type type when type == typeof(IEventSnapshot)
                => DoFetchAsync(eventFilter, queryable.OfType<EntityEventSnapshot>()),
            _ => throw new InvalidOperationException(
                $"The type {eventFilter.Type} is not supported.")
        };

        static IAsyncEnumerable<IEntityEvent> DoFetchAsync<TEntityEvent>(
            IEventFilter filter,
            IQueryable<TEntityEvent> queryable)
            where TEntityEvent : class, IEntityEvent
        {
            IQueryable<TEntityEvent> queryableResult =
                filter
                    .Apply(queryable)
                    .OfType<TEntityEvent>();

            return queryableResult
                .ToAsyncEnumerable();
        }
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
