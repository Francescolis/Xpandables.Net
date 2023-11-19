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

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aggregates.Defaults;

/// <summary>
/// Provides with base criteria for domain event filtering.
/// </summary>
public record DomainEventFilterCriteria
{
    internal static readonly ParameterExpression EventEntityParameter = Expression.Parameter(typeof(DomainEventRecord));
    internal static readonly EntityVisitor EventEntityVisitor = new(typeof(DomainEventRecord), nameof(DomainEventRecord.Data));

    /// <summary>
    /// Gets or sets the event unique identity.
    /// </summary>
    public Guid? Id { get; init; }

    /// <summary>
    /// Gets or sets the aggregate identifier to search for.
    /// </summary>
    public Guid? AggregateId { get; init; }

    /// <summary>
    /// Gets or sets the aggregate Id type name to search 
    /// for as <see cref="Regex"/> format. If null, all type will be checked.
    /// </summary>
    public string? AggregateIdName { get; init; }

    /// <summary>
    /// Gets or sets the event type name to search for 
    /// as <see cref="Regex"/> format. If null, all type will be checked.
    /// </summary>
    public string? EventTypeName { get; init; }

    /// <summary>
    /// Gets or sets the minimal version. 
    /// </summary>
    public ulong? Version { get; init; }

    /// <summary>
    /// Gets or sets the date to start search. 
    /// It can be used alone or combined with <see cref="ToCreatedOn"/>.
    /// </summary>
    public DateTime? FromCreatedOn { get; init; }

    /// <summary>
    /// Gets or sets the date to end search. It can be used alone 
    /// or combined with <see cref="FromCreatedOn"/>.
    /// </summary>
    public DateTime? ToCreatedOn { get; init; }

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
    public Expression<Func<JsonDocument, bool>>? DataCriteria { get; init; }

    /// <summary>
    /// Gets or sets the pagination.
    /// </summary>
    public Pagination? Pagination { get; init; }
}

internal sealed class EntityVisitor : ExpressionVisitor
{
    internal readonly ParameterExpression Parameter;
    private readonly Expression _expression;

    internal EntityVisitor(Type parameterType, string member)
    {
        Parameter = Expression.Parameter(parameterType);
        _expression = Expression.PropertyOrField(Parameter, member);
    }

    protected override Expression VisitParameter(ParameterExpression node)
        => node.Type == _expression.Type ? _expression : base.VisitParameter(node);
}