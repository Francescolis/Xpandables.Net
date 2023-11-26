
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

using Xpandables.Net.Extensions;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Provides with base criteria for domain event filtering.
/// </summary>
public interface IEventFilter
{
    /// <summary>
    /// Gets or sets the event unique identity.
    /// </summary>
    Guid? AggregateId { get; init; }

    /// <summary>
    /// Gets or sets the aggregate identifier type name to search for.
    /// </summary>
    string? AggregateIdTypeName { get; init; }

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
    /// Gets or sets the event type name to search for.
    /// If null, all type will be checked.
    /// </summary>
    string? EventTypeName { get; init; }

    /// <summary>
    /// Gets or sets the date to start search. 
    /// It can be used alone or combined with <see cref="ToCreatedOn"/>.
    /// </summary>
    DateTime? FromCreatedOn { get; init; }

    /// <summary>
    /// Gets or sets the event unique identity.
    /// </summary>
    Guid? Id { get; init; }

    /// <summary>
    /// Gets or sets the pagination.
    /// </summary>
    Pagination? Pagination { get; init; }

    /// <summary>
    /// Gets or sets the date to end search. It can be used alone 
    /// or combined with <see cref="FromCreatedOn"/>.
    /// </summary>
    DateTime? ToCreatedOn { get; init; }

    /// <summary>
    /// Gets or sets the minimal version. 
    /// </summary>
    ulong? Version { get; init; }

    /// <summary>
    /// Gets the filtered queryable.
    /// </summary>
    public IQueryable GetQueryableFiltered(IQueryable queryable)
    {
        ArgumentNullException.ThrowIfNull(queryable);

        if (AggregateId is not null)
        {
            var aggregateIfFilter = XpandablesExtensions
                .CreateFilterEqualExpression(nameof(AggregateId), AggregateId.Value);
            queryable = queryable.ApplyFilter(aggregateIfFilter);
        }

        if (AggregateIdTypeName is not null)
        {
            var aggregateIdTypeNameFilter = XpandablesExtensions
                .CreateFilterEqualExpression(nameof(AggregateIdTypeName), AggregateIdTypeName);
            queryable = queryable.ApplyFilter(aggregateIdTypeNameFilter);
        }

        if (Id is not null)
        {
            var idFilter = XpandablesExtensions
                .CreateFilterEqualExpression(nameof(Id), Id.Value);
            queryable = queryable.ApplyFilter(idFilter);
        }

        if (EventTypeName is not null)
        {
            var eventTypeNameFilter = XpandablesExtensions
                .CreateFilterEqualExpression(nameof(EventTypeName), EventTypeName);
            queryable = queryable.ApplyFilter(eventTypeNameFilter);
        }

        if (Version is not null)
        {
            var versionFilter = XpandablesExtensions
                .CreateFilterGreaterThanExpression(nameof(Version), Version.Value);
            queryable = queryable.ApplyFilter(versionFilter);
        }

        if (FromCreatedOn is not null)
        {
            var fromCreatedOnFilter = XpandablesExtensions
                .CreateFilterGreaterThanOrEqualExpression("CreatedOn", FromCreatedOn.Value);
            queryable = queryable.ApplyFilter(fromCreatedOnFilter);
        }

        if (ToCreatedOn is not null)
        {
            var toCreatedOnFilter = XpandablesExtensions
                .CreateFilterLessThanOrEqualExpression("CreatedOn", ToCreatedOn.Value);
            queryable = queryable.ApplyFilter(toCreatedOnFilter);
        }

        return queryable;
    }
}

/// <summary>
/// Provides with base criteria for domain event filtering.
/// </summary>
/// <typeparam name="TEventRecord">The type of event record.</typeparam>
public interface IEventFilter<TEventRecord> : IEventFilter
    where TEventRecord : class
{
    ///<inheritdoc/>
    /// <remarks>Example : <code>OrderBy = x => x.OrderBy(o => o.Version);</code></remarks>
    Func<IQueryable<TEventRecord>, IQueryable<TEventRecord>>? OrderBy { get; init; }

    ///<inheritdoc/>
    /// <remarks>You can set the value to <code>Selector = x => x;</code> to return the same value.</remarks>
    Expression<Func<TEventRecord, TEventRecord>> Selector { get; init; }

    ///<inheritdoc/>
    Expression<Func<TEventRecord, bool>> Criteria { get; init; }

    /// <summary>
    /// Gets the filtered queryable.
    /// </summary>
    IQueryable<TEventRecord> GetQueryableFiltered(IQueryable<TEventRecord> queryable);

    [EditorBrowsable(EditorBrowsableState.Never)]
    IQueryable IEventFilter.GetQueryableFiltered(IQueryable queryable)
        => GetQueryableFiltered((IQueryable<TEventRecord>)queryable);
}

/// <summary>
/// Specifies criteria with projection for domain event records.
/// </summary>
/// <typeparam name="TEventRecord">The type of the record 
/// used to persist domain event.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface IEventFilter<TEventRecord, TResult> : IEventFilter<TEventRecord>
    where TEventRecord : class
{
    ///<inheritdoc/>
    new Expression<Func<TEventRecord, TResult>> Selector { get; init; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    Expression<Func<TEventRecord, TEventRecord>> IEventFilter<TEventRecord>.Selector
    {
        get => throw new NotSupportedException();
        init => throw new NotSupportedException();
    }

    /// <summary>
    /// Gets the filtered queryable.
    /// </summary>
    new IQueryable<TResult> GetQueryableFiltered(IQueryable<TEventRecord> queryable);

    [EditorBrowsable(EditorBrowsableState.Never)]
    IQueryable<TEventRecord> IEventFilter<TEventRecord>.GetQueryableFiltered(
        IQueryable<TEventRecord> queryable) => queryable;
}