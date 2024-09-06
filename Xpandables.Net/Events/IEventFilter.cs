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

using Xpandables.Net.Primitives.Collections;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// Provides with criteria for event filtering.
/// </summary>
public interface IEventFilter : IEntityFilter
{
    /// <summary>
    /// Gets the event entity type being filtered by the current filter instance.
    /// </summary>
    Type Type { get; }

    /// <summary>
    /// Gets or sets the aggregate unique identity.
    /// </summary>
    Guid? KeyId { get; set; }

    /// <summary>
    /// Gets or sets the aggregate type name to search for.
    /// </summary>
    string? AggregateName { get; set; }

    /// <summary>
    /// Gets or sets the predicate to be applied on the Event Data content.
    /// </summary>
    /// <remarks>
    /// For example :
    /// <code>
    /// var criteria = new EventFilter()
    /// {
    ///     Id = id,
    ///     DataCriteria = x => x.RootElement.GetProperty("Version")
    ///     .GetUInt64() == version
    /// }
    /// </code>
    /// This is because Version is parsed as {"Version": 1 } and 
    /// its value is of type <see cref="ulong"/>.
    /// </remarks>
    Expression<Func<JsonDocument, bool>>? DataCriteria { get; set; }

    /// <summary>
    /// Gets or sets the event type name to search for.
    /// If null, all type will be checked.
    /// </summary>
    string? EventTypeName { get; set; }

    /// <summary>
    /// Gets or sets the event status to search for.
    /// If null, all status will be checked.
    /// </summary>
    string? Status { get; set; }

    /// <summary>
    /// Gets or sets the date to start search. 
    /// It can be used alone or combined with <see cref="ToCreatedOn"/>.
    /// </summary>
    DateTime? FromCreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the date to end search. It can be used alone 
    /// or combined with <see cref="FromCreatedOn"/>.
    /// </summary>
    DateTime? ToCreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the event unique identity.
    /// </summary>
    Guid? EventId { get; set; }

    /// <summary>
    /// Gets or sets the minimal version. 
    /// </summary>
    ulong? Version { get; set; }

    /// <summary>
    /// If defined, the filter will return only the records that have an error 
    /// (<see langword="true"/>) or not (<see langword="false"/>).
    /// </summary>
    bool? OnError { get; set; }

    /// <summary>
    /// When overridden in a derived class, determines whether the filter 
    /// instance can be applied on the specified object type.
    /// </summary>
    /// <param name="typeToFilter">The type of the object to apply filter.</param>
    /// <returns><see langword="true"/> if the instance can apply filters to the 
    /// specified object type; otherwise, <see langword="false"/>.</returns>
    bool CanFilter(Type typeToFilter);

    /// <summary>
    /// Fetches the entity events based on the specified queryable.
    /// </summary>
    /// <param name="queryable">The queryable to act on.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> that allows
    /// enumeration of the entity events.</returns>
    IEnumerable<IEntityEvent> Fetch(IQueryable queryable);

    /// <summary>
    /// Fetches the entity events asynchronously based on the specified queryable.
    /// </summary>
    /// <param name="queryable">The queryable to act on.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that allows asynchronous
    /// enumeration of the entity events.</returns>
    IAsyncEnumerable<IEntityEvent> FetchAsync(IQueryable queryable);
}

/// <summary>
/// Provides with criteria for generic entity event filtering.
/// </summary>
/// <typeparam name="TEntityEvent">The type of the entity event to filter
/// .</typeparam>
public interface IEventFilter<TEntityEvent> : IEventFilter, IEntityFilter<TEntityEvent>
    where TEntityEvent : class, IEntityEvent
{
    /// <summary>
    /// Gets the event entity type being filtered by the current filter instance.
    /// </summary>
    new Type Type { get; }

    Type IEventFilter.Type => Type;

    /// <summary>
    /// Fetches the entity events based on the specified queryable.
    /// </summary>
    /// <param name="queryable">The queryable to act on.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that allows asynchronous
    /// enumeration of the entity events.</returns>
    public virtual IEnumerable<IEntityEvent> Fetch(
        IQueryable<TEntityEvent> queryable)
    {
        ArgumentNullException.ThrowIfNull(queryable);

        return Apply(queryable)
            .AsEnumerable();
    }

    /// <summary>
    /// Fetches the entity events asynchronously based on the specified queryable.
    /// </summary>
    /// <param name="queryable">The queryable to act on.</param>
    /// <returns>An <see cref="IAsyncEnumerable{T}"/> that allows asynchronous
    /// enumeration of the entity events.</returns>
    public virtual IAsyncEnumerable<IEntityEvent> FetchAsync(
        IQueryable<TEntityEvent> queryable)
    {
        ArgumentNullException.ThrowIfNull(queryable);

        return Apply(queryable)
            .ToAsyncEnumerable();
    }

    IEnumerable<IEntityEvent> IEventFilter.Fetch(IQueryable queryable)
        => Fetch(queryable.OfType<TEntityEvent>());

    IAsyncEnumerable<IEntityEvent> IEventFilter.FetchAsync(IQueryable queryable)
        => FetchAsync(queryable.OfType<TEntityEvent>());
}