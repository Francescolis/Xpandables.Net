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
using System.Text.RegularExpressions;

using Xpandables.Net.Aggregates.DomainEvents;
using Xpandables.Net.Operations.Expressions;

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aggregates.Defaults;

/// <summary>
/// Specifies criteria with projection for domain event records.
/// </summary>
public record DomainEventFilter : DomainEventFilter<DomainEventRecord>
{
    ///<inheritdoc/>
    public sealed override Expression<Func<DomainEventRecord, DomainEventRecord>> Selector => x => x;
}

/// <summary>
/// Specifies criteria with projection for domain event records to a specific result.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <remarks>You must at least provides a value for the "Selector".</remarks>
public record DomainEventFilter<TResult> : IDomainEventFilter<DomainEventRecord, TResult>
{
    internal static readonly ParameterExpression EventEntityParameter = Expression.Parameter(typeof(DomainEventRecord));
    internal static readonly EntityVisitor EventEntityVisitor = new(typeof(DomainEventRecord), nameof(DomainEventRecord.Data));

    /// <summary>
    /// Creates a new instance of <see cref="DomainEventFilter{TResult}"/> to filter domain event records 
    /// and return result of <typeparamref name="TResult"/> type.
    /// </summary>
    /// <remarks>You must at least provides a value for the "Selector".</remarks>
    public DomainEventFilter() { }

    ///<inheritdoc/>
    public Guid? AggregateId { get; init; }

    ///<inheritdoc/>
    public Guid? Id { get; init; }

    ///<inheritdoc/>
    public string? AggregateIdName { get; init; }

    ///<inheritdoc/>
    public string? EventTypeName { get; init; }

    ///<inheritdoc/>
    public DateTime? FromCreatedOn { get; init; }

    ///<inheritdoc/>
    public DateTime? ToCreatedOn { get; init; }

    ///<inheritdoc/>
    public Expression<Func<JsonDocument, bool>>? DataCriteria { get; init; }

    ///<inheritdoc/>
    public Pagination? Pagination { get; init; }

    ///<inheritdoc/>
    public Func<IQueryable<DomainEventRecord>, IOrderedQueryable<DomainEventRecord>>? OrderBy { get; init; }

    ///<inheritdoc/>
    public virtual Expression<Func<DomainEventRecord, TResult>> Selector { get; init; } = default!;

    readonly Expression<Func<DomainEventRecord, bool>>? _criteria;

    ///<inheritdoc/>
    public Expression<Func<DomainEventRecord, bool>> Criteria
    {
        get => _criteria is not null ? GetExpression().And(_criteria) : GetExpression();
        init => _criteria = value;
    }

    /// <summary>
    /// Returns the expression criteria based on the underlying arguments.
    /// </summary>
    /// <returns>An object of <see cref="Expression{TDelegate}"/>.</returns>
    protected QueryExpression<DomainEventRecord, bool> GetExpression()
    {
        var expression = QueryExpressionFactory.Create<DomainEventRecord>();

        if (AggregateId is not null)
            expression = expression.And(x => x.AggregateId == AggregateId.Value);

        if (AggregateIdName is not null)
            expression = expression.And(x => Regex.IsMatch(AggregateIdName, x.AggregateIdName));

        if (Id is not null)
            expression = expression.And(x => x.Id == Id);

        if (EventTypeName is not null)
            expression = expression.And(x => Regex.IsMatch(EventTypeName, x.TypeName));

        if (FromCreatedOn is not null)
            expression = expression.And(x => x.CreatedOn >= FromCreatedOn.Value);

        if (ToCreatedOn is not null)
            expression = expression.And(x => x.CreatedOn <= ToCreatedOn.Value);

        if (DataCriteria is not null)
        {
            _ = Expression.Invoke(
                DataCriteria,
                Expression.PropertyOrField(EventEntityParameter, nameof(DomainEventRecord.Data)));

            var dataCriteria = Expression.Lambda<Func<DomainEventRecord, bool>>(
                EventEntityVisitor.Visit(DataCriteria.Body),
                EventEntityVisitor.Parameter);

            expression = expression.And(dataCriteria);
        }

        return expression;
    }
}