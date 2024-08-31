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
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Xpandables.Net.Events;
/// <summary>
/// Represents a domain event entity.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="EntityEventDomain"/> with 
/// the specified values.
/// </remarks>
/// <param name="id">The identifier of the event.</param>
/// <param name="version">The version of the event.</param>
/// <param name="aggregateId">The identifier of the aggregate.</param>
/// <param name="aggregateTypeName">The type name of the aggregate.</param>
/// <param name="data">The data of the event.</param>
/// <param name="eventTypeFullName">The full name of the event type.</param>
/// <param name="eventTypeName">The name of the event type.</param>
public sealed class EntityEventDomain(
    Guid id,
    string eventTypeName,
    string eventTypeFullName,
    ulong version,
    JsonDocument data,
    Guid aggregateId,
    string aggregateTypeName) :
    EntityEvent(id, eventTypeName, eventTypeFullName, version, data), IEntityEventDomain
{

    ///<inheritdoc/>
    public Guid AggregateId { get; } = aggregateId;

    ///<inheritdoc/>
    public string AggregateTypeName { get; } = aggregateTypeName;
}

/// <summary>
/// Represents an integration event entity.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="EntityEventIntegration"/> with
/// the specified values.
/// </remarks>
/// <param name="id">The identifier of the event.</param>
/// <param name="eventTypeName">The name of the event type.</param>
/// <param name="eventTypeFullName">The full name of the event type.</param>
/// <param name="version">The version of the event.</param>
/// <param name="data">The data of the event.</param>
/// <param name="errorMessage">The error message of the event.</param>
public sealed class EntityEventIntegration(
    Guid id,
    string eventTypeName,
    string eventTypeFullName,
    ulong version,
    JsonDocument data,
    string? errorMessage = default) :
    EntityEvent(id, eventTypeName, eventTypeFullName, version, data),
    IEntityEventIntegration
{
    ///<inheritdoc/>
    public string? ErrorMessage { get; set; } = errorMessage;
}

/// <summary>
/// Represents a snapshot event entity.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="EntityEventSnapshot"/> with
/// the specified values.
/// </remarks>
/// <param name="id">The identifier of the event.</param>
/// <param name="eventTypeName">The name of the event type.</param>
/// <param name="eventTypeFullName">The full name of the event type.</param>
/// <param name="data">The data of the event.</param>
/// <param name="keyId">The object identifier.</param>
/// <param name="version">The version of the event.</param>
public sealed class EntityEventSnapshot(
    Guid id,
    string eventTypeName,
    string eventTypeFullName,
    ulong version,
    JsonDocument data,
    Guid keyId) :
    EntityEvent(id, eventTypeName, eventTypeFullName, version, data),
    IEntityEventSnapshot
{

    ///<inheritdoc/>
    [Key]
    public Guid KeyId { get; } = keyId;
}