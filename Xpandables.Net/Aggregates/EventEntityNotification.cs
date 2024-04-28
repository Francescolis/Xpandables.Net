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
using System.Text.Json;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents a notification event entity.
/// </summary>
/// <remarks>
/// Initializes a new instance of <see cref="EventEntityNotification"/> with
/// the specified values.
/// </remarks>
/// <param name="id">The identifier of the event.</param>
/// <param name="eventTypeName">The name of the event type.</param>
/// <param name="eventTypeFullName">The full name of the event type.</param>
/// <param name="version">The version of the event.</param>
/// <param name="data">The data of the event.</param>
/// <param name="errorMessage">The error message of the event.</param>
public sealed class EventEntityNotification(
    Guid id,
    string eventTypeName,
    string eventTypeFullName,
    ulong version,
    JsonDocument data,
    string? errorMessage = default) :
    EventEntity(id, eventTypeName, eventTypeFullName, version, data), IEventEntityNotification
{
    ///<inheritdoc/>
    public string? ErrorMessage { get; set; } = errorMessage;
}
