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
using System.Text.Json;

namespace Xpandables.Net.Events.Providers.MSSql;

/// <summary>
/// Represents a class for MSSql event entities in a domain context.
/// <para>It uses <see cref="Guid"/> as Aggregate Id and Key, and 
/// <see cref="byte"/> array as timestamp</para>
/// </summary>
/// <remarks>
/// Initializes a new instance of the 
/// <see cref="EventEntityDomain"/> class.
/// </remarks>
/// <param name="aggregateId">The aggregate identifier.</param>
/// <param name="key">The key of the event entity.</param>
/// <param name="eventName">The name of the event.</param>
/// <param name="eventFullName">The full name of the event.</param>
/// <param name="version">The version of the event.</param>
/// <param name="eventData">The data of the event.</param>
public sealed class EventEntityDomain(
    Guid aggregateId,
    Guid key,
    string eventName,
    string eventFullName,
    ulong version,
    JsonDocument eventData) :
    EventEntity<Guid, byte[]>(key, eventName, eventFullName, version, eventData),
    IEventEntityDomain<Guid, Guid, byte[]>
{
    /// <inheritdoc/>
    public Guid AggregateId { get; } = aggregateId;
}
