
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

using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aggregates.Notifications;

/// <summary>
/// Provides with base criteria for entity notification filtering.
/// </summary>
public interface INotificationFilter
{
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
    /// Gets or sets the date to end search. It can be used alone 
    /// or combined with <see cref="FromCreatedOn"/>.
    /// </summary>
    DateTime? ToCreatedOn { get; init; }

    /// <summary>
    /// Gets or sets the event unique identity.
    /// </summary>
    Guid? Id { get; init; }

    /// <summary>
    /// Gets or sets the pagination.
    /// </summary>
    Pagination? Pagination { get; init; }

    /// <summary>
    /// Gets or sets the minimal version. 
    /// </summary>
    /// <remarks>Not used</remarks>
    ulong? Version { get; init; }
}