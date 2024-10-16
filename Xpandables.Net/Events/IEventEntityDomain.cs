/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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
using System.ComponentModel;

namespace Xpandables.Net.Events;
/// <summary>
/// Represents an event entity domain that includes aggregate information.
/// </summary>
public interface IEventEntityDomain : IEventEntity
{
    /// <summary>
    /// Gets the name of the aggregate.
    /// </summary>
    string AggregateName { get; }

    /// <summary>
    /// Gets the identifier of the aggregate.
    /// </summary>
    object AggregateId { get; }
}

/// <summary>
/// Represents an event entity domain with a specific key and timestamp type.
/// </summary>
/// <typeparam name="TAggregateId">The type of the aggregate identifier.</typeparam>
/// <typeparam name="TKey">The type of the aggregate identifier.</typeparam>
/// <typeparam name="TTimeStamp">The type of the timestamp.</typeparam>
public interface IEventEntityDomain<out TAggregateId, out TKey, out TTimeStamp> :
    IEventEntityDomain
    where TKey : notnull, IComparable
    where TTimeStamp : notnull
    where TAggregateId : struct
{
    /// <summary>
    /// Gets the identifier of the aggregate.
    /// </summary>
    new TAggregateId AggregateId { get; }

    [EditorBrowsable(EditorBrowsableState.Never)]
    object IEventEntityDomain.AggregateId => AggregateId;
}