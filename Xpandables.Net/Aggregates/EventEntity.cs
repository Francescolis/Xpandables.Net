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

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Aggregates;

/// <summary>
/// Represents an event entity.
/// </summary>
public abstract class EventEntity : Entity<Guid>, IEventEntity
{
    ///<inheritdoc/>
    public string EventTypeName { get; }

    ///<inheritdoc/>
    public string EventTypeFullName { get; }

    ///<inheritdoc/>
    [ConcurrencyCheck]
    public ulong Version { get; }

    ///<inheritdoc/>
    public JsonDocument Data { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="EventEntity"/> with the 
    /// specified values.
    /// </summary>
    /// <param name="id">The identifier of the event.</param>
    /// <param name="version">The version of the event.</param>
    /// <param name="eventTypeName">The name of the event type.</param>
    /// <param name="eventTypeFullName">The full name of the event type.</param>
    /// <param name="data">The data of the event.</param>
    protected EventEntity(
        Guid id,
        string eventTypeName,
        string eventTypeFullName,
        ulong version,
        JsonDocument data)
    {
        Id = id;
        EventTypeName = eventTypeName;
        EventTypeFullName = eventTypeFullName;
        Version = version;
        Data = data;
    }

    private bool IsDisposed;

    ///<inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected implementation of Dispose pattern.
    /// When overridden in derived classes, this method get 
    /// called when the instance will be disposed.
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            Data?.Dispose();

            // Release all managed resources here
            // Need to unregister/detach yourself from the events.
            // Always make sure the object is not null first before trying to
            // unregister/detach them!
            // Failure to unregister can be a BIG source of memory leaks
        }

        // Release all unmanaged resources here and override a finalizer below.
        // Set large fields to null.

        // Dispose has been called.
        IsDisposed = true;

        // If it is available, make the call to the
        // base class's Dispose(boolean) method
    }
}
