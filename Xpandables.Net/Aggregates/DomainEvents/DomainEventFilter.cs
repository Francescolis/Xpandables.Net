﻿/************************************************************************************************************
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

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aggregates.DomainEvents;

/// <summary>
/// Specifies criteria with projection for domain event records to a specific result.
/// </summary>
/// <remarks>You must at least provides a value for the "Selector".</remarks>
public record DomainEventFilter : IDomainEventFilter
{
    /// <summary>
    /// Creates a new instance of <see cref="DomainEventFilter"/> to filter domain event records.
    /// </summary>
    public DomainEventFilter() { }

    /// <inheritdoc/>
    public Guid? AggregateId { get; init; }

    /// <inheritdoc/>
    public string? AggregateIdTypeName { get; init; }

    /// <inheritdoc/>
    public Expression<Func<JsonDocument, bool>>? DataCriteria { get; init; }

    /// <inheritdoc/>
    public string? EventTypeName { get; init; }

    /// <inheritdoc/>
    public DateTime? FromCreatedOn { get; init; }

    /// <inheritdoc/>
    public Guid? Id { get; init; }

    /// <inheritdoc/>
    public Pagination? Pagination { get; init; }

    /// <inheritdoc/>
    public DateTime? ToCreatedOn { get; init; }

    /// <inheritdoc/>
    public ulong? Version { get; init; }


    ///// <summary>
    ///// Returns the expression criteria based on the underlying arguments.
    ///// </summary>
    ///// <returns>An object of <see cref="Expression{TDelegate}"/>.</returns>
    //protected override QueryExpression<DomainEventRecord, bool> GetExpression()
    //{
    //    var expression = QueryExpressionFactory.Create<DomainEventRecord>();

    //    if (AggregateId is not null)
    //        expression = expression.And(x =>
    //        x.AggregateId == AggregateId.Value);

    //    if (AggregateIdTypeName is not null)
    //        expression = expression.And(x => x.AggregateIdTypeName == AggregateIdTypeName);

    //    if (Id is not null)
    //        expression = expression.And(x => x.Id == Id);

    //    if (EventTypeName is not null)
    //        expression = expression.And(x => x.EventTypeName == EventTypeName);

    //    if (Version is not null)
    //        expression = expression.And(x =>
    //        x.Version > Version.Value);

    //    if (FromCreatedOn is not null)
    //        expression = expression.And(x =>
    //        x.CreatedOn >= FromCreatedOn.Value);

    //    if (ToCreatedOn is not null)
    //        expression = expression.And(x =>
    //        x.CreatedOn <= ToCreatedOn.Value);

    //    if (DataCriteria is not null)
    //    {
    //        _ = Expression.Invoke(
    //            DataCriteria,
    //            Expression.PropertyOrField(
    //                EventFilterEntityVisitor.EventEntityParameter,
    //                nameof(DomainEventRecord.Data)));

    //        var dataCriteria = Expression.Lambda<Func<DomainEventRecord, bool>>(
    //            EventFilterEntityVisitor.EventEntityVisitor.Visit(DataCriteria.Body),
    //            EventFilterEntityVisitor.EventEntityVisitor.Parameter);

    //        expression = expression.And(dataCriteria);
    //    }

    //    return expression;
    //}
}