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

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents a filter for events.
/// </summary>
public record EventFilter : IEventFilter
{
    /// <inheritdoc/>
    public Guid? AggregateId { get; init; }

    /// <inheritdoc/>
    public string? AggregateName { get; init; }

    /// <inheritdoc/>
    public Expression<Func<JsonDocument, bool>>? DataCriteria { get; init; }

    /// <inheritdoc/>
    public string? EventTypeName { get; init; }

    /// <inheritdoc/>
    public string? Status { get; init; }

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

    /// <inheritdoc/>
    public bool? OnError { get; init; }
}