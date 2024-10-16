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

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events;

/// <summary>
/// Represents an abstract base class for event entities with a specific 
/// key and version.
/// </summary>
/// <typeparam name="TKey">The type of the key.</typeparam>
/// <typeparam name="TTimeStamp">The type of the timestamp.</typeparam>
public abstract class EventEntity<TKey, TTimeStamp> :
    Entity<TKey, TTimeStamp>,
    IEventEntity<TKey, TTimeStamp>
    where TKey : notnull, IComparable
    where TTimeStamp : notnull
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EventEntity{TKey, TVersion}"/> class.
    /// </summary>
    /// <param name="id">The identifier of the event entity.</param>
    /// <param name="eventName">The name of the event.</param>
    /// <param name="eventFullName">The full name of the event.</param>
    /// <param name="version">The version of the event entity.</param>
    /// <param name="eventData">The event data as a JSON document.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="id"/>, <paramref name="eventName"/>, 
    /// <paramref name="eventFullName"/>, or <paramref name="eventData"/> is null.
    /// </exception>
    protected EventEntity(
        TKey id,
        string eventName,
        string eventFullName,
        ulong version,
        JsonDocument eventData)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        EventName = eventName ?? throw new ArgumentNullException(nameof(eventName));
        EventFullName = eventFullName ?? throw new ArgumentNullException(nameof(eventFullName));
        Version = version;
        EventData = eventData ?? throw new ArgumentNullException(nameof(eventData));
    }

    /// <inheritdoc/>
    public string EventName { get; }

    /// <inheritdoc/>
    public string EventFullName { get; }

    /// <inheritdoc/>
    public JsonDocument EventData { get; }

    /// <inheritdoc/>
    public ulong Version { get; }

    private bool IsDisposed;

    ///<inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the EventEntity and optionally 
    /// releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged 
    /// resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (disposing)
        {
            EventData?.Dispose();
        }

        // Dispose has been called.
        IsDisposed = true;
    }
}