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
using System.Linq.Expressions;
using System.Text.Json;

using Xpandables.Net.Operations.Expressions;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aggregates.Defaults;

/// <summary>
/// Provides with base criteria for domain event filtering.
/// </summary>
public record EventFilter : IEventFilter
{
    ///<inheritdoc/>
    public Guid? Id { get; init; }

    ///<inheritdoc/>
    public Guid? AggregateId { get; init; }

    ///<inheritdoc/>
    public string? AggregateIdTypeName { get; init; }

    ///<inheritdoc/>
    public string? EventTypeName { get; init; }

    ///<inheritdoc/>
    public ulong? Version { get; init; }

    ///<inheritdoc/>
    public DateTime? FromCreatedOn { get; init; }

    ///<inheritdoc/>
    public DateTime? ToCreatedOn { get; init; }

    ///<inheritdoc/>
    public Expression<Func<JsonDocument, bool>>? DataCriteria { get; init; }

    ///<inheritdoc/>
    public Pagination? Pagination { get; init; }
}

/// <summary>
/// Provides with base criteria for domain event filtering.
/// </summary>
/// <typeparam name="TEventRecord">The type of the record 
/// used to persist domain event.</typeparam>
public abstract record EventFilter<TEventRecord> : EventFilter, IEventFilter<TEventRecord>
    where TEventRecord : class
{
    ///<inheritdoc/>
    public Func<IQueryable<TEventRecord>, IQueryable<TEventRecord>>? OrderBy { get; init; }

    ///<inheritdoc/>
    public Expression<Func<TEventRecord, TEventRecord>> Selector { get; init; } = default!;

    readonly Expression<Func<TEventRecord, bool>>? _criteria;

    ///<inheritdoc/>
    public Expression<Func<TEventRecord, bool>> Criteria
    {
        get => _criteria is not null ? GetExpression().And(_criteria) : GetExpression();
        init => _criteria = value;
    }

    /// <summary>
    /// Returns the expression criteria based on the underlying arguments.
    /// </summary>
    /// <returns>An object of <see cref="Expression{TDelegate}"/>.</returns>
    protected abstract QueryExpression<TEventRecord, bool> GetExpression();

    ///<inheritdoc/>
    public IQueryable<TEventRecord> GetQueryableFiltered(
        IQueryable<TEventRecord> queryable)
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
/// Provides with base criteria for domain event filtering.
/// </summary>
/// <typeparam name="TEventRecord">The type of the record 
/// used to persist domain event.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
public abstract record EventFilter<TEventRecord, TResult> : EventFilter<TEventRecord>, IEventFilter<TEventRecord, TResult>
    where TEventRecord : class
{
    ///<inheritdoc/>
    public new Expression<Func<TEventRecord, TResult>> Selector { get; init; } = default!;

    ///<inheritdoc/>
    public new IQueryable<TResult> GetQueryableFiltered(IQueryable<TEventRecord> queryable)
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