
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
using System.Diagnostics;
using System.Text.Json;

using Xpandables.Net.Extensions;
using Xpandables.Net.IntegrationEvents;
using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates.EntityFramework;

/// <summary>
/// Represents a integration event to be written.
/// Make use of <see langword="using"/> key work when call or call dispose method.
/// </summary>
[DebuggerDisplay("Id = {" + nameof(Id) + "}")]
public sealed class IntegrationEventRecord : Entity<Guid>, IDisposable
{
    /// <summary>
    /// Constructs an integration event record from the specified integration event.
    /// </summary>
    /// <param name="event">The integration event to act with.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>An instance of <see cref="IntegrationEventRecord"/> integration 
    /// event record built from the integration event.</returns>
    public static IntegrationEventRecord FromIntegrationEvent(
        IIntegrationEvent @event,
        JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(@event);

        string typeFullName = @event.GetTypeFullName();
        JsonDocument eventData = @event.ToJsonDocument(options);

        return new(@event.Id, typeFullName, eventData);
    }

    /// <summary>
    /// Constructs a integration event from the specified record.
    /// </summary>
    /// <param name="record">The record to act with.</param>
    /// <param name="options">The serializer options.</param>
    /// <returns>An instance of integration event built from the entity.</returns>
    public static IIntegrationEvent? ToIntegrationEvent(
        IntegrationEventRecord record,
        JsonSerializerOptions? options)
    {
        ArgumentNullException.ThrowIfNull(record);

        if (!(Type.GetType(record.TypeFullName) is { } eventType))
            return null;

        object? eventObject = record.Data.Deserialize(eventType, options);
        return eventObject as IIntegrationEvent;
    }

    ///<inheritdoc/>
    private IntegrationEventRecord(
        Guid id,
        string typeFullName,
        JsonDocument data,
        string? errorMessage = default)
    {
        Id = id;
        TypeFullName = typeFullName;
        Data = data;
        ErrorMessage = errorMessage;
    }

    /// <inheritdoc/>
    public string TypeFullName { get; }

    ///<inheritdoc/>
    public JsonDocument Data { get; }

    ///<inheritdoc/>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Releases the <see cref="Data"/> resource.
    /// </summary>
    public void Dispose()
    {
        Data?.Dispose();
        GC.SuppressFinalize(this);
    }
}