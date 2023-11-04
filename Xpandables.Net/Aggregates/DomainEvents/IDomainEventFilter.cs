/************************************************************************************************************
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
************************************************************************************************************/
using System.ComponentModel;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.RegularExpressions;

using Xpandables.Net.Primitives;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates.DomainEvents;

/// <summary>
/// Specifies criteria with projection for domain event records.
/// </summary>
/// <typeparam name="TDomainEventRecord">The type of the record 
/// used to persist domain event.</typeparam>
public interface IDomainEventFilter<TDomainEventRecord>
    where TDomainEventRecord : class, IEntity
{
    /// <summary>
    /// Gets or sets the event unique identity.
    /// </summary>
    Guid? Id { get; init; }

    /// <summary>
    /// Gets or sets the aggregate identifier to search for.
    /// </summary>
    Guid? AggregateId { get; init; }

    /// <summary>
    /// Gets or sets the aggregate Id type name to search 
    /// for as <see cref="Regex"/> format. If null, all type will be checked.
    /// </summary>
    string? AggregateIdName { get; init; }

    /// <summary>
    /// Gets or sets the event type name to search for 
    /// as <see cref="Regex"/> format. If null, all type will be checked.
    /// </summary>
    string? EventTypeName { get; init; }

    /// <summary>
    /// Gets or sets the date to start search. 
    /// It can be used alone or combined with <see cref="ToCreatedOn"/>.
    /// </summary>
    DateTime? FromCreatedOn { get; init; }

    /// <summary>
    /// Gets or sets the date to end search. It can be used alone 
    /// or combined with <see cref="FromCreatedOn"/>.
    /// </summary>
    DateTime? ToCreatedOn { get; init; }

    /// <summary>
    /// Gets or sets the predicate to be applied on the Event Data content.
    /// </summary>
    /// <remarks>
    /// For example :
    /// <code>
    /// var criteria = new EventFilter{TEntity}()
    /// {
    ///     Id = id,
    ///     DataCriteria = x => x.RootElement.GetProperty("Version").GetUInt64() == version
    /// }
    /// </code>
    /// This is because Version is parsed as {"Version": 1 } and its value is of type <see cref="ulong"/>.
    /// </remarks>
    Expression<Func<JsonDocument, bool>>? DataCriteria { get; init; }

    /// <summary>
    /// Gets or sets the pagination.
    /// </summary>
    Pagination? Pagination { get; init; }

    ///<inheritdoc/>
    /// <remarks>Example : <code>OrderBy = x => x.OrderBy(o => o.Version);</code></remarks>
    Func<IQueryable<TDomainEventRecord>, IOrderedQueryable<TDomainEventRecord>>? OrderBy { get; init; }

    ///<inheritdoc/>
    /// <remarks>You can set the value to <code>Selector = x => x;</code> to return the same value.</remarks>
    Expression<Func<TDomainEventRecord, TDomainEventRecord>> Selector { get; init; }

    ///<inheritdoc/>
    Expression<Func<TDomainEventRecord, bool>> Criteria { get; init; }

    ///<inheritdoc/>
    public IQueryable<TDomainEventRecord> GetQueryableFiltered(
        IQueryable<TDomainEventRecord> queryable)
    {
        if (Criteria is not null)
            queryable = queryable.Where(Criteria);

        if (OrderBy is not null)
            queryable = OrderBy(queryable);

        if (Pagination is not null)
            queryable = queryable
                .Skip(Pagination.Value.Index * Pagination.Value.Size)
                .Take(Pagination.Value.Size);

        return queryable.Select(Selector);
    }
}

/// <summary>
/// Specifies criteria with projection for domain event records.
/// </summary>
/// <typeparam name="TDomainEventRecord">The type of the record 
/// used to persist domain event.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IDomainEventFilter<TDomainEventRecord, TResult>
    : IDomainEventFilter<TDomainEventRecord>
    where TDomainEventRecord : class, IEntity
{
    ///<inheritdoc/>
    /// <remarks>You can set the value to <code>Selector = x => x;</code> 
    /// to return the same value.</remarks>
    new Expression<Func<TDomainEventRecord, TResult>> Selector { get; init; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    Expression<Func<TDomainEventRecord, TDomainEventRecord>> IDomainEventFilter<TDomainEventRecord>.Selector
    { get => throw new NotSupportedException(); init => throw new NotSupportedException(); }

    ///<inheritdoc/>
    public new IQueryable<TResult> GetQueryableFiltered(
         IQueryable<TDomainEventRecord> queryable)
    {
        if (Criteria is not null)
            queryable = queryable.Where(Criteria);

        if (OrderBy is not null)
            queryable = OrderBy(queryable);

        if (Pagination is not null)
            queryable = queryable
                .Skip(Pagination.Value.Index * Pagination.Value.Size)
                .Take(Pagination.Value.Size);

        return queryable.Select(Selector);
    }
}
