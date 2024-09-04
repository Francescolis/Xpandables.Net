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
using System.Text.Json;

using Xpandables.Net.Expressions;
using Xpandables.Net.Primitives;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents a filter for events.
/// </summary>
/// <typeparam name="TEntityEvent">The type of entity event.</typeparam>
public abstract record EventFilter<TEntityEvent> : IEventFilter<TEntityEvent>
    where TEntityEvent : class, IEntityEvent
{
    /// <inheritdoc/>
    public abstract Type Type { get; }

    /// <inheritdoc/>
    public Guid? KeyId { get; set; }

    /// <inheritdoc/>
    public string? AggregateName { get; set; }

    /// <inheritdoc/>
    public Expression<Func<JsonDocument, bool>>? DataCriteria { get; set; }

    /// <inheritdoc/>
    public string? EventTypeName { get; set; }

    /// <inheritdoc/>
    public string? Status { get; set; }

    /// <inheritdoc/>
    public DateTime? FromCreatedOn { get; set; }

    /// <inheritdoc/>
    public Guid? EventId { get; set; }

    /// <inheritdoc/>
    public Pagination? Paging { get; set; }

    /// <inheritdoc/>
    public DateTime? ToCreatedOn { get; set; }

    /// <inheritdoc/>
    public ulong? Version { get; set; }

    /// <inheritdoc/>
    public bool? OnError { get; set; }

    ///<summary>
    /// The Criteria returns the <see cref="CriteriaCriteria"/> value.
    ///</summary>
    ///<inheritdoc/>
    public Expression<Func<TEntityEvent, bool>>? Criteria
        => CriteriaCriteria().GetExpression();

    ///<summary>
    /// The default selector returns the same entity.
    ///</summary>
    ///<inheritdoc/>
    public virtual Expression<Func<TEntityEvent, TEntityEvent>> Selector => x => x;

    ///<summary>
    /// The default order use the <see cref="IEntityEvent.Version"/> property.
    ///</summary>
    ///<inheritdoc/>
    public virtual
        Func<IQueryable<TEntityEvent>, IOrderedQueryable<TEntityEvent>>? OrderBy
        => x => x.OrderBy(o => o.Version);

    /// <summary>
    /// When overridden in a derived class, determines whether the filter 
    /// instance can be applied on the specified object type.
    /// </summary>
    /// <param name="typeToFilter">The type of the object to apply filter.</param>
    /// <returns><see langword="true"/> if the instance can apply filters the 
    /// specified object type; otherwise, <see langword="false"/>.</returns>
    public abstract bool CanFilter(Type typeToFilter);

    /// <summary>
    /// When overridden in a derived class, builds the criteria for the filter.
    ///</summary>
    protected virtual QueryExpression<TEntityEvent> CriteriaCriteria()
    {
        QueryExpression<TEntityEvent> expression
            = QueryExpressionFactory.Create<TEntityEvent>();

        if (EventId is not null)
        {
            expression = expression.And(x => x.Id == EventId);
        }

        if (EventTypeName is not null)
        {
            expression = expression.And(x =>
                x.EventTypeName.Contains(EventTypeName));
        }

        if (Version is not null)
        {
            expression = expression.And(x =>
            x.Version > Version.Value);
        }

        if (FromCreatedOn is not null)
        {
            expression = expression.And(x =>
            x.CreatedOn >= FromCreatedOn.Value);
        }

        if (ToCreatedOn is not null)
        {
            expression = expression.And(x =>
            x.CreatedOn <= ToCreatedOn.Value);
        }

        if (Status is not null)
        {
            expression = expression.And(x =>
            x.Status.Contains(Status));
        }

        if (DataCriteria is not null)
        {
            expression = expression.And(
                RepositoryExtensions
                .Compose<TEntityEvent, JsonDocument, bool>(
                    x => x.Data,
                    DataCriteria));
        }

        return expression;
    }
}